using System.Collections.Generic;
using System.Linq;
using Bloc.Expressions.Literals.StructMembers;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Utils.Attributes;
using Bloc.Values.Core;
using Bloc.Values.Types;

namespace Bloc.Expressions.Literals;

[Record]
internal sealed partial class StructLiteral : IExpression
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
}