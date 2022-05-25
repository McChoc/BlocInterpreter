using CmmInterpretor.Commands;
using CmmInterpretor.Memory;
using CmmInterpretor.Results;
using CmmInterpretor.Scanners;
using CmmInterpretor.Statements;
using CmmInterpretor.Utils;
using CmmInterpretor.Values;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CmmInterpretor
{
    public class Engine
    {
        internal Dictionary<string, Command> Commands { get; set; } = default!;

        public Action<string> Log { get; private set; } = default!;
        public Action Clear { get; private set; } = default!;

        internal int StackLimit { get; set; }
        internal int LoopLimit { get; set; }
        internal int JumpLimit { get; set; }
        internal int HopLimit { get; set; }

        internal Scope Global { get; }

        private readonly List<Statement> _statements = new();
        private Dictionary<string, int> _labels = new();

        private Engine()
        {
            var root = new Call(this);
            Global = root.Scopes.First();
        }

        public Variant<Value, Result>? Execute(string code)
        {
            var statements = StatementScanner.GetStatements(code);

            int start = _statements.Count;

            _statements.AddRange(statements);
            _labels = GetLabels(_statements);

            if (statements.Count == 1 && statements[0] is ExpressionStatement expression)
                return expression.Evaluate(Global.Call!);

            for (int i = start; i < _statements.Count; i++)
            {
                var result = _statements[i].Execute(Global.Call!);

                if (result is Throw t)
                    return t;

                if (result is Return)
                    return new Throw("No function");

                if (result is Continue || result is Break)
                    return new Throw("No loop");

                if (result is Goto g)
                {
                    if (_labels.TryGetValue(g.label, out int index))
                        i = index - 1;
                    else
                        return new Throw($"Label '{g.label}' does not exist in scope.");
                }
            }

            return null;
        }

        private static Dictionary<string, int> GetLabels(List<Statement> statements)
        {
            var labels = new Dictionary<string, int>();

            for (int i = 0; i < statements.Count; i++)
                if (statements[i].Label is not null)
                    labels.Add(statements[i].Label!, i);

            return labels;
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
