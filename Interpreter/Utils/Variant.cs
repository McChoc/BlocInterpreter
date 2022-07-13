using System;

namespace Bloc.Utils
{
    public class Variant<T1, T2>
    {
        private readonly Type _type;
        private readonly object _value;

        public Variant(T1 value)
        {
            _value = value!;
            _type = typeof(T1);
        }

        public Variant(T2 value)
        {
            _value = value!;
            _type = typeof(T2);
        }

        public T Get<T>()
        {
            if (_type != typeof(T))
                throw new InvalidOperationException();

            return (T)_value;
        }

        public bool Is<T>()
        {
            return _type == typeof(T);
        }

        public bool Is<T>(out T? result)
        {
            if (_type == typeof(T))
            {
                result = (T)_value;
                return true;
            }

            result = default;
            return false;
        }

        public static Variant<T1, T2> Coales(T1? t1, T2 t2)
        {
            if (t1 is not null)
                return new Variant<T1, T2>(t1);

            return new Variant<T1, T2>(t2);
        }

        public static implicit operator Variant<T1, T2>(T1 value)
        {
            return new(value);
        }

        public static implicit operator Variant<T1, T2>(T2 value)
        {
            return new(value);
        }
    }
}