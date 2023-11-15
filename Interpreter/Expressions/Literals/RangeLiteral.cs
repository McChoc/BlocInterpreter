using Bloc.Memory;
using Bloc.Results;
using Bloc.Utils.Helpers;
using Bloc.Values.Behaviors;
using Bloc.Values.Core;
using Bloc.Values.Types;

namespace Bloc.Expressions.Literals;

internal sealed record RangeLiteral : IExpression
{
    private readonly IExpression? _start;
    private readonly IExpression? _stop;
    private readonly IExpression? _step;
    private readonly bool _inclusive;

    internal RangeLiteral(IExpression? start, IExpression? stop, IExpression? step, bool inclusive)
    {
        _start = start;
        _stop = stop;
        _step = step;
        _inclusive = inclusive;
    }

    public IValue Evaluate(Call call)
    {
        int? start = null, stop = null, step = null;

        if (_start is not null)
        {
            var value = _start.Evaluate(call).Value;
            value = ReferenceHelper.Resolve(value, call.Engine.Options.HopLimit).Value;
            start = value switch
            {
                Null => null,
                INumeric numeric => numeric.GetInt(),
                _ => throw new Throw($"Range indices must be null_t, bool or number, not {value.GetTypeName()}")
            };
        }

        if (_stop is not null)
        {
            var value = _stop.Evaluate(call).Value;
            value = ReferenceHelper.Resolve(value, call.Engine.Options.HopLimit).Value;
            stop = value switch
            {
                Null => null,
                INumeric numeric => numeric.GetInt(),
                _ => throw new Throw($"Range indices must be null_t, bool or number, not {value.GetTypeName()}")
            };
        }

        if (_step is not null)
        {
            var value = _step.Evaluate(call).Value;
            value = ReferenceHelper.Resolve(value, call.Engine.Options.HopLimit).Value;
            step = value switch
            {
                Null => null,
                INumeric numeric when numeric.GetInt() != 0 => numeric.GetInt(),
                INumeric => throw new Throw("A range cannot have a step of 0"),
                _ => throw new Throw($"Range indices must be null_t, bool or number, not {value.GetTypeName()}")
            };
        }

        return new Range(start, stop, step, _inclusive);
    }
}