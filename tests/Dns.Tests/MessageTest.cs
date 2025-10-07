using System;
using System.Linq;
using System.Net;
using Makaretu.Dns;
using Shouldly;
using Xunit;

namespace DnsTests;

public class MessageTest
{
    /// <summary>
    ///   From https://en.wikipedia.org/wiki/Multicast_DNS
    /// </summary>
    [Fact]
    public void DecodeQuery()
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

        msg.Id.ShouldBe((ushort)0);
        msg.Questions.Count.ShouldBe(1);
        msg.Answers.Count.ShouldBe(0);
        msg.AuthorityRecords.Count.ShouldBe(0);
        msg.AdditionalRecords.Count.ShouldBe(0);

        var question = msg.Questions[0];
        question.Name.ShouldBe("appletv.local");
        question.Type.ShouldBe(DnsType.A);
        question.Class.ShouldBe(DnsClass.IN);
    }

    /// <summary>
    ///   From https://en.wikipedia.org/wiki/Multicast_DNS
    /// </summary>
    [Fact]
    public void DecodeResponse()
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

        msg.IsResponse.ShouldBeTrue();
        msg.AA.ShouldBeTrue();
        msg.Questions.Count.ShouldBe(0);
        msg.Answers.Count.ShouldBe(1);
        msg.AuthorityRecords.Count.ShouldBe(0);
        msg.AdditionalRecords.Count.ShouldBe(2);

        msg.Answers[0].Name.ShouldBe("appletv.local");
        msg.Answers[0].Type.ShouldBe(DnsType.A);
        ((ushort)msg.Answers[0].Class).ShouldBe((ushort)0x8001);
        msg.Answers[0].TTL.ShouldBe(TimeSpan.FromSeconds(30720));
        msg.Answers[0].ShouldBeOfType<ARecord>();
        ((ARecord)msg.Answers[0]).Address.ShouldBe(IPAddress.Parse("153.109.7.90"));

        var aaaa = (AAAARecord)msg.AdditionalRecords[0];
        aaaa.Name.ShouldBe("appletv.local");
        aaaa.Type.ShouldBe(DnsType.AAAA);
        ((ushort)aaaa.Class).ShouldBe((ushort)0x8001);
        aaaa.TTL.ShouldBe(TimeSpan.FromSeconds(30720));
        aaaa.Address.ShouldBe(IPAddress.Parse("fe80::223:32ff:feb1:2152"));

        var nsec = (NSECRecord)msg.AdditionalRecords[1];
        nsec.Name.ShouldBe("appletv.local");
        nsec.Type.ShouldBe(DnsType.NSEC);
        ((ushort)nsec.Class).ShouldBe((ushort)0x8001);
        nsec.TTL.ShouldBe(TimeSpan.FromSeconds(30720));
        nsec.NextOwnerName.ShouldBe("appletv.local");
    }

    [Fact]
    public void Flags()
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
        
        expected.QR.ShouldBe(actual.QR);
        expected.Opcode.ShouldBe(actual.Opcode);
        expected.AA.ShouldBe(actual.AA);
        expected.TC.ShouldBe(actual.TC);
        expected.RD.ShouldBe(actual.RD);
        expected.RA.ShouldBe(actual.RA);
        expected.Z.ShouldBe(actual.Z);
        expected.AD.ShouldBe(actual.AD);
        expected.CD.ShouldBe(actual.CD);
        expected.Status.ShouldBe(actual.Status);
    }

    [Fact]
    public void Response()
    {
        var query = new Message { Id = 1234, Opcode = MessageOperation.InverseQuery };
        query.Questions.Add(new Question { Name = "foo.org", Type = DnsType.A });
        var response = query.CreateResponse();
        
        response.IsResponse.ShouldBeTrue();
        response.Id.ShouldBe(query.Id);
        response.Opcode.ShouldBe(query.Opcode);
        response.Questions.Count.ShouldBe(1);
        response.Questions[0].ShouldBe(query.Questions[0]);
    }

    [Fact]
    public void Roundtrip()
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
        
        actual.AA.ShouldBe(expected.AA);
        actual.Id.ShouldBe(expected.Id);
        actual.IsQuery.ShouldBe(expected.IsQuery);
        actual.IsResponse.ShouldBe(expected.IsResponse);
        actual.Questions.Count.ShouldBe(1);
        actual.Answers.Count.ShouldBe(1);
        actual.AuthorityRecords.Count.ShouldBe(1);
        actual.AdditionalRecords.Count.ShouldBe(1);
    }

    [Fact]
    public void ExtendedOpcode()
    {
        var expected = new Message { Opcode = (MessageOperation)0xfff };
        expected.Opcode.ShouldBe((MessageOperation)0xfff);
        expected.AdditionalRecords.OfType<OPTRecord>().Count().ShouldBe(1);

        var actual = (Message)new Message().Read(expected.ToByteArray());
        actual.Opcode.ShouldBe(expected.Opcode);
    }

    [Fact]
    public void Issue_11()
    {
        var bytes = Convert.FromBase64String("EjSBgAABAAEAAAAABGlwZnMCaW8AABAAAcAMABAAAQAAADwAPTxkbnNsaW5rPS9pcGZzL1FtWU5RSm9LR05IVHBQeENCUGg5S2tEcGFFeGdkMmR1TWEzYUY2eXRNcEhkYW8=");
        new Message().Read(bytes).ShouldBeOfType<Message>();
    }

    [Fact]
    public void Issue_12()
    {
        var bytes = Convert.FromBase64String("AASBgAABAAQAAAABA3d3dwxvcGluaW9uc3RhZ2UDY29tAAABAAHADAAFAAEAAAA8AALAEMAQAAEAAQAAADwABCLAkCrANAABAAEAAAA8AAQ0NgUNwDQAAQABAAAAPAAEaxUAqgAAKQYAAAAAAAFlAAwBYQAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        new Message().Read(bytes).ShouldBeOfType<Message>();
    }

    [Fact]
    public void Truncation_NotRequired()
    {
        var msg = new Message();
        var originalLength = msg.Length();
        msg.Truncate(int.MaxValue);
        
        originalLength.ShouldBe(msg.Length());
        msg.TC.ShouldBeFalse();
    }

    [Fact]
    public void Truncation_Fails()
    {
        var msg = new Message();
        var originalLength = msg.Length();
        msg.Truncate(originalLength - 1);
        
        originalLength.ShouldBe(msg.Length());
        msg.TC.ShouldBeTrue();
    }

    [Fact]
    public void Truncation_AdditionalRecords()
    {
        var msg = new Message();
        msg.AdditionalRecords.Add(AddressRecord.Create("foo", IPAddress.Loopback));
        msg.AuthorityRecords.Add(AddressRecord.Create("foo", IPAddress.Loopback));
        var originalLength = msg.Length();
        msg.AdditionalRecords.Add(AddressRecord.Create("foo", IPAddress.Loopback));
        msg.AdditionalRecords.Add(AddressRecord.Create("foo", IPAddress.Loopback));
        msg.AdditionalRecords.Add(AddressRecord.Create("foo", IPAddress.Loopback));

        msg.Truncate(originalLength);
        
        originalLength.ShouldBe(msg.Length());
        msg.AdditionalRecords.Count.ShouldBe(1);
        msg.AuthorityRecords.Count.ShouldBe(1);
        msg.TC.ShouldBeFalse();
    }

    [Fact]
    public void AuthorityRecords()
    {
        var msg = new Message();
        msg.AuthorityRecords.Add(AddressRecord.Create("foo", IPAddress.Loopback));
        var originalLength = msg.Length();
        msg.AuthorityRecords.Add(AddressRecord.Create("foo", IPAddress.Loopback));
        msg.AdditionalRecords.Add(AddressRecord.Create("foo", IPAddress.Loopback));
        msg.AdditionalRecords.Add(AddressRecord.Create("foo", IPAddress.Loopback));

        msg.Truncate(originalLength);
        
        originalLength.ShouldBe(msg.Length());
        msg.AdditionalRecords.Count.ShouldBe(0);
        msg.AuthorityRecords.Count.ShouldBe(1);
        msg.TC.ShouldBeFalse();
    }

    [Fact]
    public void UseDnsSecurity()
    {
        var expected = new Message().UseDnsSecurity();
        var opt = expected.AdditionalRecords.OfType<OPTRecord>().Single();
        
        opt.DO.ShouldBeTrue("dnssec ok");
    }

    [Fact]
    public void UseDnsSecurity_OPT_Exists()
    {
        var expected = new Message();
        expected.AdditionalRecords.Add(new OPTRecord());
        expected.UseDnsSecurity();
        var opt = expected.AdditionalRecords.OfType<OPTRecord>().Single();
        
        opt.DO.ShouldBeTrue("dnssec ok");
    }

    [Fact]
    public void Dnssec_Bit()
    {
        var message = new Message();
        message.DO.ShouldBeFalse();
        message.AdditionalRecords.OfType<OPTRecord>().Count().ShouldBe(0);

        message.DO = false;
        message.DO.ShouldBeFalse();
        message.AdditionalRecords.OfType<OPTRecord>().Count().ShouldBe(1);

        message.DO = true;
        message.DO.ShouldBeTrue();
        message.AdditionalRecords.OfType<OPTRecord>().Count().ShouldBe(1);
    }

    [Fact]
    public void Stringify()
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
        text.ShouldBe(expected);
    }

    [Fact]
    public void Stringify_Edns()
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
        text.ShouldBe(expected);
    }

    [Fact]
    public void AppleMessage()
    {
        // A MDNS query from an Apple Host.  It contains a UTF8 domain name
        // and an EDNS OPT-4 option.
        const string sample = "AAAAAAAGAAAAAAABCF9ob21la2l0BF90Y3AFbG9jYWwAAAyAAQ9fY29tcGFuaW9uLWxpbmvAFQAMgAEIX2FpcnBsYXnAFQAMgAEFX3Jhb3DAFQAMgAEbQ2hyaXN0b3BoZXLigJlzIE1hY0Jvb2sgUHJvwCUAEIABDF9zbGVlcC1wcm94eQRfdWRwwBoADAABAAApBaAAABGUABIABAAOAJB6e4qbc5l4e4qbc5k=";
        var buffer1 = Convert.FromBase64String(sample);
        var m = new Message();
        m.Read(buffer1);

        m.Questions[4].Name.ShouldNotBeNull();
        m.Questions[4].Name.Labels[0].ShouldBe("Christopher’s MacBook Pro");
        m.Questions[0].ToString().ShouldBe("_homekit._tcp.local CLASS32769 PTR");
    }
}