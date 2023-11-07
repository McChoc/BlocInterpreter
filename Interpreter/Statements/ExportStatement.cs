using System.Collections.Generic;
using System.Linq;
using Bloc.Expressions;
using Bloc.Identifiers;
using Bloc.Memory;
using Bloc.Pointers;
using Bloc.Results;
using Bloc.Utils.Attributes;
using Bloc.Variables;

namespace Bloc.Statements;

[Record]
internal sealed partial class ExportStatement : Statement
{
    internal List<Export> Exports { get; } = new();

    internal override IEnumerable<IResult> Execute(Call call)
    {
        foreach (var export in Exports)
        {
            try
            {
                var value = export.Expression.Evaluate(call);

                string name;

                if (export.Alias is not null)
                {
                    name = export.Alias.GetName(call);
                }
                else
                {
                    var pointer = value switch
                    {
                        UnresolvedPointer unresolvedPointer => unresolvedPointer.Resolve(),
                        VariablePointer variablePointer => variablePointer,
                        _ => throw new Throw("The expression does not have a name")
                    };

                    name = pointer.Variable switch
                    {
                        StackVariable variable => variable.Name,
                        StructVariable variable => variable.Name,
                        _ => throw new Throw("The expression does not have a name")
                    };
                }

                call.Module.Exports[name] = value.Value;
            }
            catch (Throw exception)
            {
                return new[] { exception };
            }
        }

        return Enumerable.Empty<IResult>();
    }

    internal sealed record Export(IExpression Expression, INamedIdentifier? Alias);
}