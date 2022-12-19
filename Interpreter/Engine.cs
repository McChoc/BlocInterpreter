using System;
using System.Collections.Generic;
using System.Linq;
using Bloc.Commands;
using Bloc.Expressions;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Scanners;
using Bloc.Statements;
using Bloc.Utils;
using Bloc.Values;

namespace Bloc
{
    public class Engine
    {
        internal Dictionary<string, Command> Commands { get; set; } = null!;

        public Action<string> Log { get; private set; } = null!;
        public Action Clear { get; private set; } = null!;
        public Action Exit { get; private set; } = null!;

        internal int StackLimit { get; set; }
        internal int LoopLimit { get; set; }
        internal int JumpLimit { get; set; }
        internal int HopLimit { get; set; }

        public Call GlobalCall { get; }
        public Scope GlobalScope { get; }

        private Engine()
        {
            GlobalCall = new Call(this);
            GlobalScope = GlobalCall.Scopes.First();
        }

        public static void Compile(string code, out IExpression? expression, out List<Statement> statements)
        {
            statements = StatementScanner.GetStatements(code);

            if (statements.Count == 1 && statements[0] is ExpressionStatement statement)
                expression = statement.Expression;
            else
                expression = null;
        }

        public Result? Evaluate(IExpression expression, out Value? value)
        {
            try
            {
                value = expression.Evaluate(GlobalCall).Value.Copy();
                return null;
            }
            catch (Result result)
            {
                value = null;
                return result;
            }
        }

        public Result? Execute(List<Statement> statements)
        {
            var labels = StatementUtil.GetLabels(statements);

            for (var i = 0; i < statements.Count; i++)
            {
                switch (statements[i].Execute(GlobalCall).FirstOrDefault())
                {
                    case Continue:
                        throw new Throw("A continue statement can only be used inside a loop");

                    case Break:
                        throw new Throw("A break statement can only be used inside a loop");

                    case Yield:
                        throw new Throw("A yield statement can only be used inside a generator");

                    case Return:
                        throw new Throw("A return statement can only be used inside a function");

                    case Throw @throw:
                        return @throw;

                    case Goto @goto:
                        if (labels.TryGetValue(@goto.Label, out var label))
                        {
                            if (++label.Count > JumpLimit)
                                return new Throw("The jump limit was reached.");

                            i = label.Index - 1;

                            continue;
                        }

                        return new Throw($"Label '{@goto.Label}' does not exist in scope.");
                }
            }

            return null;
        }

        public class Builder
        {
            private readonly Dictionary<string, Command> _commands = new();

            private Action<string> _log = _ => { };
            private Action _clear = () => { };
            private Action _exit = () => { };

            private int _hopLimit = 1000;
            private int _jumpLimit = 1000;
            private int _loopLimit = 1000;
            private int _stackLimit = 1000;

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

            public Builder OnExit(Action actionn)
            {
                _exit = actionn;
                return this;
            }

            public Builder AddCommand(Command command)
            {
                _commands.Add(command.Name, command);
                return this;
            }

            public Engine Build()
            {
                return new()
                {
                    Commands = _commands,
                    Log = _log,
                    Clear = _clear,
                    Exit = _exit,
                    StackLimit = _stackLimit,
                    LoopLimit = _loopLimit,
                    JumpLimit = _jumpLimit,
                    HopLimit = _hopLimit
                };
            }
        }
    }
}