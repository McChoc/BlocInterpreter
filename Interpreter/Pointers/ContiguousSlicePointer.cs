using System.Collections.Generic;
using System.Linq;
using Bloc.Results;
using Bloc.Utils.Helpers;
using Bloc.Values.Core;
using Bloc.Values.Types;
using Bloc.Variables;

namespace Bloc.Pointers;

public sealed class ContiguousSlicePointer : Pointer
{
    private readonly Array _array;
    private readonly int _start, _end;

    internal ContiguousSlicePointer(Array array, Range range)
    {
        _array = array;
        (_start, _end, _) = RangeHelper.Deconstruct(range, array.Values.Count);
    }

    public override Value Get()
    {
        var values = GetVariables()
            .Select(x => x.Value.GetOrCopy())
            .ToList();

        return new Array(values);
    }

    public override Value Set(Value value)
    {
        if (value is not Array array)
            throw new Throw("Only an array can be assigned to a slice");

        foreach (var variable in GetVariables())
            variable.Delete(false);

        var values = array.Values
            .Select(x => new ArrayVariable(x.Value.GetOrCopy(true), _array));

        _array.Values.InsertRange(_start, values);

        return array;
    }

    public override Value Delete()
    {
        var values = new List<Value>();

        foreach (var variable in GetVariables())
        {
            values.Add(variable.Value.GetOrCopy());
            variable.Delete(false);
        }

        return new Array(values);
    }

    private List<ArrayVariable> GetVariables()
    {
        var variables = new List<ArrayVariable>();

        for (int i = _start; i < _end; i++)
            variables.Add((ArrayVariable)_array.Values[i]);

        return variables;
    }
}