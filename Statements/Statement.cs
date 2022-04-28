using CmmInterpretor.Data;
using CmmInterpretor.Results;
using CmmInterpretor.Values;
using System.Collections.Generic;

namespace CmmInterpretor.Statements
{
    public abstract class Statement
    {
        public abstract IResult Execute(Call call);

        protected static IResult ExecuteBlock(List<Statement> statements, Call call)
        {
            var labels = GetLabels(statements);

            try
            {
                call.Push();

                for (int i = 0; i < statements.Count; i++)
                {
                    var result = statements[i].Execute(call); ;

                    if (result is not IValue)
                    {
                        if (result is Goto g)
                        {
                            if (labels.TryGetValue(g.label, out int index))
                                i = index - 1;
                            else
                                return result;
                        }
                        else
                        {
                            return result;
                        }
                    }
                }
            }
            finally
            {
                call.Pop();
            }

            return new Void();
        }

        protected static IResult ExecuteBlockInLoop(List<Statement> statements, Dictionary<string, int> labels, Call call)
        {
            for (int i = 0; i < statements.Count; i++)
            {
                var result = statements[i].Execute(call);

                if (result is not IValue)
                {
                    if (result is Goto g)
                    {
                        if (labels.TryGetValue(g.label, out int index))
                            i = index - 1;
                        else
                            return result;
                    }
                    else
                    {
                        return result;
                    }
                }
            }

            return new Void();
        }

        protected static Dictionary<string, int> GetLabels(List<Statement> statements)
        {
            var labels = new Dictionary<string, int>();

            for (int i = 0; i < statements.Count; i++)
                if (statements[i] is LabelStatement lbl)
                    labels.Add(lbl.Label, i);

            return labels;
        }
    }
}
