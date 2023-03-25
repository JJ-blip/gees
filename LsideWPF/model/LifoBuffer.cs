using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace LsideWPF.model
{
    public class LifoBuffer<T> : LinkedList<T>
    {
        private int capacity;

        public LifoBuffer(int capacity)
        {
            this.capacity = capacity;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public LifoBuffer(LifoBuffer<T> lifo)
        {
            this.capacity = lifo.capacity;
            foreach (T item in lifo)
            {
                this.Add(item);
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Add(T item)
        {
            if (Count == capacity) RemoveLast();
            AddFirst(item);
        }
    }
}
