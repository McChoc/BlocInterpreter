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

    internal VariableCollection Captures { get; }
    internal VariableCollection Params { get; }
    internal List<Scope> Scopes { get; }

    internal Call(Engine engine)
    {
        Engine = engine;
        Captures = new();
        Params = new();
        Scopes = new();
        MakeScope();
    }

    internal Call(Call parent, VariableCollection captures, VariableCollection @params)
        : this(parent.Engine)
    {
        _stack = parent._stack + 1;

        if (_stack > Engine.Options.StackLimit)
            throw new Throw("The stack limit was reached");

        Captures = captures;
        Params = @params;
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
        Variable? nonLocal = null;
        Variable? global = null;

        foreach (var scope in Scopes)
        {
            if (scope.Variables.TryGetValue(name, out stack))
            {
                local = stack.Peek();
                break;
            }
        }

        if (Params.Variables.TryGetValue(name, out stack))
            param = stack.Peek();

        if (Captures.Variables.TryGetValue(name, out stack))
            nonLocal = stack.Peek();

        if (Engine.GlobalScope.Variables.TryGetValue(name, out stack))
            global = stack.Peek();

        return new UnresolvedPointer(name, local, param, nonLocal, global);
    }

    public VariablePointer Set(bool mask, bool mutable, string name, Value value)
    {
        if (!mask && Scopes[^1].Variables.ContainsKey(name))
            throw new Throw($"Variable '{name}' was already defined in scope");

        var variable = new StackVariable(mutable, name, value, Scopes[^1]);

        Scopes[^1].Add(variable);

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

        Capture(Params, captures, callback);

        Capture(Captures, captures, callback);

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