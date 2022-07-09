using System.Collections.Generic;
using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Values;

namespace Bloc.Variables
{
    internal abstract class Variable : IValue, IExpression
    {
        public List<Reference> References { get; } = new();

        public abstract Value Value { get; set; }

        IValue IExpression.Evaluate(Call _) => this;

        public abstract void Destroy();
    }
}