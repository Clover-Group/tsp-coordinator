using System.Text.Json.Serialization;

namespace TspCoordinator.Data.TspApi.V2;

public interface ISourceDataTransformationConf
{

}

public class NarrowDataUnfoldingConf : ISourceDataTransformationConf
{
    public string KeyColumn { get; set; }
    public string DefaultValueColumn { get; set; }
    public Dictionary<String, long> FieldsTimeoutsMs { get; set; }

    public Dictionary<String, List<String>>? ValueColumnMapping { get; set; }

    public long DefaultTimeout { get; set; }
}

public class WideDataFillingConf : ISourceDataTransformationConf
{
    public Dictionary<String, long> FieldsTimeoutsMs { get; set; }

    public long DefaultTimeout { get; set; }
}

public interface ISourceDataTransformation
{
    public string Type { get; }
    public ISourceDataTransformationConf Config { get; }
}

public class NarrowDataUnfolding : ISourceDataTransformation
{
    [JsonInclude]
    public string Type { get => "NarrowDataUnfolding"; set { } }

    [JsonInclude]
    public ISourceDataTransformationConf Config { get; set; }
}

public class WideDataFilling : ISourceDataTransformation
{
    [JsonInclude]
    public string Type { get => "WideDataFilling"; set { } }

    [JsonInclude]
    public ISourceDataTransformationConf Config { get; set; }
}