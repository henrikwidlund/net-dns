using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Makaretu.Dns;

namespace DnsTests;

public class MessageTest
{
    /// <summary>
    ///   From https://en.wikipedia.org/wiki/Multicast_DNS
    /// </summary>
    [Test]
    public async Task DecodeQuery()
    {
        var bytes = new byte[]
        {
            0x00, 0x00,             // Transaction ID
            0x00, 0x00,             // Flags
            0x00, 0x01,             // Number of questions
            0x00, 0x00,             // Number of answers
            0x00, 0x00,             // Number of authority resource records
            0x00, 0x00,             // Number of additional resource records
            0x07, 0x61, 0x70, 0x70, 0x6c, 0x65, 0x74, 0x76, // "appletv"
            0x05, 0x6c, 0x6f, 0x63, 0x61, 0x6c, // "local"
            0x00,                   // Terminator
            0x00, 0x01,             // Type (A record)
            0x00, 0x01              // Class
        };
        
        var msg = new Message();
        msg.Read(bytes, 0, bytes.Length);

        await Assert.That(msg.Id).IsEqualTo((ushort)0);
        await Assert.That(msg.Questions).HasCount(1);
        await Assert.That(msg.Answers).HasCount().Zero();
        await Assert.That(msg.AuthorityRecords).HasCount().Zero();
        await Assert.That(msg.AdditionalRecords).HasCount().Zero();

        var question = msg.Questions[0];
        await Assert.That(question.Name).IsEquatableOrEqualTo("appletv.local");
        await Assert.That(question.Type).IsEqualTo(DnsType.A);
        await Assert.That(question.Class).IsEqualTo(DnsClass.IN);
    }

    /// <summary>
    ///   From https://en.wikipedia.org/wiki/Multicast_DNS
    /// </summary>
    [Test]
    public async Task DecodeResponse()
    {
        var bytes = new byte[]
        {
            0x00, 0x00, 0x84, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x02, 0x07, 0x61, 0x70, 0x70,
            0x6c, 0x65, 0x74, 0x76, 0x05, 0x6c, 0x6f, 0x63, 0x61, 0x6c, 0x00, 0x00, 0x01, 0x80, 0x01, 0x00,
            0x00, 0x78, 0x00, 0x00, 0x04, 0x99, 0x6d, 0x07, 0x5a, 0xc0, 0x0c, 0x00, 0x1c, 0x80, 0x01, 0x00,
            0x00, 0x78, 0x00, 0x00, 0x10, 0xfe, 0x80, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x02, 0x23, 0x32,
            0xff, 0xfe, 0xb1, 0x21, 0x52, 0xc0, 0x0c, 0x00, 0x2f, 0x80, 0x01, 0x00, 0x00, 0x78, 0x00, 0x00,
            0x08, 0xc0, 0x0c, 0x00, 0x04, 0x40, 0x00, 0x00, 0x08
        };
        var msg = new Message();
        msg.Read(bytes, 0, bytes.Length);

        await Assert.That(msg.IsResponse).IsTrue();
        await Assert.That(msg.AA).IsTrue();
        await Assert.That(msg.Questions).HasCount().Zero();
        await Assert.That(msg.Answers).HasCount(1);
        await Assert.That(msg.AuthorityRecords).HasCount().Zero();
        await Assert.That(msg.AdditionalRecords).HasCount(2);

        await Assert.That(msg.Answers[0].Name).IsEquatableOrEqualTo("appletv.local");
        await Assert.That(msg.Answers[0].Type).IsEqualTo(DnsType.A);
        await Assert.That((ushort)msg.Answers[0].Class).IsEqualTo((ushort)0x8001);
        await Assert.That(msg.Answers[0].TTL).IsEqualTo(TimeSpan.FromSeconds(30720));
        await Assert.That(msg.Answers[0]).IsTypeOf<ARecord>();
        await Assert.That(((ARecord)msg.Answers[0]).Address).IsEqualTo(IPAddress.Parse("153.109.7.90"));

        var aaaa = (AAAARecord)msg.AdditionalRecords[0];
        await Assert.That(aaaa.Name).IsEquatableOrEqualTo("appletv.local");
        await Assert.That(aaaa.Type).IsEqualTo(DnsType.AAAA);
        await Assert.That((ushort)aaaa.Class).IsEqualTo((ushort)0x8001);
        await Assert.That(aaaa.TTL).IsEqualTo(TimeSpan.FromSeconds(30720));
        await Assert.That(aaaa.Address).IsEqualTo(IPAddress.Parse("fe80::223:32ff:feb1:2152"));

        var nsec = (NSECRecord)msg.AdditionalRecords[1];
        await Assert.That(nsec.Name).IsEquatableOrEqualTo("appletv.local");
        await Assert.That(nsec.Type).IsEqualTo(DnsType.NSEC);
        await Assert.That((ushort)nsec.Class).IsEqualTo((ushort)0x8001);
        await Assert.That(nsec.TTL).IsEqualTo(TimeSpan.FromSeconds(30720));
        await Assert.That(nsec.NextOwnerName).IsEquatableOrEqualTo("appletv.local");
    }

    [Test]
    public async Task Flags()
    {
        var expected = new Message
        {
            QR = true,
            Opcode = MessageOperation.Status,
            AA = true,
            TC = true,
            RD = true,
            RA = true,
            Z = 1,
            AD = true,
            CD = true,
            Status = MessageStatus.Refused
        };
        
        var actual = new Message();
        actual.Read(expected.ToByteArray());
        
        await Assert.That(expected.QR).IsEqualTo(actual.QR);
        await Assert.That(expected.Opcode).IsEqualTo(actual.Opcode);
        await Assert.That(expected.AA).IsEqualTo(actual.AA);
        await Assert.That(expected.TC).IsEqualTo(actual.TC);
        await Assert.That(expected.RD).IsEqualTo(actual.RD);
        await Assert.That(expected.RA).IsEqualTo(actual.RA);
        await Assert.That(expected.Z).IsEqualTo(actual.Z);
        await Assert.That(expected.AD).IsEqualTo(actual.AD);
        await Assert.That(expected.CD).IsEqualTo(actual.CD);
        await Assert.That(expected.Status).IsEqualTo(actual.Status);
    }

    [Test]
    public async Task Response()
    {
        var query = new Message { Id = 1234, Opcode = MessageOperation.InverseQuery };
        query.Questions.Add(new Question { Name = "foo.org", Type = DnsType.A });
        var response = query.CreateResponse();
        
        await Assert.That(response.IsResponse).IsTrue();
        await Assert.That(response.Id).IsEqualTo(query.Id);
        await Assert.That(response.Opcode).IsEqualTo(query.Opcode);
        await Assert.That(response.Questions).HasCount(1);
        await Assert.That(response.Questions[0]).IsEqualTo(query.Questions[0]);
    }

    [Test]
    public async Task Roundtrip()
    {
        var expected = new Message
        {
            AA = true,
            QR = true,
            Id = 1234
        };
        
        expected.Questions.Add(new Question { Name = "emanon.org" });
        expected.Answers.Add(new ARecord { Name = "emanon.org", Address = IPAddress.Parse("127.0.0.1") });
        expected.AuthorityRecords.Add(new SOARecord
        {
            Name = "emanon.org",
            PrimaryName = "erehwon",
            Mailbox = "hostmaster.emanon.org"
        });
        
        expected.AdditionalRecords.Add(new ARecord { Name = "erehwon", Address = IPAddress.Parse("127.0.0.1") });
        var actual = (Message)new Message().Read(expected.ToByteArray());
        
        await Assert.That(actual.AA).IsEqualTo(expected.AA);
        await Assert.That(actual.Id).IsEqualTo(expected.Id);
        await Assert.That(actual.IsQuery).IsEqualTo(expected.IsQuery);
        await Assert.That(actual.IsResponse).IsEqualTo(expected.IsResponse);
        await Assert.That(actual.Questions).HasCount(1);
        await Assert.That(actual.Answers).HasCount(1);
        await Assert.That(actual.AuthorityRecords).HasCount(1);
        await Assert.That(actual.AdditionalRecords).HasCount(1);
    }

    [Test]
    public async Task ExtendedOpcode()
    {
        var expected = new Message { Opcode = (MessageOperation)0xfff };
        await Assert.That(expected.Opcode).IsEqualTo((MessageOperation)0xfff);
        await Assert.That(expected.AdditionalRecords.OfType<OPTRecord>()).HasCount(1);

        var actual = (Message)new Message().Read(expected.ToByteArray());
        await Assert.That(actual.Opcode).IsEqualTo(expected.Opcode);
    }

    [Test]
    public async Task Issue_11()
    {
        var bytes = Convert.FromBase64String("EjSBgAABAAEAAAAABGlwZnMCaW8AABAAAcAMABAAAQAAADwAPTxkbnNsaW5rPS9pcGZzL1FtWU5RSm9LR05IVHBQeENCUGg5S2tEcGFFeGdkMmR1TWEzYUY2eXRNcEhkYW8=");
        await Assert.That(new Message().Read(bytes)).IsTypeOf<Message>();
    }

    [Test]
    public async Task Issue_12()
    {
        var bytes = Convert.FromBase64String("AASBgAABAAQAAAABA3d3dwxvcGluaW9uc3RhZ2UDY29tAAABAAHADAAFAAEAAAA8AALAEMAQAAEAAQAAADwABCLAkCrANAABAAEAAAA8AAQ0NgUNwDQAAQABAAAAPAAEaxUAqgAAKQYAAAAAAAFlAAwBYQAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        await Assert.That(new Message().Read(bytes)).IsTypeOf<Message>();
    }

    [Test]
    public async Task Truncation_NotRequired()
    {
        var msg = new Message();
        var originalLength = msg.Length();
        msg.Truncate(int.MaxValue);
        
        await Assert.That(originalLength).IsEqualTo(msg.Length());
        await Assert.That(msg.TC).IsFalse();
    }

    [Test]
    public async Task Truncation_Fails()
    {
        var msg = new Message();
        var originalLength = msg.Length();
        msg.Truncate(originalLength - 1);
        
        await Assert.That(originalLength).IsEqualTo(msg.Length());
        await Assert.That(msg.TC).IsTrue();
    }

    [Test]
    public async Task Truncation_AdditionalRecords()
    {
        var msg = new Message();
        msg.AdditionalRecords.Add(AddressRecord.Create("foo", IPAddress.Loopback));
        msg.AuthorityRecords.Add(AddressRecord.Create("foo", IPAddress.Loopback));
        var originalLength = msg.Length();
        msg.AdditionalRecords.Add(AddressRecord.Create("foo", IPAddress.Loopback));
        msg.AdditionalRecords.Add(AddressRecord.Create("foo", IPAddress.Loopback));
        msg.AdditionalRecords.Add(AddressRecord.Create("foo", IPAddress.Loopback));

        msg.Truncate(originalLength);
        
        await Assert.That(originalLength).IsEqualTo(msg.Length());
        await Assert.That(msg.AdditionalRecords).HasCount(1);
        await Assert.That(msg.AuthorityRecords).HasCount(1);
        await Assert.That(msg.TC).IsFalse();
    }

    [Test]
    public async Task AuthorityRecords()
    {
        var msg = new Message();
        msg.AuthorityRecords.Add(AddressRecord.Create("foo", IPAddress.Loopback));
        var originalLength = msg.Length();
        msg.AuthorityRecords.Add(AddressRecord.Create("foo", IPAddress.Loopback));
        msg.AdditionalRecords.Add(AddressRecord.Create("foo", IPAddress.Loopback));
        msg.AdditionalRecords.Add(AddressRecord.Create("foo", IPAddress.Loopback));

        msg.Truncate(originalLength);
        
        await Assert.That(originalLength).IsEqualTo(msg.Length());
        await Assert.That(msg.AdditionalRecords).HasCount().Zero();
        await Assert.That(msg.AuthorityRecords).HasCount(1);
        await Assert.That(msg.TC).IsFalse();
    }

    [Test]
    public async Task UseDnsSecurity()
    {
        var expected = new Message().UseDnsSecurity();
        var opt = expected.AdditionalRecords.OfType<OPTRecord>().Single();
        
        await Assert.That(opt.DO).IsTrue();
    }

    [Test]
    public async Task UseDnsSecurity_OPT_Exists()
    {
        var expected = new Message();
        expected.AdditionalRecords.Add(new OPTRecord());
        expected.UseDnsSecurity();
        var opt = expected.AdditionalRecords.OfType<OPTRecord>().Single();
        
        await Assert.That(opt.DO).IsTrue();
    }

    [Test]
    public async Task Dnssec_Bit()
    {
        var message = new Message();
        await Assert.That(message.DO).IsFalse();
        await Assert.That(message.AdditionalRecords.OfType<OPTRecord>()).HasCount().Zero();

        message.DO = false;
        await Assert.That(message.DO).IsFalse();
        await Assert.That(message.AdditionalRecords.OfType<OPTRecord>()).HasCount(1);

        message.DO = true;
        await Assert.That(message.DO).IsTrue();
        await Assert.That(message.AdditionalRecords.OfType<OPTRecord>()).HasCount(1);
    }

    [Test]
    public async Task Stringify()
    {
        var m = new Message
        {
            AA = true,
            QR = true,
            Id = 1234
        };
        
        m.Questions.Add(new Question { Name = "emanon.org", Type = DnsType.A });
        m.Answers.Add(new ARecord { Name = "emanon.org", Address = IPAddress.Parse("127.0.0.1") });
        m.AuthorityRecords.Add(new SOARecord
        {
            Name = "emanon.org",
            PrimaryName = "erehwon",
            Mailbox = "hostmaster.emanon.org"
        });

        var text = m.ToString();
        const string expected = """
                                 ;; Header: QR AA RCODE=NoError

                                 ;; Question
                                 emanon.org IN A

                                 ;; Answer
                                 emanon.org IN A 127.0.0.1

                                 ;; Authority
                                 emanon.org 0 IN SOA erehwon hostmaster.emanon.org 0 0 0 0 0

                                 ;; Additional
                                 ;;  (empty)

                                 """;
        await Assert.That(text).IsEqualTo(expected);
    }

    [Test]
    public async Task Stringify_Edns()
    {
        const string sample = "AH6FDwEAAAEAAAAAAAEEaXBmcwJpbwAAEAABAAApBQAAAAAAAFoACwAC1MAADABQ8bbi5IwN3llzr84N11j2dG7+7lE5aBzanfc1yvO3LcgvS0TuT3Xvz6yVWcVBa8YnFwehfSyT6YiaCEaV2BNlvIIG3YwUCCX4Dh6kpA9WmDI=";
        var buffer1 = Convert.FromBase64String(sample);
        var buffer2 = new byte[buffer1.Length - 2];
        Array.Copy(buffer1, 2, buffer2, 0, buffer2.Length);
        var m = new Message();
        m.Read(buffer2);

        var text = m.ToString();
        const string expected = """
                                 ;; Header: RD RCODE=NoError

                                 ;; Question
                                 ipfs.io IN TXT

                                 ;; Answer
                                 ;;  (empty)

                                 ;; Authority
                                 ;;  (empty)

                                 ;; Additional
                                 ; EDNS: version: 0, udp 1280
                                 ;   Keepalive = 01:30:46.4000000
                                 ;   Padding = 80


                                 """;
        await Assert.That(text).IsEqualTo(expected);
    }

    [Test]
    public async Task AppleMessage()
    {
        // A MDNS query from an Apple Host.  It contains a UTF8 domain name
        // and an EDNS OPT-4 option.
        const string sample = "AAAAAAAGAAAAAAABCF9ob21la2l0BF90Y3AFbG9jYWwAAAyAAQ9fY29tcGFuaW9uLWxpbmvAFQAMgAEIX2FpcnBsYXnAFQAMgAEFX3Jhb3DAFQAMgAEbQ2hyaXN0b3BoZXLigJlzIE1hY0Jvb2sgUHJvwCUAEIABDF9zbGVlcC1wcm94eQRfdWRwwBoADAABAAApBaAAABGUABIABAAOAJB6e4qbc5l4e4qbc5k=";
        var buffer1 = Convert.FromBase64String(sample);
        var m = new Message();
        m.Read(buffer1);

        await Assert.That(m.Questions[4].Name).IsNotNull();
        await Assert.That(m.Questions[4].Name!.Labels[0]).IsEqualTo("Christopher’s MacBook Pro");
        await Assert.That(m.Questions[0].ToString()).IsEqualTo("_homekit._tcp.local CLASS32769 PTR");
    }
}