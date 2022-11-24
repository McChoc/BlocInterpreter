using System.Collections.Generic;
using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Pointers;
using Bloc.Results;

namespace Bloc.Statements
{
    public abstract record Statement
    {
        internal string? Label { get; set; }

        internal abstract IEnumerable<Result> Execute(Call call);

        private protected static (IPointer?, Throw?) EvaluateExpression(IExpression expression, Call call)
        {
            IPointer? value = null;
            Throw? @throw = null;

            try
            {
                value = expression.Evaluate(call);
            }
            catch (Throw t)
            {
                @throw = t;
            }

            return (value, @throw);
        }

        private protected static IEnumerable<Result> ExecuteBlock(List<Statement> statements, Dictionary<string, Label> labels, Call call)
        {
            for (var i = 0; i < statements.Count; i++)
            {
                foreach (var result in statements[i].Execute(call))
                {
                    switch (result)
                    {
                        case Goto @goto:
                            if (!labels.TryGetValue(@goto.Label, out var label))
                                goto default;

                            if (++label.Count > call.Engine.JumpLimit)
                            {
                                yield return new Throw("The jump limit was reached.");
                                yield break;
                            }

                            i = label.Index - 1;

                            goto Next;

                        case Yield:
                            yield return result;
                            break;

                        default:
                            yield return result;
                            yield break;
                    }
                }

            Next:;
            }
        }
    }
}