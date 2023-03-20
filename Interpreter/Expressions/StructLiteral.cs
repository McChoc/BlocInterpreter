using System;
using System.Collections.Generic;
using System.Linq;
using Bloc.Memory;
using Bloc.SubExpressions;
using Bloc.Utils.Extensions;
using Bloc.Values;

namespace Bloc.Expressions;

internal sealed class StructLiteral : IExpression
{
    private readonly List<IMember> _members;

    internal StructLiteral(List<IMember> members)
    {
        _members = members;
    }

    public IValue Evaluate(Call call)
    {
        var values = _members
            .SelectMany(x => x.GetMembers(call))
            .ToDictionary();

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