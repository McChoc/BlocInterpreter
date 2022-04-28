using CmmInterpretor.Data;
using CmmInterpretor.Exceptions;
using CmmInterpretor.Results;
using CmmInterpretor.Statements;
using CmmInterpretor.Tokens;
using System.Collections.Generic;
using System.Linq;

namespace CmmInterpretor.Values
{
    public class Function : Value
    {
        public bool Async { get; set; }
        public List<string> Names { get; set; } = new();
        public List<Statement> Code { get; set; }
        public Scope Captures { get; set; }

        public Function() => Code = new List<Statement>();
        public Function(List<Statement> code) => Code = code;
        public Function(List<Token> expression) => Code = new() { new ReturnStatement(expression) };

        public IResult Call(Array variables, Engine engine)
        {
            if (Async)
            {
                return new Task();
            }
            else
            {
                var call = new Call(engine, Captures);

                call.SetParams(variables);

                for (int i = 0; i < Names.Count; i++)
                    call.Set(Names[i], new Variable(Names[i], variables.Count > i ? variables[i] : new Null(), call.Scopes[^1]));

                var labels = new Dictionary<string, int>();

                for (int i = 0; i < Code.Count; i++)
                    if (Code[i] is LabelStatement lbl)
                        labels.Add(lbl.Label, i);

                for (int i = 0; i < Code.Count; i++)
                {
                    var result = Code[i].Execute(call); ;

                    if (result is not IValue)
                    {
                        if (result is Continue || result is Break)
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
                                return result;
                        }
                        else
                        {
                            return result;
                        }
                    }
                }

                return new Void();
            }
        }

        public override VariableType TypeOf() => VariableType.Function;

        public override Value Copy() => new Function()
        {
            Async = Async,
            Names = Names,
            Code = Code,
            Captures = Captures
        };

        public override bool Equals(Value other)
        {
            if (other is not Function func)
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

        public override IResult Explicit<T>()
        {
            if (typeof(T) == typeof(Bool))
                return Bool.True;

            if (typeof(T) == typeof(String))
                return new String(ToString());

            if (typeof(T) == typeof(Function))
                return this;

            return new Throw($"Cannot cast function as {typeof(T)}");
        }

        public override string ToString(int _) => $"{(Async ? "async" : "")}({ string.Join(", ", Names) }) {{...}}";
    }
}
