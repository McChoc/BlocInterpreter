using System.Collections.Generic;
using Bloc.Pointers;
using Bloc.Results;
using Bloc.Values;

namespace Bloc.Variables
{
    public abstract class Variable : IVariable
    {
        protected Value _value;

        public Variable(Value value)
        {
            if (value is Void)
                throw new Throw("'void' cannot be assigned to a variable");

            _value = value;
        }

        internal List<Pointer> Pointers { get; } = new();

        public virtual Value Value
        {
            get => _value;
            set => _value = value is not Void
                ? value
                : throw new Throw("'void' cannot be assigned to a variable");
        }

        public virtual void Delete()
        {
            foreach (var pointer in Pointers)
                pointer.Invalidate();

            Value.Destroy();
        }
    }
}