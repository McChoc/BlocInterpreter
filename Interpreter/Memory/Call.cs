using System;
using System.Collections.Generic;
using System.Linq;
using Bloc.Pointers;
using Bloc.Results;
using Bloc.Values;
using Bloc.Variables;

namespace Bloc.Memory;

public sealed class Call
{
    private readonly int _stack;

    public Engine Engine { get; }

    internal Variable? Recall { get; }
    internal VariableCollection Captures { get; }
    internal VariableCollection Params { get; }
    internal LinkedList<Scope> Scopes { get; }

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

        if (_stack > Engine.StackLimit)
            throw new Throw("The stack limit was reached");

        Captures = captures;
        Params = @params;
    }

    internal Call(Call parent, VariableCollection captures, VariableCollection @params, Func recall)
        : this(parent.Engine)
    {
        _stack = parent._stack + 1;

        if (_stack > Engine.StackLimit)
            throw new Throw("The stack limit was reached");

        Captures = captures;
        Params = @params;

        Recall = new HeapVariable(false, recall);
    }

    internal Scope MakeScope()
    {
        return new Scope(this);
    }

    internal void Destroy()
    {
        Recall?.Delete();

        foreach (var scope in Scopes)
            scope.Dispose();
    }

    internal UnresolvedPointer Get(string name)
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

    internal VariablePointer Set(bool mask, bool mutable, string name, Value value)
    {
        if (!mask && Scopes.Last.Value.Variables.ContainsKey(name))
            throw new Throw($"Variable {name} was already defined in scope");

        var variable = new StackVariable(mutable, name, value, Scopes.Last.Value);

        Scopes.Last.Value.Add(variable);

        return new VariablePointer(variable);
    }

    internal VariableCollection ValueCapture()
    {
        var captures = new VariableCollection();

        foreach (var scope in Scopes)
        {
            foreach (var (name, originalStack) in scope.Variables)
            {
                if (!captures.Variables.ContainsKey(name))
                    captures.Variables.Add(name, new());

                var newStack = captures.Variables[name];

                foreach (var variable in originalStack.Reverse())
                    newStack.Push(new(false, name, variable.Value.Copy(), captures));
            }
        }

        return captures;
    }

    internal VariableCollection ReferenceCapture()
    {
        var captures = new VariableCollection();

        foreach (var scope in Scopes)
        {
            foreach (var (name, originalStack) in scope.Variables)
            {
                if (!captures.Variables.ContainsKey(name))
                    captures.Variables.Add(name, new());

                var newStack = captures.Variables[name];

                foreach (var variable in originalStack.Reverse())
                    newStack.Push(new(false, name, new Reference(new VariablePointer(variable)), captures));
            }
        }

        return captures;
    }
}