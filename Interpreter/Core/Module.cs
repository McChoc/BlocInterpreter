using System.Collections.Generic;
using Bloc.Memory;
using Bloc.Values.Core;

namespace Bloc.Core;

public class Module
{
    public bool Imported { get; set; }

    public string Path { get; set; }

    public Call TopLevelCall { get; }
    public Scope TopLevelScope { get; }

    public Dictionary<string, Value> Exports { get; }

    public Module(string path, Engine engine)
    {
        Path = path;
        TopLevelCall = new(engine, this);
        TopLevelScope = TopLevelCall.Scopes[0];
        Exports = new();
    }
}