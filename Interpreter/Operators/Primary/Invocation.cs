using System.Collections.Generic;
using System.Linq;
using Bloc.Expressions;
using Bloc.Interfaces;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Utils;
using Bloc.Values;

namespace Bloc.Operators
{
    internal sealed class Invocation : IExpression
    {
        private readonly IExpression _expression;
        private readonly List<IExpression> _posParameters;
        private readonly Dictionary<string, IExpression> _nameParameters;

        internal Invocation(IExpression expression, List<IExpression> posParameters, Dictionary<string, IExpression> nameParameters)
        {
            _expression = expression;
            _posParameters = posParameters;
            _nameParameters = nameParameters;
        }

        public IValue Evaluate(Call call)
        {
            var value = _expression.Evaluate(call).Value;

            value = ReferenceUtil.Dereference(value, call.Engine.HopLimit).Value;

            if (value is not IInvokable invokable)
                throw new Throw("The '()' operator can only be applied to a func or a type");

            var args = _posParameters.Select(x => x.Evaluate(call).Value.Copy()).ToList();
            var kwargs = _nameParameters.ToDictionary(x => x.Key, x => x.Value.Evaluate(call).Value.Copy());

            return invokable.Invoke(args, kwargs, call);
        }

        public override int GetHashCode()
        {
            return System.HashCode.Combine(_expression, _posParameters.Count, _nameParameters.Count);
        }

        public override bool Equals(object other)
        {
            if (other is not Invocation invocation)
                return false;

            if (!_expression.Equals(invocation._expression))
                return false;

            if (!_posParameters.SequenceEqual(invocation._posParameters))
                return false;

            if (_nameParameters.Count != invocation._nameParameters.Count)
                return false;

            foreach (var key in _nameParameters.Keys)
            {
                if (!invocation._nameParameters.TryGetValue(key, out var expression))
                    return false;

                if (!_nameParameters[key].Equals(expression))
                    return false;
            }

            return true;
        }
    }
}