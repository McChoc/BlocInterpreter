using Bloc.Identifiers;
using Bloc.Memory;
using Bloc.Patterns;
using Bloc.Results;
using Bloc.Utils.Helpers;
using Bloc.Values.Behaviors;
using Bloc.Values.Core;
using Bloc.Values.Types;

namespace Bloc.Expressions.Operators;

internal sealed record MatchDeclarationOperator : IExpression
{
    private readonly IExpression? _expression;
    private readonly IIdentifier _identifier;

    internal MatchDeclarationOperator(IExpression? expression, IIdentifier identifier)
    {
        _expression = expression;
        _identifier = identifier;
    }

    public IValue Evaluate(Call call)
    {
        IPatternNode? root = null;

        if (_expression is not null)
        {
            var value = _expression.Evaluate(call).Value;

            value = ReferenceHelper.Resolve(value, call.Engine.Options.HopLimit).Value;

            if (value is not IPattern pattern)
                throw new Throw($"Cannot apply operator '->' on operand of type {value.GetTypeName()}");

            root = pattern.GetRoot();
        }

        return new Pattern(new DeclarationPattern(root, _identifier));
    }
}