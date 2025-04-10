namespace TspCoordinator.Data.Grammars;

using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using TspCoordinator.Data.Grammars.Generated;

public class AstBuilder : ITspDslVisitor<IAst>
{
    public IAst Visit(IParseTree tree) => tree.Accept(this);

    public IAst VisitAbs_func([NotNull] TspDslParser.Abs_funcContext context)
    {
        return new FunctionCall
        {
            FunctionName = "abs",
            Arguments = [VisitExpr(context.expr())]
        };
    }

    public IAst VisitAligned_func([NotNull] TspDslParser.Aligned_funcContext context)
    {
        throw new NotImplementedException();
    }

    public IAst VisitArithmetical_func([NotNull] TspDslParser.Arithmetical_funcContext context)
    {
        if (context.avg_func() is not null) return VisitAvg_func(context.avg_func());
        if (context.avgof_func() is not null) return VisitAvgof_func(context.avgof_func());
        if (context.minof_func() is not null) return VisitMinof_func(context.minof_func());
        if (context.maxof_func() is not null) return VisitMaxof_func(context.maxof_func());
        if (context.aligned_func() is not null) return VisitAligned_func(context.aligned_func());
        if (context.derivation_func() is not null) return VisitDerivation_func(context.derivation_func());
        if (context.lag_func() is not null) return VisitLag_func(context.lag_func());
        if (context.abs_func() is not null) return VisitAbs_func(context.abs_func());
        if (context.custom_func() is not null) return VisitCustom_func(context.custom_func());
        // unreachable
        return null;
    }

    public IAst VisitAvgof_func([NotNull] TspDslParser.Avgof_funcContext context)
    {
        return new FunctionCall
        {
            FunctionName = "avgof",
            Arguments = [.. context.expr().Select(e => VisitExpr(e))]
        };
    }

    public IAst VisitAvg_func([NotNull] TspDslParser.Avg_funcContext context)
    {
        return new AggregateFunctionCall
        {
            FunctionName = "avg",
            Argument = VisitExpr(context.expr()),
            Window = VisitTime(context.time()).Millis,
        };
    }

    public IAst VisitBoolean_const([NotNull] TspDslParser.Boolean_constContext context)
    {
        bool value = false;
        if (context.K_FALSE() != null) value = false;
        if (context.K_TRUE() != null) value = true;
        return new Constant<bool> { Value = value };
    }

    public IAst VisitBoolean_expr([NotNull] TspDslParser.Boolean_exprContext context)
    {
        if (context.boolean_const() is not null) return VisitBoolean_const(context.boolean_const());
        if (context.boolean_func() is not null) return VisitBoolean_func(context.boolean_func());
        if (context.comparison() is not null) return VisitComparison(context.comparison());
        if (context.K_NOT() is not null) return new FunctionCall
        {
            FunctionName = "not",
            Arguments = [VisitBoolean_expr(context.boolean_expr(0))]
        };
        if (context.K_AND() is not null) return new FunctionCall
        {
            FunctionName = "and",
            Arguments = [VisitBoolean_expr(context.boolean_expr(0)), VisitBoolean_expr(context.boolean_expr(1))]
        };
        if (context.K_XOR() is not null) return new FunctionCall
        {
            FunctionName = "xor",
            Arguments = [VisitBoolean_expr(context.boolean_expr(0)), VisitBoolean_expr(context.boolean_expr(1))]
        };
        if (context.K_OR() is not null) return new FunctionCall
        {
            FunctionName = "or",
            Arguments = [VisitBoolean_expr(context.boolean_expr(0)), VisitBoolean_expr(context.boolean_expr(1))]
        };
        return VisitBoolean_expr(context.boolean_expr(0));
    }

    public IAst VisitBoolean_func([NotNull] TspDslParser.Boolean_funcContext context)
    {
        if (context.increasing_func() is not null) return VisitIncreasing_func(context.increasing_func());
        if (context.decreasing_func() is not null) return VisitDecreasing_func(context.decreasing_func());
        if (context.custom_func() is not null) return VisitCustom_func(context.custom_func());
        if (context.isnull_func() is not null) return VisitIsnull_func(context.isnull_func());
        // unreachable
        return null;
    }

    public IAst VisitChildren(IRuleNode node)
    {
        throw new NotImplementedException();
    }

    public IAst VisitComparison([NotNull] TspDslParser.ComparisonContext context)
    {
        return new FunctionCall
        {
            FunctionName = VisitComparison_operator(context.comparison_operator()).Value,
            Arguments = [VisitExpr(context.expr(0)), VisitExpr(context.expr(1))]
        };
    }

    public Constant<string> VisitComparison_operator([NotNull] TspDslParser.Comparison_operatorContext context)
    {
        if (context.GT() is not null) return new Constant<string> { Value = "gt" };
        if (context.GT_EQ() is not null) return new Constant<string> { Value = "ge" };
        if (context.LT() is not null) return new Constant<string> { Value = "lt" };
        if (context.LT_EQ() is not null) return new Constant<string> { Value = "lt" };
        if (context.equality_operator()?.EQ() is not null) return new Constant<string> { Value = "eq" };
        if (context.equality_operator()?.NOT_EQ() is not null) return new Constant<string> { Value = "ne" };
        if (context.equality_operator()?.NOT_EQ2() is not null) return new Constant<string> { Value = "ne" };
        return new Constant<string> { Value = "" };
    }

    public IAst VisitCustom_func([NotNull] TspDslParser.Custom_funcContext context)
    {
        return new FunctionCall
        {
            FunctionName = VisitIdentifier(context.identifier()).Value,
            Arguments = [VisitExpr(context.expr())]
        };
    }

    public IAst VisitDecreasing_func([NotNull] TspDslParser.Decreasing_funcContext context)
    {
        return new FunctionCall
        {
            FunctionName = "decreasing",
            Arguments = [
                VisitExpr(context.expr()),
                VisitNumerical_expr(context.numerical_expr(0)),
                VisitNumerical_expr(context.numerical_expr(1))
            ]
        };
    }

    public IAst VisitDerivation_func([NotNull] TspDslParser.Derivation_funcContext context)
    {
        throw new NotImplementedException();
    }

    public IAst VisitEquality_operator([NotNull] TspDslParser.Equality_operatorContext context)
    {
        if (context.EQ() is not null) return new Constant<string> { Value = "eq" };
        if (context.NOT_EQ() is not null || context.NOT_EQ2() is not null) return new Constant<string> { Value = "ne" };
        return null;
    }

    public IAst VisitErrorNode(IErrorNode node)
    {
        throw new NotImplementedException();
    }

    public IAst VisitExpr([NotNull] TspDslParser.ExprContext context)
    {
        if (context.number() is not null) return VisitNumber(context.number());
        if (context.@string() is not null) return VisitString(context.@string());
        if (context.identifier() is not null) return VisitIdentifier(context.identifier());
        if (context.arithmetical_func() is not null) return VisitArithmetical_func(context.arithmetical_func());
        if (context.MINUS() is not null && context.LPAR() is not null && context.RPAR() is not null) return new FunctionCall
        {
            FunctionName = "neg",
            Arguments = [VisitExpr(context.expr(0))]
        };
        if (context.GT().Length > 0) return new FunctionCall
        {
            FunctionName = "json", // TODO: array/object value
            Arguments = [VisitExpr(context.expr(0)), VisitExpr(context.expr(1))]
        };
        if (context.MULT() is not null) return new FunctionCall
        {
            FunctionName = "mul",
            Arguments = [VisitExpr(context.expr(0)), VisitExpr(context.expr(1))]
        };
        if (context.DIV() is not null) return new FunctionCall
        {
            FunctionName = "div",
            Arguments = [VisitExpr(context.expr(0)), VisitExpr(context.expr(1))]
        };
        if (context.MINUS() is not null) return new FunctionCall
        {
            FunctionName = "sub",
            Arguments = [VisitExpr(context.expr(0)), VisitExpr(context.expr(1))]
        };
        if (context.PLUS() is not null) return new FunctionCall
        {
            FunctionName = "add",
            Arguments = [VisitExpr(context.expr(0)), VisitExpr(context.expr(1))]
        };
        return VisitExpr(context.expr(0));
    }

    public Identifier VisitIdentifier([NotNull] TspDslParser.IdentifierContext context) => new()
    {
        Value = context.GetText().Trim('\"')
    };

    public IAst VisitIncreasing_func([NotNull] TspDslParser.Increasing_funcContext context)
    {
        return new FunctionCall
        {
            FunctionName = "increasing",
            Arguments = [
                VisitExpr(context.expr()),
                VisitNumerical_expr(context.numerical_expr(0)),
                VisitNumerical_expr(context.numerical_expr(1))
            ]
        };
    }

    public IAst VisitIsnull_func([NotNull] TspDslParser.Isnull_funcContext context)
    {
        return new FunctionCall
        {
            FunctionName = "isnull",
            Arguments = [
                VisitExpr(context.expr())
            ]
        };
    }

    public IAst VisitLag_func([NotNull] TspDslParser.Lag_funcContext context)
    {
        return new AggregateFunctionCall
        {
            FunctionName = "isnull",
            Argument = VisitExpr(context.expr()),
            Window = context.time() is not null ? VisitTime(context.time()).Millis : 1,
        };
    }

    public IAst VisitMain_rule([NotNull] TspDslParser.Main_ruleContext context)
    {
        return VisitTrilean_expr(context.trilean_expr());
    }

    public IAst VisitMaxof_func([NotNull] TspDslParser.Maxof_funcContext context)
    {
        return new FunctionCall
        {
            FunctionName = "maxof",
            Arguments = [.. context.expr().Select(e => VisitExpr(e))]
        };
    }

    public IAst VisitMinof_func([NotNull] TspDslParser.Minof_funcContext context)
    {
        return new FunctionCall
        {
            FunctionName = "minof",
            Arguments = [.. context.expr().Select(e => VisitExpr(e))]
        };
    }

    public Constant<double> VisitNumber([NotNull] TspDslParser.NumberContext context)
    {
        double value = Double.Parse(context.NUMBER().GetText());
        return new Constant<double> { Value = value };
    }

    public Constant<double> VisitNumerical_expr([NotNull] TspDslParser.Numerical_exprContext context)
    {
        return VisitNumber(context.number());
    }

    public IAst VisitNumeric_range([NotNull] TspDslParser.Numeric_rangeContext context)
    {
        return new Range<double>
        {
            From = VisitNumerical_expr(context.numerical_expr(0)).Value,
            To = VisitNumerical_expr(context.numerical_expr(1)).Value
        };
    }

    public IAst VisitString([NotNull] TspDslParser.StringContext context)
    {
        return new Constant<string> { Value = context.GetText() };
    }

    public IAst VisitTerminal(ITerminalNode node)
    {
        throw new NotImplementedException();
    }

    public Time VisitTime([NotNull] TspDslParser.TimeContext context)
    {
        return new Time() { Millis = context.time_token().Select(t => VisitTime_token(t).Millis).Sum() };
    }

    public IAst VisitTimes([NotNull] TspDslParser.TimesContext context)
    {
        throw new NotImplementedException();
    }

    public IAst VisitTimes_cond([NotNull] TspDslParser.Times_condContext context)
    {
        throw new NotImplementedException();
    }

    public IAst VisitTime_cond([NotNull] TspDslParser.Time_condContext context)
    {
        throw new NotImplementedException();
    }

    public TimeInterval VisitTime_interval([NotNull] TspDslParser.Time_intervalContext context)
    {
        // TODO: actual time intervals
        return new TimeInterval
        {
            Min = 0,
            Max = 0
        };
    }

    public IAst VisitTime_range([NotNull] TspDslParser.Time_rangeContext context)
    {
        throw new NotImplementedException();
    }

    public Time VisitTime_token([NotNull] TspDslParser.Time_tokenContext context)
    {
        double value = VisitNumerical_expr(context.numerical_expr()).Value;
        double multiplier = 1;
        if (context.K_MS() is not null) multiplier = 1;
        if (context.K_SEC() is not null) multiplier = 1_000;
        if (context.K_MIN() is not null) multiplier = 60_000;
        if (context.K_HR() is not null) multiplier = 3_600_000;
        if (context.K_DAY() is not null) multiplier = 86_400_000;
        return new Time { Millis = (long)(value * multiplier) };

    }

    public IAst VisitTime_with_abs_tol([NotNull] TspDslParser.Time_with_abs_tolContext context)
    {
        var time = VisitTime(context.time(0));
        var tolerance = VisitTime(context.time(1));
        return new TimeInterval { Min = time.Millis - tolerance.Millis, Max = time.Millis + tolerance.Millis };
    }

    public IAst VisitTime_with_rel_tol([NotNull] TspDslParser.Time_with_rel_tolContext context)
    {
        var time = VisitTime(context.time());
        var tolerance = VisitNumerical_expr(context.numerical_expr());
        return new TimeInterval
        {
            Min = (long)(time.Millis * (1 - tolerance.Value)),
            Max = (long)(time.Millis * (1 + tolerance.Value))
        };
    }

    public IAst VisitTrilean_expr([NotNull] TspDslParser.Trilean_exprContext context)
    {
        if (context.K_ANDTHEN() is not null) return new AndThen
        {
            First = VisitTrilean_expr(context.trilean_expr(0)),
            Second = VisitTrilean_expr(context.trilean_expr(1))
        };
        if (context.K_FOR() is not null) return new Timer
        {
            Condition = VisitTrilean_expr(context.trilean_expr(0)),
            TimeInterval = VisitTime_interval(context.time_interval()),
            Gap = 0 // TODO: actual window? 
        };
        if (context.K_AND() is not null) return new FunctionCall
        {
            FunctionName = "and",
            Arguments = [VisitTrilean_expr(context.trilean_expr(0)), VisitTrilean_expr(context.trilean_expr(1))]
        };
        if (context.K_OR() is not null) return new FunctionCall
        {
            FunctionName = "and",
            Arguments = [VisitTrilean_expr(context.trilean_expr(0)), VisitTrilean_expr(context.trilean_expr(1))]
        };
        if (context.K_WAIT() is not null) return new Waiter
        {
            Condition = VisitTrilean_expr(context.trilean_expr(0)),
            Gap = 0,
            Window = VisitTime(context.time()).Millis
        };
        if (context.LPAR() is not null && context.RPAR() is not null) return VisitTrilean_expr(context.trilean_expr(0));
        if (context.boolean_expr() is not null) return VisitBoolean_expr(context.boolean_expr());
        // TODO: Unreachable?
        return null;
    }

    IAst ITspDslVisitor<IAst>.VisitComparison_operator(TspDslParser.Comparison_operatorContext context)
    {
        return VisitComparison_operator(context);
    }

    IAst ITspDslVisitor<IAst>.VisitIdentifier(TspDslParser.IdentifierContext context)
    {
        return VisitIdentifier(context);
    }

    #region Auto-implementations

    IAst ITspDslVisitor<IAst>.VisitNumber(TspDslParser.NumberContext context)
    {
        return VisitNumber(context);
    }

    IAst ITspDslVisitor<IAst>.VisitNumerical_expr(TspDslParser.Numerical_exprContext context)
    {
        return VisitNumerical_expr(context);
    }

    IAst ITspDslVisitor<IAst>.VisitTime(TspDslParser.TimeContext context)
    {
        return VisitTime(context);
    }

    IAst ITspDslVisitor<IAst>.VisitTime_interval(TspDslParser.Time_intervalContext context)
    {
        return VisitTime_interval(context);
    }

    IAst ITspDslVisitor<IAst>.VisitTime_token(TspDslParser.Time_tokenContext context)
    {
        return VisitTime_token(context);
    }

    #endregion
}