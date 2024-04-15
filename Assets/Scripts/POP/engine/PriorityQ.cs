
namespace POP
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using static System.ArgumentNullException;
    public class PriorityQ<T> : IEquatable<PriorityQ<T>>, IEnumerable<T> where T : notnull
    {
        private SortedSet<T> sortedSet;
        public IComparer<T> Comparer { get; }

        public int Count
        {
            get { return sortedSet.Count; }
        }

        public PriorityQ(IComparer<T> comparer)
        {
            this.Comparer = comparer;
            this.sortedSet = new SortedSet<T>(Comparer);
        }

        public PriorityQ() : this(Comparer<T>.Default) { }

        public void Enqueue(T item)
        {
            sortedSet.Add(item);
        }

        public T Dequeue()
        {
            T item = sortedSet.Min;
            sortedSet.Remove(item);
            return item;
        }

        public void EnqueueRange(IEnumerable<T> items)
        {
            foreach (T item in items)
            {
                sortedSet.Add(item);
            }
        }

        public T Peek()
        {
            return sortedSet.Min;
        }

        public void Clear()
        {
            sortedSet.Clear();
        }

        public bool Contains(T item)
        {
            return sortedSet.Contains(item);
        }

        public T DequeueEnqueue(T item)
        {
            T min = sortedSet.Min;
            sortedSet.Remove(min);
            sortedSet.Add(item);
            return min;
        }

        public T EnqueueDequeue(T item)
        {
            sortedSet.Add(item);
            T min = sortedSet.Min;
            sortedSet.Remove(min);
            return min;
        }

        public bool Equals(PriorityQ<T> other)
        {
            if (other is null)
                return false;
            return this.sortedSet.SetEquals(other.sortedSet);
        }

        public override bool Equals(object obj)
        {
            return obj is PriorityQ<T> q && this.Equals(q);
        }

        public override int GetHashCode()
        {
            return sortedSet.GetHashCode();
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (T item in sortedSet)
            {
                sb.Append(item.ToString());
                sb.Append(", ");
            }
            return sb.ToString();
        }

        public bool TryDequeue(out T item)
        {
            if (sortedSet.Count > 0)
            {
                item = Dequeue();
                return true;
            }
            item = default;
            return false;
        }

        public bool TryPeek(out T item)
        {
            if (sortedSet.Count > 0)
            {
                item = Peek();
                return true;
            }
            item = default;
            return false;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return sortedSet.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

#nullable enable

        public static bool operator ==(PriorityQ<T>? left, PriorityQ<T>? right) { return left is null ? (left is null && right is null) : left.Equals(right); }
        public static bool operator !=(PriorityQ<T>? left, PriorityQ<T>? right) { return !(left is null ? (left is null && right is null) : left.Equals(right)); }
        public static bool operator ==(PriorityQ<T>? left, object right) { return left is null ? (left is null && right is null) : left.Equals(right); }
        public static bool operator !=(PriorityQ<T>? left, object right) { return !(left is null ? (left is null && right is null) : left.Equals(right)); }
        public static bool operator ==(object left, PriorityQ<T>? right) { return right is null ? (left is null && right is null) : right.Equals(left); }
        public static bool operator !=(object left, PriorityQ<T>? right) { return !(right is null ? (left is null && right is null) : right.Equals(left)); }
    }



    public enum SearchStrategy
    {
        BFS,
        AStar
    }

    public class NodeComparer : IComparer<Node>
    {
        public SearchStrategy SearchStrategy { get; }
        public NodeComparer(SearchStrategy searchStrategy) { SearchStrategy = searchStrategy; }

        public int Eval_Fn(Node node)
        {
            /* 
             *      A* search algorithm:::
             *
             *      f(n) = h(n) + g(n)
             *      h(n) = open preconditions of the partial plan (inside the agenda) 
             *      g(n) = path cost from the start node to the current node
            */

            /* 
             *      BFS search algorithm:::
             *
             *      f(n) = g(n)
             *      g(n) = path cost (level) from the start node to the current node
            */

            return SearchStrategy switch
            {
                SearchStrategy.AStar => node.pathCost + node.agenda.Count,
                SearchStrategy.BFS => node.pathCost,
                _ => throw new NotImplementedException()
            };
        }

        public int Compare(Node x, Node y)
        {
            return Eval_Fn(x) - Eval_Fn(y);
        }
    }
}