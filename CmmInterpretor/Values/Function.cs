using CmmInterpretor.Data;
using CmmInterpretor.Exceptions;
using CmmInterpretor.Results;
using CmmInterpretor.Statements;
using CmmInterpretor.Tokens;
using CmmInterpretor.Variables;
using System.Collections.Generic;

namespace CmmInterpretor.Values
{
    public class Function : Value, IInvokable
    {
        public bool Async { get; set; }
        public List<string> Names { get; set; } = new();
        public List<Statement> Code { get; set; }
        public Scope Captures { get; set; }

        public override VariableType Type => VariableType.Function;

        public Function() => Code = new List<Statement>();
        public Function(List<Statement> code) => Code = code;
        public Function(List<Token> expression) => Code = new() { new ReturnStatement(expression) };

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

        public override bool Implicit<T>(out T value)
        {
            if (typeof(T) == typeof(Bool))
            {
                value = Bool.True as T;
                return true;
            }

            if (typeof(T) == typeof(String))
            {
                value = new String(ToString()) as T;
                return true;
            }

            if (typeof(T) == typeof(Function))
            {
                value = this as T;
                return true;
            }

            value = null;
            return false;
        }

        public override IResult Implicit(VariableType type)
        {
            return type switch
            {
                VariableType.Bool => Bool.True,
                VariableType.String => new String(ToString()),
                VariableType.Function => this,
                _ => new Throw($"Cannot implicitly cast function as {type.ToString().ToLower()}")
            };
        }

        public override IResult Explicit(VariableType type)
        {
            return type switch
            {
                VariableType.Bool => Bool.True,
                VariableType.String => new String(ToString()),
                VariableType.Function => this,
                _ => new Throw($"Cannot cast function as {type.ToString().ToLower()}")
            };
        }

        public override string ToString(int _) => $"{(Async ? "async" : "")}({ string.Join(", ", Names) }) {{...}}";

        public IResult Invoke(List<Value> values, Call parent)
        {
            if (Async)
            {
                return new Task();
            }

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
                        labels.Add(Code[i].Label, i);

                for (int i = 0; i < Code.Count; i++)
                {
                    var result = Code[i].Execute(call); ;

                    if (result is not IValue)
                    {
                        if (result is Continue or Break)
                        {
                            throw new SyntaxError("No loop");
                        }
                        else if (result is Return r)
                        {
                            return r.value;
                        }
                        else if (result is Goto g)
                        {
                            if (labels.TryGetValue(g.label, out int index))
                                i = index - 1;
                            else
                                throw new SyntaxError("No such label in scope");
                        }
                        else
                        {
                            return result;
                        }
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
