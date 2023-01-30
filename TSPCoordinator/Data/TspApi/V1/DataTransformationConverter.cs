using System.Text.Json;
using System.Text.Json.Serialization;

namespace TspCoordinator.Data.TspApi.V1;

public class DataTransformationConverter : JsonConverter<ISourceDataTransformation>
{
    public override ISourceDataTransformation? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        try
        {
            //var readerCopy = reader;
            var res = JsonSerializer.Deserialize<V1.NarrowDataUnfolding>(ref reader, options);
            if (res?.Type != "NarrowDataUnfolding") throw new ArgumentNullException();
            //reader = readerCopy;
            return res;
        }
        catch
        {
            try
            {
                //var readerCopy = reader;
                var res = JsonSerializer.Deserialize<V1.WideDataFilling>(ref reader, options);
                if (res?.Type != "WideDataFilling") throw new ArgumentNullException();
                //reader = readerCopy;
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
            case V1.NarrowDataUnfolding v:
                JsonSerializer.Serialize(writer, v, options);
                break;
            case V1.WideDataFilling v:
                JsonSerializer.Serialize(writer, v, options);
                break;
            case null:
                JsonSerializer.Serialize(writer, value, options);
                break;
        };
    }
}