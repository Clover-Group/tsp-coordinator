namespace TspCoordinator.Data.Grammars;

public interface IAst
{

}

public class Time : IAst
{
    public required long Millis { get; set; }

    public static Time operator +(Time a, Time b) => new() { Millis = a.Millis + b.Millis };
}

public class TimeInterval : IAst
{
    public required long Min { get; set; }
    public required long Max { get; set; }
}

public enum ValueType
{
    Int8, Int16, Int32, Int64, Float32, Float64, Boolean, String, Unknown
}

public class Interval
{
    public required long Min { get; set; }
    public required long Max { get; set; }
}

public class Constant<T> : IAst
{
    public required T Value { get; set; }
}

public class Identifier : IAst
{
    public required string Value { get; set; }
}

public class Range<T> : IAst
{
    public required T From { get; set; }
    public required T To { get; set; }
}

public class FunctionCall : IAst
{
    public required string FunctionName { get; set; }
    public required IEnumerable<IAst> Arguments { get; set; }
}

public class ReducerFunctionCall : IAst
{
    public required string FunctionName { get; set; }
    public required IEnumerable<IAst> Arguments { get; set; }
}

public class AndThen : IAst
{
    public required IAst First { get; set; }
    public required IAst Second { get; set; }
}

public class Timer : IAst
{
    public required IAst Condition { get; set; }
    public required TimeInterval TimeInterval { get; set; }
    public required long? Gap { get; set; }
}

public class Waiter : IAst
{
    public required IAst Condition { get; set; }
    public required long Window { get; set; }
    public required long? Gap { get; set; }
}

public class ForWithInterval : IAst
{
    public required IAst Condition { get; set; }
    public required bool Exactly { get; set; }
    public required long Window { get; set; }
    public required TimeInterval TimeInterval { get; set; }
    public required bool FromStart { get; set; }

}

public class AggregateFunctionCall : IAst
{
    public required string FunctionName { get; set; }
    public required IAst Argument { get; set; }
    public required long Window { get; set; }
}

public class Cast : IAst
{
    public required IAst Argument { get; set; }
    public required ValueType Type { get; set; }
}