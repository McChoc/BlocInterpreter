using CmmInterpretor.Memory;
using CmmInterpretor.Results;
using System.Collections.Generic;

namespace CmmInterpretor.Statements
{
    public abstract class Statement
    {
        public string? Label { get; set; }

        public abstract Result? Execute(Call call);

        protected static Result? ExecuteBlock(List<Statement> statements, Call call)
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

        protected static Result? ExecuteBlockInLoop(List<Statement> statements, Dictionary<string, int> labels, Call call)
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

        protected static Dictionary<string, int> GetLabels(List<Statement> statements)
        {
            var labels = new Dictionary<string, int>();

            for (int i = 0; i < statements.Count; i++)
                if (statements[i].Label is not null)
                    labels.Add(statements[i].Label!, i);

            return labels;
        }
    }
}
