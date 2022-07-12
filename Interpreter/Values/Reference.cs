using Bloc.Interfaces;
using Bloc.Memory;
using Bloc.Pointers;
using Bloc.Results;
using System.Collections.Generic;

namespace Bloc.Values
{
    public class Reference : Value, IInvokable, IIndexable, IIterable
    {
        internal Reference() => Pointer = new VariablePointer(null);

        internal Reference(Pointer pointer) => Pointer = pointer;

        internal Pointer Pointer { get; }

        public override ValueType GetType() => ValueType.Reference;

        public override bool Equals(Value other)
        {
            if (other is not Reference reference)
                return false;

            return Pointer.Equals(reference.Pointer);
        }

        public override T Implicit<T>()
        {
            if (typeof(T) == typeof(Reference))
                return (this as T)!;

            return Pointer.Get().Implicit<T>();
        }

        public override Value Explicit(ValueType type)
        {
            if (type == ValueType.Reference)
                return this;

            return Pointer.Get().Explicit(type);
        }

        public override string ToString(int _)
        {
            return "[reference]";
        }

        public Value Invoke(List<Value> values, Call call)
        {
            var value = Pointer.Get();

            if (value is not IInvokable invokable)
                throw new Throw("You can only invoke a function or a type.");

            return invokable.Invoke(values, call);
        }

        public IPointer Index(Value index, Call call)
        {
            var value = Pointer.Get();

            if (value is not IIndexable indexable)
                throw new Throw("You can only index a string, an array or a struct.");

            return indexable.Index(index, call);
        }

        public IEnumerable<Value> Iterate()
        {
            var value = Pointer.Get();
            
            if (value is not IIterable iterable)
                throw new Throw("You can only iterate over a range, a string or an array.");

            return iterable.Iterate();
        }
    }
}
