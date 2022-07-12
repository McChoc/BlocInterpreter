using System.Collections.Generic;
using System.Linq;
using Bloc.Pointers;
using Bloc.Results;
using Bloc.Values;
using Bloc.Variables;

namespace Bloc.Memory
{
    public class Call
    {
        private readonly int _stack = 0;

        internal Call(Engine engine)
        {
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
            Params = new HeapVariable(new Array(@params.Cast<IVariable>().ToList()));
            Params.Value.Assign();
        }

        public Engine Engine { get; }

        internal Call? Parent { get; }
        internal Scope? Captures { get; }

        internal Variable? Params { get; }
        internal Variable? Recall { get; }

        internal List<Scope> Scopes { get; }

        internal void Push()
        {
            Scopes.Add(new());
        }

        internal void Pop()
        {
            Scopes[^1].Destroy();
            Scopes.RemoveAt(Scopes.Count - 1);
        }

        internal void Destroy()
        {
            Recall?.Delete();
            Params?.Delete();

            foreach (var scope in Scopes)
                scope.Destroy();
        }

        internal Pointer Get(string name)
        {
            StackVariable variable;

            for (var i = Scopes.Count - 1; i >= 0; i--)
                if (Scopes[i].Variables.TryGetValue(name, out variable))
                    return new VariablePointer(variable);

            if (Captures is not null && Captures.Variables.TryGetValue(name, out variable))
                return new VariablePointer(variable);

            return new UndefinedPointer(name);
        }

        internal Pointer Set(string name, Value value)
        {
            var variable = new StackVariable(name, value, Scopes[^1]);

            Scopes[^1].Variables[name] = variable;

            return new VariablePointer(variable);
        }

        internal Scope Capture()
        {
            var captures = new Scope();

            foreach (var scope in Scopes)
                foreach (var (key, value) in scope.Variables)
                    captures.Variables[key] = value;

            return captures;
        }
    }
}