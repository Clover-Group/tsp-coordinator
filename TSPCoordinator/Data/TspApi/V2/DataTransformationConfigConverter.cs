using System.Text.Json;
using System.Text.Json.Serialization;

namespace TspCoordinator.Data.TspApi.V2;

public class DataTransformationConfigConverter : JsonConverter<ISourceDataTransformationConf>
{
    public override ISourceDataTransformationConf? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        try
        {
            //var readerCopy = reader;
            var res = JsonSerializer.Deserialize<V2.NarrowDataUnfoldingConf>(ref reader, options);
            //reader = readerCopy;
            return res;
        }
        catch
        {
            try
            {
                //var readerCopy = reader;
                var res = JsonSerializer.Deserialize<V2.WideDataFillingConf>(ref reader, options);
                //reader = readerCopy;
                return res;
            }
            catch (Exception e)
            {
                throw new JsonException(e.Message);
            }
        }
    }

    public override void Write(Utf8JsonWriter writer, ISourceDataTransformationConf value, JsonSerializerOptions options)
    {
        switch (value)
        {
            case V2.NarrowDataUnfoldingConf v:
                JsonSerializer.Serialize(writer, v, options);
                break;
            case V2.WideDataFillingConf v:
                JsonSerializer.Serialize(writer, v, options);
                break;
        };
    }
}