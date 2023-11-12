using System.Collections.Generic;
using Bloc.Memory;
using Bloc.Values.Core;

namespace Bloc.Values.Behaviors;

internal interface IIndexable
{
    IValue Index(List<Value> args, Call call);
}