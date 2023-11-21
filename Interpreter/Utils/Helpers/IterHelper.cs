using System.Collections.Generic;
using Bloc.Core;
using Bloc.Results;
using Bloc.Values.Core;
using Bloc.Values.Types;

namespace Bloc.Utils.Helpers;

internal static class IterHelper
{
    internal static IEnumerable<Value> CheckedIterate(Iter iter, IEngineOptions options)
    {
        int loopCount = 0;

        foreach (var item in iter.Iterate())
        {
            if (++loopCount > options.LoopLimit)
                throw new Throw("The loop limit was reached.");

            yield return item;
        }
    }
}