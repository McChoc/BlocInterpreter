using System.Collections.Generic;
using System.Linq;
using Bloc.Results;
using Bloc.Utils.Extensions;
using Bloc.Utils.Helpers;
using Bloc.Values.Core;
using Bloc.Variables;
using Array = Bloc.Values.Types.Array;
using Range = Bloc.Values.Types.Range;

namespace Bloc.Pointers;

public sealed class SlicePointer : Pointer
{
    private readonly Array _array;
    private readonly Range _range;

    internal SlicePointer(Array array, Range range)
    {
        _array = array;
        _range = range;
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

        if (_range.Step is null)
            AssignContinuous(array);
        else
            AssignDiscrete(array);

        return array;
    }

    public override Value Delete()
    {
        var values = new List<Value>();

        foreach (var variable in GetVariables())
        {
            values.Add(variable.Value.GetOrCopy());
            variable.Delete();
        }

        return new Array(values);
    }

    private void AssignContinuous(Array array)
    {
        foreach (var variable in GetVariables())
            variable.Delete();

        var values = array.Values
            .Select(x => new ArrayVariable(x.Value, _array));

        var (start, _, _) = RangeHelper.Deconstruct(_range, _array.Values.Count);

        if (start > _array.Values.Count)
            throw new Throw("Index out of range");

        _array.Values.InsertRange(start, values);
    }

    private void AssignDiscrete(Array array)
    {
        var variables = GetVariables();

        if (variables.Count != array.Values.Count)
            throw new Throw("Mismatch number of elements inside the slice and the array");

        foreach (var (variable, value) in variables.Zip(array.Values))
        {
            variable.Value.Destroy();
            variable.Value = value.Value;
        }
    }

    private List<ArrayVariable> GetVariables()
    {
        var (start, end, step) = RangeHelper.Deconstruct(_range, _array.Values.Count);

        var variables = new List<ArrayVariable>();

        for (int i = start; i != end && end - i > 0 == step > 0; i += step)
        {
            if (i < 0 || i >= _array.Values.Count)
                throw new Throw("Index out of range");

            variables.Add((ArrayVariable)_array.Values[i]);
        }

        return variables;
    }
}