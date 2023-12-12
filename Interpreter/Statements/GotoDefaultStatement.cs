﻿using System.Collections.Generic;
using Bloc.Memory;
using Bloc.Results;
using Bloc.Utils.Attributes;

namespace Bloc.Statements;

[Record]
internal sealed partial class GotoDefaultStatement : Statement
{
    internal override IEnumerable<IResult> Execute(Call call)
    {
        yield return new GotoDefault();
    }
}