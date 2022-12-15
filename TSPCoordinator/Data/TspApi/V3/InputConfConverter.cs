using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TspCoordinator.Data.TspApi.V3;

public class InputConfConverter : JsonConverter<IInputConf>
{
    public override IInputConf? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        try
        {
            var res = JsonSerializer.Deserialize<V3.JdbcInputConf>(ref reader, options);
            if (res?.JdbcUrl == null) throw new ArgumentNullException();
            return res;
        }
        catch
        {
            try
            {
                return JsonSerializer.Deserialize<V3.KafkaInputConf>(ref reader, options);
            }
            catch (Exception e)
            {
                throw new JsonException(e.Message);
            }
        }
    }

    public override void Write(Utf8JsonWriter writer, IInputConf value, JsonSerializerOptions options)
    {
        switch (value)
        {
            case V3.JdbcInputConf v:
                JsonSerializer.Serialize(writer, v, options);
                break;
            case V3.KafkaInputConf v:
                JsonSerializer.Serialize(writer, v, options);
                break;
        };
    }
}