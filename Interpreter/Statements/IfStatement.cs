using System.Collections.Generic;
using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Utils;
using Bloc.Values;

namespace Bloc.Statements
{
    internal class IfStatement : Statement
    {
        internal IExpression Condition { get; set; } = default!;
        internal List<Statement> If { get; set; } = default!;
        internal List<Statement> Else { get; set; } = new();

        internal override Result? Execute(Call call)
        {
            try
            {
                var value = Condition.Evaluate(call);

                if (!value.Value.Is(out Bool? @bool))
                    return new Throw("Cannot implicitly convert to bool");

                var statements = @bool!.Value ? If : Else;
                var labels = StatementUtil.GetLabels(statements);

                call.Push();
                var result = ExecuteBlock(statements, labels, call);
                call.Pop();

                return result;
            }
            catch (Result result)
            {
                return result;
            }
        }
    }
}