using CmmInterpretor.Values;

namespace CmmInterpretor.Results
{
    public class Return : Result
    {
        public Value value;

        public Return() => value = Void.Value;

        public Return(Value value) => this.value = value;
    }
}
