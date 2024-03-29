﻿using Bloc.Pointers;
using Bloc.Variables;

namespace Bloc.Values
{
    public abstract class Value : IVariable, IPointer
    {
        Value IPointer.Value => this;
        Value IVariable.Value => this;

        public abstract new ValueType GetType();

        internal virtual Value Copy() => this;
        internal virtual void Assign() { }
        internal virtual void Destroy() { }

        public abstract bool Equals(Value other);

        public abstract Value Explicit(ValueType type);
        public abstract T Implicit<T>() where T : Value;

        public bool Is<T>(out T? value) where T : Value
        {
            try
            {
                value = Implicit<T>();
                return true;
            }
            catch
            {
                value = null;
                return false;
            }
        }

        public abstract string ToString(int depth);
        public override string ToString() => ToString(0);
    }
}