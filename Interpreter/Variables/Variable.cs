using System.Collections.Generic;
using Bloc.Pointers;
using Bloc.Values;

namespace Bloc.Variables
{
    public abstract class Variable : IVariable
    {
        public Variable(Value value)
        {
            Value = value;
            Pointers = new();
        }

        internal List<Pointer> Pointers { get; }

        public Value Value { get; set; }

        public virtual void Delete()
        {
            foreach (var pointer in Pointers)
                pointer.Invalidate();

            Value.Destroy();
        }
    }
}