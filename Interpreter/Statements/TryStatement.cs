using System.Collections.Generic;
using System.Linq;
using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Utils;
using Bloc.Values;

namespace Bloc.Statements
{
    internal sealed class TryStatement : Statement
    {
        internal List<Statement> Try { get; set; } = new();
        internal List<Catch> Catches { get; set; } = new();
        internal List<Statement> Finally { get; set; } = new();

        internal override IEnumerable<Result> Execute(Call call)
        {
            Result? result = null;

            {
                var labels = StatementUtil.GetLabels(Try);

                call.Push();

                foreach (var r in ExecuteBlock(Try, labels, call))
                {
                    if (r is not Yield)
                    {
                        result = r;
                        break;
                    }

                    yield return r;
                }

                call.Pop();
            }

            if (result is Throw @throw)
            {
                foreach (var @catch in Catches)
                {
                    bool caugth;

                    call.Push();
                    call.Set(true, true, @catch.Name, @throw.Value);

                    if (@catch.Expression is null)
                    {
                        caugth = true;
                    }
                    else
                    {
                        if (!Bool.TryImplicitCast(@catch.Expression.Evaluate(call).Value, out var @bool))
                        {
                            yield return new Throw("Cannot implicitly convert to bool");
                            yield break;
                        }

                        caugth = @bool.Value;
                    }

                    if (!caugth)
                    {
                        call.Pop();
                    }
                    else
                    {
                        result = null;

                        var labels = StatementUtil.GetLabels(@catch.Statements);

                        foreach (var r in ExecuteBlock(@catch.Statements, labels, call))
                        {
                            if (r is not Yield)
                            {
                                result = r;
                                break;
                            }

                            yield return r;
                        }

                        call.Pop();
                        break;
                    }
                }
            }

            {
                var labels = StatementUtil.GetLabels(Finally);

                call.Push();

                foreach (var r in ExecuteBlock(Finally, labels, call))
                {
                    if (r is not Yield)
                    {
                        result = r;
                        break;
                    }

                    yield return r;
                }

                call.Pop();
            }

            if (result is not null)
                yield return result;
        }

        public override int GetHashCode()
        {
            return System.HashCode.Combine(Label, Try.Count, Catches.Count, Finally.Count);
        }

        public override bool Equals(object other)
        {
            return other is TryStatement statement &&
                Label == statement.Label &&
                Try.SequenceEqual(statement.Try) &&
                Catches.SequenceEqual(statement.Catches) &&
                Finally.SequenceEqual(statement.Finally);
        }

        internal sealed class Catch
        {
            internal string Name { get; set; }

            internal IExpression? Expression { get; set; }

            internal List<Statement> Statements { get; set; }

            internal Catch(string name, IExpression? expression, List<Statement> statements)
            {
                Name = name;
                Expression = expression;
                Statements = statements;
            }

            public override int GetHashCode()
            {
                return System.HashCode.Combine(Name, Expression, Statements.Count);
            }

            public override bool Equals(object other)
            {
                return other is Catch @catch &&
                       Name == @catch.Name &&
                       Equals(Expression, @catch.Expression) &&
                       Statements.SequenceEqual(@catch.Statements);
            }
        }
    }
}