namespace TspCoordinator.Data.TspApi.V2;

public class Context
{
    public string Field { get; set; }
    public Dictionary<String, String> Data { get; set; }
}

public class EventSchema
{
    public string UnitIdField { get; set; }
    public string FromTsField { get; set; }
    public string ToTsField { get; set; }
    public (string, int) AppIdFieldVal { get; set; }
    public string PatternIdField { get; set; }
    public string SubunitIdField { get; set; }
    public string IncidentIdField { get; set; }
    public Context? Context { get; set; }
}