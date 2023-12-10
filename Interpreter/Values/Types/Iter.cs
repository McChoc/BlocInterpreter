using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Bloc.Expressions;
using Bloc.Expressions.Literals;
using Bloc.Expressions.Operators;
using Bloc.Identifiers;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Statements;
using Bloc.Utils.Attributes;
using Bloc.Utils.Helpers;
using Bloc.Values.Core;

namespace Bloc.Values.Types;

[Record]
public sealed partial class Iter : Value
{
    private int _index;

    [DoNotCompare]
    private IEnumerator<IResult> _results;

    private readonly Call _call;
    private readonly List<Statement> _statements;

    [DoNotCompare]
    private readonly Dictionary<string, LabelInfo> _labels;

    internal Iter(Call call, List<Statement> statements)
    {
        _results = Enumerable.Empty<IResult>().GetEnumerator();

        _call = call;
        _statements = statements;
        _labels = StatementHelper.GetLabels(statements);
    }

    public override ValueType GetType() => ValueType.Iter;
    public override string ToString() => "<iter>";

    internal static bool TryImplicitCast(IValue value, [NotNullWhen(true)] out Iter? iter, Call call)
    {
        try
        {
            iter = Construct(new() { value.Value }, call);
            return true;
        }
        catch
        {
            iter = null;
            return false;
        }
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
                        if (++label.JumpCount > _call.Engine.Options.JumpLimit)
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
            _results = Enumerable.Empty<IResult>().GetEnumerator();
        }
    }

    internal static Iter Construct(List<Value> values, Call call)
    {
        switch (values)
        {
            case []:
            case [Null]:
                return new(null!, new());

            case [Iter iter]:
                return iter;

            case [Range range]:
                var statements = new List<Statement>();
                var (start, stop, step) = RangeHelper.GetLoopParameters(range);

                if (step != 0 && !double.IsNaN(step) && !double.IsInfinity(step))
                {
                    statements.Add(new ForStatement(false)
                    {
                        Initialisation = range.Start.Inclusive
                            ? new AssignmentOperator(new LetOperator(new StaticIdentifier("i")), new NumberLiteral(start))
                            : new AssignmentOperator(new LetOperator(new StaticIdentifier("i")), new NumberLiteral(start + step)),
                        Condition = (step, range.Stop.Inclusive) switch
                        {
                            ( < 0, false) => new GreaterThanOperator(new NamedIdentifierExpression(new StaticIdentifier("i")), new NumberLiteral(stop)),
                            ( < 0, true) => new GreaterEqualOperator(new NamedIdentifierExpression(new StaticIdentifier("i")), new NumberLiteral(stop)),
                            (_, false) => new LessThanOperator(new NamedIdentifierExpression(new StaticIdentifier("i")), new NumberLiteral(stop)),
                            (_, true) => new LessEqualOperator(new NamedIdentifierExpression(new StaticIdentifier("i")), new NumberLiteral(stop))
                        },
                        Increment = new AdditionAssignment(new NamedIdentifierExpression(new StaticIdentifier("i")), new NumberLiteral(step)),
                        Statement = new YieldStatement(new NamedIdentifierExpression(new StaticIdentifier("i")))
                    });
                }

                return new Iter(new Call(call, new(), new(), new()), statements);

            case [String]:
            case [Array]:
            case [Struct]:
            case [Tuple]:
                var array = Array.Construct(values, call);
                var @params = new VariableCollection();
                @params.Add(false, "items", array);

                return new Iter(new Call(call, new(), new(), @params), new()
                {
                    new ForStatement(false)
                    {
                        Initialisation = new AssignmentOperator(new LetOperator(new StaticIdentifier("i")), new NumberLiteral(0)),
                        Condition = new LessThanOperator(new NamedIdentifierExpression(new StaticIdentifier("i")), new NumberLiteral(array.Values.Count)),
                        Increment = new IncrementPrefix(new NamedIdentifierExpression(new StaticIdentifier("i"))),
                        Statement = new YieldStatement(new IndexerOperator(new NamedIdentifierExpression(new StaticIdentifier("items")), new()
                        {
                            new IndexerOperator.Argument(new NamedIdentifierExpression(new StaticIdentifier("i")), false)
                        }))
                    }
                });

            case [_]:
                throw new Throw($"'iter' does not have a constructor that takes a '{values[0].GetTypeName()}'");

            default:
                throw new Throw($"'iter' does not have a constructor that takes {values.Count} arguments");
        }
    }
}