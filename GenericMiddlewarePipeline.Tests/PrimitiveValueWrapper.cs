namespace GenericMiddlewarePipeline.Tests
{
    public class Wrapped<T> where T : struct
    {
        private T _value;

        public Wrapped(T value)
        {
            Assign(value);
        }

        public void Assign(T value)
        {
            _value = value;
        }

        public static implicit operator Wrapped<T>(T value)
        {
            return new Wrapped<T>(value);
        }

        public static explicit operator T(Wrapped<T> wrapper)
        {
            return wrapper._value;
        }

        public static bool operator ==(Wrapped<T> a, Wrapped<T> b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(Wrapped<T> a, Wrapped<T> b)
        {
            return !a.Equals(b);
        }

        public override bool Equals(object? obj)
        {
            return _value.Equals(obj);
        }

        public override int GetHashCode()
        {
            return _value.GetHashCode();
        }

        public override string? ToString()
        {
            return _value.ToString();
        }
    }
}
