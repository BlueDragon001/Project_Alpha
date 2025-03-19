using System.Collections.Generic;

/// <summary>
/// Implements a fixed-size circular buffer using a Queue.
/// When the buffer reaches its capacity, oldest items are removed to make room for new ones.
/// </summary>
/// <typeparam name="T">The type of elements stored in the buffer</typeparam>
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
