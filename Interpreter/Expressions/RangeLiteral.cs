using Bloc.Interfaces;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Utils.Helpers;
using Bloc.Values;

namespace Bloc.Expressions;

internal sealed record RangeLiteral : IExpression
{
    private readonly IExpression? _start;
    private readonly IExpression? _end;
    private readonly IExpression? _step;

    internal RangeLiteral(IExpression? start, IExpression? end, IExpression? step)
    {
        _start = start;
        _end = end;
        _step = step;
    }

    public IValue Evaluate(Call call)
    {
        int? start = null, end = null;
        int step = 1;

        if (_start is not null)
        {
            var value = _start.Evaluate(call).Value;

            value = ReferenceHelper.Resolve(value, call.Engine.HopLimit).Value;

            if (value is not IScalar scalar)
                throw new Throw($"Cannot apply operator ':' on type {value.GetType().ToString().ToLower()}");

            start = scalar.GetInt();
        }

        if (_end is not null)
        {
            var value = _end.Evaluate(call).Value;

            value = ReferenceHelper.Resolve(value, call.Engine.HopLimit).Value;

            if (value is not IScalar scalar)
                throw new Throw($"Cannot apply operator ':' on type {value.GetType().ToString().ToLower()}");

            end = scalar.GetInt();
        }

        if (_step is not null)
        {
            var value = _step.Evaluate(call).Value;

            value = ReferenceHelper.Resolve(value, call.Engine.HopLimit).Value;

            if (value is not IScalar scalar)
                throw new Throw($"Cannot apply operator ':' on type {value.GetType().ToString().ToLower()}");

            step = scalar.GetInt();

            if (step == 0)
                throw new Throw("A range cannot have a step of 0");
        }

        return new Range(start, end, step);
    }
}