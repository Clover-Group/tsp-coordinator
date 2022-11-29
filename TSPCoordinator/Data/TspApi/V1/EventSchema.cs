namespace TspCoordinator.Data.TspApi.V1;

public class EventSchema
{
  public string? SourceIdField { get; set; }
  public string FromTsField { get; set; }
  public string ToTsField { get; set; }
  public (string, int) AppIdFieldVal { get; set; }
  public string PatternIdField { get; set; }
  public string? ProcessingTsField { get; set; }
  public string? ContextField { get; set; }
  public List<string> ForwardedFields { get; set; } = new List<string>();
}