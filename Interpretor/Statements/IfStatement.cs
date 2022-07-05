using System.Collections.Generic;
using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Values;

namespace Bloc.Statements
{
    internal class IfStatement : Statement
    {
        internal IExpression Condition { get; set; } = default!;
        internal List<Statement> IfBody { get; set; } = default!;
        internal List<Statement> ElseBody { get; set; } = new();

        internal override Result? Execute(Call call)
        {
            try
            {
                var value = Condition.Evaluate(call);

                if (!value.Is(out Bool? @bool))
                    return new Throw("Cannot implicitly convert to bool");

                if (@bool!.Value)
                    return ExecuteBlock(IfBody, call);
                return ExecuteBlock(ElseBody, call);
            }
            catch (Result result)
            {
                return result;
            }
        }
    }
}