using System.Collections.Generic;
public class RingBuffer<T>
{
   private readonly Queue<T> queue= new();
   private readonly int maxCapacity;
   public RingBuffer(int capacity) => maxCapacity = capacity;
   public void Enqueue(T item)
   {
      if (queue.Count >= maxCapacity) queue.Dequeue();
      queue.Enqueue(item);
   }
   public T Dequeue() => queue.Dequeue();
   public T Peek() => queue.Peek();

    public int Count => queue.Count;
}
