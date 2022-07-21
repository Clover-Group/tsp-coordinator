namespace TspCoordinator.Data.TspApi.V3;

public class Request
{
    public string Uuid { get; set; }
    public IInputConf Source { get; set; }
    public List<IOutputConf> Sinks { get; set; }

    public List<Pattern> Patterns { get; set; }

    public int Priority { get; set; }
}