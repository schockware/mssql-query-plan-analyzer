using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using Encased.Contracts;

namespace QueryPlan.Contracts;

public class QueryText : Uuid7Entity
{
    public QueryText() { }

    public QueryText(Server server, PlanSource source, byte[] compressedText)
    {
        Server = server;
        PlanSource = source;
        var text = Decompress(compressedText);
        Text = text;
        Id = ComputeHash(text);
    }

    public string Text { get; set; } = string.Empty;
    public Server Server { get; set; }
    public PlanSource PlanSource { get; set; }
    public Guid TextHash => Id;

    private static Guid ComputeHash(string text)
    {
        var bytes = MD5.HashData(Encoding.UTF8.GetBytes(text));
        return new Guid(bytes);
    }

    private static string Decompress(byte[] compressed)
    {
        try
        {
            using var input = new MemoryStream(compressed);
            using var gzip = new GZipStream(input, CompressionMode.Decompress);
            using var output = new MemoryStream();
            gzip.CopyTo(output);
            var bytes = output.ToArray();
            if (bytes.Length >= 2 && bytes[0] == 0xFF && bytes[1] == 0xFE)
                return Encoding.Unicode.GetString(bytes, 2, bytes.Length - 2);
            return Encoding.Unicode.GetString(bytes);
        }
        catch { return string.Empty; }
    }
}
