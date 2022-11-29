using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TspCoordinator.Data.TspApi.V3;

public class OutputConfConverter : JsonConverter<IOutputConf>
{
    public override IOutputConf? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        try
        {
            var res = JsonSerializer.Deserialize<V3.JdbcOutputConf>(ref reader, options);
            if (res?.JdbcUrl == null) throw new ArgumentNullException();
            return res;
        }
        catch
        {
            try
            {
                return JsonSerializer.Deserialize<V3.KafkaOutputConf>(ref reader, options);
            }
            catch
            {
                return null;
            }
        }
    }

    public override void Write(Utf8JsonWriter writer, IOutputConf value, JsonSerializerOptions options)
    {
        switch (value)
        {
            case V3.JdbcOutputConf v:
                JsonSerializer.Serialize(writer, v, options);
                break;
            case V3.KafkaOutputConf v:
                JsonSerializer.Serialize(writer, v, options);
                break;
        };
    }
}