using CmmInterpretor.Data;
using CmmInterpretor.Values;

namespace CmmInterpretor.Results
{
    public class Return : IResult
    {
        public Value value;

        public Return() => value = Void.Value;

        public Return(Value value) => this.value = value;
    }
}
