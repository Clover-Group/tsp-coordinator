namespace TspCoordinator.Data.TspApi.V2;

public class Request
{
    public string Uuid { get; set; } = System.Guid.NewGuid().ToString();
    public IInputConf Source { get; set; } = default! ;
    public IOutputConf Sink { get; set; } = default! ;

    public List<Pattern> Patterns { get; set; } = default! ;

    public int Priority { get; set; }
}