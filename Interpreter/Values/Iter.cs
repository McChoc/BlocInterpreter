using System.Collections.Generic;
using System.Linq;
using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Operators;
using Bloc.Results;
using Bloc.Statements;
using Bloc.Utils;
using Bloc.Variables;

namespace Bloc.Values
{
    public sealed class Iter : Value
    {
        private int _index;

        private readonly Call _call;
        private readonly List<Statement> _statements;
        private readonly Dictionary<string, Label> _labels;

        internal Iter(Call call, List<Statement> statements)
        {
            _call = call;
            _statements = statements;
            _labels = StatementUtil.GetLabels(statements);
        }

        internal override ValueType GetType() => ValueType.Func;

        public override bool Equals(Value other)
        {
            if (other is not Iter iter)
                return false;

            if (_index != iter._index)
                return false;

            if (_call != iter._call)
                return false;

            if (!Enumerable.SequenceEqual(_statements, iter._statements))
                return false;

            if (!Enumerable.SequenceEqual(_labels, iter._labels))
                return false;

            return true;
        }

        public IEnumerable<Value> Iterate()
        {
            Value value;

            while ((value = Next()) is not Void)
                yield return value;
        }

        internal Value Next()
        {
            while (_index < _statements.Count)
            {
                var result = _statements[_index++].Execute(_call);

                if (result is Continue or Break)
                    throw new Throw("No loop");

                if (result is Throw)
                    throw result;

                if (result is Return)
                    return Void.Value;

                if (result is Yield yield)
                    return yield.Value;

                if (result is Goto @goto)
                {
                    if (_labels.TryGetValue(@goto.Label, out var label))
                    {
                        label.Count++;

                        if (label.Count > _call.Engine.JumpLimit)
                            throw new Throw("The jump limit was reached.");

                        _index = label.Index;

                        continue;
                    }

                    throw new Throw("No such label in scope");
                }
            }

            return Void.Value;
        }

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

                            var scope = new Scope();
                            scope.Variables.Add("start", new StackVariable("start", new Number(range.Start ?? (range.Step >= 0 ? 0 : -1)), scope));
                            scope.Variables.Add("end", new StackVariable("end", new Number(range.End ?? (range.Step >= 0 ? inf : -inf)), scope));
                            scope.Variables.Add("step", new StackVariable("step", new Number(range.Step), scope));

                            return new(new(call, scope), new()
                            {
                                new ForStatement(
                                    initialisation: new Assignment(new Let(new Identifier("i")), new Identifier("start")),
                                    condition:      new Less(new Multiplication(new Identifier("i"), new Identifier("step")), new Multiplication(new Identifier("end"), new Identifier("step"))),
                                    increment:      new AdditionAssignment(new Identifier("i"), new Identifier("step")))
                                {
                                    new YieldStatement(new Identifier("i"))
                                }
                            });
                        }

                        case String @string:
                        {
                            var scope = new Scope();
                            scope.Variables.Add("value", new StackVariable("value", @string, scope));

                            return new(new(call, scope), new()
                            {
                                new ForStatement(
                                    initialisation: new Assignment(new Let(new Identifier("i")), new NumberLiteral(0)),
                                    condition:      new Less(new Identifier("i"), new NumberLiteral(@string.Value.Length)),
                                    increment:      new PreIncrement(new Identifier("i")))
                                {
                                    new YieldStatement(new Indexer(new Identifier("value"), new Identifier("i")))
                                }
                            });
                        }

                        case Array array:
                        {
                            var scope = new Scope();
                            scope.Variables.Add("items", new StackVariable("items", array, scope));

                            return new(new(call, scope), new()
                            {
                                new ForStatement(
                                    initialisation: new Assignment(new Let(new Identifier("i")), new NumberLiteral(0)),
                                    condition:      new Less(new Identifier("i"), new NumberLiteral(array.Values.Count)),
                                    increment:      new PreIncrement(new Identifier("i")))
                                {
                                    new YieldStatement(new Indexer(new Identifier("items"), new Identifier("i")))
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

        public override string ToString(int _)
        {
            return "[iter]";
        }
    }
}