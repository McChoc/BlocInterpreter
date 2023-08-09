using System;
using Bloc.Values.Core;
using String = Bloc.Values.Types.String;

namespace Bloc.Results;

public sealed class Throw : Exception, IResult
{
    public Value Value { get; }

    public Throw(Value value) => Value = value;

    public Throw(string text) => Value = new String(text);
}