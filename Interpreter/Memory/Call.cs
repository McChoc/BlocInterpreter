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

    internal VariableCollection ModuleVariables { get; }
    internal VariableCollection ClosureVariables { get; }
    internal VariableCollection ParamsVariables { get; }
    internal List<Scope> Scopes { get; }

    internal Call(Engine engine, Module module)
    {
        Engine = engine;
        Module = module;

        Scopes = new();
        MakeScope();

        ModuleVariables = Scopes[0];
        ClosureVariables = new();
        ParamsVariables = new();
    }

    internal Call(Call parent, VariableCollection moduleVariables, VariableCollection closureVariables, VariableCollection paramsVariables)
    {
        Engine = parent.Engine;
        Module = parent.Module;

        _stack = parent._stack + 1;

        if (_stack > Engine.Options.StackLimit)
            throw new Throw("The stack limit was reached");

        ModuleVariables = moduleVariables;
        ClosureVariables = closureVariables;
        ParamsVariables = paramsVariables;
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
        Variable? @params = null;
        Variable? closure = null;
        Variable? module = null;
        Variable? global = null;

        foreach (var scope in Scopes)
        {
            if (scope.Variables.TryGetValue(name, out stack))
            {
                local = stack.Peek();
                break;
            }
        }

        if (ParamsVariables.Variables.TryGetValue(name, out stack))
            @params = stack.Peek();

        if (ClosureVariables.Variables.TryGetValue(name, out stack))
            closure = stack.Peek();

        if (ModuleVariables.Variables.TryGetValue(name, out stack))
            module = stack.Peek();

        if (Engine.GlobalVariables.Variables.TryGetValue(name, out stack))
            global = stack.Peek();

        return new UnresolvedPointer(name, local, @params, closure, module, global);
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

        Capture(ParamsVariables, captures, callback);
        Capture(ClosureVariables, captures, callback);

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