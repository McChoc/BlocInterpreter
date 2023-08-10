using System.Collections.Generic;
using Bloc.Memory;
using Bloc.Pointers;
using Bloc.Results;
using Bloc.Utils.Attributes;
using Bloc.Values.Core;
using Bloc.Values.Types;

namespace Bloc.Expressions.Literals;

[Record]
internal sealed partial class TupleLiteral : IExpression
{
    private readonly List<IExpression> _expressions;

    internal TupleLiteral(List<IExpression> expressions)
    {
        _expressions = expressions;
    }

    public IValue Evaluate(Call call)
    {
        var values = new List<IValue>();

        foreach (var expression in _expressions)
        {
            var value = expression.Evaluate(call);

            if (value is Void)
                throw new Throw("'void' is not assignable");

            if (value is Pointer)
                values.Add(value);
            else
                values.Add(value.Value.GetOrCopy());
        }

        return new Tuple(values);
    }
}