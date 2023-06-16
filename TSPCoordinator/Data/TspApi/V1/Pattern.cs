namespace TspCoordinator.Data.TspApi.V1;

public class Pattern
{
    public string Id { get; set; } = default! ;
    public string SourceCode { get; set; } = default! ;
    public Dictionary<String, String>? Payload { get; set; }
    public List<String>? ForwardedFields { get; set; }
}