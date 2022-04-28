using CmmInterpretor.Values;
using System.Collections.Generic;

namespace CmmInterpretor.Data
{
    public class Variable
    {
        private readonly Scope scope;

        internal List<Decorator> decorators;

        public bool final;
        public string name;
        public Value value;

        public List<Reference> References { get; } = new();

        internal Variable(string name, Value value, Scope scope, bool final = false, List<Decorator> decorators = null)
        {
            this.final = final;
            this.name = name;
            this.scope = scope;
            this.decorators = decorators ?? new List<Decorator>();
            SetValue(value);
        }

        public void SetValue (Value value)
        {
            //    foreach (Decorator decorator in decorators)
            //    {
            //        List<Value> arguments = new List<Value>() { currentValue };
            //        arguments.AddRange(decorator.arguments);
            //        currentValue = scope.Call.Engine.Dereference(decorator.pointer).Value().AsFunction().Call(new Array(arguments), scope.Call.Engine);
            //    }

            this.value = value;
        }

        public void Destroy()
        {
            scope?.Variables.Remove(name);
            foreach (var reference in References)
                reference.Invalidate();
        }
    }
}
