using System.Collections.Generic;
using System.Linq;
using Bloc.Interfaces;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Statements;
using Bloc.Utils;

namespace Bloc.Values
{
    public sealed class Func : Value, IInvokable
    {
        private readonly FunctionType _type;
        private readonly CaptureMode _mode;

        private readonly Scope _captures;
        private readonly List<Parameter> _parameters;
        private readonly List<Statement> _statements;
        private readonly Dictionary<string, Label> _labels;

        internal Func(FunctionType type, CaptureMode mode, Scope captures, List<Parameter> parameters, List<Statement> statements)
        {
            _type = type;
            _mode = mode;

            _captures = captures;
            _parameters = parameters;
            _statements = statements;

            _labels = StatementUtil.GetLabels(statements);
        }

        internal override ValueType GetType() => ValueType.Func;

        internal static Func Construct(List<Value> values)
        {
            return values.Count switch
            {
                0 => new(FunctionType.Synchronous, CaptureMode.None, new(), new(), new()),
                1 => values[0] switch
                {
                    Null => new(FunctionType.Synchronous, CaptureMode.None, new(), new(), new()),
                    Func func => func,
                    _ => throw new Throw($"'func' does not have a constructor that takes a '{values[0].GetType().ToString().ToLower()}'")
                },
                _ => throw new Throw($"'func' does not have a constructor that takes {values.Count} arguments")
            };
        }

        internal override string ToString(int _)
        {
            return "[func]";
        }

        public override int GetHashCode()
        {
            return System.HashCode.Combine(_type, _mode, _parameters.Count, _statements.Count);
        }

        public override bool Equals(object other)
        {
            if (other is not Func func)
                return false;

            if (_type != func._type)
                return false;

            if (_mode != func._mode)
                return false;

            if (!_parameters.SequenceEqual(func._parameters))
                return false;

            if (!_statements.SequenceEqual(func._statements))
                return false;

            if (_captures.Variables.Count != func._captures.Variables.Count)
                return false;

            foreach (var key in _captures.Variables.Keys)
            {
                if (!func._captures.Variables.ContainsKey(key))
                    return false;

                if (!_captures.Variables[key].SequenceEqual(func._captures.Variables[key]))
                    return false;
            }

            return true;
        }

        public Value Invoke(List<Value> values, Call parent)
        {
            for (int i = 0; i < values.Count; i++)
                if (values[i] == Void.Value)
                    values[i] = i < _parameters.Count
                        ? _parameters[i].Value
                        : Null.Value;

            var call = new Call(parent, _captures, this, new(values));

            for (var i = 0; i < _parameters.Count; i++)
            {
                var (name, value) = _parameters[i];

                if (i < values.Count)
                    value = values[i];

                call.Set(true, true, name, value);
            }

            return _type switch
            {
                FunctionType.Asynchronous => new Task(() => Execute(call)),
                FunctionType.Generator => new Iter(call, _statements),
                _ => Execute(call),
            };
        }

        private Value Execute(Call call)
        {
            try
            {
                for (var i = 0; i < _statements.Count; i++)
                {
                    switch (_statements[i].Execute(call).FirstOrDefault())
                    {
                        case Continue:
                            throw new Throw("A continue statement can only be used inside a loop");

                        case Break:
                            throw new Throw("A break statement can only be used inside a loop");

                        case Yield:
                            throw new Throw("A yield statement can only be used inside a generator");

                        case Throw @throw:
                            throw @throw;

                        case Return @return:
                            return @return.Value.Copy();

                        case Goto @goto:
                            if (_labels.TryGetValue(@goto.Label, out var label))
                            {
                                if (++label.Count > call.Engine.JumpLimit)
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