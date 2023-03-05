using System.Collections.Generic;
using Bloc.Statements;

namespace Bloc.Utils.Helpers;

internal static class StatementHelper
{
    internal static Dictionary<string, Label> GetLabels(List<Statement> statements)
    {
        var labels = new Dictionary<string, Label>();

        for (var i = 0; i < statements.Count; i++)
            if (statements[i].Label is not null)
                labels.Add(statements[i].Label!, new(i));

        return labels;
    }
}