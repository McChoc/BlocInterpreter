using System;
using System.Collections.Generic;
using System.Linq;
using Bloc.Expressions;
using Bloc.Expressions.Literals;
using Bloc.Expressions.Operators;
using Bloc.Identifiers;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Statements;
using Bloc.Utils.Helpers;
using Bloc.Values.Core;
using ValueType = Bloc.Values.Core.ValueType;

namespace Bloc.Values.Types;

public sealed class Iter : Value
{
    private int _index;
    private IEnumerator<IResult> _results;

    private readonly Call _call;
    private readonly List<Statement> _statements;
    private readonly Dictionary<string, Label> _labels;

    internal Iter(Call call, List<Statement> statements)
    {
        _results = Enumerable.Empty<IResult>().GetEnumerator();

        _call = call;
        _statements = statements;
        _labels = StatementHelper.GetLabels(statements);
    }

    internal override ValueType GetType() => ValueType.Iter;

    internal static Iter Construct(List<Value> values, Call call)
    {
        switch (values)
        {
            case []:
            case [Null]:
                return new(null!, new());

            case [Iter iter]:
                return iter;

            case [String @string]:
                {
                    var @params = new VariableCollection();
                    @params.Add(new(false, "value", @string, @params));

                    return new(new(call, new(), @params), new()
                {
                    new ForStatement(false)
                    {
                        Initialisation = new AssignmentOperator(new LetOperator(new NameIdentifier("i")), new NumberLiteral(0)),
                        Condition = new LessThanOperator(new IdentifierExpression("i"), new NumberLiteral(@string.Value.Length)),
                        Increment = new IncrementPrefix(new IdentifierExpression("i")),
                        Statement = new YieldStatement(new IndexerOperator(new IdentifierExpression("value"), new IdentifierExpression("i")))
                    }
                });
                }

            case [Array array]:
                {
                    var @params = new VariableCollection();
                    @params.Add(new(false, "items", array, @params));

                    return new(new(call, new(), @params), new()
                {
                    new ForStatement(false)
                    {
                        Initialisation = new AssignmentOperator(new LetOperator(new NameIdentifier("i")), new NumberLiteral(0)),
                        Condition = new LessThanOperator(new IdentifierExpression("i"), new NumberLiteral(array.Values.Count)),
                        Increment = new IncrementPrefix(new IdentifierExpression("i")),
                        Statement = new YieldStatement(new IndexerOperator(new IdentifierExpression("items"), new IdentifierExpression("i")))
                    }
                });
                }

            case [Range range]:
                {
                    var (start, end, step) = RangeHelper.Deconstruct(range);

                    var @params = new VariableCollection();
                    @params.Add(new(false, "start", new Number(start), @params));
                    @params.Add(new(false, "end", new Number(end), @params));
                    @params.Add(new(false, "step", new Number(step), @params));

                    return new(new(call, new(), @params), new()
                {
                    new ForStatement(false)
                    {
                        Initialisation = new AssignmentOperator(new LetOperator(new NameIdentifier("i")), new IdentifierExpression("start")),
                        Condition = new LessThanOperator(
                            new MultiplicationOperator(new IdentifierExpression("i"), new IdentifierExpression("step")),
                            new MultiplicationOperator(new IdentifierExpression("end"), new IdentifierExpression("step"))),
                        Increment = new AdditionAssignment(new IdentifierExpression("i"), new IdentifierExpression("step")),
                        Statement = new YieldStatement(new IdentifierExpression("i"))
                    }
                });
                }

            case [_]:
                throw new Throw($"'iter' does not have a constructor that takes a '{values[0].GetTypeName()}'");

            default:
                throw new Throw($"'iter' does not have a constructor that takes {values.Count} arguments");
        }
    }

    internal static bool TryImplicitCast(IValue value, out Iter iter, Call call)
    {
        try
        {
            iter = Construct(new() { value.Value }, call);
            return true;
        }
        catch
        {
            iter = null!;
            return false;
        }
    }

    public override string ToString()
    {
        return "[iter]";
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(_index, _statements.Count);
    }

    public override bool Equals(object other)
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
                        if (++label.Count > _call.Engine.Options.JumpLimit)
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
}