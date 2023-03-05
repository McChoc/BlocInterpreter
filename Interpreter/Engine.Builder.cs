using System;
using System.Collections.Generic;
using Bloc.Commands;

namespace Bloc;

public sealed partial class Engine
{
    public sealed class Builder
    {
        private int _hopLimit = 1000;
        private int _jumpLimit = 1000;
        private int _loopLimit = 1000;
        private int _stackLimit = 1000;

        private Action<string> _log = _ => { };
        private Action _clear = () => { };
        private Action _exit = () => { };

        private readonly Dictionary<string, CommandInfo> _commands = new();

        public Builder(params string[] _) { }

        public Builder SetHopLimit(int limit)
        {
            _hopLimit = limit;
            return this;
        }

        public Builder SetJumpLimit(int limit)
        {
            _jumpLimit = limit;
            return this;
        }

        public Builder SetLoopLimit(int limit)
        {
            _loopLimit = limit;
            return this;
        }

        public Builder SetStackLimit(int limit)
        {
            _stackLimit = limit;
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

        public Builder AddCommand(CommandInfo command)
        {
            _commands.Add(command.Name, command);
            return this;
        }

        public Engine Build()
        {
            return new Engine()
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