using CmmInterpretor.Results;

namespace CmmInterpretor.Data
{
    public interface IIndexable
    {
        IResult Index(Value value, Call call);
    }
}
