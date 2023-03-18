using System.Collections.Generic;
using System.Linq;
using Bloc.Results;
using Bloc.Utils.Extensions;
using Bloc.Utils.Helpers;
using Bloc.Values;
using Bloc.Variables;
using Array = Bloc.Values.Array;
using Range = Bloc.Values.Range;

namespace Bloc.Pointers;

internal sealed class SlicePointer : Pointer
{
    internal Array Array { get; }
    internal Range Range { get; }

    internal SlicePointer(Array array, Range range)
    {
        Array = array;
        Range = range;
    }

    internal override Value Get()
    {
        var values = GetVariables()
            .Select(x => x.Value.GetOrCopy())
            .ToList();

        return new Array(values);
    }

    internal override Value Set(Value value)
    {
        if (value is not Array array)
            throw new Throw("Only an array can be assigned to a slice");

        if (Range.Step is null)
            AssignContinuous(array);
        else
            AssignDiscrete(array);

        return array;
    }

    internal override Value Delete()
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
            .Select(x => new ArrayVariable(x.Value, Array));

        var (start, _, _) = RangeHelper.Deconstruct(Range, Array.Values.Count);

        Array.Values.InsertRange(start, values);
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
        var (start, end, step) = RangeHelper.Deconstruct(Range, Array.Values.Count);

        var variables = new List<ArrayVariable>();

        for (int i = start; i != end && end - i > 0 == step > 0; i += step)
        {
            if (i < 0 || i >= Array.Values.Count)
                throw new Throw("Index out of range");

            variables.Add((ArrayVariable)Array.Values[i]);
        }

        return variables;
    }
}