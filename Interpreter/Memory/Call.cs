using System.Collections.Generic;
using System.Linq;
using Bloc.Pointers;
using Bloc.Results;
using Bloc.Values;
using Bloc.Variables;

namespace Bloc.Memory
{
    public sealed class Call
    {
        private readonly int _stack;

        public Engine Engine { get; }

        internal Variable? Params { get; }
        internal Variable? Recall { get; }

        internal Scope Captures { get; }
        internal Stack<Scope> Scopes { get; }

        internal Call(Engine engine)
        {
            Engine = engine;
            Captures = new();
            Scopes = new();
            Push();
        }

        internal Call(Call parent, Scope captures)
            : this(parent.Engine)
        {
            _stack = parent._stack + 1;

            if (_stack > Engine.StackLimit)
                throw new Throw("The stack limit was reached");

            Captures = captures;
        }

        internal Call(Call parent, Scope captures, Func recall, Array @params)
            : this(parent.Engine)
        {
            _stack = parent._stack + 1;

            if (_stack > Engine.StackLimit)
                throw new Throw("The stack limit was reached");

            Captures = captures;

            Recall = new HeapVariable(false, recall);
            Params = new HeapVariable(false, @params);
        }

        internal void Push() => Scopes.Push(new());

        internal void Pop() => Scopes.Pop().Dispose();

        internal void Destroy()
        {
            Recall?.Delete();
            Params?.Delete();

            foreach (var scope in Scopes)
                scope.Dispose();
        }

        internal UnresolvedPointer Get(string name)
        {
            Stack<StackVariable> stack;

            Variable? local = null;
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

            if (Captures.Variables.TryGetValue(name, out stack))
                nonLocal = stack.Peek();

            if (Engine.GlobalScope.Variables.TryGetValue(name, out stack))
                global = stack.Peek();

            return new UnresolvedPointer(name, local, nonLocal, global);
        }

        internal VariablePointer Set(bool mask, bool mutable, string name, Value value)
        {
            if (!mask && Scopes.Peek().Variables.ContainsKey(name))
                throw new Throw($"Variable {name} was already defined in scope");

            var variable = new StackVariable(mutable, name, value, Scopes.Peek());

            Scopes.Peek().Add(variable);

            return new VariablePointer(variable);
        }

        internal Scope ValueCapture()
        {
            var captures = new Scope();

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

        internal Scope ReferenceCapture()
        {
            var captures = new Scope();

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
}