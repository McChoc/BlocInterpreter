using System.Collections.Generic;
using Bloc.Pointers;
using Bloc.Results;
using Bloc.Values;

namespace Bloc.Variables;

public abstract class Variable : IValue
{
    private readonly bool _mutable;

    private Value _value;

    internal List<Pointer> Pointers { get; } = new();

    public Value Value
    {
        get => _value;
        set
        {
            if (!_mutable)
                throw new Throw("Cannot assign a value to a readonly variable");

            _value = value;
        }
    }

    public Variable(bool mutable, Value value)
    {
        _mutable = mutable;
        _value = value;
    }

    public virtual void Delete()
    {
        foreach (var pointer in Pointers)
            pointer.Invalidate();

        Value.Destroy();
    }
}