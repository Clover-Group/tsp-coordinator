namespace TspCoordinator.Data.Grammars;

class TypeChecker(IAst ast, int patternId, Dictionary<string, ValueType> types)
{
    private readonly IAst ast = ast;
    private readonly int patternId = patternId;
    private readonly Dictionary<string, ValueType> types = types;

    public ValueType CheckTypes(ref CheckResponse checkResponse) => CheckAstNode(ast, ref checkResponse);

    private ValueType CheckAstNode(IAst ast, ref CheckResponse checkResponse)
    {
        switch (ast)
        {
            case Constant<byte>: return ValueType.Int8;
            case Constant<short>: return ValueType.Int16;
            case Constant<int>: return ValueType.Int32;
            case Constant<long>: return ValueType.Int64;
            case Constant<float>: return ValueType.Float32;
            case Constant<double>: return ValueType.Float64;
            case Identifier id:
                var t = types.GetValueOrDefault(id.Value, ValueType.Unknown);
                if (t == ValueType.Unknown) checkResponse.Warnings.Add(new IssueInfo(patternId, $"Column {id.Value} has unknown type"));
                return t;
            case FunctionCall fc:
                {
                    List<ValueType> argTypes = [];
                    foreach (var arg in fc.Arguments) argTypes.Add(CheckAstNode(arg, ref checkResponse));
                    // TODO: Check function and get the return value
                    return ValueType.Float64;
                }
            case ReducerFunctionCall rfc:
                {
                    List<ValueType> argTypes = [];
                    foreach (var arg in rfc.Arguments) argTypes.Add(CheckAstNode(arg, ref checkResponse));
                    // TODO: Check function and get the return value
                    return ValueType.Float64;
                }
            case AndThen at:
                {
                    CheckAstNode(at.First, ref checkResponse);
                    CheckAstNode(at.Second, ref checkResponse);
                    return ValueType.Boolean;
                }
            case Timer tm:
                {
                    CheckAstNode(tm.Condition, ref checkResponse);
                    return ValueType.Boolean;
                }
            case Waiter wt:
                {
                    CheckAstNode(wt.Condition, ref checkResponse);
                    return ValueType.Boolean;
                }
            case ForWithInterval fwi:
                {
                    CheckAstNode(fwi.Condition, ref checkResponse);
                    return ValueType.Boolean;
                }
            case AggregateFunctionCall afc: return CheckAstNode(afc.Argument, ref checkResponse);
            case Cast ca:
                {
                    CheckAstNode(ca.Argument, ref checkResponse);
                    return ca.Type;
                }
            default: return ValueType.Unknown;
        }
    }
}