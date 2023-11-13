using System.Collections.Generic;
using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Values.Types;

namespace Bloc.Commands.Arguments;

internal sealed record ExpressionArgument : IArgument
{
    private readonly bool _unpack;
    private readonly IExpression _expression;

    internal ExpressionArgument(IExpression expression, bool unpack = false)
    {
        _unpack = unpack;
        _expression = expression;
    }

    public IEnumerable<string> GetArguments(Call call)
    {
        var value = _expression.Evaluate(call);

        if (!_unpack)
        {
            yield return String.ImplicitCast(value).Value;
        }
        else
        {
            if (!Iter.TryImplicitCast(value, out var iter, call))
                throw new Throw("Cannot implicitly convert to iter");

            foreach (var item in iter.Iterate())
                yield return String.ImplicitCast(item).Value;
        }
    }
}