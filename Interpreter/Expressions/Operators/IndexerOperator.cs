using System.Collections.Generic;
using System.Linq;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Utils.Helpers;
using Bloc.Values.Behaviors;
using Bloc.Values.Core;
using Bloc.Values.Types;

namespace Bloc.Expressions.Operators;

internal sealed record IndexerOperator : IExpression
{
    private readonly IExpression _expression;
    private readonly List<Argument> _arguments;

    internal IndexerOperator(IExpression expression, List<Argument> arguments)
    {
        _expression = expression;
        _arguments = arguments;
    }

    public IValue Evaluate(Call call)
    {
        var value = _expression.Evaluate(call).Value;
        value = ReferenceHelper.Resolve(value, call.Engine.Options.HopLimit).Value;

        if (value is not IIndexable indexable)
            throw new Throw("The '[]' operator can only be apllied to strings and arrays");

        var args = new List<Value>();

        foreach (var argument in _arguments)
        {
            var val = argument.Expression.Evaluate(call).Value.GetOrCopy();

            if (!argument.Unpack)
            {
                args.Add(val);
            }
            else
            {
                val = ReferenceHelper.Resolve(val, call.Engine.Options.HopLimit).Value;

                if (!Iter.TryImplicitCast(val, out var iter, call))
                    throw new Throw("Cannot implicitly convert to iter");

                args.AddRange(IterHelper.CheckedIterate(iter, call.Engine.Options));
            }
        }

        return indexable.Index(args, call);
    }

    internal sealed record Argument(IExpression Expression, bool Unpack);
}