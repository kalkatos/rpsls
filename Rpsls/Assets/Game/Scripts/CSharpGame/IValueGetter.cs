// (c) 2023 Alex Kalkatos
// This code is licensed under MIT license (see LICENSE.txt for details)

using System;

namespace Kalkatos
{
    /// <summary>
    /// Simple interface that retrieves a value based on a type.
    /// </summary>
    /// <typeparam name="T">Any type.</typeparam>
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
