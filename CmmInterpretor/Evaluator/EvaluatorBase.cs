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
            EvaluateRanges,
            EvaluateExponentials,
            EvaluateMultiplicatives,
            EvaluateAdditives,
            EvaluateShifts,
            EvaluateComparisons,
            EvaluateRelations,
            EvaluateEqualities,
            EvaluateLogicalANDs,
            EvaluateLogicalXORs,
            EvaluateLogicalORs,
            EvaluateConditionalANDs,
            EvaluateConditionalXORs,
            EvaluateConditionalORs,
            EvaluateTernaries,
            EvaluateAssignments,
            EvaluateFunctions,
            EvaluateTuples
        };

        public static IResult Evaluate(List<Token> expression, Call call) =>
            Evaluate(expression, call, operations.Length - 1);

        private static IResult Evaluate(List<Token> expression, Call call, int precedence) =>
            operations[precedence](expression, call, precedence);
    }
}
