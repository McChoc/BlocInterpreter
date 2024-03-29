﻿using Bloc.Memory;
using Bloc.Results;
using Bloc.Values;

namespace Bloc.Pointers
{
    internal class UndefinedPointer : Pointer
    {
        internal UndefinedPointer(string name) => Name = name;

        internal string Name { get; }

        internal override Pointer Define(Value value, Call call) => call.Set(Name, value);

        internal override Value Get() => throw new Throw($"Variable {Name} was not defined in scope");

        internal override Value Set(Value _) => throw new Throw($"Variable {Name} was not defined in scope");

        internal override Value Delete() => throw new Throw($"Variable {Name} was not defined in scope");

        internal override void Invalidate() => throw new Throw($"Variable {Name} was not defined in scope");

        internal override bool Equals(Pointer _) => false;
    }
}