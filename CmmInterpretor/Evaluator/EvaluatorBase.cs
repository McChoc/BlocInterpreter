using CmmInterpretor.Data;
using CmmInterpretor.Results;
using CmmInterpretor.Tokens;
using System;
using System.Collections.Generic;

namespace CmmInterpretor
{
    public static partial class Evaluator
    {
        private static readonly Func<List<Token>, Call, int, IResult>[] operations = new Func<List<Token>, Call, int, IResult>[]
        {
            EvaluatePrimaries,
            EvaluateUnaries,
            EvaluateExponentials,
            EvaluateMultiplicatives,
            EvaluateAdditives,
            EvaluateShifts,
            EvaluateLogicalANDs,
            EvaluateLogicalXORs,
            EvaluateLogicalORs,
            EvaluateComparisons,
            EvaluateRanges,
            EvaluateRelations,
            EvaluateEqualities,
            EvaluateConditionalANDs,
            EvaluateConditionalXORs,
            EvaluateConditionalORs,
            EvaluateTernaries,
            EvaluateAssignments,
            EvaluateTuples,
            EvaluateFunctions
        };

        public static IResult Evaluate(List<Token> expression, Call call) =>
            Evaluate(expression, call, operations.Length - 1);

        private static IResult Evaluate(List<Token> expression, Call call, int precedence) =>
            operations[precedence](expression, call, precedence);
    }
}
