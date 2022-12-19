using System.Collections.Generic;
using Bloc.Pointers;
using Bloc.Results;
using Bloc.Values;

namespace Bloc.Variables
{
    public abstract class Variable : IVariable
    {
        private readonly bool _mutable;

        private Value _value;

        internal List<Pointer> Pointers { get; } = new();

        public Value Value
        {
            get => _value;
            set
            {
                if (value is Void)
                    throw new Throw("'void' cannot be assigned to a variable");

                if (!_mutable)
                    throw new Throw("Cannot assign a value to a readonly variable");

                _value = value;
            }
        }

        public Variable(bool mutable, Value value)
        {
            if (value is Void)
                throw new Throw("'void' cannot be assigned to a variable");

            _mutable = mutable;
            _value = value;
        }

        public virtual void Delete()
        {
            foreach (var pointer in Pointers)
                pointer.Invalidate();

            Value.Destroy();
        }
    }
}