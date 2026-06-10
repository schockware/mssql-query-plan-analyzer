using Microsoft.Extensions.Options;
using QueryPlan.Contracts;

namespace Encased.Access.System;

public class FolderAccess(IOptions<FolderAccess.Config> options)
{
    private readonly string[] _allowedExtensions = [".xml", ".sqlplan"];
    public IEnumerable<string> GetFileNames()
    {
        return Directory.GetFiles(options.Value.Root)
                        .Where(f => _allowedExtensions.Contains(Path.GetExtension(f)));
    }
    public QueryPlanRecord Get(string fileName)
    {
        var xml = File.ReadAllText(fileName);
        return new QueryPlanRecord()
        {
            Plan = xml,
        };
    }

    public class Config
    {
        public string Root { get; set; } = string.Empty;
    }
}