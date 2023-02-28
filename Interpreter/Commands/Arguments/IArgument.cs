using System.Collections.Generic;
using Bloc.Memory;

namespace Bloc.Commands.Arguments;

internal interface IArgument
{
    IEnumerable<string> GetArguments(Call call);
}