using System;
using System.Collections.Generic;
using System.Linq;
using Bloc.Expressions.SubExpressions;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Values;

namespace Bloc.Expressions.Literals;

internal sealed class StructLiteral : IExpression
{
    private readonly List<IMember> _members;

    internal StructLiteral(List<IMember> members)
    {
        _members = members;
    }

    public IValue Evaluate(Call call)
    {
        var values = new Dictionary<string, Value>();

        foreach (var (key, value) in _members.SelectMany(x => x.GetMembers(call)))
        {
            if (values.ContainsKey(key))
                throw new Throw("Duplicate key");

            values[key] = value;
        }

        return new Struct(values);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(_members.Count);
    }

    public override bool Equals(object other)
    {
        return other is StructLiteral literal &&
            _members.SequenceEqual(literal._members);
    }
}