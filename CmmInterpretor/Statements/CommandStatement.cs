using CmmInterpretor.Data;
using CmmInterpretor.Results;
using CmmInterpretor.Tokens;
using CmmInterpretor.Values;
using System.Collections.Generic;

namespace CmmInterpretor.Statements
{
    public class CommandStatement : Statement
    {
        public List<List<Token>> commands = new();

        public override IResult Execute(Call call)
        {
            Value output = Void.Value;

            foreach (var command in commands)
            {
                var words = new List<string>();

                foreach (var token in command)
                {
                    var result = GetText(token, call);

                    if (result is not IValue value)
                        return result;

                    words.Add(((String)value.Value).Value);
                }

                string name = words[0];
                string[] args = words.ToArray()[1..];

                if (call.Engine.Commands.TryGetValue(name, out var c))
                    output = c.Call(args, output, call);
                else
                    output = new String("Unknown command.");
            }

            if (output.Implicit(out String str))
                call.Engine.Log(str.Value);

            return Void.Value;
        }

        private static IResult GetText(Token token, Call call)
        {
            if (token.type == TokenType.Command)
                return new String(token.Text);

            if (token.type == TokenType.Literal)
                return (String)token.value;

            if (token.type == TokenType.Interpolated)
            {
                var result = call.Interpolate(token);

                if (result is not IValue value)
                    return result;

                if (!value.Implicit(out String str))
                    return new Throw("Cannot implicitly convert to string");

                return str;
            }

            if (token.type == TokenType.Identifier)
            {
                string identifier = token.Text;

                if (!call.TryGet(identifier, out Variable var))
                    return new Throw($"Variable '{identifier}' was not defined in scope");

                if (!var.Value.Implicit(out String str))
                    return new Throw("Cannot implicitly convert to string");

                return str;
            }

            throw new System.Exception();
        }
    }
}
