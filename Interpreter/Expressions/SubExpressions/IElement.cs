using System.Collections.Generic;
using Bloc.Memory;
using Bloc.Values;

namespace Bloc.Expressions.SubExpressions;

internal interface IElement
{
    IEnumerable<Value> GetElements(Call call);
}