using System.Collections.Generic;
using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Pointers;
using Bloc.Results;
using Bloc.Utils;

namespace Bloc.Statements
{
    internal sealed record LockStatement : Statement
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

            switch (value)
            {
                case VariablePointer { Variable: not null } pointer:

                    var labels = StatementUtil.GetLabels(Statements);

                    lock (pointer.Variable)
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
                    break;

                case SlicePointer { Variables: not null }:
                    yield return new Throw("You cannot lock a slice");
                    yield break;

                case VariablePointer or SlicePointer:
                    yield return new Throw("Invalid reference");
                    yield break;

                case UndefinedPointer pointer:
                    yield return new Throw($"Variable {pointer.Name} was not defined in scope");
                    yield break;

                default:
                    yield return new Throw("You can only lock a variable");
                    yield break;
            }
        }
    }
}