namespace TspCoordinator.Data.TspApi.V1;

public class Pattern
{
    public string Id { get; set; }
    public string SourceCode { get; set; }
    public Dictionary<String, String>? Payload { get; set; }
    public List<String>? ForwardedFields { get; set; }
}