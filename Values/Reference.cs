using CmmInterpretor.Data;
using CmmInterpretor.Results;

namespace CmmInterpretor.Values
{
    public class Reference : Value
    {
        public Pointer Pointer { get; set; }

        public override VariableType TypeOf() => VariableType.Reference;

        public Reference(Pointer pointer) => Pointer = pointer;

        public override Value Copy() => new Reference(Pointer);

        public void Invalidate()
        {
            Pointer = null;
        }

        public override bool Equals(Value other)
        {
            if (other is not Reference r)
                return false;

            if (Pointer.Variable != r.Pointer.Variable)
                return false;

            if (Pointer.Accessors.Count != r.Pointer.Accessors.Count)
                return false;

            for (int i = 0; i < Pointer.Accessors.Count; i++)
                if (Pointer.Accessors[i] != r.Pointer.Accessors[i])
                    return false;

            return true;
        }

        public override bool Implicit<T>(out T value)
        {
            if (typeof(T) == typeof(Reference))
            {
                value = this as T;
                return true;
            }
            else if (Pointer.Variable.value is T t)
            {
                value = t;
                return true;
            }

            value = null;
            return false;
        }

        public override IResult Explicit<T>()
        {
            if (typeof(T) == typeof(Reference))
                return this;

            if (Pointer.Variable.value is T t)
                return t;

            return new Throw($"Cannot cast void as {typeof(T)}");
        }

        public override string ToString(int _) => "[reference]";
    }
}
