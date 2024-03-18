using System;
using System.Collections;
using System.Collections.Generic;

namespace ScapeCore.Traceability.Syntax
{
    public readonly record struct WordString : ICollection<string>
    {
        public readonly int lenght;
        public readonly string[] words;
        public WordString(string[] words)
        {
            this.words = words;
            lenght = words.Length;
        }
        public WordString(string input) : this(input.Split(' ', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)) { }

        public int Count => ((ICollection<string>)words).Count;

        public bool IsReadOnly => ((ICollection<string>)words).IsReadOnly;

        public void Add(string item)
        {
            ((ICollection<string>)words).Add(item);
        }

        public void Clear()
        {
            ((ICollection<string>)words).Clear();
        }

        public bool Contains(string item)
        {
            return ((ICollection<string>)words).Contains(item);
        }

        public void CopyTo(string[] array, int arrayIndex)
        {
            ((ICollection<string>)words).CopyTo(array, arrayIndex);
        }

        public IEnumerator<string> GetEnumerator()
        {
            return ((IEnumerable<string>)words).GetEnumerator();
        }

        public bool Remove(string item)
        {
            return ((ICollection<string>)words).Remove(item);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return words.GetEnumerator();
        }
    }
}