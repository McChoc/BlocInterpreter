using System.Collections.Generic;
using System.Linq;
using Bloc.Results;
using Bloc.Utils.Extensions;
using Bloc.Values.Core;
using Bloc.Values.Types;

namespace Bloc.Pointers;

public sealed class SlicePointer : Pointer
{
    private readonly List<Pointer> _pointers;

    internal SlicePointer(List<Pointer> pointers)
    {
        _pointers = pointers;
    }

    public override Value Get()
    {
        var values = _pointers
            .Select(x => x.Get().GetOrCopy())
            .ToList();

        return new Array(values);
    }

    public override Value Set(Value value)
    {
        if (value is not Array array)
            throw new Throw("Only an array can be assigned to a slice");

        if (_pointers.Count != array.Values.Count)
            throw new Throw($"An array of length {array.Values.Count} cannot be assigned to a discrete slice of length {_pointers.Count}.");

        foreach (var (pointer, val) in _pointers.Zip(array.Values))
            pointer.Set(val.Value);

        return array;
    }

    public override Value Delete()
    {
        var values = new List<Value>();

        foreach (var pointer in _pointers)
        {
            var value = pointer.Delete();
            values.Add(value);
        }

        return new Array(values);
    }
}