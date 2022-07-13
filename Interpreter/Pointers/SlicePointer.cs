using System.Collections.Generic;
using System.Linq;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Values;
using Bloc.Variables;

namespace Bloc.Pointers
{
    internal class SlicePointer : Pointer
    {
        internal SlicePointer(List<Variable> variables)
        {
            Variables = variables;

            foreach (var variable in variables)
                variable.Pointers.Add(this);
        }

        internal List<Variable>? Variables { get; private set; }

        internal override Pointer Define(Value _0, Call _1)
        {
            throw new Throw("The left part of an assignement must be a variable");
        }

        internal override Value Get()
        {
            if (Variables is null)
                throw new Throw("Invalid reference");

            return new Array(Variables.Select(v => v.Value.Copy()).ToList<IVariable>());
        }

        internal override Value Set(Value value)
        {
            if (Variables is null)
                throw new Throw("Invalid reference");

            if (!value.Is(out Array? array))
                throw new Throw("You can only assign an array to a slice");

            if (Variables.Count != array!.Values.Count)
                throw new Throw("Mismatch number of elements inside the slice and the array");

            value = value.Copy();
            value.Assign();

            foreach (var (var, val) in Variables.Zip(array.Values, (a, b) => (a, b)))
            {
                var.Value.Destroy();
                var.Value = val.Value;
            }

            return array;
        }

        internal override Value Delete()
        {
            if (Variables is null)
                throw new Throw("Invalid reference");

            var values = new List<IVariable>(Variables.Count);

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