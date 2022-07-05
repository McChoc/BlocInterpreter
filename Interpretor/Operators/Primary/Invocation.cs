using System.Collections.Generic;
using System.Linq;
using Bloc.Expressions;
using Bloc.Interfaces;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Values;

namespace Bloc.Operators.Primary
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

        public IValue Evaluate(Call call)
        {
            var value = _expression.Evaluate(call);

            if (value.Value is not IInvokable invk)
                throw new Throw("You can only invoke a function or a type.");

            var args = new List<Value>();

            foreach (var parameter in _parameters)
                args.Add(parameter.Evaluate(call).Value);

            if (args.Count == 1 && args[0] is Array array)
                args = array.Values.Select(v => v.Value).ToList();

            return invk.Invoke(args, call);
        }
    }
}