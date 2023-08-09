using Bloc.Memory;
using Bloc.Values.Core;

namespace Bloc.Commands;

public interface ICommandInfo
{
    string Name { get; }
    string Description { get; }
    Value Call(string[] args, Value input, Call call);
}
