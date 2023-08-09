using System.Collections.Generic;
using Bloc.Memory;
using Bloc.Values.Core;

namespace Bloc.Expressions.Literals.StructMembers;

internal interface IMember
{
    IEnumerable<(string, Value)> GetMembers(Call call);
}