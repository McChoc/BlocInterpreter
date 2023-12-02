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
    private readonly Range _range;

    internal ContiguousSlicePointer(Array array, Range range)
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

        foreach (var variable in GetVariables())
            variable.Delete(false);

        var (_, start, _) = RangeHelper.GetSliceParameters(_range, _array.Values.Count);
        int index = NumberHelper.Round(start);
        var values = array.Values.Select(x => new ArrayVariable(x.Value.GetOrCopy(true), _array));

        _array.Values.InsertRange(index, values);

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
        var (count, start, step) = RangeHelper.GetSliceParameters(_range, _array.Values.Count);

        if (!double.IsNaN(step))
        {
            for (int i = 0; i < count; i++)
            {
                int index = NumberHelper.Round(start + step * i);
                var variable = (ArrayVariable)_array.Values[index];
                variables.Add(variable);
            }
        }

        return variables;
    }
}