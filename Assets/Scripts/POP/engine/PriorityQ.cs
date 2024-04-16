
namespace POP
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using static System.ArgumentNullException;
    public class PriorityQ<TElement, TPriority> : IEquatable<PriorityQ<TElement, TPriority>>, IEnumerable<TElement> where TElement : notnull where TPriority : notnull
    {
#nullable enable
        private SortedDictionary<TPriority, Queue<TElement>> sortedDict;
        public IComparer<TPriority> Comparer { get; }

        public int Count
        {
            get { return sortedDict.Count + sortedDict.Values.Sum(q => q.Count - 1); }
        }
        public bool IsEmpty() { return Count == 0; }

        public PriorityQ(IComparer<TPriority> comparer)
        {
            this.Comparer = comparer;
            this.sortedDict = new SortedDictionary<TPriority, Queue<TElement>>(Comparer);
        }

        public PriorityQ() : this(Comparer<TPriority>.Default) { Console.WriteLine("PriorityQ null"); }

        public void Enqueue(TElement item, TPriority priority)
        {
            if (!sortedDict.ContainsKey(priority))
            {
                sortedDict.Add(priority, new Queue<TElement>());
            }
            sortedDict[priority].Enqueue(item);
        }


        public void Enqueue(TElement item)
        {
            Enqueue(item, this.Count == 0 ? default! : sortedDict.Keys.Last());
        }

        public TElement Dequeue()
        {
            TPriority priority = sortedDict.Keys.First();
            TElement item = sortedDict[priority].Dequeue() ?? throw new InvalidOperationException("Queue is empty");
            if (sortedDict[priority].Count == 0)
            {
                sortedDict.Remove(priority);
            }
            return item;
        }

        public void EnqueueRange(IEnumerable<TElement> items)
        {
            foreach (TElement item in items)
            {
                Enqueue(item);
            }
        }

        public void EnqueueRange(IEnumerable<TElement> items, TPriority priority)
        {
            foreach (TElement item in items)
            {
                Enqueue(item, priority);
            }
        }

        public void EnqueueRange(IEnumerable<TElement> items, IEnumerable<TPriority> priorities)
        {
            IEnumerator<TPriority> priorityEnum = priorities.GetEnumerator();
            foreach (TElement item in items)
            {
                priorityEnum.MoveNext();
                Enqueue(item, priorityEnum.Current);
            }
        }

        public TElement Peek()
        {
            return sortedDict.Values.First().Peek();
        }

        public void Clear()
        {
            sortedDict.Clear();
        }

        public bool Contains(TElement item)
        {
            return sortedDict.Values.Any(q => q.Contains(item));
        }

        public TElement DequeueEnqueue(TElement item)
        {
            TElement min = Dequeue();
            Enqueue(item);
            return min;
        }

        public TElement EnqueueDequeue(TElement item)
        {
            Enqueue(item);
            return Dequeue();
        }

        public bool Equals(PriorityQ<TElement, TPriority>? other)
        {
            if (other is null)
                return false;
            return this.sortedDict.SequenceEqual(other.sortedDict);
        }

        public override bool Equals(object? obj)
        {
            return obj is PriorityQ<TElement, TPriority> q && this.Equals(q);
        }

        public override int GetHashCode()
        {
            return sortedDict.GetHashCode();
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var item in this)
            {
                sb.Append(item);
                sb.Append(", ");
            }
            return sb.ToString();
        }

#nullable disable
        public bool TryDequeue(out TElement item)
        {
            if (sortedDict.Count > 0)
            {
                item = Dequeue();
                return true;
            }
            item = default;
            return false;
        }
        public bool TryPeek(out TElement item)
        {
            if (sortedDict.Count > 0)
            {
                item = Peek();
                return true;
            }
            item = default;
            return false;
        }

        public IEnumerator<TElement> GetEnumerator()
        {
            return sortedDict.Values.SelectMany(q => q).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

#nullable enable

        public static bool operator ==(PriorityQ<TElement, TPriority>? left, PriorityQ<TElement, TPriority>? right) { return left is null ? (left is null && right is null) : left.Equals(right); }
        public static bool operator !=(PriorityQ<TElement, TPriority>? left, PriorityQ<TElement, TPriority>? right) { return !(left is null ? (left is null && right is null) : left.Equals(right)); }
        public static bool operator ==(PriorityQ<TElement, TPriority>? left, object right) { return left is null ? (left is null && right is null) : left.Equals(right); }
        public static bool operator !=(PriorityQ<TElement, TPriority>? left, object right) { return !(left is null ? (left is null && right is null) : left.Equals(right)); }
        public static bool operator ==(object left, PriorityQ<TElement, TPriority>? right) { return right is null ? (left is null && right is null) : right.Equals(left); }
        public static bool operator !=(object left, PriorityQ<TElement, TPriority>? right) { return !(right is null ? (left is null && right is null) : right.Equals(left)); }
    }


    public enum SearchStrategy
    {
        BFS,
        DFS,
        AStar
    }

}