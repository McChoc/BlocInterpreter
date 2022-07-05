using Bloc.Values;

namespace Bloc.Results
{
    public class Return : Result
    {
        public Value value;

        public Return() => value = Void.Value;

        public Return(Value value) => this.value = value;
    }
}