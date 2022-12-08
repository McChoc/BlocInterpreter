using Bloc.Memory;
using Bloc.Results;
using Bloc.Values;
using Bloc.Variables;

namespace Bloc.Pointers
{
    internal sealed class UnresolvedPointer : Pointer
    {
        internal string Name { get; }

        internal Variable? Local { get; }
        internal Variable? NonLocal { get; }
        internal Variable? Global { get; }

        internal UnresolvedPointer(string name, Variable? local, Variable? nonLocal, Variable? global)
        {
            Name = name;

            Local = local;
            NonLocal = nonLocal;
            Global = global;
        }

        internal VariablePointer Resolve()
        {
            var variable = Local ?? NonLocal ?? Global ?? throw new Throw($"Variable {Name} was not defined in scope");

            return new VariablePointer(variable);
        }

        internal VariablePointer Define(bool mask, bool mutable, Value value, Call call)
        {
            return call.Set(mask, mutable, Name, value);
        }

        internal override Value Get() => Resolve().Get();

        internal override Value Set(Value value) => Resolve().Set(value);

        internal override Value Delete() => Resolve().Delete();

        internal override void Invalidate() => throw new System.Exception();

        internal override bool Equals(Pointer other) => throw new System.Exception();
    }
}