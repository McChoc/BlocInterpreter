using System.Collections.Generic;
using Bloc.Exceptions;
using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Values;

namespace Bloc.Statements
{
    public abstract class Statement
    {
        internal string? Label { get; set; }
        internal virtual bool Checked
        {
            get => throw new System.NotImplementedException();
            set {
                if (!value)
                    throw new SyntaxError(0, 0, "unchecked is not valid for this statement");
            }
        }

        internal abstract IEnumerable<Result> Execute(Call call);

        public abstract override int GetHashCode();

        public abstract override bool Equals(object other);

        public static bool operator ==(Statement a, Statement b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(Statement a, Statement b)
        {
            return !a.Equals(b);
        }

        private protected static (IValue?, Throw?) EvaluateExpression(IExpression expression, Call call)
        {
            IValue? value = null;
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