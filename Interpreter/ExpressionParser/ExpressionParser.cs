using System.Collections.Generic;
using Bloc.Expressions;
using Bloc.Tokens;

namespace Bloc;

internal static partial class ExpressionParser
{
    private delegate IExpression ParseCallback(List<Token> tokens, int precedence);

    private static readonly ParseCallback[] operations =
    {
        ParseAtoms,
        ParsePrimaries,
        ParseUnaries,
        ParseRanges,
        ParseQueries,
        ParseExponentiations,
        ParseMultiplicatives,
        ParseAdditives,
        ParseShifts,
        ParseComparisons,
        ParseRelations,
        ParseEqualities,
        ParseBitwiseANDs,
        ParseBitwiseXORs,
        ParseBitwiseORs,
        ParseBooleanANDs,
        ParseBooleanXORs,
        ParseBooleanORs,
        ParseCatches,
        ParseCoalescings,
        ParseConditionals,
        ParseAssignments,
        ParseFunctions,
        ParseTuples
    };

    internal static IExpression Parse(List<Token> expression)
    {
        return Parse(expression, operations.Length - 1);
    }

    private static IExpression Parse(List<Token> expression, int precedence)
    {
        return operations[precedence](expression, precedence);
    }
}