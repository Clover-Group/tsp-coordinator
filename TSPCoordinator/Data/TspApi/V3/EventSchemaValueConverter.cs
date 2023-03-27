using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TspCoordinator.Data.TspApi.V3;

public class EventSchemaValueConverter : JsonConverter<V3.IEventSchemaValue>
{
    public override V3.IEventSchemaValue? Read(
      ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        try
        {
            return JsonSerializer.Deserialize<V3.IntegerEventSchemaValue>(ref reader, options);
        }
        catch
        {
            try
            {
                return JsonSerializer.Deserialize<V3.FloatEventSchemaValue>(ref reader, options);
            }
            catch
            {
                try
                {
                    return JsonSerializer.Deserialize<V3.StringEventSchemaValue>(ref reader, options);
                }
                catch
                {
                    try
                    {
                        return JsonSerializer.Deserialize<V3.ObjectEventSchemaValue>(ref reader, options);
                    }
                    catch
                    {
                        return null;
                    }
                }
            }
        }
    }

    public override void Write(
      Utf8JsonWriter writer, V3.IEventSchemaValue value, JsonSerializerOptions options)
    {
        switch (value)
        {
            case V3.IntegerEventSchemaValue v:
                JsonSerializer.Serialize(writer, v, options);
                break;
            case V3.FloatEventSchemaValue v:
                JsonSerializer.Serialize(writer, v, options);
                break;
            case V3.StringEventSchemaValue v:
                JsonSerializer.Serialize(writer, v, options);
                break;
            case V3.ObjectEventSchemaValue v:
                JsonSerializer.Serialize(writer, v, options);
                break;
        };
    }
    //JsonSerializer.Serialize(writer, (TImplementation)value!, options);
}