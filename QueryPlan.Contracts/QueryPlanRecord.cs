using System.IO.Compression;
using System.Text;
using System.Xml.Serialization;
using Encased.Contracts;
using QueryPlan.Contracts.ShowPlanDotNet.Showplan;

namespace QueryPlan.Contracts;

public class QueryPlanRecord : Uuid7Entity
{
    private static readonly XmlSerializer Serializer = new(typeof(ShowPlanXML));

    public QueryPlanRecord() { }

    public QueryPlanRecord(Guid sessionId, Server server, PlanSource source, byte[] compressedPlan, Guid textHash)
    {
        SampleSessionId = sessionId;
        Server = server;
        PlanSource = source;
        TextHash = textHash;
        Plan = Decompress(compressedPlan);
    }

    public string Plan { get; set; } = string.Empty;
    public Guid SampleSessionId { get; set; }
    public Server Server { get; set; }
    public PlanSource PlanSource { get; set; }
    public Guid TextHash { get; set; }

    public bool TryGetPlanXml(out ShowPlanXML showPlan)
    {
        showPlan = null!;
        if (string.IsNullOrWhiteSpace(Plan)) return false;
        try
        {
            using var reader = new StringReader(Plan);
            showPlan = (ShowPlanXML)Serializer.Deserialize(reader)!;
            return showPlan != null;
        }
        catch { return false; }
    }

    private static string Decompress(byte[] compressed)
    {
        try
        {
            using var input = new MemoryStream(compressed);
            using var gzip = new GZipStream(input, CompressionMode.Decompress);
            using var output = new MemoryStream();
            gzip.CopyTo(output);
            // SQL Server COMPRESS() wraps UTF-16 LE NVARCHAR content
            var bytes = output.ToArray();
            // Strip BOM if present
            if (bytes.Length >= 2 && bytes[0] == 0xFF && bytes[1] == 0xFE)
                return Encoding.Unicode.GetString(bytes, 2, bytes.Length - 2);
            return Encoding.Unicode.GetString(bytes);
        }
        catch
        {
            return string.Empty;
        }
    }
}
