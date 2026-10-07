namespace Makaretu.Dns;

/// <summary>
///   Contains the IPv6 address of the named resource.
/// </summary>
public class AAAARecord : IPAddressRecord
{
    /// <summary>
    ///   Creates a new instance of the <see cref="AAAARecord"/> class.
    /// </summary>
    public AAAARecord() => Type = DnsType.AAAA;
}
