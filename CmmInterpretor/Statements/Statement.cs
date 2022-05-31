using CmmInterpretor.Memory;
using CmmInterpretor.Results;
using System.Collections.Generic;

namespace CmmInterpretor.Statements
{
    internal abstract class Statement
    {
        internal string? Label { get; set; }

        internal abstract Result? Execute(Call call);

        private protected static Result? ExecuteBlock(List<Statement> statements, Call call)
        {
            var labels = GetLabels(statements);

            try
            {
                call.Push();

                for (int i = 0; i < statements.Count; i++)
                {
                    var result = statements[i].Execute(call);

                    if (result is Goto g)
                    {
                        if (labels.TryGetValue(g.label, out int index))
                            i = index - 1;
                        else
                            return result;
                    }
                    else if (result is not null)
                    {
                        return result;
                    }
                }
            }
            finally
            {
                call.Pop();
            }

            return null;
        }

        private protected static Result? ExecuteBlockInLoop(List<Statement> statements, Dictionary<string, int> labels, Call call)
        {
            for (int i = 0; i < statements.Count; i++)
            {
                var result = statements[i].Execute(call);

                if (result is Goto g)
                {
                    if (labels.TryGetValue(g.label, out int index))
                        i = index - 1;
                    else
                        return result;
                }
                else if (result is not null)
                {
                    return result;
                }
            }

            return null;
        }

        private protected static Dictionary<string, int> GetLabels(List<Statement> statements)
        {
            var labels = new Dictionary<string, int>();

            for (int i = 0; i < statements.Count; i++)
                if (statements[i].Label is not null)
                    labels.Add(statements[i].Label!, i);

            return labels;
        }
    }
}
