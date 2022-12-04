using System.Collections.Generic;
using System.Linq;
using Bloc.Extensions;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Values;
using Bloc.Variables;

namespace Bloc.Pointers
{
    internal sealed class SlicePointer : Pointer
    {
        internal SlicePointer(List<ArrayVariable> variables)
        {
            Variables = variables;

            foreach (var variable in variables)
                variable.Pointers.Add(this);
        }

        internal List<ArrayVariable>? Variables { get; private set; }

        internal override Pointer Define(bool _0, Value _1, Call _2)
        {
            throw new Throw("The left part of an assignement must be a variable");
        }

        internal override Value Get()
        {
            if (Variables is null)
                throw new Throw("Invalid reference");

            return new Array(Variables
                .Select(v => v.Value.Copy())
                .ToList());
        }

        internal override Value Set(Value value)
        {
            if (Variables is null)
                throw new Throw("Invalid reference");

            if (value is not Array array)
                throw new Throw("You can only assign an array to a slice");

            if (Variables.Count != array.Variables.Count)
                throw new Throw("Mismatch number of elements inside the slice and the array");

            foreach (var (var, val) in Variables.Zip(array.Variables))
            {
                var.Value.Destroy();
                var.Value = val.Value.Copy();
            }

            return array;
        }

        internal override Value Delete()
        {
            if (Variables is null)
                throw new Throw("Invalid reference");

            var values = new List<Value>(Variables.Count);

            foreach (var variable in Variables)
            {
                values.Add(variable.Value.Copy());
                variable.Delete();
            }

            return new Array(values);
        }

        internal override void Invalidate()
        {
            Variables = null;
        }

        internal override bool Equals(Pointer other)
        {
            if (other is not SlicePointer pointer)
                return false;

            if (Variables is null || pointer.Variables is null)
                return false;

            if (Variables.Count != pointer.Variables.Count)
                return false;

            for (var i = 0; i < Variables.Count; i++)
                if (Variables[i] != pointer.Variables[i])
                    return false;

            return true;
        }
    }
}