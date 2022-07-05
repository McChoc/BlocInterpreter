using Bloc.Expressions;
using Bloc.Interfaces;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Values;

namespace Bloc.Operators.Primary
{
    internal class Indexer : IExpression
    {
        private readonly IExpression _expression;
        private readonly IExpression _index;

        internal Indexer(IExpression expression, IExpression index)
        {
            _expression = expression;
            _index = index;
        }

        public IValue Evaluate(Call call)
        {
            var value = _expression.Evaluate(call);

            if (value.Value is not IIndexable indx)
                throw new Throw("You can only index a string, an array or a struct.");

            value = _index.Evaluate(call);

            return indx.Index(value.Value, call);
        }
    }
}