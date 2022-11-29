namespace TspCoordinator.Data.TspApi.V2;

public class Pattern 
{
    public int Id { get; set; }
    public string SourceCode { get; set; }
    public Dictionary<String, String>? Payload { get; set; }
    public int? Subunit { get; set; }
    public List<String>? ForwardedFields { get; set; }
}