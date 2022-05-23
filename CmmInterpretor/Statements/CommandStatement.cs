using CmmInterpretor.Memory;
using CmmInterpretor.Results;
using CmmInterpretor.Tokens;
using CmmInterpretor.Values;
using CmmInterpretor.Variables;
using System.Collections.Generic;

namespace CmmInterpretor.Statements
{
    public class CommandStatement : Statement
    {
        public List<List<Token>> Commands { get; } = new();

        public override Result? Execute(Call call)
        {
            Value output = Void.Value;

            foreach (var command in Commands)
            {
                var words = new List<string>();

                foreach (var token in command)
                {
                    try
                    {
                        words.Add(GetText(token, call));
                    }
                    catch (Result result)
                    {
                        return result;
                    }
                }

                string name = words[0];
                string[] args = words.ToArray()[1..];

                if (call.Engine.Commands.TryGetValue(name, out var c))
                    output = c.Call(args, output, call);
                else
                    output = new String("Unknown command.");
            }

            if (output.Is(out String? str))
                call.Engine.Log(str!.Value);

            return null;
        }

        private static string GetText(Token token, Call call)
        {
            if (token.type == TokenType.Command)
                return token.Text;

            if (token.type == TokenType.Literal)
                return ((String)token.value).Value;

            if (token.type == TokenType.Interpolated)
            {
                var expression = ExpressionParser.ParseInterpolatedString(token);

                var value = expression.Evaluate(call);

                return ((String)value).Value;
            }

            if (token.type == TokenType.Identifier)
            {
                string identifier = token.Text;

                if (!call.TryGet(identifier, out Variable? variable))
                    throw new Throw($"Variable '{identifier}' was not defined in scope");

                if (!variable!.Value.Is(out String? str))
                    throw new Throw("Cannot implicitly cast to string");

                return str!.Value;
            }

            throw new System.Exception();
        }
    }
}
