using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace LsideWPF.model
{
    public class FillOnceBuffer<T> : LinkedList<T>
    {
        private int capacity;

        public FillOnceBuffer(int capacity)
        {
            this.capacity = capacity;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public FillOnceBuffer(FillOnceBuffer<T> buffer)
        {
            this.capacity = buffer.capacity;
            foreach (T item in buffer)
            {
                this.Add(item);
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Add(T item)
        {
            if (Count == capacity) return;
            AddFirst(item);
        }
    }
}
