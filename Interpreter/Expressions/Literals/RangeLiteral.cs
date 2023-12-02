using Bloc.Memory;
using Bloc.Results;
using Bloc.Utils.Helpers;
using Bloc.Values.Behaviors;
using Bloc.Values.Core;
using Bloc.Values.Types;

namespace Bloc.Expressions.Literals;

internal sealed record RangeLiteral : IExpression
{
    private readonly Index _start;
    private readonly Index _stop;
    private readonly IExpression? _step;

    internal record Index(IExpression? Expression, bool Inclusive);

    internal RangeLiteral(Index start, Index stop, IExpression? step)
    {
        _start = start;
        _stop = stop;
        _step = step;
    }

    public IValue Evaluate(Call call)
    {
        Range.Index start, stop;
        double? step;

        if (_start.Expression is null)
        {
            start = new(null, _start.Inclusive);
        }
        else
        {
            var value = _start.Expression.Evaluate(call).Value;
            value = ReferenceHelper.Resolve(value, call.Engine.Options.HopLimit).Value;
            start = value switch
            {
                Null => new(null, _start.Inclusive),
                INumeric numeric => new(numeric.GetDouble(), _start.Inclusive),
                _ => throw new Throw($"Range indices must be null_t, bool or number, not {value.GetTypeName()}")
            };
        }

        if (_stop.Expression is null)
        {
            stop = new(null, _stop.Inclusive);
        }
        else
        {
            var value = _stop.Expression.Evaluate(call).Value;
            value = ReferenceHelper.Resolve(value, call.Engine.Options.HopLimit).Value;
            stop = value switch
            {
                Null => new(null, _start.Inclusive),
                INumeric numeric => new(numeric.GetDouble(), _stop.Inclusive),
                _ => throw new Throw($"Range indices must be null_t, bool or number, not {value.GetTypeName()}")
            };
        }

        if (_step is null)
        {
            step = null;
        }
        else
        {
            var value = _step.Evaluate(call).Value;
            value = ReferenceHelper.Resolve(value, call.Engine.Options.HopLimit).Value;
            step = value switch
            {
                Null => null,
                INumeric numeric => numeric.GetDouble(),
                _ => throw new Throw($"Range indices must be null_t, bool or number, not {value.GetTypeName()}")
            };
        }

        return new Range(start, stop, step);
    }
}