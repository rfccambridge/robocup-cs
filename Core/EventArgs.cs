using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RFC.Core
{
    /// <summary>
    /// EventArguments class with arbitrary data item. Useful for custom events.
    /// </summary>
    /// <typeparam name="T">Type of the data item</typeparam>
    public class EventArgs<T> : EventArgs
    {
        T _data;
        public T Data
        {
            get { return _data; }
        }
        public EventArgs(T data)
        {
            _data = data;
        }
    }
}
