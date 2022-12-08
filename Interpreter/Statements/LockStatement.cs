using System.Collections.Generic;
using System.Linq;
using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Pointers;
using Bloc.Results;
using Bloc.Utils;
using Bloc.Variables;

namespace Bloc.Statements
{
    internal sealed class LockStatement : Statement
    {
        internal IExpression Expression { get; set; } = null!;
        internal List<Statement> Statements { get; set; } = null!;

        internal override IEnumerable<Result> Execute(Call call)
        {
            var (value, exception) = EvaluateExpression(Expression, call);

            if (exception is not null)
            {
                yield return exception;
                yield break;
            }

            Variable variable;

            switch (value)
            {
                case UnresolvedPointer pointer:
                    variable = pointer.Resolve().Variable!;
                    break;

                case VariablePointer { Variable: not null } pointer:
                    variable = pointer.Variable;
                    break;

                case SlicePointer { Variables: not null }:
                    yield return new Throw("You cannot lock a slice");
                    yield break;

                case VariablePointer or SlicePointer:
                    yield return new Throw("Invalid reference");
                    yield break;

                default:
                    yield return new Throw("You can only lock a variable");
                    yield break;
            }

            var labels = StatementUtil.GetLabels(Statements);

            lock (variable)
            {
                try
                {
                    call.Push();

                    foreach (var result in ExecuteBlock(Statements, labels, call))
                    {
                        yield return result;

                        if (result is not Yield)
                            yield break;
                    }
                }
                finally
                {
                    call.Pop();
                }
            }
        }

        public override int GetHashCode()
        {
            return System.HashCode.Combine(Label, Expression, Statements.Count);
        }

        public override bool Equals(object other)
        {
            return other is LockStatement statement &&
                Label == statement.Label &&
                Expression.Equals(statement.Expression) &&
                Statements.SequenceEqual(statement.Statements);
        }
    }
}