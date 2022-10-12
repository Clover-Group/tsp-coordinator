namespace TspCoordinator.Data.TspApi.V1;

public class Request
{
    public string Uuid { get; set; } = System.Guid.NewGuid().ToString();
    public IInputConf Source { get; set; }
    public IOutputConf Sink { get; set; }

    public List<Pattern> Patterns { get; set; }
}