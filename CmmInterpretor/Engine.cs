using CmmInterpretor.Commands;
using CmmInterpretor.Data;
using CmmInterpretor.Exceptions;
using CmmInterpretor.Results;
using CmmInterpretor.Statements;
using CmmInterpretor.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using Void = CmmInterpretor.Values.Void;

namespace CmmInterpretor
{
    public class Engine
    {
        internal Dictionary<string, Command> Commands { get; set; }

        internal Action<string> Log { get; set; }
        internal Action Clear { get; set; }

        internal int StackLimit { get; set; }
        internal int LoopLimit { get; set; }
        internal int JumpLimit { get; set; }
        internal int HopLimit { get; set; }

        internal Scope Global { get; }

        private Engine()
        {
            var root = new Call(this);
            Global = root.Scopes.First();
        }

        public IResult Execute(string code)
        {
            var labels = new Dictionary<string, int>();
            var statements = new List<Statement>();

            var statementScanner = new StatementScanner(new TokenScanner(code));

            for (int i = 0; statementScanner.HasNextStatement(); i++)
            {
                var statement = statementScanner.GetNextStatement();

                statements.Add(statement);

                if (statement.Label is not null)
                    labels.Add(statement.Label, i);
            }

            Value output = null;

            for (int i = 0; i < statements.Count; i++)
            {
                var result = statements[i].Execute(Global.Call);

                if (result is IValue value)
                {
                    output = value.Value;
                }
                else if (result is Goto g)
                {
                    if (labels.TryGetValue(g.label, out int index))
                        i = index - 1;
                    else
                        throw new SyntaxError($"Label '{g.label}' does not exist in scope.");
                }
                else if (result is Continue || result is Break)
                {
                    throw new SyntaxError("No loop");
                }
                else
                {
                    return result;
                }
            }

            return statements.Count == 1 ? output : Void.Value;
        }

        public class Builder
        {
            private readonly Dictionary<string, Command> _commands = new();

            private Action<string> _log = _ => { };
            private Action _clear = () => { };

            private int _stackLimit = 1000;
            private int _loopLimit = 1000;
            private int _jumpLimit = 1000;
            private int _hopLimit = 100;

            public Builder(params string[] _) { }

            public Builder SetStackLimit(int limit)
            {
                _stackLimit = limit;
                return this;
            }

            public Builder SetLoopLimit(int limit)
            {
                _loopLimit = limit;
                return this;
            }

            public Builder SetJumpLimit(int limit)
            {
                _jumpLimit = limit;
                return this;
            }

            public Builder SetHopLimit(int limit)
            {
                _hopLimit = limit;
                return this;
            }

            public Builder OnLog(Action<string> action)
            {
                _log = action;
                return this;
            }

            public Builder OnClear(Action action)
            {
                _clear = action;
                return this;
            }

            public Builder AddCommand(Command command)
            {
                _commands.Add(command.Name, command);
                return this;
            }

            public Engine Build() => new()
            {
                Commands = _commands,
                Log = _log,
                Clear = _clear,
                StackLimit = _stackLimit,
                LoopLimit = _loopLimit,
                JumpLimit = _jumpLimit,
                HopLimit = _hopLimit
            };
        }
    }
}
