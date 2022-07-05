using System.Collections.Generic;
using System.Linq;
using Bloc.Results;
using Bloc.Values;
using Bloc.Variables;

namespace Bloc.Memory
{
    public class Call
    {
        private readonly int _stack;

        internal Call(Engine engine)
        {
            _stack = 0;

            Engine = engine;
            Scopes = new List<Scope>();
            Push();
        }

        internal Call(Call parent, Scope captures, Function recall, List<Value> @params)
            : this(parent.Engine)
        {
            _stack = parent._stack + 1;

            if (_stack > Engine.StackLimit)
                throw new Throw("The stack limit was reached");

            Parent = parent;
            Captures = captures;

            Recall = new HeapVariable(recall);
            Params = new HeapVariable(new Array(@params.Cast<IValue>().ToList()));
            Params.Value.Assign();
        }

        internal Engine Engine { get; }

        internal Call? Parent { get; }
        internal Scope? Captures { get; }

        internal Variable? Recall { get; }
        internal Variable? Params { get; }

        internal List<Scope> Scopes { get; }

        internal void Push()
        {
            Scopes.Add(new(this));
        }

        internal void Pop()
        {
            Scopes[^1].Destroy();
            Scopes.RemoveAt(Scopes.Count - 1);
        }

        internal void Destroy()
        {
            Recall?.Destroy();
            Params?.Destroy();

            foreach (var scope in Scopes)
                scope.Destroy();
        }

        internal bool TryAdd(StackVariable variable)
        {
            if (Scopes[^1].Variables.ContainsKey(variable.Name))
                return false;

            Scopes[^1].Variables.Add(variable.Name, variable);
            return true;
        }

        internal bool TryGet(string name, out Variable? var)
        {
            for (var i = Scopes.Count - 1; i >= 0; i--)
                if (Scopes[i].Variables.ContainsKey(name))
                {
                    var = Scopes[i].Variables[name];
                    return true;
                }

            if (Captures is not null && Captures.Variables.ContainsKey(name))
            {
                var = Captures.Variables[name];
                return true;
            }

            var = null;
            return false;
        }

        internal void Set(string name, Variable variable)
        {
            Scopes[^1].Variables[name] = variable;
        }

        internal Scope Capture()
        {
            var captures = new Scope(null);

            foreach (var scope in Scopes)
            foreach (var pair in scope.Variables)
                captures.Variables[pair.Key] = pair.Value;

            return captures;
        }
    }
}