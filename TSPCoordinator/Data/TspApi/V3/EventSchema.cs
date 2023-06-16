namespace TspCoordinator.Data.TspApi.V3;

public interface IEventSchemaValue
{
    public string Type { get; }
}

public class IntegerEventSchemaValue : IEventSchemaValue
{
    public string Type { get; set; } = "int64";

    public long Value { get; set; }
}

public class FloatEventSchemaValue : IEventSchemaValue
{
    public string Type { get; set; } = "float64";

    public double Value { get; set; }
}

public class StringEventSchemaValue : IEventSchemaValue
{
    public string Type { get; set; } = "string";

    public string Value { get; set; } = "";
}

public class ObjectEventSchemaValue : IEventSchemaValue
{
    public string Type { get; set; } = "object";

    public Dictionary<String, IEventSchemaValue> Value { get; set; } = new Dictionary<string, IEventSchemaValue>();
}

public class EventSchema
{
    public Dictionary<String, IEventSchemaValue> Data { get; set; } = new Dictionary<string, IEventSchemaValue>();
}