using TspCoordinator.Data.TspApi.V3;

namespace TspCoordinator.Data.Grammars;

public class CheckRequest
{
    public Dictionary<string, string> ColumnTypes { get; set; } = [];
    public List<Pattern> Patterns { get; set; } = [];
}

public class IssueInfo(int patternId, string data)
{
    public int PatternId { get; set; } = patternId;
    public string Data { get; set; } = data;
}

public class CheckResponse
{
    public List<IssueInfo> Errors { get; set; } = [];
    public List<IssueInfo> Warnings { get; set; } = [];
}