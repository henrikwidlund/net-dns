using System.Text;

namespace Makaretu.Dns;

/// <summary>
///   Text strings.
/// </summary>
/// <remarks>
///   TXT RRs are used to hold descriptive text.  The semantics of the text
///   depends on the domain where it is found.
/// </remarks>
public class TXTRecord : ResourceRecord
{
    /// <summary>
    ///   Creates a new instance of the <see cref="TXTRecord"/> class.
    /// </summary>
    public TXTRecord() => Type = DnsType.TXT;

    /// <summary>
    ///  The sequence of strings.
    /// </summary>
    public List<string> Strings { get; set; } = [];

    /// <inheritdoc />
    public override void ReadData(WireReader reader, in int length)
    {
        var localLength = length;
        while (localLength > 0)
        {
            var s = reader.ReadString();
            Strings.Add(s);
            localLength -= Encoding.UTF8.GetByteCount(s) + 1;
        }
    }

    /// <inheritdoc />
    public override void ReadData(PresentationReader reader)
    {
        while (!reader.IsEndOfLine())
            Strings.Add(reader.ReadString());
    }
    
    /// <inheritdoc />
    public override void WriteData(WireWriter writer)
    {
        foreach (var s in Strings)
            writer.WriteString(s);
    }

    /// <inheritdoc />
    public override void WriteData(PresentationWriter writer)
    {
        var next = false;
        foreach (var s in Strings)
        {
            if (next)
                writer.WriteSpace();
            
            writer.WriteString(s, appendSpace: false);
            next = true;
        }
    }
}