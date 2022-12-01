using System.Text.Json.Serialization;
using TupleAsJsonArray;

namespace TspCoordinator.Data.TspApi;

public static class JsonConverters
{
    public static readonly JsonConverter[] Converters = new JsonConverter[]
    {
        new TupleConverterFactory(),
        //new V1.InputConfConverter(),
        //new V1.OutputConfConverter(),
        new TypeMappingConverter<V1.IInputConf, V1.JdbcInputConf>(),
        new TypeMappingConverter<V1.IInputConf, V1.KafkaInputConf>(),
        new TypeMappingConverter<V1.IOutputConf, V1.JdbcOutputConf>(),
        new TypeMappingConverter<V1.IOutputConf, V1.KafkaOutputConf>(),
        new TypeMappingConverter<V1.ISourceDataTransformation, V1.NarrowDataUnfolding>(),
        new TypeMappingConverter<V1.ISourceDataTransformation, V1.WideDataFilling>(),
        new TypeMappingConverter<V1.ISourceDataTransformationConf, V1.NarrowDataUnfoldingConf>(),
        new TypeMappingConverter<V1.ISourceDataTransformationConf, V1.WideDataFillingConf>(),

        //new V2.InputConfConverter(),
        //new V2.OutputConfConverter(),
        new TypeMappingConverter<V2.IInputConf, V2.JdbcInputConf>(),
        new TypeMappingConverter<V2.IInputConf, V2.KafkaInputConf>(),
        new TypeMappingConverter<V2.IOutputConf, V2.JdbcOutputConf>(),
        new TypeMappingConverter<V2.IOutputConf, V2.KafkaOutputConf>(),
        new TypeMappingConverter<V2.ISourceDataTransformation, V2.NarrowDataUnfolding>(),
        new TypeMappingConverter<V2.ISourceDataTransformation, V2.WideDataFilling>(),
        new TypeMappingConverter<V2.ISourceDataTransformationConf, V2.NarrowDataUnfoldingConf>(),
        new TypeMappingConverter<V2.ISourceDataTransformationConf, V2.WideDataFillingConf>(),

        new V3.InputConfConverter(),
        new V3.OutputConfConverter(),
        new TypeMappingConverter<V3.ISourceDataTransformation, V3.NarrowDataUnfolding>(),
        new TypeMappingConverter<V3.ISourceDataTransformation, V3.WideDataFilling>(),
        new TypeMappingConverter<V3.ISourceDataTransformationConf, V3.NarrowDataUnfoldingConf>(),
        new TypeMappingConverter<V3.ISourceDataTransformationConf, V3.WideDataFillingConf>(),
        new V3.EventSchemaValueConverter()
    };
}