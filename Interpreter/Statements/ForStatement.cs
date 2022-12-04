using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Utils;
using Bloc.Values;

namespace Bloc.Statements
{
    internal sealed class ForStatement : Statement, IEnumerable
    {
        private bool @checked;
        internal override bool Checked
        {
            get => @checked;
            set => @checked = value;
        }

        internal IExpression? Initialisation { get; set; }
        internal IExpression? Condition { get; set; }
        internal IExpression? Increment { get; set; }
        internal List<Statement> Statements { get; set; }

        internal ForStatement(IExpression? initialisation, IExpression? condition, IExpression? increment)
        {
            Initialisation = initialisation;
            Condition = condition;
            Increment = increment;
            Statements = new();
        }

        internal override IEnumerable<Result> Execute(Call call)
        {
            try
            {
                call.Push();

                if (Initialisation is not null)
                {
                    var (_, exception) = EvaluateExpression(Initialisation, call);
                    
                    if (exception is not null)
                    {
                        yield return exception;
                        yield break;
                    }
                }

                var loopCount = 0;
                var labels = StatementUtil.GetLabels(Statements);

                while (true)
                {
                    if (Condition is not null)
                    {
                        var (value, exception) = EvaluateExpression(Condition, call);

                        if (exception is not null)
                        {
                            yield return exception;
                            yield break;
                        }

                        if (!Bool.TryImplicitCast(value!.Value, out var @bool))
                        {
                            yield return new Throw("Cannot implicitly convert to bool");
                            yield break;
                        }

                        if (!@bool.Value)
                            break;
                    }

                    if (Checked && ++loopCount > call.Engine.LoopLimit)
                    {
                        yield return new Throw("The loop limit was reached.");
                        yield break;
                    }

                    try
                    {
                        call.Push();

                        foreach (var result in ExecuteBlock(Statements, labels, call))
                        {
                            switch (result)
                            {
                                case Continue:
                                    goto Continue;

                                case Break:
                                    goto Break;

                                case Yield:
                                    yield return result;
                                    break;

                                default:
                                    yield return result;
                                    yield break;
                            }
                        }
                    }
                    finally
                    {
                        call.Pop();
                    }

                Continue:
                    if (Increment is not null)
                    {
                        var (_, exception) = EvaluateExpression(Increment, call);

                        if (exception is not null)
                        {
                            yield return exception;
                            yield break;
                        }
                    }
                }

            Break:;
            }
            finally
            {
                call.Pop();
            }
        }

        public override int GetHashCode()
        {
            return System.HashCode.Combine(Label, Checked, Initialisation, Condition, Increment, Statements.Count);
        }

        public override bool Equals(object other)
        {
            return other is ForStatement statement &&
                Label == statement.Label &&
                Equals(Initialisation, statement.Initialisation) &&
                Equals(Condition, statement.Condition) &&
                Equals(Increment, statement.Increment) &&
                Statements.SequenceEqual(statement.Statements);
        }

        IEnumerator IEnumerable.GetEnumerator() => Statements.GetEnumerator();
        internal void Add(Statement statement) => Statements.Add(statement);
        internal void Add(List<Statement> statements) => Statements.AddRange(statements);
    }
}