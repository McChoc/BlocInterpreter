using System.Collections.Generic;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Utils;

namespace Bloc.Statements
{
    public abstract class Statement
    {
        internal string? Label { get; set; }

        internal abstract Result? Execute(Call call);

        private protected static Result? ExecuteBlock(List<Statement> statements, Dictionary<string, Label> labels, Call call)
        {
            for (var i = 0; i < statements.Count; i++)
            {
                var result = statements[i].Execute(call);

                if (result is Goto g)
                {
                    if (labels.TryGetValue(g.Label, out var label))
                    {
                        label.Count++;

                        if (label.Count > call.Engine.JumpLimit)
                            return new Throw("The jump limit was reached.");

                        i = label.Index - 1;

                        continue;
                    }

                    return result;
                }

                if (result is not null)
                    return result;
            }

            return null;
        }
    }
}