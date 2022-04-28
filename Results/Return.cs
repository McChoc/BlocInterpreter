using CmmInterpretor.Data;
using CmmInterpretor.Values;

namespace CmmInterpretor.Results
{
    public class Return : IResult
    {
        public Value value;

        public Return() => value = new Void();

        public Return(Value value) => this.value = value;
    }
}
