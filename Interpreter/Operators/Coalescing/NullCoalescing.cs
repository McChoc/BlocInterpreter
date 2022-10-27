﻿using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Pointers;
using Bloc.Results;
using Bloc.Utils;
using Bloc.Values;

namespace Bloc.Operators
{
    internal class NullCoalescing : IExpression
    {
        private readonly IExpression _left;
        private readonly IExpression _right;

        internal NullCoalescing(IExpression left, IExpression right)
        {
            _left = left;
            _right = right;
        }

        public IPointer Evaluate(Call call)
        {
            var value = _left.Evaluate(call).Value;

            value = ReferenceUtil.Dereference(value, call.Engine.HopLimit).Value;

            if (value.GetType() == ValueType.Void)
                throw new Throw("Cannot apply operator ?? to type 'void'");

            if (value.GetType() == ValueType.Null)
                return _right.Evaluate(call).Value;

            return value;
        }
    }
}
