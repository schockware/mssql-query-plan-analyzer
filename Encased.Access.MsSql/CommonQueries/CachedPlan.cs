namespace Encased.Access.MsSql.CommonQueries;

public class CachedPlan
{
    public static readonly string GetHandles = "SELECT DISTINCT usecounts, cacheobjtype, objtype, PlanHandle = plan_handle FROM sys.dm_exec_cached_plans";
    public static readonly string GetTextAndPlan = "SELECT \nCompressedText = COMPRESS(CAST(st.text as NVARCHAR(MAX))), CompressedPlan = COMPRESS(CAST(query_plan as NVARCHAR(MAX)))\nFROM sys.dm_exec_cached_plans AS cp\nCROSS APPLY sys.dm_exec_sql_text(cp.plan_handle) AS st\nOUTER APPLY sys.dm_exec_query_plan(cp.plan_handle) AS qps WHERE cp.plan_handle IN ({handles})";

    public static string GetTextAndPlanWithLike (string like) => @$"SELECT 
CompressedText = COMPRESS(CAST(st.text as NVARCHAR(MAX))), CompressedPlan = COMPRESS(CAST(query_plan as NVARCHAR(MAX)))
FROM sys.dm_exec_cached_plans AS cp
CROSS APPLY sys.dm_exec_sql_text(cp.plan_handle) AS st
OUTER APPLY sys.dm_exec_query_plan(cp.plan_handle) AS qps 
WHERE st.text LIKE '%{like.Replace("_", @"\_")}%' ESCAPE '\'";
    
    public class Handle
    {
        public int UseCounts { get; set; }
        public string CacheObjType { get; set; } = string.Empty;
        public string ObjType { get; set; } = string.Empty;
        public byte[] PlanHandle { get; set; } = [];
    }

    public class TextAndPlan
    {
        public byte[] CompressedText { get; set; } = [];
        public byte[] CompressedPlan { get; set; } = [];
    }
    
    public class Parameters
    {
        public required byte[] Bin64_PlanHandle1 { get; init; }
        public required byte[] Bin64_PlanHandle2 { get; init; }
        public required byte[] Bin64_PlanHandle3 { get; init; }
        public required byte[] Bin64_PlanHandle4 { get; init; }
        public required byte[] Bin64_PlanHandle5 { get; init; }
        public required byte[] Bin64_PlanHandle6 { get; init; }
        public required byte[] Bin64_PlanHandle7 { get; init; }
        public required byte[] Bin64_PlanHandle8 { get; init; }
        public required byte[] Bin64_PlanHandle9 { get; init; }
        public required byte[] Bin64_PlanHandle10 { get; init; }
    }
}