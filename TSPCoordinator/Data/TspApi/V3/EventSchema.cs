namespace TspCoordinator.Data.TspApi.V3;

public interface IEventSchemaValue
{
    public string Type { get; }
}

public class IntegerEventSchemaValue : IEventSchemaValue
{
    public string Type { get; set; }

    public long Value { get; set; }
}

public class FloatEventSchemaValue : IEventSchemaValue
{
    public string Type { get; set; }

    public double Value { get; set; }
}

public class StringEventSchemaValue : IEventSchemaValue
{
    public string Type { get; set; }

    public string Value { get; set; }
}

public class ObjectEventSchemaValue : IEventSchemaValue
{
    public string Type { get; set; }

    public Dictionary<String, IEventSchemaValue> Value { get; set; }
}

public class EventSchema
{
    public Dictionary<String, IEventSchemaValue> Data { get; set; }
}