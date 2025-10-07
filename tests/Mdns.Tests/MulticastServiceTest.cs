using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Makaretu.Dns;
using Shouldly;
using Xunit;

namespace Makaretu.Mdns;

public class MulticastServiceTest
{
    [Fact]
    public void Can_Create()
    {
        var mdns = new MulticastService();
        
        mdns.ShouldNotBeNull();
        mdns.IgnoreDuplicateMessages.ShouldBeTrue();
    }

    [Fact]
    public async Task StartStop()
    {
        var action = static async () =>
        {
            var mdns = new MulticastService();
            await mdns.Start(CancellationToken.None);
            mdns.Stop();
        };

        await action.ShouldNotThrowAsync();
    }

    [Fact]
    public async Task SendQuery()
    {
        var ready = new ManualResetEvent(false);
        var done = new ManualResetEvent(false);
        Message msg = null;

        using var mdns = new MulticastService();
        mdns.NetworkInterfaceDiscovered += _ =>
        {
            ready.Set();
            return Task.CompletedTask;
        };
        
        mdns.QueryReceived += e =>
        {
            if ("some-service.local" == e.Message.Questions[0].Name)
            {
                msg = e.Message;
                e.IsLegacyUnicast.ShouldBeFalse();
                done.Set();
            }

            return Task.CompletedTask;
        };
        
        try
        {
            await mdns.Start(CancellationToken.None);
            ready.WaitOne(TimeSpan.FromSeconds(1)).ShouldBeTrue("ready timeout");

            await mdns.SendQuery("some-service.local");
            done.WaitOne(TimeSpan.FromSeconds(1)).ShouldBeTrue("query timeout");
            msg.Questions[0].Name.ShouldBe("some-service.local");
            msg.Questions[0].Class.ShouldBe(DnsClass.IN);
        }
        finally
        {
            mdns.Stop();
        }
    }

    [Fact]
    public async Task SendUnicastQuery()
    {
        var ready = new ManualResetEvent(false);
        var done = new ManualResetEvent(false);
        Message msg = null;

        using var mdns = new MulticastService();
        mdns.NetworkInterfaceDiscovered += _ =>
        {
            ready.Set();
            return Task.CompletedTask;
        };
        
        mdns.QueryReceived += e =>
        {
            msg = e.Message;
            done.Set();
            return Task.CompletedTask;
        };
        
        try
        {
            await mdns.Start(CancellationToken.None);
            ready.WaitOne(TimeSpan.FromSeconds(1)).ShouldBeTrue("ready timeout");

            await mdns.SendUnicastQuery("some-service.local");
            done.WaitOne(TimeSpan.FromSeconds(1)).ShouldBeTrue("query timeout");
            msg.Questions[0].Name.ShouldBe("some-service.local");
            msg.Questions[0].Class.ShouldBe(DnsClass.IN + 0x8000);
        }
        finally
        {
            mdns.Stop();
        }
    }

    [Fact]
    public async Task ReceiveAnswer()
    {
        var service = $"{Guid.NewGuid()}.local";
        var done = new ManualResetEvent(false);
        Message response = null;

        using var mdns = new MulticastService();
        mdns.NetworkInterfaceDiscovered += _ => mdns.SendQuery(service);
        mdns.QueryReceived += async e =>
        {
            var msg = e.Message;
            if (msg.Questions.Exists(q => q.Name == service))
            {
                var res = msg.CreateResponse();
                res.Answers.Add(new ARecord
                {
                    Name = service,
                    Address = IPAddress.Parse("127.1.1.1")
                });
                await mdns.SendAnswer(res);
            }
        };
        
        mdns.AnswerReceived += e =>
        {
            var msg = e.Message;
            if (msg.Answers.Exists(answer => answer.Name == service))
            {
                response = msg;
                done.Set();
            }
            
            return Task.CompletedTask;
        };
        
        await mdns.Start(CancellationToken.None);
        
        done.WaitOne(TimeSpan.FromSeconds(1)).ShouldBeTrue("answer timeout");
        response.ShouldNotBeNull();
        response.IsResponse.ShouldBeTrue();
        response.Status.ShouldBe(MessageStatus.NoError);
        response.AA.ShouldBeTrue();
        var a = (ARecord)response.Answers[0];
        a.Address.ShouldBe(IPAddress.Parse("127.1.1.1"));
    }

    [Fact]
    public async Task ReceiveLegacyUnicastAnswer()
    {
        var service = $"{Guid.NewGuid()}.local";
        var ready = new ManualResetEvent(false);

        var query = new Message();
        query.Questions.Add(new Question
        {
            Name = service,
            Type = DnsType.A
        });
        
        var packet = query.ToByteArray();
        using var client = new UdpClient();
        MulticastService.IncludeLoopbackInterfaces = true;
        using var mdns = new MulticastService();
        mdns.NetworkInterfaceDiscovered += _ =>
        {
            ready.Set();
            return Task.CompletedTask;
        };
        
        mdns.QueryReceived += async e =>
        {
            var msg = e.Message;
            if (msg.Questions.Exists(q => q.Name == service))
            {
                var res = msg.CreateResponse();
                res.Answers.Add(new ARecord
                {
                    Name = service,
                    Address = IPAddress.Parse("127.1.1.1")
                });
                await mdns.SendAnswer(res, e);
            }
        };
        
        await mdns.Start(CancellationToken.None);
        
        ready.WaitOne(TimeSpan.FromSeconds(1)).ShouldBeTrue("ready timeout");
        MulticastService.IncludeLoopbackInterfaces = false;
        await client.SendAsync(packet, packet.Length, "224.0.0.251", 5353);
        
        using CancellationTokenSource cts = new(5000);
        var r = await client.ReceiveAsync(cts.Token);

        var response = new Message();
        response.Read(r.Buffer, 0, r.Buffer.Length);
        response.IsResponse.ShouldBeTrue();
        response.Status.ShouldBe(MessageStatus.NoError);
        response.AA.ShouldBeTrue();
        response.Questions.Count.ShouldBe(1);
        var a = (ARecord)response.Answers[0];
        a.Address.ShouldBe(IPAddress.Parse("127.1.1.1"));
        a.Name.ShouldBe(service);
        a.TTL.ShouldBe(TimeSpan.FromSeconds(10));
    }

    [Fact]
    public async Task ReceiveAnswer_IPv4()
    {
        var service = $"{Guid.NewGuid()}.local";
        var done = new ManualResetEvent(false);
        Message response = null;

        using var mdns = new MulticastService();
        mdns.UseIpv4 = true;
        mdns.UseIpv6 = false;
        mdns.NetworkInterfaceDiscovered += _ => mdns.SendQuery(service);
        mdns.QueryReceived += async e =>
        {
            var msg = e.Message;
            if (msg.Questions.Exists(q => q.Name == service))
            {
                var res = msg.CreateResponse();
                res.Answers.Add(new ARecord
                {
                    Name = service,
                    Address = IPAddress.Parse("127.1.1.1")
                });
                await mdns.SendAnswer(res);
            }
        };
        
        mdns.AnswerReceived += e =>
        {
            var msg = e.Message;
            if (msg.Answers.Exists(answer => answer.Name == service))
            {
                response = msg;
                done.Set();
            }
                
            return Task.CompletedTask;
        };
        
        await mdns.Start(CancellationToken.None);
        
        done.WaitOne(TimeSpan.FromSeconds(1)).ShouldBeTrue("answer timeout");
        response.ShouldNotBeNull();
        response.IsResponse.ShouldBeTrue();
        response.Status.ShouldBe(MessageStatus.NoError);
        response.AA.ShouldBeTrue();
        var a = (ARecord)response.Answers[0];
        a.Address.ShouldBe(IPAddress.Parse("127.1.1.1"));
    }

    [Fact(Skip = "IPv6 is not supported on this host")]
    public async Task ReceiveAnswer_IPv6()
    {
        if (!Socket.OSSupportsIPv6)
            return;

        var service = $"{Guid.NewGuid()}.local";
        var done = new ManualResetEvent(false);
        Message response = null;
        MulticastService.IncludeLoopbackInterfaces = true;
        using var mdns = new MulticastService();
        if (!MulticastService.GetNetworkInterfaces().Any())
            return;
        mdns.UseIpv4 = false;
        mdns.UseIpv6 = true;
        mdns.NetworkInterfaceDiscovered += async _ =>
        {
            await mdns.SendQuery(service);
            MulticastService.IncludeLoopbackInterfaces = false;
        };
        
        mdns.QueryReceived += async e =>
        {
            var msg = e.Message;
            if (msg.Questions.Any(q => q.Name == service))
            {
                var res = msg.CreateResponse();
                res.Answers.Add(new AAAARecord
                {
                    Name = service,
                    Address = IPAddress.Parse("::1")
                });
                await mdns.SendAnswer(res);
            }
        };
        
        mdns.AnswerReceived += e =>
        {
            var msg = e.Message;
            if (msg.Answers.Exists(answer => answer.Name == service))
            {
                response = msg;
                done.Set();
            }

            return Task.CompletedTask;
        };
        
        await mdns.Start(CancellationToken.None);
        
        done.WaitOne(TimeSpan.FromSeconds(1)).ShouldBeTrue("answer timeout");
        response.ShouldNotBeNull();
        response.IsResponse.ShouldBeTrue();
        response.Status.ShouldBe(MessageStatus.NoError);
        response.AA.ShouldBeTrue();
        var a = (AAAARecord)response.Answers[0];
        a.Address.ShouldBe(IPAddress.Parse("::1"));
    }

    [Fact]
    public async Task ReceiveErrorAnswer()
    {
        var service = $"{Guid.NewGuid()}.local";
        var done = new ManualResetEvent(false);

        using var mdns = new MulticastService();
        mdns.NetworkInterfaceDiscovered += _ => mdns.SendQuery(service);
        mdns.QueryReceived += async e =>
        {
            var msg = e.Message;
            if (msg.Questions.Exists(q => q.Name == service))
            {
                var res = msg.CreateResponse();
                res.Status = MessageStatus.Refused;
                res.Answers.Add(new ARecord
                {
                    Name = service,
                    Address = IPAddress.Parse("127.1.1.1")
                });
                await mdns.SendAnswer(res);
            }
        };
        
        mdns.AnswerReceived += e =>
        {
            var msg = e.Message;
            if (msg.Answers.Exists(a => a.Name == service))
            {
                done.Set();
            }
            
            return Task.CompletedTask;
        };
        
        try
        {
            await mdns.Start(CancellationToken.None);
            done.WaitOne(TimeSpan.FromSeconds(0.5)).ShouldBeFalse("answer was not ignored");
        }
        finally
        {
            mdns.Stop();
        }
    }

    [Fact]
    public async Task Nics()
    {
        var done = new ManualResetEvent(false);
        using var mdns = new MulticastService();
        IEnumerable<NetworkInterface> nics = null;
        mdns.NetworkInterfaceDiscovered += e =>
        {
            nics = e.NetworkInterfaces;
            done.Set();
            
            return Task.CompletedTask;
        };
        
        await mdns.Start(CancellationToken.None);
        
        try
        {
            done.WaitOne(TimeSpan.FromSeconds(1)).ShouldBeTrue("timeout");
            nics.Any().ShouldBeTrue();
        }
        finally
        {
            mdns.Stop();
        }
    }

    [Fact]
    public async Task SendQuery_TooBig()
    {
        var done = new ManualResetEvent(false);
        using var mdns = new MulticastService();
        mdns.NetworkInterfaceDiscovered += _ =>
        {
            done.Set();
            return Task.CompletedTask;
        };
        
        await mdns.Start(CancellationToken.None);
        
        try
        {
            done.WaitOne(TimeSpan.FromSeconds(1)).ShouldBeTrue("no nic");

            var query = new Message();
            query.Questions.Add(new Question { Name = "foo.bar.org" });
            query.AdditionalRecords.Add(new NULLRecord { Name = "foo.bar.org", Data = new byte[9000] });
            await ExceptionAssert.ThrowsAsync<ArgumentOutOfRangeException>(async () =>
            {
                await mdns.SendQuery(query);
            });
        }
        finally
        {
            mdns.Stop();
        }
    }

    [Fact]
    public async Task SendAnswer_TooBig()
    {
        var done = new ManualResetEvent(false);
        using var mdns = new MulticastService();
        mdns.NetworkInterfaceDiscovered += _ =>
        {
            done.Set();
            return Task.CompletedTask;
        };
        
        await mdns.Start(CancellationToken.None);
        
        try
        {
            done.WaitOne(TimeSpan.FromSeconds(1)).ShouldBeTrue("no nic");
            var answer = new Message();
            answer.Answers.Add(new ARecord { Name = "foo.bar.org", Address = IPAddress.Loopback });
            answer.Answers.Add(new NULLRecord { Name = "foo.bar.org", Data = new byte[9000] });
            await ExceptionAssert.ThrowsAsync<ArgumentOutOfRangeException>(async () =>
            {
                await mdns.SendAnswer(answer);
            });
        }
        finally
        {
            mdns.Stop();
        }
    }

    [Fact]
    public async Task Multiple_Services()
    {
        var service = $"{Guid.NewGuid()}.local";
        var done = new ManualResetEvent(false);
        Message response = null;

        using var a = new MulticastService();
        a.QueryReceived += async e =>
        {
            var msg = e.Message;
            if (msg.Questions.Exists(q => q.Name == service))
            {
                var res = msg.CreateResponse();
                var addresses = MulticastService.GetIPAddresses()
                    .Where(static ip => ip.AddressFamily == AddressFamily.InterNetwork);
                
                foreach (var address in addresses)
                {
                    res.Answers.Add(new ARecord
                    {
                        Name = service,
                        Address = address
                    });
                }
                
                await a.SendAnswer(res);
            }
        };

        using var b = new MulticastService();
        b.NetworkInterfaceDiscovered += _ => b.SendQuery(service);
        b.AnswerReceived += e =>
        {
            var msg = e.Message;
            if (msg.Answers.Exists(ans => ans.Name == service))
            {
                response = msg;
                done.Set();
            }
            
            return Task.CompletedTask;
        };
        
        try
        {
            await a.Start(CancellationToken.None);
            await b.Start(CancellationToken.None);
            
            done.WaitOne(TimeSpan.FromSeconds(1)).ShouldBeTrue("answer timeout");
            response.ShouldNotBeNull();
            response.IsResponse.ShouldBeTrue();
            response.Status.ShouldBe(MessageStatus.NoError);
            response.AA.ShouldBeTrue();
            response.Answers.Count.ShouldNotBe(0);
        }
        finally
        {
            b.Stop();
            a.Stop();
        }
    }

    [Fact]
    public void IPAddresses()
    {
        var addresses = MulticastService.GetIPAddresses().ToArray();
        addresses.Length.ShouldNotBe(0);
    }

    [Fact]
    public async Task Disposable()
    {
        using (var mdns = new MulticastService())
            mdns.ShouldNotBeNull();

        using (var mdns = new MulticastService())
        {
            mdns.ShouldNotBeNull();
            await mdns.Start(CancellationToken.None);
        }
    }

    [Fact]
    public async Task Resolve()
    {
        var service = $"{Guid.NewGuid()}.local";
        var query = new Message();
        query.Questions.Add(new Question { Name = service, Type = DnsType.ANY });
        using var cancellation = new CancellationTokenSource(2000);

        using var mdns = new MulticastService();
        mdns.QueryReceived += async e =>
        {
            var msg = e.Message;
            if (msg.Questions.Exists(q => q.Name == service))
            {
                var res = msg.CreateResponse();
                res.Answers.Add(new ARecord
                {
                    Name = service,
                    Address = IPAddress.Parse("127.1.1.1")
                });
                await mdns.SendAnswer(res);
            }
        };
        
        await mdns.Start(CancellationToken.None);
        var response = await mdns.ResolveAsync(query, cancellation.Token);
        
        response.ShouldNotBeNull("no response");
        response.IsResponse.ShouldBeTrue();
        response.Status.ShouldBe(MessageStatus.NoError);
        response.AA.ShouldBeTrue();
        var a = (ARecord)response.Answers[0];
        a.Address.ShouldBe(IPAddress.Parse("127.1.1.1"));
    }

    [Fact]
    public async Task Resolve_NoAnswer()
    {
        var service = $"{Guid.NewGuid()}.local";
        var query = new Message();
        query.Questions.Add(new Question { Name = service, Type = DnsType.ANY });
        using var cancellation = new CancellationTokenSource(500);

        using var mdns = new MulticastService();
        await mdns.Start(CancellationToken.None);
        await ExceptionAssert.ThrowsAsync<TaskCanceledException>(async () =>
        {
            await mdns.ResolveAsync(query, cancellation.Token);
        });
    }

    [Fact]
    public async Task DuplicateResponse()
    {
        var service = $"{Guid.NewGuid()}.local";
        using var mdns = new MulticastService();
        var answerCount = 0;
        mdns.NetworkInterfaceDiscovered += async _ =>
        {
            await mdns.SendQuery(service);
            await Task.Delay(250);
            await mdns.SendQuery(service);
        };
        
        mdns.QueryReceived += async e =>
        {
            var msg = e.Message;
            if (msg.Questions.Exists(q => q.Name == service))
            {
                var res = msg.CreateResponse();
                res.Answers.Add(new ARecord
                {
                    Name = service,
                    Address = IPAddress.Parse("127.1.1.1")
                });
                await mdns.SendAnswer(res);
            }
        };
        
        mdns.AnswerReceived += e =>
        {
            var msg = e.Message;
            if (msg.Answers.Exists(answer => answer.Name == service))
                ++answerCount;
                
            return Task.CompletedTask;
        };
        
        await mdns.Start(CancellationToken.None);
        await Task.Delay(1000);
        
        answerCount.ShouldBe(1);
    }

    [Fact]
    public async Task NoDuplicateResponse()
    {
        var service = $"{Guid.NewGuid()}.local";

        using var mdns = new MulticastService();
        var answerCount = 0;
        mdns.NetworkInterfaceDiscovered += async _ =>
        {
            await mdns.SendQuery(service);
            await Task.Delay(250);
            await mdns.SendQuery(service);
        };
        
        mdns.QueryReceived += async e =>
        {
            var msg = e.Message;
            if (msg.Questions.Exists(q => q.Name == service))
            {
                var res = msg.CreateResponse();
                res.Answers.Add(new ARecord
                {
                    Name = service,
                    Address = IPAddress.Parse("127.1.1.1")
                });
                
                await mdns.SendAnswer(res, checkDuplicate: false);
            }
        };
        
        mdns.AnswerReceived += e =>
        {
            var msg = e.Message;
            if (msg.Answers.Exists(answer => answer.Name == service))
                ++answerCount;
            
            return Task.CompletedTask;
        };
        
        await mdns.Start(CancellationToken.None);
        await Task.Delay(2000);
        answerCount.ShouldBe(1);

        await mdns.SendQuery(service);
        await Task.Delay(2000);
        answerCount.ShouldBe(2);
    }

    [Fact]
    public async Task Multiple_Listeners()
    {
        var ready1 = new ManualResetEvent(false);
        var ready2 = new ManualResetEvent(false);
        using var mdns1 = new MulticastService();
        using var mdns2 = new MulticastService();
        mdns1.NetworkInterfaceDiscovered += _ =>
        {
            ready1.Set();
            return Task.CompletedTask;
        };
        
        await mdns1.Start(CancellationToken.None);

        mdns2.NetworkInterfaceDiscovered += _ =>
        {
            ready2.Set();
            return Task.CompletedTask;
        };
        
        await mdns2.Start(CancellationToken.None);

        ready1.WaitOne(TimeSpan.FromSeconds(1)).ShouldBeTrue("ready1 timeout");
        ready2.WaitOne(TimeSpan.FromSeconds(1)).ShouldBeTrue("ready2 timeout");
    }

    [Fact]
    public async Task MalformedMessage()
    {
        byte[] malformedMessage = null;
        using var mdns = new MulticastService();
        mdns.MalformedMessage += e =>
        {
            malformedMessage = e;
            return Task.CompletedTask;
        };

        var msg = new byte[] { 0xff };
        var endPoint = new IPEndPoint(IPAddress.Loopback, 5353);
        var udp = new UdpReceiveResult(msg, endPoint);
        await mdns.OnDnsMessage(udp);

        malformedMessage.ShouldBe(msg);
    }
}