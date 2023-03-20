using System.Collections.Generic;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Values;

namespace Bloc.Expressions.Members;

internal sealed record UnpackedMember(IExpression Expression) : IMember
{
    public IEnumerable<(string, Value)> GetMembers(Call call)
    {
        var value = Expression.Evaluate(call).Value.GetOrCopy();

        if (value is not Struct @struct)
            throw new Throw("Only a struct can be unpacked using the struct unpack syntax");

        foreach (var (key, val) in @struct.Values)
            yield return (key, val.Value);
    }
}