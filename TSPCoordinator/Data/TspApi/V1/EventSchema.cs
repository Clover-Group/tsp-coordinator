namespace TspCoordinator.Data.TspApi.V1;

public class EventSchema
{
    public string? UnitIdField { get; set; } 
    public string FromTsField { get; set; } = default! ;
    public string ToTsField { get; set; } = default! ;
    public (string, int) AppIdFieldVal { get; set; }
    public string PatternIdField { get; set; } = default! ;
    public string SubunitIdField { get; set; } = default! ;
}