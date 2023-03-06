using System;

namespace Kalkatos
{
	public interface IValueGetter<T>
    {
        T GetValue();
    }

    [Serializable]
    public class ValueGetter<T> : IValueGetter<T>
    {
        public T Value;

        public virtual T GetValue () => Value;
    }
}
