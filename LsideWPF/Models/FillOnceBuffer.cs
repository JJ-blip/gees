namespace LsideWPF.Models
{
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    public class FillOnceBuffer<T> : LinkedList<T>
    {
        private readonly int capacity;

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
            if (this.Count == this.capacity)
            {
                return;
            }

            this.AddFirst(item);
        }
    }
}
