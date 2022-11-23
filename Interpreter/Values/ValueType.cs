namespace Bloc.Values
{
    public enum ValueType
    {
        Void,
        Null,
        Bool,
        Number,
        Range,
        String,
        Array,
        Struct,
        Tuple,
        Func,
        Task,
        Iter,
        Reference,
        Extern,
        Type
    }

    internal enum FunctionType
    {
        Synchronous,
        Asynchronous,
        Generator
    }

    internal enum CaptureMode
    {
        None,
        Value,
        Reference
    }
}