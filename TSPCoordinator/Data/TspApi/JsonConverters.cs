using System.Text.Json.Serialization;
using TupleAsJsonArray;

namespace TspCoordinator.Data.TspApi;

public static class JsonConverters
{
    public static readonly JsonConverter[] Converters = new JsonConverter[]
    {
        new TupleConverterFactory(),

        new V1.InputConfConverter(),
        new V1.OutputConfConverter(),
        new V1.DataTransformationConverter(),
        new V1.DataTransformationConfigConverter(),

        new V2.InputConfConverter(),
        new V2.OutputConfConverter(),
        new V2.DataTransformationConverter(),
        new V2.DataTransformationConfigConverter(),

        new V3.InputConfConverter(),
        new V3.OutputConfConverter(),
        new V3.DataTransformationConverter(),
        new V3.DataTransformationConfigConverter(),
        new V3.EventSchemaValueConverter()
    };
}