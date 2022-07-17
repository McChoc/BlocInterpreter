using System.Collections.Generic;
using Bloc.Interfaces;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Statements;
using Bloc.Utils;

namespace Bloc.Values
{
    internal enum CaptureMode
    {
        None,
        Value,
        Reference
    }

    public class Function : Value, IInvokable
    {
        internal bool Async { get; set; }
        internal CaptureMode Mode { get; set; }

        internal List<string> Names { get; set; } = new();
        internal List<Statement> Code { get; set; } = new();
        internal Scope Captures { get; set; } = new();

        public override ValueType GetType() => ValueType.Function;

        internal override Value Copy()
        {
            return new Function
            {
                Async = Async,
                Mode = Mode,
                Names = Names,
                Code = Code,
                Captures = Captures.Copy()
            };
        }

        public override bool Equals(Value other)
        {
            if (other is not Function func)
                return false;

            if (Async != func.Async)
                return false;

            if (Mode != func.Mode)
                return false;

            if (Names.Count != func.Names.Count)
                return false;

            if (Code.Count != func.Code.Count)
                return false;

            if (Captures.Variables.Count != func.Captures.Variables.Count)
                return false;

            for (var i = 0; i < Names.Count; i++)
                if (Names[i] != func.Names[i])
                    return false;

            for (var i = 0; i < Code.Count; i++)
                if (Code[i] != func.Code[i])
                    return false;

            foreach (var key in Captures.Variables.Keys)
                if (!Captures.Variables[key].Value.Equals(func.Captures.Variables[key].Value))
                    return false;

            return true;
        }

        public override T Implicit<T>()
        {
            if (typeof(T) == typeof(Null))
                return (Null.Value as T)!;

            if (typeof(T) == typeof(Bool))
                return (Bool.True as T)!;

            if (typeof(T) == typeof(String))
                return (new String(ToString()) as T)!;

            if (typeof(T) == typeof(Function))
                return (this as T)!;

            throw new Throw($"Cannot implicitly cast function as {typeof(T).Name.ToLower()}");
        }

        public override Value Explicit(ValueType type)
        {
            return type switch
            {
                ValueType.Null => Null.Value,
                ValueType.Bool => Bool.True,
                ValueType.String => new String(ToString()),
                ValueType.Function => this,
                _ => throw new Throw($"Cannot cast function as {type.ToString().ToLower()}")
            };
        }

        public override string ToString(int _)
        {
            return "[function]";
        }

        public Value Invoke(List<Value> values, Call parent)
        {
            if (!Async)
                return Call(values, parent);
            else
                return new Task(() => Call(values, parent));
        }

        private Value Call(List<Value> values, Call parent)
        {
            var call = new Call(parent, Captures, this, values);

            try
            {
                var labels = StatementUtil.GetLabels(Code);

                for (var i = 0; i < Names.Count; i++)
                {
                    var name = Names[i];
                    var value = i < values.Count && values[i] != Void.Value ? values[i] : Null.Value;

                    call.Set(name, value);
                }

                for (var i = 0; i < Code.Count; i++)
                {
                    var result = Code[i].Execute(call);

                    if (result is Continue or Break)
                        throw new Throw("No loop");

                    if (result is Throw or Exit)
                        throw result;

                    if (result is Return r)
                        return r.Value;

                    if (result is Goto g)
                    {
                        if (labels.TryGetValue(g.Label, out var label))
                        {
                            label.Count++;

                            if (label.Count > call.Engine.JumpLimit)
                                throw new Throw("The jump limit was reached.");

                            i = label.Index - 1;

                            continue;
                        }

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