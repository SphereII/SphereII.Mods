using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCoreQueue<T> : IEnumerable<T>
    {
        private readonly Queue<T> _queue;
        private readonly int _maxCount;

        public SCoreQueue(int maxCount = 5)
        {
            _queue = new Queue<T>(maxCount);
            _maxCount = maxCount;
        }

        public void Add(T item)
        {
            if (_queue.Count == _maxCount)
            {
                _queue.Dequeue();
            }
            _queue.Enqueue(item);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _queue.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }


