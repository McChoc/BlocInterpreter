using System.Collections.Generic;

namespace CmmInterpretor.Data
{
    public class Scope
    {
        public Call Call { get; }

        public Dictionary<string, Variable> Variables { get; } = new();

        public Scope(Call call)
        {
            Call = call;
        }
    }
}
