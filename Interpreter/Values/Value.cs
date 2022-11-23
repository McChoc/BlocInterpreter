using Bloc.Pointers;
using Bloc.Variables;

namespace Bloc.Values
{
    public abstract class Value : IVariable, IPointer
    {
        Value IPointer.Value => this;
        Value IVariable.Value => this;

        internal abstract new ValueType GetType();

        internal virtual Value Copy() => this;

        internal virtual void Assign() { }

        internal virtual void Destroy() { }

        public abstract bool Equals(Value other);

        public abstract string ToString(int depth);

        public override string ToString() => ToString(0);
    }
}