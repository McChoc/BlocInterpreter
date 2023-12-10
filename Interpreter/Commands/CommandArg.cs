using System.Collections.Generic;
using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Utils.Helpers;
using Bloc.Values.Core;
using Bloc.Values.Types;

namespace Bloc.Commands;

internal sealed record CommandArg
{
    private readonly bool _unpack;
    private readonly IExpression _expression;

    internal CommandArg(IExpression expression, bool unpack = false)
    {
        _unpack = unpack;
        _expression = expression;
    }

    public IEnumerable<Value> GetArguments(Call call)
    {
        var value = _expression.Evaluate(call);

        if (!_unpack)
        {
            yield return value.Value;
        }
        else
        {
            value = ReferenceHelper.Resolve(value, call.Engine.Options.HopLimit).Value;

            if (!Iter.TryImplicitCast(value, out var iter, call))
                throw new Throw("Cannot implicitly convert to iter");

            foreach (var item in IterHelper.CheckedIterate(iter, call.Engine.Options))
                yield return item;
        }
    }
}