using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Pointers;
using Bloc.Results;
using Bloc.Utils;
using Bloc.Values;

namespace Bloc.Operators
{
    internal class Range : IExpression
    {
        private readonly IExpression? _start;
        private readonly IExpression? _end;
        private readonly IExpression? _step;

        internal Range(IExpression? start, IExpression? end, IExpression? step)
        {
            _start = start;
            _end = end;
            _step = step;
        }

        public IPointer Evaluate(Call call)
        {
            int? start = null, end = null;
            int step = 1;

            if (_start is not null)
            {
                var value = _start.Evaluate(call).Value;

                value = ReferenceUtil.Dereference(value, call.Engine.HopLimit).Value;

                if (!value.Is(out Number? number))
                    throw new Throw("");

                start = number!.ToInt();
            }

            if (_end is not null)
            {
                var value = _end.Evaluate(call).Value;

                value = ReferenceUtil.Dereference(value, call.Engine.HopLimit).Value;

                if (!value.Is(out Number? number))
                    throw new Throw("");

                end = number!.ToInt();
            }

            if (_step is not null)
            {
                var value = _step.Evaluate(call).Value;

                value = ReferenceUtil.Dereference(value, call.Engine.HopLimit).Value;

                if (!value.Is(out Number? number))
                    throw new Throw("");

                step = number!.ToInt();
            }

            if (step == 0)
                throw new Throw("A range cannot have a step of 0");

            return new Values.Range(start, end, step);
        }
    }
}