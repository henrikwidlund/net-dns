using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Makaretu.Dns;

namespace Makaretu.Mdns;

[ParallelLimiter<SingleTestRateLimit>]
public class ServiceDiscoveryTest
{
    [Test]
    public async Task Disposable()
    {
        using (var sd = await ServiceDiscovery.CreateInstance(cancellationToken: TestContext.Current!.Execution.CancellationToken))
            await Assert.That(sd).IsNotNull();

        var mdns = new MulticastService();
        using (var sd = await ServiceDiscovery.CreateInstance(mdns, cancellationToken: TestContext.Current!.Execution.CancellationToken))
            await Assert.That(sd).IsNotNull();
    }

    [Test]
    public async Task Advertises_Service()
    {
        var service = new ServiceProfile("x", "_sdtest-1._udp", 1024, [IPAddress.Loopback]);
        var done = new ManualResetEvent(false);

        var mdns = new MulticastService();
        mdns.NetworkInterfaceDiscovered += _ =>
            mdns.SendQuery(ServiceDiscovery.ServiceName, DnsClass.IN, DnsType.PTR);
        
        mdns.AnswerReceived += e =>
        {
            var msg = e.Message;
            if (msg.Answers.OfType<PTRRecord>().Any(p => p.DomainName == service.QualifiedServiceName && ((int)p.Class & MulticastService.CacheFlushBit) != 0))
                Assert.Fail("shared PTR records should not have cache-flush set");

            if (msg.Answers.OfType<PTRRecord>().Any(p => p.DomainName == service.QualifiedServiceName))
                done.Set();
            
            return Task.CompletedTask;
        };
        
        try
        {
            using var sd = await ServiceDiscovery.CreateInstance(mdns, cancellationToken: TestContext.Current!.Execution.CancellationToken);
            sd.Advertise(service);
            await mdns.Start(TestContext.Current!.Execution.CancellationToken);

            await Assert.That(done.WaitOne(TimeSpan.FromSeconds(1))).IsTrue().Because("query timeout");
        }
        finally
        {
            mdns.Stop();
        }
    }

    [Test]
    public async Task Advertises_SharedService()
    {
        var service = new ServiceProfile("x", "_sdtest-1._udp", 1024, [IPAddress.Loopback], true);
        var done = new ManualResetEvent(false);
        
        await Assert.That(service.SharedProfile).IsTrue().Because("Shared Profile was not set");

        using var mdns = new MulticastService();
        mdns.NetworkInterfaceDiscovered += _ => mdns.SendQuery(service.QualifiedServiceName);
        mdns.AnswerReceived += e =>
        {
            var msg = e.Message;
            if (msg.Answers.OfType<PTRRecord>().Any(p => p.DomainName == service.QualifiedServiceName && ((int)p.Class & MulticastService.CacheFlushBit) != 0))
                Assert.Fail("shared PTR records should not have cache-flush set");
            
            if (msg.AdditionalRecords.OfType<SRVRecord>().Any(s => (s.Name == service.FullyQualifiedName && ((int)s.Class & MulticastService.CacheFlushBit) == 0)))
                done.Set();
            
            return Task.CompletedTask;
        };
        
        try
        {
            using var sd = await ServiceDiscovery.CreateInstance(mdns, cancellationToken: TestContext.Current!.Execution.CancellationToken);
            sd.Advertise(service);
            await mdns.Start(TestContext.Current!.Execution.CancellationToken);
            
            await Assert.That(done.WaitOne(TimeSpan.FromSeconds(1))).IsTrue().Because("query timeout");
        }
        finally
        {
            mdns.Stop();
        }
    }

    [Test]
    public async Task Probe_Service()
    {
        var service = new ServiceProfile("z", "_sdtest-11._udp", 1024, [IPAddress.Loopback]);
        var done = new ManualResetEvent(false);

        using var mdns = new MulticastService();
        using var sd = await ServiceDiscovery.CreateInstance(mdns, cancellationToken: TestContext.Current!.Execution.CancellationToken);
        mdns.NetworkInterfaceDiscovered += async _ =>
            {
                if (await sd.Probe(service))
                    done.Set();
            };
        
        try
        {
            sd.Advertise(service);
            await mdns.Start(TestContext.Current!.Execution.CancellationToken);
            
            await Assert.That(done.WaitOne(TimeSpan.FromSeconds(3))).IsTrue().Because("Probe timeout");
        }
        finally
        {
            mdns.Stop();
        }
    }

    [Test]
    public async Task Probe_Service2()
    {
        var service = new ServiceProfile("z", "_sdtest-11._udp", 1024, [IPAddress.Loopback]);

        using var sd = await ServiceDiscovery.CreateInstance(cancellationToken: TestContext.Current!.Execution.CancellationToken);
        sd.Advertise(service);
        await sd.Mdns!.Start(TestContext.Current!.Execution.CancellationToken);
        
        var mdns = new MulticastService();
        using var sd2 = await ServiceDiscovery.CreateInstance(mdns, cancellationToken: TestContext.Current!.Execution.CancellationToken);
        mdns.NetworkInterfaceDiscovered += async _ =>
        {
            await Assert.That(await sd2.Probe(service)).IsTrue();
        };
        
        try
        {
            await mdns.Start(TestContext.Current!.Execution.CancellationToken);
        }
        finally
        {
            mdns.Stop();
        }
    }

    [Test]
    public async Task Probe_Service3()
    {
        var service = new ServiceProfile("z", "_sdtest-11._udp", 1024, [IPAddress.Loopback]);

        var mdns = new MulticastService();
        using var sd = await ServiceDiscovery.CreateInstance(mdns, cancellationToken: TestContext.Current!.Execution.CancellationToken);
        mdns.NetworkInterfaceDiscovered += async _ =>
        {
            await Assert.That(await sd.Probe(service)).IsFalse();
        };
        
        try
        {
            await mdns.Start(TestContext.Current!.Execution.CancellationToken);
        }
        finally
        {
            mdns.Stop();
        }
    }

    [Test]
    public async Task Advertises_ServiceInstances()
    {
        var service = new ServiceProfile("x", "_sdtest-1._udp", 1024, [IPAddress.Loopback]);
        var done = new ManualResetEvent(false);

        using var mdns = new MulticastService();
        mdns.NetworkInterfaceDiscovered += _ => mdns.SendQuery(service.QualifiedServiceName, DnsClass.IN, DnsType.PTR);
        mdns.AnswerReceived += e =>
        {
            var msg = e.Message;
            if (msg.Answers.OfType<PTRRecord>().Any(p => p.DomainName == service.FullyQualifiedName))
                done.Set();
            
            return Task.CompletedTask;
        };
        
        try
        {
            using var sd = await ServiceDiscovery.CreateInstance(mdns, cancellationToken: TestContext.Current!.Execution.CancellationToken);
            sd.Advertise(service);
            await mdns.Start(TestContext.Current!.Execution.CancellationToken);
            
            await Assert.That(done.WaitOne(TimeSpan.FromSeconds(1))).IsTrue().Because("query timeout");
        }
        finally
        {
            mdns.Stop();
        }
    }

    [Test]
    public async Task Advertises_ServiceInstance_Address()
    {
        var service = new ServiceProfile("x2", "_sdtest-1._udp", 1024, [IPAddress.Loopback]);
        var done = new ManualResetEvent(false);

        using var mdns = new MulticastService();
        mdns.NetworkInterfaceDiscovered += _ => mdns.SendQuery(service.HostName, DnsClass.IN, DnsType.A);
        mdns.AnswerReceived += e =>
        {
            var msg = e.Message;
            if (msg.Answers.OfType<ARecord>().Any(p => p.Name == service.HostName))
                done.Set();
            
            return Task.CompletedTask;
        };
        try
        {
            using var sd = await ServiceDiscovery.CreateInstance(mdns, cancellationToken: TestContext.Current!.Execution.CancellationToken);
            sd.Advertise(service);
            await mdns.Start(TestContext.Current!.Execution.CancellationToken);
            
            await Assert.That(done.WaitOne(TimeSpan.FromSeconds(1))).IsTrue().Because("query timeout");
        }
        finally
        {
            mdns.Stop();
        }
    }

    [Test]
    public async Task Advertises_ServiceInstance_Subtype()
    {
        var service = new ServiceProfile("x2", "_sdtest-1._udp", 1024, [IPAddress.Loopback]);
        service.Subtypes.Add("_example");
        var done = new ManualResetEvent(false);

        using var mdns = new MulticastService();
        mdns.NetworkInterfaceDiscovered += _ => mdns.SendQuery("_example._sub._sdtest-1._udp.local", DnsClass.IN, DnsType.PTR);
        mdns.AnswerReceived += e =>
        {
            var msg = e.Message;
            if (msg.Answers.OfType<PTRRecord>().Any(p => p.DomainName == service.FullyQualifiedName))
                done.Set();
            
            return Task.CompletedTask;
        };
        
        try
        {
            using var sd = await ServiceDiscovery.CreateInstance(mdns, cancellationToken: TestContext.Current!.Execution.CancellationToken);
            sd.Advertise(service);
            await mdns.Start(TestContext.Current!.Execution.CancellationToken);
            
            await Assert.That(done.WaitOne(TimeSpan.FromSeconds(1))).IsTrue().Because("query timeout");
        }
        finally
        {
            mdns.Stop();
        }
    }

    [Test]
    public async Task Discover_AllServices()
    {
        var service = new ServiceProfile("x", "_sdtest-2._udp", 1024);
        var done = new ManualResetEvent(false);
        using var mdns = new MulticastService();
        using var sd = await ServiceDiscovery.CreateInstance(mdns, cancellationToken: TestContext.Current!.Execution.CancellationToken);

        mdns.NetworkInterfaceDiscovered += _ => sd.QueryAllServices();
        sd.ServiceDiscovered += serviceName =>
        {
            if (serviceName == service.QualifiedServiceName)
                done.Set();
            
            return Task.CompletedTask;
        };
        
        try
        {
            sd.Advertise(service);
            await mdns.Start(TestContext.Current!.Execution.CancellationToken);
            
            await Assert.That(done.WaitOne(TimeSpan.FromSeconds(1))).IsTrue().Because("DNS-SD query timeout");
        }
        finally
        {
            mdns.Stop();
        }
    }

    [Test]
    public async Task Discover_AllServices_Unicast()
    {
        var service = new ServiceProfile("x", "_sdtest-5._udp", 1024);
        var done = new ManualResetEvent(false);
        using var mdns = new MulticastService();
        using var sd = await ServiceDiscovery.CreateInstance(mdns, cancellationToken: TestContext.Current!.Execution.CancellationToken);

        mdns.NetworkInterfaceDiscovered += _ => sd.QueryUnicastAllServices();
        sd.ServiceDiscovered += serviceName =>
        {
            if (serviceName == service.QualifiedServiceName)
                done.Set();
            
            return Task.CompletedTask;
        };
        try
        {
            sd.Advertise(service);
            await mdns.Start(TestContext.Current!.Execution.CancellationToken);
            
            await Assert.That(done.WaitOne(TimeSpan.FromSeconds(1))).IsTrue().Because("DNS-SD query timeout");
        }
        finally
        {
            mdns.Stop();
        }
    }

    [Test]
    public async Task Discover_ServiceInstance()
    {
        var service = new ServiceProfile("y", "_sdtest-2._udp", 1024);
        var done = new ManualResetEvent(false);
        using var mdns = new MulticastService();
        using var sd = await ServiceDiscovery.CreateInstance(mdns, cancellationToken: TestContext.Current!.Execution.CancellationToken);

        mdns.NetworkInterfaceDiscovered += async _ =>
        {
            await sd.QueryServiceInstances(service.ServiceName!);
        };

        sd.ServiceInstanceDiscovered += async e =>
        {
            if (e.ServiceInstanceName == service.FullyQualifiedName)
            {
                await Assert.That(e.Message).IsNotNull();
                done.Set();
            }
        };
        
        try
        {
            sd.Advertise(service);
            await mdns.Start(TestContext.Current!.Execution.CancellationToken);
            
            await Assert.That(done.WaitOne(TimeSpan.FromSeconds(1))).IsTrue().Because("instance not found");
        }
        finally
        {
            mdns.Stop();
        }
    }

    [Test]
    public async Task Discover_ServiceInstance_with_Subtype()
    {
        var service1 = new ServiceProfile("x", "_sdtest-2._udp", 1024);
        var service2 = new ServiceProfile("y", "_sdtest-2._udp", 1024);
        service2.Subtypes.Add("apiv2");
        var done = new ManualResetEvent(false);
        using var mdns = new MulticastService();
        using var sd = await ServiceDiscovery.CreateInstance(mdns, cancellationToken: TestContext.Current!.Execution.CancellationToken);

        mdns.NetworkInterfaceDiscovered += async _ =>
        {
            await sd.QueryServiceInstances("_sdtest-2._udp", "apiv2");
        };

        sd.ServiceInstanceDiscovered += async e =>
        {
            if (e.ServiceInstanceName == service2.FullyQualifiedName)
            {
                await Assert.That(e.Message).IsNotNull();
                done.Set();
            }
        };
        
        try
        {
            sd.Advertise(service1);
            sd.Advertise(service2);
            await mdns.Start(TestContext.Current!.Execution.CancellationToken);
            
            await Assert.That(done.WaitOne(TimeSpan.FromSeconds(1))).IsTrue().Because("instance not found");
        }
        finally
        {
            mdns.Stop();
        }
    }

    [Test]
    public async Task Discover_ServiceInstance_Unicast()
    {
        var service = new ServiceProfile("y", "_sdtest-5._udp", 1024);
        var done = new ManualResetEvent(false);
        using var mdns = new MulticastService();
        using var sd = await ServiceDiscovery.CreateInstance(mdns, cancellationToken: TestContext.Current!.Execution.CancellationToken);

        mdns.NetworkInterfaceDiscovered += async _ =>
        {
            await sd.QueryServiceInstances(service.ServiceName!);
        };

        sd.ServiceInstanceDiscovered += async e =>
        {
            if (e.ServiceInstanceName == service.FullyQualifiedName)
            {
                await Assert.That(e.Message).IsNotNull();
                done.Set();
            }
        };
        
        try
        {
            sd.Advertise(service);
            await mdns.Start(TestContext.Current!.Execution.CancellationToken);
            
            await Assert.That(done.WaitOne(TimeSpan.FromSeconds(1))).IsTrue().Because("instance not found");
        }
        finally
        {
            mdns.Stop();
        }
    }

    [Test]
    public async Task Discover_ServiceInstance_WithAnswersContainingAdditionRecords()
    {
        var service = new ServiceProfile("y", "_sdtest-2._udp", 1024, [IPAddress.Parse("127.1.1.1")]);
        var done = new ManualResetEvent(false);

        using var mdns = new MulticastService();
        using var sd = await ServiceDiscovery.CreateInstance(mdns, cancellationToken: TestContext.Current!.Execution.CancellationToken);
        sd.AnswersContainsAdditionalRecords = true;
        
        Message discovered = null;

        mdns.NetworkInterfaceDiscovered += async _ =>
        {
            await sd.QueryServiceInstances(service.ServiceName!);
        };

        sd.ServiceInstanceDiscovered += async e =>
        {
            if (e.ServiceInstanceName == service.FullyQualifiedName)
            {
                await Assert.That(e.Message).IsNotNull();
                discovered = e.Message;
                done.Set();
            }
        };

        sd.Advertise(service);

        await mdns.Start(TestContext.Current!.Execution.CancellationToken);

        await Assert.That(done.WaitOne(TimeSpan.FromSeconds(3))).IsTrue().Because("instance not found");

        const int additionalRecordsCount = 1 + // SRVRecord
                                           1 + // TXTRecord
                                           1; // AddressRecord

        const int answersCount = additionalRecordsCount +
                                 1; // PTRRecord

        await Assert.That(discovered.AdditionalRecords.Count).IsEqualTo(0);
        await Assert.That(discovered.Answers.Count).IsEqualTo(answersCount);
    }

    [Test]
    public async Task Unadvertise()
    {
        var service = new ServiceProfile("z", "_sdtest-7._udp", 1024);
        var done = new ManualResetEvent(false);
        using var mdns = new MulticastService();
        using var sd = await ServiceDiscovery.CreateInstance(mdns, cancellationToken: TestContext.Current!.Execution.CancellationToken);

        mdns.NetworkInterfaceDiscovered += _ => sd.QueryAllServices();
        sd.ServiceInstanceShutdown += e =>
        {
            if (e.ServiceInstanceName == service.FullyQualifiedName)
                done.Set();
            
            return Task.CompletedTask;
        };
        
        try
        {
            sd.Advertise(service);
            await mdns.Start(TestContext.Current!.Execution.CancellationToken);
            await sd.Unadvertise(service);
            await Assert.That(done.WaitOne(TimeSpan.FromSeconds(1))).IsTrue().Because("goodbye timeout");
        }
        finally
        {
            mdns.Stop();
        }
    }
    
    [Test]
    public async Task ReverseAddressMapping()
    {
        var service = new ServiceProfile("x9", "_sdtest-1._udp", 1024, [IPAddress.Loopback, IPAddress.IPv6Loopback]);
        var arpaAddress = IPAddress.Loopback.GetArpaName();
        var done = new ManualResetEvent(false);

        using var mdns = new MulticastService();
        Message response = null;
        mdns.NetworkInterfaceDiscovered += _ => mdns.SendQuery(arpaAddress, DnsClass.IN, DnsType.PTR);
        mdns.AnswerReceived += e =>
        {
            var msg = e.Message;
            if (msg.Answers.OfType<PTRRecord>().Any(p => p.Name == arpaAddress))
            {
                response = msg;
                done.Set();
            }
            
            return Task.CompletedTask;
        };
        
        try
        {
            using var sd = await ServiceDiscovery.CreateInstance(mdns, cancellationToken: TestContext.Current!.Execution.CancellationToken);
            sd.Advertise(service);
            await mdns.Start(TestContext.Current!.Execution.CancellationToken);
            
            await Assert.That(done.WaitOne(TimeSpan.FromSeconds(1))).IsTrue().Because("query timeout");

            var answers = response.Answers
                .OfType<PTRRecord>()
                .Where(ptr => service.HostName == ptr.DomainName);

            foreach (var answer in answers)
            {
                await Assert.That(answer.Name).IsEquatableOrEqualTo(arpaAddress);
                await Assert.That(answer.TTL).IsGreaterThan(TimeSpan.Zero);
                await Assert.That(answer.Class).IsEqualTo(DnsClass.IN);
            }
        }
        finally
        {
            mdns.Stop();
        }
    }

    [Test]
    public async Task ResourceRecords()
    {
        var profile = new ServiceProfile("me", "_myservice._udp", 1234, [IPAddress.Loopback]);
        profile.Subtypes.Add("apiv2");
        profile.AddProperty("someprop", "somevalue");

        using var sd = await ServiceDiscovery.CreateInstance(cancellationToken: TestContext.Current!.Execution.CancellationToken);
        sd.Advertise(profile);

        await Assert.That(sd.NameServer.Catalog!).IsNotNull();

        var resourceRecords = sd.NameServer.Catalog.Values.SelectMany(static node => node.Resources);
        foreach (var r in resourceRecords)
            Console.WriteLine(r.ToString());
    }

    [Test]
    public async Task Announce_ContainsSharedRecords()
    {
        var service = new ServiceProfile("z", "_sdtest-4._udp", 1024, [IPAddress.Loopback]);
        var done = new ManualResetEvent(false);

        using var mdns = new MulticastService();
        mdns.AnswerReceived += e =>
        {
            var msg = e.Message;
            if (msg.Answers.OfType<PTRRecord>().Any(p => p.DomainName == service.FullyQualifiedName))
                done.Set();
            
            return Task.CompletedTask;
        };
        
        try
        {
            using var sd = await ServiceDiscovery.CreateInstance(mdns, cancellationToken: TestContext.Current!.Execution.CancellationToken);
            mdns.NetworkInterfaceDiscovered += async _ =>
            {
                await Assert.That(await sd.Probe(service)).IsFalse();
                await sd.Announce(service);
            };
            
            await mdns.Start(TestContext.Current!.Execution.CancellationToken);

            await Assert.That(done.WaitOne(TimeSpan.FromSeconds(3))).IsTrue().Because("announce timeout");
        }
        finally
        {
            mdns.Stop();
        }
    }

    [Test]
    public async Task Announce_ContainsResourceRecords()
    {
        var service = new ServiceProfile("z", "_sdtest-4._udp", 1024, [IPAddress.Loopback]);
        var done = new ManualResetEvent(false);

        using var mdns = new MulticastService();
        mdns.AnswerReceived += e =>
        {
            var msg = e.Message;
            //Remove Cache-Flush bit
            foreach (var answer in e.Message.Answers)
                answer.Class = (DnsClass)((ushort)answer.Class & ~MulticastService.CacheFlushBit);
            
            if (service.Resources.Any(r => !msg.Answers.Contains(r)))
            {
                return Task.CompletedTask;
            }
            
            done.Set();
            
            return Task.CompletedTask;
        };
        
        try
        {
            using var sd = await ServiceDiscovery.CreateInstance(mdns, cancellationToken: TestContext.Current!.Execution.CancellationToken);
            mdns.NetworkInterfaceDiscovered += async _ =>
                {
                    await Assert.That(await sd.Probe(service)).IsFalse();
                    await sd.Announce(service);
                };
            
            await mdns.Start(TestContext.Current!.Execution.CancellationToken);

            await Assert.That(done.WaitOne(TimeSpan.FromSeconds(3))).IsTrue().Because("announce timeout");
        }
        finally
        {
            mdns.Stop();
        }
    }

    [Test]
    public async Task Announce_SentThrice()
    {
        var service = new ServiceProfile("z", "_sdtest-4._udp", 1024, [IPAddress.Loopback]);
        var done = new ManualResetEvent(false);
        var nanswers = 0;
        var stopWatch = new Stopwatch();
        
        using var mdns = new MulticastService
        {
            IgnoreDuplicateMessages = false
        };
        
        mdns.AnswerReceived += e =>
        {
            var msg = e.Message;
            if (msg.Answers.OfType<PTRRecord>().Any(p => p.DomainName == service.FullyQualifiedName) && ++nanswers == 3)
                done.Set();
            
            return Task.CompletedTask;
        };
        
        try
        {
            using var sd = await ServiceDiscovery.CreateInstance(mdns, cancellationToken: TestContext.Current!.Execution.CancellationToken);
            mdns.NetworkInterfaceDiscovered += async _ =>
            {
                await Assert.That(await sd.Probe(service)).IsFalse();
                stopWatch.Start();
                await sd.Announce(service, 3);
            };

            await mdns.Start(TestContext.Current!.Execution.CancellationToken);
            
            await Assert.That(done.WaitOne(TimeSpan.FromSeconds(4))).IsTrue().Because("announce timeout");
            stopWatch.Stop();
            if (stopWatch.ElapsedMilliseconds < 3000)
                Assert.Fail("Announcing too fast");
        }
        finally
        {
            mdns.Stop();
        }
    }
}