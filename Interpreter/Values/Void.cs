﻿using Bloc.Results;

namespace Bloc.Values
{
    public class Void : Value
    {
        private Void() { }

        public static Void Value { get; } = new();

        public override ValueType GetType() => ValueType.Void;

        public override void Assign() => throw new Throw("You cannot assign void to a variable");

        public override bool Equals(IValue other)
        {
            return other.Value is Void;
        }

        public override T Implicit<T>()
        {
            throw new Throw($"Cannot implicitly cast void as {typeof(T).Name.ToLower()}");
        }

        public override IValue Explicit(ValueType type)
        {
            throw new Throw($"Cannot cast void as {type.ToString().ToLower()}");
        }

        public override string ToString(int _) => "void";
    }
}