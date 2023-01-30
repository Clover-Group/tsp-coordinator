using System.Text.Json;
using System.Text.Json.Serialization;

namespace TspCoordinator.Data.TspApi.V2;

public class DataTransformationConverter : JsonConverter<ISourceDataTransformation>
{
    public override ISourceDataTransformation? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        try
        {
            var res = JsonSerializer.Deserialize<V2.NarrowDataUnfolding>(ref reader, options);
            if (res?.Type != "NarrowDataUnfolding") throw new ArgumentNullException();
            return res;
        }
        catch
        {
            try
            {
                var res = JsonSerializer.Deserialize<V2.WideDataFilling>(ref reader, options);
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
            case V2.NarrowDataUnfolding v:
                JsonSerializer.Serialize(writer, v, options);
                break;
            case V2.WideDataFilling v:
                JsonSerializer.Serialize(writer, v, options);
                break;
            case null:
                JsonSerializer.Serialize(writer, value, options);
                break;
        };
    }
}