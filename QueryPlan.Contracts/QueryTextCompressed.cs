using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using Encased.Contracts;

namespace QueryPlan.Contracts;

public class QueryTextCompressed : Uuid7Entity
{
    public QueryTextCompressed() { }

    public QueryTextCompressed(Server server, PlanSource source, byte[] compressedText)
    {
        Server = server;
        PlanSource = source;
        CompressedText = compressedText;
        Id = ComputeHash(compressedText);
    }

    public byte[] CompressedText { get; set; } = [];
    public Server Server { get; set; }
    public PlanSource PlanSource { get; set; }
    public Guid TextHash => Id;

    private static Guid ComputeHash(byte[] data)
    {
        var bytes = MD5.HashData(data);
        return new Guid(bytes);
    }
}
