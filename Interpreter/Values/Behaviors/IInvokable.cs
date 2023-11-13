using System.Collections.Generic;
using Bloc.Memory;
using Bloc.Values.Core;

namespace Bloc.Values.Behaviors;

internal interface IInvokable
{
    Value Invoke(List<Value?> args, Dictionary<string, Value> kwargs, Call call);
}