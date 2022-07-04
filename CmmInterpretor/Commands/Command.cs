using CmmInterpretor.Memory;
using CmmInterpretor.Values;

namespace CmmInterpretor.Commands
{
    public delegate Value CommandDelegate(string[] command, Value input, Call call);

    public class Command
    {
        private readonly CommandDelegate _function;

        public Command(string name, string description, CommandDelegate function)
        {
            Name = name;
            Description = description;
            _function = function;
        }

        public string Name { get; }
        public string Description { get; }

        public Value Call(string[] command, Value input, Call call) => _function(command, input, call);
    }
}