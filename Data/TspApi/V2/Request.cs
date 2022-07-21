namespace TspCoordinator.Data.TspApi.V2;

public class Request
{
    public string Uuid { get; set; }
    public IInputConf Source { get; set; }
    public IOutputConf Sink { get; set; }

    public List<Pattern> Patterns { get; set; }

    public int Priority { get; set; }
}