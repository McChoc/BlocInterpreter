using Bloc.Memory;
using Bloc.Values;

namespace Bloc.Commands
{
    public delegate Value CommandCallback(string[] command, Value input, Call call);

    public sealed class Command
    {
        private readonly CommandCallback _callback;

        public string Name { get; }
        public string Description { get; }

        public Command(string name, string description, CommandCallback callback)
        {
            Name = name;
            Description = description;
            _callback = callback;
        }

        public Value Call(string[] command, Value input, Call call) => _callback(command, input, call);
    }
}