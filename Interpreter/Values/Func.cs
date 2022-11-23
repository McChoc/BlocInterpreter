using System.Collections.Generic;
using Bloc.Interfaces;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Statements;
using Bloc.Utils;

namespace Bloc.Values
{
    public sealed class Func : Value, IInvokable
    {
        internal FunctionType Type { get; set; }
        internal CaptureMode Mode { get; set; }

        internal Scope? Captures { get; set; }
        internal List<Parameter> Parameters { get; set; }
        internal List<Statement> Statements { get; set; }
        internal Dictionary<string, Label> Labels { get; set; }

        internal Func(FunctionType type, CaptureMode mode, Scope? captures, List<Parameter> parameters, List<Statement> statements)
        {
            Type = type;
            Mode = mode;

            Captures = captures;
            Parameters = parameters;
            Statements = statements;

            Labels = StatementUtil.GetLabels(statements);
        }

        internal override ValueType GetType() => ValueType.Func;

        internal override Value Copy()
        {
            return new Func(Type, Mode, Captures, Parameters, Statements);
        }

        public override bool Equals(Value other)
        {
            if (other is not Func func)
                return false;

            if (Type != func.Type)
                return false;

            if (Mode != func.Mode)
                return false;

            if (Parameters.Count != func.Parameters.Count)
                return false;

            if (Statements.Count != func.Statements.Count)
                return false;

            if (Captures is null != func.Captures is null)
                return false;

            if (Captures is not null && func.Captures is not null)
                if (Captures.Variables.Count != func.Captures.Variables.Count)
                    return false;

            for (var i = 0; i < Parameters.Count; i++)
                if (Parameters[i] != func.Parameters[i])
                    return false;

            for (var i = 0; i < Statements.Count; i++)
                if (Statements[i] != func.Statements[i]) // TODO fix statement equality
                    return false;

            if (Captures is not null && func.Captures is not null)
                foreach (var key in Captures.Variables.Keys)
                    if (!Captures.Variables[key].Value.Equals(func.Captures.Variables[key].Value))
                        return false;

            return true;
        }

        internal static Func Construct(List<Value> values)
        {
            return values.Count switch
            {
                0 => new(FunctionType.Synchronous, CaptureMode.None, null, new(), new()),
                1 => values[0] switch
                {
                    Null => new(FunctionType.Synchronous, CaptureMode.None, null, new(), new()),
                    Func func => func,
                    _ => throw new Throw($"'func' does not have a constructor that takes a '{values[0].GetType().ToString().ToLower()}'")
                },
                _ => throw new Throw($"'func' does not have a constructor that takes {values.Count} arguments")
            };
        }

        public override string ToString(int _)
        {
            return "[func]";
        }

        public Value Invoke(List<Value> values, Call parent)
        {
            for (int i = 0; i < values.Count; i++)
                if (values[i] == Void.Value)
                    values[i] = i < Parameters.Count
                        ? Parameters[i].Value
                        : Null.Value;

            var call = new Call(parent, Captures, this, values);

            for (var i = 0; i < Parameters.Count; i++)
            {
                var (name, value) = Parameters[i];

                if (i < values.Count)
                    value = values[i];

                call.Set(name, value);
            }

            return Type switch
            {
                FunctionType.Asynchronous => new Task(() => Execute(call)),
                FunctionType.Generator => new Iter(call, Statements),
                _ => Execute(call),
            };
        }

        private Value Execute(Call call)
        {
            try
            {
                for (var i = 0; i < Statements.Count; i++)
                {
                    var result = Statements[i].Execute(call);

                    if (result is Yield)
                        throw new Throw("A yield statement can only be used inside a generator");

                    if (result is Continue or Break)
                        throw new Throw("No loop");

                    if (result is Throw)
                        throw result;

                    if (result is Return r)
                        return r.Value.Copy();

                    if (result is Goto g)
                    {
                        if (Labels.TryGetValue(g.Label, out var label))
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

        internal sealed record Parameter
        {
            internal string Name { get; }
            internal Value Value { get; }

            internal Parameter(string name, Value value)
            {
                Name = name;
                Value = value;
            }

            internal void Deconstruct(out string name, out Value value)
            {
                name = Name;
                value = Value;
            }
        }
    }
}