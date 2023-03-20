using System.Collections.Generic;
using Bloc.Memory;
using Bloc.Values;

namespace Bloc.Expressions.SubExpressions;

internal interface IMember
{
    IEnumerable<(string, Value)> GetMembers(Call call);
}