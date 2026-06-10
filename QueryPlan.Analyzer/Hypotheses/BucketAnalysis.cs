using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using QueryPlan.Analyzer.Analyses;
using QueryPlan.Analyzer.Analyses.StatementAnalyses.PredicateAnalyses;
using QueryPlan.Analyzer.Contracts;
using QueryPlan.Contracts;

namespace QueryPlan.Analyzer.Hypotheses;

public partial class BucketAnalysis
{
    public Bucket Analyze(IAnalysisResults manifest, Bucket bucket)
    {
        var step =
            manifest.CompletedPlanSteps.FirstOrDefault(step => step is AnalyzeParameterSniffing
                {
                    IsParameterSniffed: true
                }) as
                AnalyzeParameterSniffing;
        bucket.ParameterizedRootId = step?.RootId ?? 0;

        var rootOnlyStep = manifest.SeekPredicateAnalyses.SelectMany(a => a.CompletedSteps)
            .FirstOrDefault(s => s is OnlyHasRootIdSeekPredicate
            {
                IsFound: true
            }) as OnlyHasRootIdSeekPredicate;
        bucket.RootOnlyIndexSeek = rootOnlyStep != null;
        bucket.Tables = rootOnlyStep?.TableNames ?? [];
        return bucket;
    }

    public Bucket Analyze(QueryText text)
    {
        const string sizeDefault = "1";
        var sizeRegex = QueryBucketSizeRegex();
        var whitelistRegex = QueryBucketSizeWhitelistRegex();
        var sizeMatch = sizeRegex.Match(text.Text);
        var whitelistMatch = whitelistRegex.Match(text.Text);

        var size = whitelistMatch.Success
            ? whitelistMatch.Groups[1].Value
            : sizeMatch.Success
                ? sizeMatch.Groups[1].Value
                : "not found";

        return new Bucket(size) { Text = text.Text };
    }

    public class Bucket(string size)
    {
        public string Size { get; set; } = size;
        public int ParameterizedRootId { get; set; }
        public bool RootOnlyIndexSeek { get; set; }
        public List<string> Tables { get; set; } = [];
        public string Text { get; set; } = string.Empty;
        public string PlanXml { get; set; } = string.Empty;

        public override string ToString()
        {
            var text = RootOnlyIndexSeek ? Text : string.Empty;
            var xml = RootOnlyIndexSeek ? PlanXml : string.Empty;
            return $"{ParameterizedRootId},{Size},{RootOnlyIndexSeek},\"{string.Join(",",Tables)}\",\"{text}\"";
        }
    }

    [GeneratedRegex("query_bucket_size=(.*);")]
    private static partial Regex QueryBucketSizeRegex();

    [GeneratedRegex("/* Bucketed by BucketByQueryTagAndWhitelist to value (.*)*/")]
    private static partial Regex QueryBucketSizeWhitelistRegex();
}