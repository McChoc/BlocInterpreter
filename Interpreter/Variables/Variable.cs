using System.Collections.Generic;
using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Values;

namespace Bloc.Variables
{
    internal abstract class Variable : IValue, IExpression
    {
        public List<Reference> References { get; } = new();

        IValue IExpression.Evaluate(Call _) => this;

        public abstract Value Value { get; set; }

        public ValueType Type => Value.Type;

        Value IValue.Copy() => Value.Copy();
        void IValue.Assign() => Value.Assign();
        public abstract void Destroy();

        bool IValue.Equals(IValue other) => Value.Equals(other);

        T IValue.Implicit<T>() => Value.Implicit<T>();
        IValue IValue.Explicit(ValueType type) => Value.Explicit(type);

        string IValue.ToString(int depth) => Value.ToString(depth);

        public bool Is<T>(out T? value) where T : Value
        {
            try
            {
                value = Value.Implicit<T>();
                return true;
            }
            catch
            {
                value = null;
                return false;
            }
        }
    }
}