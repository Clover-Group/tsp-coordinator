using System.Text.Json;
using System.Text.Json.Serialization;

namespace TspCoordinator.Data.TspApi.V3;

public class DataTransformationConverter : JsonConverter<ISourceDataTransformation>
{
    public override ISourceDataTransformation? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        try
        {
            var res = JsonSerializer.Deserialize<V3.NarrowDataUnfolding>(ref reader, options);
            if (res?.Type != "NarrowDataUnfolding") throw new ArgumentNullException();
            return res;
        }
        catch
        {
            try
            {
                var res = JsonSerializer.Deserialize<V3.WideDataFilling>(ref reader, options);
                if (res?.Type != "WideDataFilling") throw new ArgumentNullException();
                return res;
            }
            catch (Exception e)
            {
                throw new JsonException(e.Message);
            }
        }
    }

    public override void Write(Utf8JsonWriter writer, ISourceDataTransformation value, JsonSerializerOptions options)
    {
        switch (value)
        {
            case V3.NarrowDataUnfolding v:
                JsonSerializer.Serialize(writer, v, options);
                break;
            case V3.WideDataFilling v:
                JsonSerializer.Serialize(writer, v, options);
                break;
            case null:
                JsonSerializer.Serialize(writer, value, options);
                break;
        };
    }
}