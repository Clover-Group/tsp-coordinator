namespace TspCoordinator.Data.TspApi.V2;

public class Context
{
    public string Field { get; set; } = default! ;
    public Dictionary<String, String> Data { get; set; } = default! ;
}

public class EventSchema
{
    public string UnitIdField { get; set; } = default! ;
    public string FromTsField { get; set; } = default! ;
    public string ToTsField { get; set; } = default! ;
    public (string, int) AppIdFieldVal { get; set; }
    public string PatternIdField { get; set; } = default! ;
    public string SubunitIdField { get; set; } = default! ;
    public string IncidentIdField { get; set; } = default! ;
    public Context? Context { get; set; }
}