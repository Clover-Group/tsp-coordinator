namespace TspCoordinator.Data.TspApi.V3;

public class SourceWithType
{
    public string Type { get; set; } = default! ;
    public IInputConf Config { get; set; } = default! ;
}

public class SinkWithType
{
    public string Type { get; set; } = default! ;
    public IOutputConf Config { get; set; } = default! ;
}

public class Request
{
    public string Uuid { get; set; } = System.Guid.NewGuid().ToString();
    public SourceWithType Source { get; set; } = default! ;
    public List<SinkWithType> Sinks { get; set; } = default! ;

    public List<Pattern> Patterns { get; set; } = default! ;

    public int Priority { get; set; }
}