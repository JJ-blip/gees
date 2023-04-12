namespace LsideWPF.Services
{
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    public class LifoBuffer<T> : LinkedList<T>
    {
        private readonly int capacity;

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
            if (this.Count == this.capacity)
            {
                this.RemoveLast();
            }

            this.AddFirst(item);
        }
    }
}
