namespace TspCoordinator.Data.TspApi.V3;

public class SourceWithType
{
    public string Type { get; set; }
    public IInputConf Config { get; set; }
}

public class SinkWithType
{
    public string Type { get; set; }
    public IOutputConf Config { get; set; }
}

public class Request
{
    public string Uuid { get; set; } = System.Guid.NewGuid().ToString();
    public SourceWithType Source { get; set; }
    public List<SinkWithType> Sinks { get; set; }

    public List<Pattern> Patterns { get; set; }

    public int Priority { get; set; }
}