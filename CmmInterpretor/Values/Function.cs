using CmmInterpretor.Interfaces;
using CmmInterpretor.Memory;
using CmmInterpretor.Results;
using CmmInterpretor.Statements;
using CmmInterpretor.Variables;
using System.Collections.Generic;

namespace CmmInterpretor.Values
{
    public class Function : Value, IInvokable
    {
        internal bool Async { get; set; }
        internal List<string> Names { get; set; } = new();
        internal List<Statement> Code { get; set; } = new();
        internal Scope Captures { get; set; } = new(null);

        public override ValueType Type => ValueType.Function;

        public override Value Copy() => new Function()
        {
            Async = Async,
            Names = Names,
            Code = Code,
            Captures = Captures
        };

        public override bool Equals(IValue other)
        {
            if (other.Value is not Function func)
                return false;

            if (Async != func.Async)
                return false;

            if (Names.Count != func.Names.Count)
                return false;

            if (Code.Count != func.Code.Count)
                return false;

            if (Captures.Variables.Count != func.Captures.Variables.Count)
                return false;

            for (int i = 0; i < Names.Count; i++)
                if (Names[i] != func.Names[i])
                    return false;

            for (int i = 0; i < Code.Count; i++)
                if (Code[i] != func.Code[i])
                    return false;

            foreach (string key in Captures.Variables.Keys)
                if (!Captures.Variables[key].Equals(func.Captures.Variables[key]))
                    return false;

            return true;
        }

        public override T Implicit<T>()
        {
            if (typeof(T) == typeof(Bool))
                return (Bool.True as T)!;

            if (typeof(T) == typeof(String))
                return (new String(ToString()) as T)!;

            if (typeof(T) == typeof(Function))
                return (this as T)!;

            throw new Throw($"Cannot implicitly cast function as {typeof(T).Name.ToLower()}");
        }

        public override IValue Explicit(ValueType type)
        {
            return type switch
            {
                ValueType.Bool => Bool.True,
                ValueType.String => new String(ToString()),
                ValueType.Function => this,
                _ => throw new Throw($"Cannot cast function as {type.ToString().ToLower()}")
            };
        }

        public override string ToString(int _) => $"{(Async ? "async " : "")}({ string.Join(", ", Names) }) {{...}}";

        public IValue Invoke(List<Value> values, Call parent)
        {
            if (!Async)
                return Call(values, parent);
            else
                return new Task(System.Threading.Tasks.Task.Run(() => Call(values, parent)));
        }

        private Value Call(List<Value> values, Call parent)
        {
            var call = new Call(parent, Captures, this, values);

            try
            {
                for (int i = 0; i < Names.Count; i++)
                {
                    var name = Names[i];
                    var value = i < values.Count && values[i] != Void.Value ? values[i] : Null.Value;

                    call.Set(name, new StackVariable(value, name, call.Scopes[^1]));
                }

                var labels = new Dictionary<string, int>();

                for (int i = 0; i < Code.Count; i++)
                    if (Code[i].Label is not null)
                        labels.Add(Code[i].Label!, i);

                for (int i = 0; i < Code.Count; i++)
                {
                    var result = Code[i].Execute(call); ;

                    if (result is Return r)
                        return r.value;

                    if (result is Throw or Exit)
                        throw result;

                    if (result is Continue or Break)
                        throw new Throw("No loop");

                    if (result is Goto g)
                    {
                        if (labels.TryGetValue(g.label, out int index))
                            i = index - 1;
                        else
                            throw new Throw("No such label in scope");
                    }
                }

                return Void.Value;
            }
            finally
            {
                call.Destroy();
            }
        }
    }
}
