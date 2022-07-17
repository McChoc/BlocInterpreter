﻿using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Pointers;
using Bloc.Results;
using Bloc.Utils;
using Bloc.Values;

namespace Bloc.Operators
{
    internal class BooleanAnd : IExpression
    {
        private readonly IExpression _left;
        private readonly IExpression _right;

        internal BooleanAnd(IExpression left, IExpression right)
        {
            _left = left;
            _right = right;
        }

        public IPointer Evaluate(Call call)
        {
            var value = _left.Evaluate(call).Value;

            value = ReferenceUtil.Dereference(value, call.Engine).Value;

            if (!value.Is(out Bool? @bool))
                throw new Throw("Cannot implicitly convert to bool");

            if (!@bool!.Value)
                return value;

            return _right.Evaluate(call).Value;
        }
    }
}