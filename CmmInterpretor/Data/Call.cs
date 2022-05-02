using CmmInterpretor.Exceptions;
using CmmInterpretor.Extensions;
using CmmInterpretor.Results;
using CmmInterpretor.Tokens;
using CmmInterpretor.Values;
using CmmInterpretor.Variables;
using System.Collections.Generic;

namespace CmmInterpretor.Data
{
    public class Call
    {
        public Engine Engine { get; }

        public Variable Global { get; private set; }
        public Variable This { get; private set; }
        public Variable Recall { get; private set; }
        public Variable Params { get; private set; }

        public Scope Parent { get; }
        public Scope Captures { get; }
        public List<Scope> Scopes { get; }

        public Call(Engine engine, Scope captures = null)
        {
            Engine = engine;
            Scopes = new List<Scope>();
            Captures = captures ?? new Scope(this);
            Push();
        }

        //public void SetGlobal(Scope scope)
        //{
        //    Global = new Variable(null);
        //}

        //public void SetThis(Variable var)
        //{
        //    This = var;
        //}

        //public void SetRecall(Variable var)
        //{
        //    Recall = var;
        //}

        public void SetParams(Array arr)
        {
            Params = new HeapVariable(arr);
        }

        public void Push() => Scopes.Add(new Scope(this));
        public void Pop() => Scopes.RemoveAt(Scopes.Count - 1);

        public bool TryAdd(StackVariable variable)
        {
            if (Scopes[^1].Variables.ContainsKey(variable.Name))
                return false;

            Scopes[^1].Variables.Add(variable.Name, variable);
            return true;
        }

        public bool TryGet (string name, out Variable var)
        {
            for (int i = Scopes.Count - 1; i >= 0; i--)
            {
                if (Scopes[i].Variables.ContainsKey(name))
                {
                    var = Scopes[i].Variables[name];
                    return true;
                }
            }

            if (Captures.Variables.ContainsKey(name))
            {
                var = Captures.Variables[name];
                return true;
            }

            var = null;
            return false;
        }

        public void Set(string name, Variable variable)
        {
            Scopes[^1].Variables[name] = variable;
        }

        public Scope Capture()
        {
            var captures = new Scope(null);

            foreach (var scope in Scopes)
                foreach (var pair in scope.Variables)
                    captures.Variables[pair.Key] = pair.Value;

            return captures;
        }


        public IResult Interpolate(Token token)
        {
            var (text, tokens) = ((string, List<Token>))token.value;

            var strings = new string[tokens.Count];

            for (int i = 0; i < tokens.Count; i++)
            {
                var result = Evaluator.Evaluate((List<Token>)tokens[i].value, this);

                if (result is not IValue value)
                    return result;

                if (!value.Implicit(out String str))
                    return new Throw("Cannot implicitly convert to string");

                strings[i] = str.Value;
            }

            return new String(string.Format(text, strings));
        }

        public IResult Initialize(Token token)
        {
            var text = token.Text;

            var scanner = new TokenScanner(text);
            var tokens = new List<Token>();

            while (scanner.HasNextToken())
                tokens.Add(scanner.GetNextToken());

            if (tokens.Count == 0)
                return Null.Value;
            
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

                    var result = Evaluator.Evaluate(line.GetRange(2..), this);

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

                    var result = Evaluator.Evaluate(line, this);

                    if (result is not IValue value)
                        return result;

                    values.Add(value.Value);
                }

                return new Array(values);
            }
        }
    }
}
