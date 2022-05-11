using CmmInterpretor.Data;
using CmmInterpretor.Exceptions;
using CmmInterpretor.Extensions;
using CmmInterpretor.Results;
using CmmInterpretor.Tokens;
using CmmInterpretor.Values;
using System.Collections.Generic;

namespace CmmInterpretor
{
    public static partial class Evaluator
    {
        private static readonly System.Func<List<Token>, Call, int, IResult>[] operations = new System.Func<List<Token>, Call, int, IResult>[]
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

        public static IResult Interpolate(Token token, Call call)
        {
            var (text, tokens) = ((string, List<Token>))token.value;

            var strings = new string[tokens.Count];

            for (int i = 0; i < tokens.Count; i++)
            {
                var result = Evaluate((List<Token>)tokens[i].value, call);

                if (result is not IValue value)
                    return result;

                if (!value.Implicit(out String str))
                    return new Throw("Cannot implicitly convert to string");

                strings[i] = str.Value;
            }

            return new String(string.Format(text, strings));
        }

        public static IResult Initialize(Token token, Call call)
        {
            var text = token.Text;

            var scanner = new TokenScanner(text);
            var tokens = new List<Token>();

            while (scanner.HasNextToken())
                tokens.Add(scanner.GetNextToken());

            if (tokens.Count == 0)
                throw new SyntaxError("Ambiguous literal. To create an empty array or an empty struct use a default constructor.");

            var lines = tokens.Split(Token.Comma);

            if (lines.Count > 0 && lines[^1].Count == 0)
                lines.RemoveAt(lines.Count - 1);

            if (tokens.Count >= 2 && tokens[0].type == TokenType.Identifier && tokens[1] is { type: TokenType.Operator, value: "=" })
            {
                var values = new Dictionary<string, IValue>();

                foreach (var line in lines)
                {
                    if (line.Count <= 0 || line[0].type != TokenType.Identifier)
                        throw new SyntaxError("Missing identifier");

                    var name = line[0].Text;

                    if (line.Count <= 1 || line[1] is not { type: TokenType.Operator, value: "=" })
                        throw new SyntaxError("Missing =");

                    if (line.Count <= 2)
                        throw new SyntaxError("Missing expression");

                    var result = Evaluate(line.GetRange(2..), call);

                    if (result is not IValue value)
                        return result;

                    values.Add(name, value.Value);
                }

                return new Struct(values);
            }
            else
            {
                var values = new List<IValue>();

                foreach (var line in lines)
                {
                    if (line.Count <= 0)
                        throw new SyntaxError("Missing expression");

                    var result = Evaluator.Evaluate(line, call);

                    if (result is not IValue value)
                        return result;

                    values.Add(value.Value);
                }

                return new Array(values);
            }
        }
    }
}
