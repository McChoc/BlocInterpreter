using CmmInterpretor.Results;
using CmmInterpretor.Values;
using System.Collections.Generic;

namespace CmmInterpretor.Data
{
    public class Pointer : IValue, IResult
    {
        private readonly Engine _engine;

        public Variable Variable { get; }

        public List<object> Accessors { get; } = new();

        public Pointer(Variable variable, Engine engine)
        {
            Variable = variable;
            _engine = engine;
        }

        Value IValue.Value() => Get();

        public Pointer Copy() => new(Variable, _engine);

        public Value Get()
        {
            return Variable.value;
        }

        public Value Set(Value value)
        {
            Variable.SetValue(value);
            return value.Copy();
        }

        public void Remove()
        {
            Variable.Destroy();
        }

        public IResult NameOf()
        {
            return Variable.name != null ? new String(Variable.name) : new Throw("This expression does not have a name.");
        }

        //private Value ProcessAccessors(Value start)
        //{
        //    Value variable = start;

        //    foreach (object accessor in Accessors)
        //    {
        //        if (accessor is string key)
        //            variable = variable.AsObject().Variables[key];
        //        else if (accessor is int idx)
        //            variable = variable.AsArray().Variables[idx];
        //    }

        //    return variable;
        //}

        //private Value ProcessAccessors(Value variable, Value value, int index = 0)
        //{
        //    if (index >= Accessors.Count)
        //        return value;

        //    if (Accessors[index] is string key)
        //    {
        //        if (!(variable is Null || variable is Object))
        //            throw new Throw("It was not an object.");

        //        Object obj = variable.AsObject();
        //        obj.Variables.TryGetValue(key, out Value currentValue);
        //        obj.Variables[key] = ProcessAccessors(currentValue, value, index + 1);
        //        return obj;
        //    }

        //    if (Accessors[index] is int idx)
        //    {
        //        if (variable is Null || variable is Array)
        //        {
        //            Array arr = variable.AsArray();
        //            Value currentValue = idx < arr.Variables.Count ? arr.Variables[idx] : null;
        //            currentValue = ProcessAccessors(currentValue, value, index + 1);

        //            while (idx >= arr.Variables.Count)
        //                arr.Variables.Add(new Null());

        //            arr.Variables[idx] = currentValue;
        //            return arr;
        //        }

        //        if (variable is String str)
        //        {
        //            StringBuilder builder = new StringBuilder(str.Value);
        //            Value currentValue = idx < builder.Length ? new String(builder[idx].ToString()) : null;
        //            currentValue = ProcessAccessors(currentValue, value, index + 1);

        //            while (idx >= builder.Length)
        //                builder.Append('\0');

        //            builder[idx] = currentValue.AsString().Value[0];
        //            str.Value = builder.ToString();
        //            return str;
        //        }

        //        throw new Throw("It was not a string or an array.");
        //    }

        //    throw new System.Exception();
        //}
    }
}
