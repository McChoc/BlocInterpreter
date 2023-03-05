using System.Collections.Generic;
using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Values;

namespace Bloc.Commands.Arguments;

internal class ExpressionArgument : IArgument
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
            var @string = String.ImplicitCast(value.Value);

            yield return @string.Value;
            yield break;
        }

        if (value.Value is not Array array)
            throw new Throw("Only an array can be unpacked using the array unpack syntax");

        foreach (var item in array.Values)
        {
            var @string = String.ImplicitCast(item.Value);

            yield return @string.Value;
        }
    }
}