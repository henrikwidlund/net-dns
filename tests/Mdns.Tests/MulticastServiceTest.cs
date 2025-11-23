using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Makaretu.Dns;

namespace Makaretu.Mdns;

public class MulticastServiceTest
{
    [Test]
    public async Task Can_Create()
    {
        var mdns = new MulticastService();

        await Assert.That(mdns).IsNotNull();
        await Assert.That(mdns.IgnoreDuplicateMessages).IsTrue();
    }

    [Test]
    public async Task StartStop()
    {
        await Assert.That((Func<Task>)Action).IsCompletedSuccessfully();
        return;

        static async Task Action()
        {
            var mdns = new MulticastService();
            await mdns.Start(TestContext.Current!.Execution.CancellationToken);
            mdns.Stop();
        }
    }

    [Test]
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
        
        mdns.QueryReceived += async e =>
        {
            if ("some-service.local" == e.Message.Questions[0].Name)
            {
                msg = e.Message;
                await Assert.That(e.IsLegacyUnicast).IsFalse();
                done.Set();
            }
        };
        
        try
        {
            await mdns.Start(TestContext.Current!.Execution.CancellationToken);
            await Assert.That(ready.WaitOne(TimeSpan.FromSeconds(1))).IsTrue().Because("ready timeout");

            await mdns.SendQuery("some-service.local");
            await Assert.That(done.WaitOne(TimeSpan.FromSeconds(1))).IsTrue().Because("query timeout");
            await Assert.That(msg.Questions[0].Name?.ToString()).IsEqualTo("some-service.local");
            await Assert.That(msg.Questions[0].Class).IsEqualTo(DnsClass.IN);
        }
        finally
        {
            mdns.Stop();
        }
    }

    [Test]
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
            await mdns.Start(TestContext.Current!.Execution.CancellationToken);
            await Assert.That(ready.WaitOne(TimeSpan.FromSeconds(1))).IsTrue().Because("ready timeout");

            await mdns.SendUnicastQuery("some-service.local");
            await Assert.That(done.WaitOne(TimeSpan.FromSeconds(1))).IsTrue().Because("query timeout");
            await Assert.That(msg.Questions[0].Name?.ToString()).IsEqualTo("some-service.local");
            await Assert.That(msg.Questions[0].Class).IsEqualTo(DnsClass.IN + 0x8000);
        }
        finally
        {
            mdns.Stop();
        }
    }

    [Test]
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
            if (msg.Questions.Any(q => q.Name == service))
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
            if (msg.Answers.Any(answer => answer.Name == service))
            {
                response = msg;
                done.Set();
            }
            
            return Task.CompletedTask;
        };
        
        await mdns.Start(TestContext.Current!.Execution.CancellationToken);
        
        await Assert.That(done.WaitOne(TimeSpan.FromSeconds(1))).IsTrue().Because("answer timeout");
        await Assert.That(response).IsNotNull();
        await Assert.That(response.IsResponse).IsTrue();
        await Assert.That(response.Status).IsEqualTo(MessageStatus.NoError);
        await Assert.That(response.AA).IsTrue();
        var a = (ARecord)response.Answers[0];
        await Assert.That(a.Address).IsEqualTo(IPAddress.Parse("127.1.1.1"));
    }

    [Test]
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
            if (msg.Questions.Any(q => q.Name == service))
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
        
        await mdns.Start(TestContext.Current!.Execution.CancellationToken);
        
        await Assert.That(ready.WaitOne(TimeSpan.FromSeconds(1))).IsTrue().Because("ready timeout");
        MulticastService.IncludeLoopbackInterfaces = false;
        await client.SendAsync(packet, packet.Length, "224.0.0.251", 5353);
        
        using CancellationTokenSource cts = new(5000);
        var r = await client.ReceiveAsync(cts.Token);

        var response = new Message();
        response.Read(r.Buffer, 0, r.Buffer.Length);
        await Assert.That(response.IsResponse).IsTrue();
        await Assert.That(response.Status).IsEqualTo(MessageStatus.NoError);
        await Assert.That(response.AA).IsTrue();
        await Assert.That(response.Questions.Count).IsEqualTo(1);
        var a = (ARecord)response.Answers[0];
        await Assert.That(a.Address).IsEqualTo(IPAddress.Parse("127.1.1.1"));
        await Assert.That(a.Name?.ToString()).IsEqualTo(service);
        await Assert.That(a.TTL).IsEqualTo(TimeSpan.FromSeconds(10));
    }

    [Test]
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
            if (msg.Questions.Any(q => q.Name == service))
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
            if (msg.Answers.Any(answer => answer.Name == service))
            {
                response = msg;
                done.Set();
            }
                
            return Task.CompletedTask;
        };
        
        await mdns.Start(TestContext.Current!.Execution.CancellationToken);
        
        await Assert.That(done.WaitOne(TimeSpan.FromSeconds(1))).IsTrue().Because("answer timeout");
        await Assert.That(response).IsNotNull();
        await Assert.That(response.IsResponse).IsTrue();
        await Assert.That(response.Status).IsEqualTo(MessageStatus.NoError);
        await Assert.That(response.AA).IsTrue();
        var a = (ARecord)response.Answers[0];
        await Assert.That(a.Address).IsEqualTo(IPAddress.Parse("127.1.1.1"));
    }

    [Test]
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
            if (msg.Answers.Any(answer => answer.Name == service))
            {
                response = msg;
                done.Set();
            }

            return Task.CompletedTask;
        };
        
        await mdns.Start(TestContext.Current!.Execution.CancellationToken);
        
        await Assert.That(done.WaitOne(TimeSpan.FromSeconds(1))).IsTrue().Because("answer timeout");
        await Assert.That(response).IsNotNull();
        await Assert.That(response.IsResponse).IsTrue();
        await Assert.That(response.Status).IsEqualTo(MessageStatus.NoError);
        await Assert.That(response.AA).IsTrue();
        var a = (AAAARecord)response.Answers[0];
        await Assert.That(a.Address).IsEqualTo(IPAddress.Parse("::1"));
    }

    [Test]
    public async Task ReceiveErrorAnswer()
    {
        var service = $"{Guid.NewGuid()}.local";
        var done = new ManualResetEvent(false);

        using var mdns = new MulticastService();
        mdns.NetworkInterfaceDiscovered += _ => mdns.SendQuery(service);
        mdns.QueryReceived += async e =>
        {
            var msg = e.Message;
            if (msg.Questions.Any(q => q.Name == service))
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
            if (msg.Answers.Any(a => a.Name == service))
            {
                done.Set();
            }
            
            return Task.CompletedTask;
        };
        
        try
        {
            await mdns.Start(TestContext.Current!.Execution.CancellationToken);
            await Assert.That(done.WaitOne(TimeSpan.FromSeconds(0.5))).IsFalse().Because("answer was not ignored");
        }
        finally
        {
            mdns.Stop();
        }
    }

    [Test]
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
        
        await mdns.Start(TestContext.Current!.Execution.CancellationToken);
        
        try
        {
            await Assert.That(done.WaitOne(TimeSpan.FromSeconds(1))).IsTrue().Because("timeout");
            await Assert.That(nics.Any()).IsTrue();
        }
        finally
        {
            mdns.Stop();
        }
    }

    [Test]
    public async Task SendQuery_TooBig()
    {
        var done = new ManualResetEvent(false);
        using var mdns = new MulticastService();
        mdns.NetworkInterfaceDiscovered += _ =>
        {
            done.Set();
            return Task.CompletedTask;
        };
        
        await mdns.Start(TestContext.Current!.Execution.CancellationToken);
        
        try
        {
            await Assert.That(done.WaitOne(TimeSpan.FromSeconds(1))).IsTrue().Because("no nic");

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

    [Test]
    public async Task SendAnswer_TooBig()
    {
        var done = new ManualResetEvent(false);
        using var mdns = new MulticastService();
        mdns.NetworkInterfaceDiscovered += _ =>
        {
            done.Set();
            return Task.CompletedTask;
        };
        
        await mdns.Start(TestContext.Current!.Execution.CancellationToken);
        
        try
        {
            await Assert.That(done.WaitOne(TimeSpan.FromSeconds(1))).IsTrue().Because("no nic");
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

    [Test]
    public async Task Multiple_Services()
    {
        var service = $"{Guid.NewGuid()}.local";
        var done = new ManualResetEvent(false);
        Message response = null;

        using var a = new MulticastService();
        a.QueryReceived += async e =>
        {
            var msg = e.Message;
            if (msg.Questions.Any(q => q.Name == service))
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
            if (msg.Answers.Any(ans => ans.Name == service))
            {
                response = msg;
                done.Set();
            }
            
            return Task.CompletedTask;
        };
        
        try
        {
            await a.Start(TestContext.Current!.Execution.CancellationToken);
            await b.Start(TestContext.Current!.Execution.CancellationToken);
            
            await Assert.That(done.WaitOne(TimeSpan.FromSeconds(1))).IsTrue().Because("answer timeout");
            await Assert.That(response).IsNotNull();
            await Assert.That(response.IsResponse).IsTrue();
            await Assert.That(response.Status).IsEqualTo(MessageStatus.NoError);
            await Assert.That(response.AA).IsTrue();
            await Assert.That(response.Answers.Count).IsNotEqualTo(0);
        }
        finally
        {
            b.Stop();
            a.Stop();
        }
    }

    [Test]
    public async Task IPAddresses()
    {
        var addresses = MulticastService.GetIPAddresses().ToArray();
        await Assert.That(addresses.Length).IsNotEqualTo(0);
    }

    [Test]
    public async Task Disposable()
    {
        using (var mdns = new MulticastService())
            await Assert.That(mdns).IsNotNull();

        using (var mdns = new MulticastService())
        {
            await Assert.That(mdns).IsNotNull();
            await mdns.Start(TestContext.Current!.Execution.CancellationToken);
        }
    }

    [Test]
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
            if (msg.Questions.Any(q => q.Name == service))
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
        
        await mdns.Start(TestContext.Current!.Execution.CancellationToken);
        var response = await mdns.ResolveAsync(query, cancellation.Token);
        
        await Assert.That(response).IsNotNull().Because("no response");
        await Assert.That(response.IsResponse).IsTrue();
        await Assert.That(response.Status).IsEqualTo(MessageStatus.NoError);
        await Assert.That(response.AA).IsTrue();
        var a = (ARecord)response.Answers[0];
        await Assert.That(a.Address).IsEqualTo(IPAddress.Parse("127.1.1.1"));
    }

    [Test]
    public async Task Resolve_NoAnswer()
    {
        var service = $"{Guid.NewGuid()}.local";
        var query = new Message();
        query.Questions.Add(new Question { Name = service, Type = DnsType.ANY });
        using var cancellation = new CancellationTokenSource(500);

        using var mdns = new MulticastService();
        await mdns.Start(TestContext.Current!.Execution.CancellationToken);
        await ExceptionAssert.ThrowsAsync<TaskCanceledException>(async () =>
        {
            await mdns.ResolveAsync(query, cancellation.Token);
        });
    }

    [Test]
    public async Task DuplicateResponse()
    {
        var service = $"{Guid.NewGuid()}.local";
        using var mdns = new MulticastService();
        var answerCount = 0;
        mdns.NetworkInterfaceDiscovered += async _ =>
        {
            await mdns.SendQuery(service);
            await Task.Delay(250, TestContext.Current!.Execution.CancellationToken);
            await mdns.SendQuery(service);
        };
        
        mdns.QueryReceived += async e =>
        {
            var msg = e.Message;
            if (msg.Questions.Any(q => q.Name == service))
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
            if (msg.Answers.Any(answer => answer.Name == service))
                ++answerCount;
                
            return Task.CompletedTask;
        };
        
        await mdns.Start(TestContext.Current!.Execution.CancellationToken);
        await Task.Delay(1000, TestContext.Current!.Execution.CancellationToken);
        
        await Assert.That(answerCount).IsEqualTo(1);
    }

    [Test]
    public async Task NoDuplicateResponse()
    {
        var service = $"{Guid.NewGuid()}.local";

        using var mdns = new MulticastService();
        var answerCount = 0;
        mdns.NetworkInterfaceDiscovered += async _ =>
        {
            await mdns.SendQuery(service);
            await Task.Delay(250, TestContext.Current!.Execution.CancellationToken);
            await mdns.SendQuery(service);
        };
        
        mdns.QueryReceived += async e =>
        {
            var msg = e.Message;
            if (msg.Questions.Any(q => q.Name == service))
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
            if (msg.Answers.Any(answer => answer.Name == service))
                ++answerCount;
            
            return Task.CompletedTask;
        };
        
        await mdns.Start(TestContext.Current!.Execution.CancellationToken);
        await Task.Delay(2000, TestContext.Current!.Execution.CancellationToken);
        await Assert.That(answerCount).IsEqualTo(1);

        await mdns.SendQuery(service);
        await Task.Delay(2000, TestContext.Current!.Execution.CancellationToken);
        await Assert.That(answerCount).IsEqualTo(2);
    }

    [Test]
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
        
        await mdns1.Start(TestContext.Current!.Execution.CancellationToken);

        mdns2.NetworkInterfaceDiscovered += _ =>
        {
            ready2.Set();
            return Task.CompletedTask;
        };
        
        await mdns2.Start(TestContext.Current!.Execution.CancellationToken);

        await Assert.That(ready1.WaitOne(TimeSpan.FromSeconds(1))).IsTrue().Because("ready1 timeout");
        await Assert.That(ready2.WaitOne(TimeSpan.FromSeconds(1))).IsTrue().Because("ready2 timeout");
    }

    [Test]
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

        await Assert.That(malformedMessage).IsEqualTo(msg);
    }
}