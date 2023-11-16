using System;
using System.Collections.Generic;
using System.Linq;
using Bloc.Core;
using Bloc.Pointers;
using Bloc.Results;
using Bloc.Values.Core;
using Bloc.Values.Types;
using Bloc.Variables;

namespace Bloc.Memory;

public sealed class Call
{
    private readonly int _stack;

    public Engine Engine { get; }
    public Module Module { get; }

    internal VariableCollection ToplvlVariables { get; }
    internal VariableCollection OuterVariables { get; }
    internal VariableCollection ParamVariables { get; }
    internal List<Scope> Scopes { get; }

    internal Call(Engine engine, Module module)
    {
        Engine = engine;
        Module = module;

        Scopes = new();
        MakeScope();

        ToplvlVariables = Scopes[0];
        OuterVariables = new();
        ParamVariables = new();
    }

    internal Call(Call parent, VariableCollection toplvlVariables, VariableCollection outerVariables, VariableCollection paramVariables)
    {
        Engine = parent.Engine;
        Module = parent.Module;

        _stack = parent._stack + 1;

        if (_stack > Engine.Options.StackLimit)
            throw new Throw("The stack limit was reached");

        ToplvlVariables = toplvlVariables;
        OuterVariables = outerVariables;
        ParamVariables = paramVariables;
        Scopes = new();
        MakeScope();
    }

    internal Scope MakeScope()
    {
        return new Scope(this);
    }

    internal void Destroy()
    {
        while (Scopes.Count > 0)
            Scopes[^1].Dispose();
    }

    public UnresolvedPointer Get(string name)
    {
        Stack<StackVariable> stack;

        Variable? local = null;
        Variable? param = null;
        Variable? outer = null;
        Variable? toplvl = null;
        Variable? global = null;

        foreach (var scope in Scopes.Reverse<Scope>())
        {
            if (scope.Variables.TryGetValue(name, out stack))
            {
                local = stack.Peek();
                break;
            }
        }

        if (ParamVariables.Variables.TryGetValue(name, out stack))
            param = stack.Peek();

        if (OuterVariables.Variables.TryGetValue(name, out stack))
            outer = stack.Peek();

        if (ToplvlVariables.Variables.TryGetValue(name, out stack))
            toplvl = stack.Peek();

        if (Engine.GlobalVariables.Variables.TryGetValue(name, out stack))
            global = stack.Peek();

        return new UnresolvedPointer(name, local, param, outer, toplvl, global);
    }

    public VariablePointer Set(string name, Value value, bool mutable, bool mask, VariableScope scope)
    {
        var targetCollection = scope switch
        {
            VariableScope.Global => Engine.GlobalVariables,
            VariableScope.Module => Module.TopLevelScope,
            _ => Scopes[^1],
        };

        if (!mask && targetCollection.Variables.ContainsKey(name))
            throw new Throw($"Variable '{name}' was already defined in scope");

        var variable = targetCollection.Add(mutable, name, value);

        return new VariablePointer(variable);
    }

    internal VariableCollection ValueCapture()
    {
        return Capture(x => x.Value.GetOrCopy(true));
    }

    internal VariableCollection ReferenceCapture()
    {
        return Capture(x => new Reference(new VariablePointer(x)));
    }

    private VariableCollection Capture(Func<StackVariable, Value> callback)
    {
        var captures = new VariableCollection();

        foreach (var scope in Scopes)
            Capture(scope, captures, callback);

        Capture(ParamVariables, captures, callback);
        Capture(OuterVariables, captures, callback);

        return captures;
    }

    private static void Capture(VariableCollection from, VariableCollection to, Func<StackVariable, Value> callback)
    {
        foreach (var (name, originalStack) in from.Variables)
        {
            if (!to.Variables.ContainsKey(name))
                to.Variables.Add(name, new());

            var stack = to.Variables[name];

            foreach (var variable in originalStack.Reverse())
                stack.Push(new(false, name, callback(variable), to));
        }
    }
}