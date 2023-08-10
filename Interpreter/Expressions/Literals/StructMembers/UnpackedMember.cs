using System.Collections.Generic;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Values.Core;
using Bloc.Values.Types;

namespace Bloc.Expressions.Literals.StructMembers;

internal sealed record UnpackedMember : IMember
{
    private readonly IExpression _expression;

    public UnpackedMember(IExpression expression)
    {
        _expression = expression;
    }

    public IEnumerable<(string, Value)> GetMembers(Call call)
    {
        var value = _expression.Evaluate(call).Value.GetOrCopy();

        if (value is not Struct @struct)
            throw new Throw("Only a struct can be unpacked using the struct unpack syntax");

        foreach (var (key, val) in @struct.Values)
            yield return (key, val.Value);
    }
}