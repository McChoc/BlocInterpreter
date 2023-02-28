using System;
using System.Collections.Generic;
using System.Linq;
using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Operators;
using Bloc.Results;
using Bloc.Statements;
using Bloc.Utils.Helpers;

namespace Bloc.Values
{
    public sealed class Iter : Value
    {
        private int _index;
        private IEnumerator<Result> _results;

        private readonly Call _call;
        private readonly List<Statement> _statements;
        private readonly Dictionary<string, Label> _labels;

        internal Iter(Call call, List<Statement> statements)
        {
            _results = Enumerable.Empty<Result>().GetEnumerator();

            _call = call;
            _statements = statements;
            _labels = StatementUtil.GetLabels(statements);
        }

        internal override ValueType GetType() => ValueType.Func;

        internal static Iter Construct(List<Value> values, Call call)
        {
            switch (values.Count)
            {
                case 0:
                    return new(null!, new());

                case 1:
                    switch (values[0])
                    {
                        case Null:
                            return new(null!, new());

                        case Range range:
                        {
                            var inf = double.PositiveInfinity;

                            var @params = new VariableCollection();
                            @params.Add(new(false, "start", new Number(range.Start ?? (range.Step >= 0 ? 0 : -1)), @params));
                            @params.Add(new(false, "end", new Number(range.End ?? (range.Step >= 0 ? inf : -inf)), @params));
                            @params.Add(new(false, "step", new Number(range.Step), @params));

                            return new(new(call, new(), @params), new()
                            {
                                new ForStatement(false)
                                {
                                    Initialisation = new Assignment(new Let(new Identifier("i")), new Identifier("start")),
                                    Condition = new Less(new Multiplication(new Identifier("i"), new Identifier("step")), new Multiplication(new Identifier("end"), new Identifier("step"))),
                                    Increment = new AdditionAssignment(new Identifier("i"), new Identifier("step")),
                                    Statement = new YieldStatement(new Identifier("i"))
                                }
                            });
                        }

                        case String @string:
                        {
                            var @params = new VariableCollection();
                            @params.Add(new(false, "value", @string, @params));

                            return new(new(call, new(), @params), new()
                            {
                                new ForStatement(false)
                                {
                                    Initialisation = new Assignment(new Let(new Identifier("i")), new NumberLiteral(0)),
                                    Condition = new Less(new Identifier("i"), new NumberLiteral(@string.Value.Length)),
                                    Increment = new PreIncrement(new Identifier("i")),
                                    Statement = new YieldStatement(new Indexer(new Identifier("value"), new Identifier("i")))
                                }
                            });
                        }

                        case Array array:
                        {
                            var @params = new VariableCollection();
                            @params.Add(new(false, "items", array, @params));

                            return new(new(call, new(), @params), new()
                            {
                                new ForStatement(false)
                                {
                                    Initialisation = new Assignment(new Let(new Identifier("i")), new NumberLiteral(0)),
                                    Condition = new Less(new Identifier("i"), new NumberLiteral(array.Variables.Count)),
                                    Increment = new PreIncrement(new Identifier("i")),
                                    Statement = new YieldStatement(new Indexer(new Identifier("items"), new Identifier("i")))
                                }
                            });
                        }

                        case Iter iter:
                            return iter;

                        default:
                            throw new Throw($"'iter' does not have a constructor that takes a '{values[0].GetType().ToString().ToLower()}'");
                    }

                default:
                    throw new Throw($"'iter' does not have a constructor that takes {values.Count} arguments");
            }
        }

        internal static bool TryImplicitCast(Value value, out Iter iter, Call call)
        {
            try
            {
                iter = Construct(new() { value }, call);
                return true;
            }
            catch
            {
                iter = null!;
                return false;
            }
        }

        internal override Value Copy()
        {
            return new Iter(_call, _statements);
        }

        public override string ToString()
        {
            return "[iter]";
        }

        public override int GetHashCode()
        {
            return System.HashCode.Combine(_index, _statements.Count);
        }

        public override bool Equals(object other)  // TODO fix iter copy and equality
        {
            if (other is not Iter iter)
                return false;

            if (_index != iter._index)
                return false;

            if (_call != iter._call)
                return false;

            if (!_statements.SequenceEqual(iter._statements))
                return false;

            foreach (var key in _labels.Keys)
                if (_labels[key].Count != iter._labels[key].Count)
                    return false;

            return true;
        }

        internal IEnumerable<Value> Iterate()
        {
            Value value;

            while ((value = Next()) is not Void)
                yield return value;
        }

        internal Value Next()
        {
            while (true)
            {
                while (!_results.MoveNext())
                {
                    if (_index >= _statements.Count)
                    {
                        End();
                        return Void.Value;
                    }

                    _results = _statements[_index++].Execute(_call).GetEnumerator();
                }

                switch (_results.Current)
                {
                    case Continue:
                        End();
                        throw new Throw("A continue statement can only be used inside a loop");

                    case Break:
                        End();
                        throw new Throw("A break statement can only be used inside a loop");

                    case Throw @throw:
                        End();
                        throw @throw;

                    case Return @return:
                        End();
                        return @return.Value;

                    case Yield yield:
                        return yield.Value;

                    case Goto @goto:
                        if (_labels.TryGetValue(@goto.Label, out var label))
                        {
                            if (++label.Count > _call.Engine.JumpLimit)
                            {
                                End();
                                throw new Throw("The jump limit was reached.");
                            }

                            _index = label.Index;
                            continue;
                        }

                        End();
                        throw new Throw("No such label in scope");
                }
            }

            void End()
            {
                _index = _statements.Count;
                _results = Enumerable.Empty<Result>().GetEnumerator();
            }
        }
    }
}