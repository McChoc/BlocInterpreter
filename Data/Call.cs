using CmmInterpretor.Exceptions;
using CmmInterpretor.Extensions;
using CmmInterpretor.Results;
using CmmInterpretor.Tokens;
using CmmInterpretor.Values;
using System.Collections.Generic;

namespace CmmInterpretor.Data
{
    public class Call
    {
        private Variable _params;

        public Engine Engine { get; }

        public Pointer Global { get; private set; }
        public Pointer This { get; private set; }
        public Pointer Recall { get; private set; }
        public Pointer Params { get; private set; }

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

        public void SetGlobal(Scope scope)
        {
            Global = new Pointer(null, null);
        }

        public void SetThis(Pointer ptr)
        {
            This = ptr;
        }

        public void SetRecall(Pointer ptr)
        {
            Recall = ptr;
        }

        public void SetParams(Array arr)
        {
            _params = new Variable(null, arr, null);
            Params = new Pointer(_params, Engine);
        }

        public void Push() => Scopes.Add(new Scope(this));
        public void Pop() => Scopes.RemoveAt(Scopes.Count - 1);

        public bool TryAdd(Variable variable, out Pointer pointer)
        {
            if (Scopes[^1].Variables.ContainsKey(variable.name))
            {
                pointer = null;
                return false;
            }

            Scopes[^1].Variables.Add(variable.name, variable);
            pointer = new Pointer(variable, Engine);
            return true;
        }

        public bool TryGet (string name, out Pointer pointer)
        {
            for (int i = Scopes.Count - 1; i >= 0; i--)
            {
                if (Scopes[i].Variables.ContainsKey(name))
                {
                    pointer = new Pointer(Scopes[i].Variables[name], Engine);
                    return true;
                }
            }

            if (Captures.Variables.ContainsKey(name))
            {
                pointer = new Pointer(Captures.Variables[name], Engine);
                return true;
            }

            pointer = null;
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

                if (!value.Value().Implicit(out String str))
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
            {
                return new Null();
            }
            else
            {
                var lines = tokens.Split(Token.Comma);

                if (tokens.Count >= 2 && tokens[0].type == TokenType.Identifier && tokens[1].type == TokenType.Operator && tokens[1].Text == "=")
                {
                    var values = new Dictionary<string, Value>();

                    foreach (var line in lines)
                    {
                        if (line.Count <= 0 || line[0].type != TokenType.Identifier)
                            throw new SyntaxError("Missing identifier");

                        var name = line[0].Text;

                        if (line.Count <= 1 || line[1].type != TokenType.Operator || line[1].Text != "=")
                            throw new SyntaxError("Missing =");

                        if (line.Count <= 2)
                            throw new SyntaxError("Missing expression");

                        var result = Evaluator.Evaluate(line.GetRange(2..), this);

                        if (result is not IValue value)
                            return result;

                        values.Add(name, value.Value());
                    }

                    return new Struct(values);
                }
                else
                {
                    var values = new List<Value>();

                    foreach (var line in lines)
                    {
                        if (line.Count <= 0)
                            throw new SyntaxError("Missing expression");

                        var result = Evaluator.Evaluate(line, this);

                        if (result is not IValue value)
                            return result;

                        values.Add(value.Value());
                    }

                    return new Array(values);
                }
            }
        }
    }
}
