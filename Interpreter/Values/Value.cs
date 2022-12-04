namespace Bloc.Values
{
    public abstract class Value : IValue
    {
        Value IValue.Value => this;

        internal abstract new ValueType GetType();

        internal virtual Value Copy() => this;

        internal virtual void Destroy() { }

        internal abstract string ToString(int depth);

        public override string ToString() => ToString(0);

        public abstract override int GetHashCode();

        public abstract override bool Equals(object other);

        public static bool operator ==(Value a, Value b) => a.Equals(b);

        public static bool operator !=(Value a, Value b) => !a.Equals(b);
    }
}