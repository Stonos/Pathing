﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace BhModule.Community.Pathing {
    public class SafeList<T> : IList<T> {

        private class SafeEnumerator<TEnumerator> : IEnumerator<TEnumerator> {

            private readonly IEnumerator<TEnumerator> _inner;
            private readonly ReaderWriterLockSlim     _rwLock;

            public SafeEnumerator(IEnumerator<TEnumerator> inner, ReaderWriterLockSlim rwLock) {
                _inner = inner;
                _rwLock = rwLock;
            }

            public bool MoveNext() {
                return _inner.MoveNext();
            }

            public void Reset() {
                _inner.Reset();
            }

            public object Current => _inner.Current;

            TEnumerator IEnumerator<TEnumerator>.Current => _inner.Current;

            public void Dispose() {
                _rwLock.ExitReadLock();
            }

        }
        
        private readonly ReaderWriterLockSlim _listLock = new();

        private List<T> _innerList;

        public bool IsReadOnly => false;

        public bool IsEmpty { get; private set; } = true;

        public SafeList() {
            _innerList = new List<T>();
        }

        public SafeList(IEnumerable<T> existingControls) {
            _innerList = new List<T>(existingControls);

            this.IsEmpty = !_innerList.Any();
        }

        public IEnumerator<T> GetEnumerator() {
            _listLock.EnterReadLock();

            return new SafeEnumerator<T>(_innerList.GetEnumerator(), _listLock);
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        public void Add(T item) {
            if (this.Contains(item) || item == null) return;

            _listLock.EnterWriteLock();
            _innerList.Add(item);
            this.IsEmpty = false;
            _listLock.ExitWriteLock();
        }

        public void AddRange(IEnumerable<T> items) {
            _listLock.EnterWriteLock();
            _innerList.AddRange(items);
            this.IsEmpty = !_innerList.Any();
            _listLock.ExitWriteLock();
        }

        public void SetRange(IEnumerable<T> items) {
            _listLock.EnterWriteLock();
            _innerList   = items.ToList();
            this.IsEmpty = !_innerList.Any();
            _listLock.ExitWriteLock();
        }

        public void Clear() {
            _listLock.EnterWriteLock();
            _innerList.Clear();
            this.IsEmpty = true;
            _listLock.ExitWriteLock();
        }

        public bool Contains(T item) {
            _listLock.EnterReadLock();

            try {
                return _innerList.Contains(item);
            } finally {
                _listLock.ExitReadLock();
            }
        }


        /// <summary>
        /// Do not use.
        /// </summary>
        [Obsolete("Do not use. Throws an exception.")]
        public void CopyTo(T[] array, int arrayIndex) {
            throw new InvalidOperationException($"{nameof(CopyTo)} not supported.  If using LINQ, ensure you call .ToList or .ToArray directly on {nameof(SafeList<T>)} first.");
        }

        public bool Remove(T item) {
            _listLock.EnterWriteLock();

            try {
                return _innerList.Remove(item);
            } finally {
                this.IsEmpty = !_innerList.Any();
                _listLock.ExitWriteLock();
            }
        }

        public int Count {
            get {
                _listLock.EnterReadLock();

                try {
                    return _innerList.Count;
                } finally {
                    _listLock.ExitReadLock();
                }
            }
        }

        public List<T> ToList() {
            _listLock.EnterReadLock();

            try {
                return new List<T>(_innerList);
            } finally {
                _listLock.ExitReadLock();
            }
        }

        public T[] ToArray() {
            _listLock.EnterReadLock();
            var items = new T[_innerList.Count];
            _innerList.CopyTo(items, 0);
            _listLock.ExitReadLock();

            return items;
        }

        public int IndexOf(T item) {
            _listLock.EnterReadLock();

            try {
                return _innerList.Count;
            } finally {
                _listLock.ExitReadLock();
            }
        }

        public void Insert(int index, T item) {
            _listLock.EnterWriteLock();
            _innerList.Insert(index, item);
            _listLock.ExitWriteLock();
        }

        public void RemoveAt(int index) {
            _listLock.EnterWriteLock();
            _innerList.RemoveAt(index);
            _listLock.ExitWriteLock();
        }

        public T this[int index] {
            get {
                _listLock.EnterReadLock();

                try {
                    return _innerList[index];
                } finally {
                    _listLock.ExitReadLock();
                }
            }
            set {
                _listLock.EnterWriteLock();
                _innerList[index] = value;
                _listLock.ExitWriteLock();
            }
        }

        ~SafeList() {
            _listLock?.Dispose();
        }

    }
}
