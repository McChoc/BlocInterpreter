namespace Bloc.Values
{
    public abstract class Value : IValue
    {
        Value IValue.Value => this;

        internal abstract new ValueType GetType();

        internal virtual Value Copy() => this;

        internal virtual void Destroy() { }

        public abstract override string ToString();

        public abstract override int GetHashCode();

        public abstract override bool Equals(object other);

        public static bool operator ==(Value a, Value b) => a.Equals(b);

        public static bool operator !=(Value a, Value b) => !a.Equals(b);
    }
}