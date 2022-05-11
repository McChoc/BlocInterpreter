using CmmInterpretor.Values;
using CmmInterpretor.Variables;
using System.Collections.Generic;
using System.Linq;

namespace CmmInterpretor.Data
{
    public class Call
    {
        public Engine Engine { get; }

        public Call Parent { get; }
        public Scope Captures { get; }

        public Variable Recall { get; }
        public Variable Params { get; }

        public List<Scope> Scopes { get; }

        public Call(Engine engine)
        {
            Engine = engine;
            Scopes = new();
            Push();
        }

        public Call(Call parent, Scope captures, Function recall, List<Value> @params)
            : this(parent.Engine)
        {
            Parent = parent;
            Captures = captures;

            Recall = new HeapVariable(recall);
            Params = new HeapVariable(new Array(@params.Cast<IValue>().ToList()));
            Params.Value.Assign();
        }

        public void Push() => Scopes.Add(new Scope(this));

        public void Pop()
        {
            Scopes[^1].Destroy();
            Scopes.RemoveAt(Scopes.Count - 1);
        }

        public void Destroy()
        {
            Recall.Destroy();
            Params.Destroy();

            foreach (Scope scope in Scopes)
                scope.Destroy();
        }

        public bool TryAdd(StackVariable variable)
        {
            if (Scopes[^1].Variables.ContainsKey(variable.Name))
                return false;

            Scopes[^1].Variables.Add(variable.Name, variable);
            return true;
        }

        public bool TryGet (string name, out Variable var)
        {
            for (int i = Scopes.Count - 1; i >= 0; i--)
            {
                if (Scopes[i].Variables.ContainsKey(name))
                {
                    var = Scopes[i].Variables[name];
                    return true;
                }
            }

            if (Captures is not null && Captures.Variables.ContainsKey(name))
            {
                var = Captures.Variables[name];
                return true;
            }

            var = null;
            return false;
        }

        public void Set(string name, Variable variable)
        {
            Scopes[^1].Variables[name] = variable;
        }

        public Scope Capture()
        {
            var captures = new Scope(null);

            foreach (var scope in Scopes)
                foreach (var pair in scope.Variables)
                    captures.Variables[pair.Key] = pair.Value;

            return captures;
        }
    }
}
