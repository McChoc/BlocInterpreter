using System.Collections.Generic;
using Bloc.Memory;
using Bloc.Values.Core;

namespace Bloc.Expressions.Literals.ArrayElements;

internal interface IElement
{
    IEnumerable<Value> GetElements(Call call);
}