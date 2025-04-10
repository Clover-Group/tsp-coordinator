using Antlr4.Runtime;
using TspCoordinator.Data.Grammars.Generated;

namespace TspCoordinator.Data.Grammars;

public class CheckService()
{
    public CheckResponse Check(CheckRequest request)
    {
        CheckResponse checkResponse = new();
        foreach (var pattern in request.Patterns)
        {
            // Check syntax
            var input = pattern.SourceCode;
            try
            {
                var inputStream = new AntlrInputStream(input);
                var lexer = new TspDslLexer(inputStream);
                var tokenStream = new CommonTokenStream(lexer);
                var parser = new TspDslParser(tokenStream);
                parser.AddErrorListener(new TspDslErrorListener(pattern.Id, checkResponse));
                var parsed = parser.main_rule();
                // Do not proceed if syntax errors occur
                if (parser.NumberOfSyntaxErrors > 0)
                {
                    checkResponse.Warnings.Add(new IssueInfo(pattern.Id, $"{parser.NumberOfSyntaxErrors} syntax errors in the pattern, stopping further analysis"));
                    continue;
                }
                var visitor = new AstBuilder();
                var ast = visitor.VisitMain_rule(parsed);
                var types = request.ColumnTypes.Select(kv => (kv.Key, kv.Value switch
                {
                    "int8" => ValueType.Int8,
                    "int16" => ValueType.Int16,
                    "int32" => ValueType.Int32,
                    "int64" => ValueType.Int64,
                    "float32" => ValueType.Float32,
                    "float64" => ValueType.Float64,
                    "boolean" => ValueType.Boolean,
                    "string" => ValueType.String,
                    _ => ValueType.Unknown,
                })).ToDictionary();
                var typeChecker = new TypeChecker(ast, pattern.Id, types);
                typeChecker.CheckTypes(ref checkResponse);
            }
            catch (Exception e)
            {
                checkResponse.Errors.Add(new IssueInfo(pattern.Id, e.ToString()));
            }
        }
        return checkResponse;
    }
}

internal class TspDslErrorListener(int patternId, CheckResponse checkResponse) : IAntlrErrorListener<IToken>
{
    private readonly CheckResponse checkResponse = checkResponse;

    public void SyntaxError(TextWriter output, IRecognizer recognizer, IToken offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e)
    {
        string errorMessage = "line " + line + ":" + charPositionInLine + " " + msg;
        checkResponse.Errors.Add(new IssueInfo(patternId, errorMessage));
    }
}