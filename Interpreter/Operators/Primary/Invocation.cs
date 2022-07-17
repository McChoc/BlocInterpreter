using System.Collections.Generic;
using System.Linq;
using Bloc.Expressions;
using Bloc.Interfaces;
using Bloc.Memory;
using Bloc.Pointers;
using Bloc.Results;
using Bloc.Utils;
using Bloc.Values;

namespace Bloc.Operators
{
    internal class Invocation : IExpression
    {
        private readonly IExpression _expression;
        private readonly List<IExpression> _parameters;

        internal Invocation(IExpression expression, List<IExpression> parameters)
        {
            _expression = expression;
            _parameters = parameters;
        }

        public IPointer Evaluate(Call call)
        {
            var value = _expression.Evaluate(call).Value;

            value = ReferenceUtil.Dereference(value, call.Engine.HopLimit).Value;

            if (value is not IInvokable invokable)
                throw new Throw("The '()' operator can only be apllied to a function or a type");

            var args = new List<Value>(_parameters.Count);

            foreach (var parameter in _parameters)
                args.Add(parameter.Evaluate(call).Value);

            if (args.Count == 1 && args[0] is Array array)
                args = array.Values.Select(v => v.Value).ToList();

            return invokable.Invoke(args, call);
        }
    }
}