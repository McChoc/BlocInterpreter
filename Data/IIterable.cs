namespace CmmInterpretor.Data
{
    public interface IIterable
    {
        int Count { get; }
        Value this[int index] { get; }
    }
}
