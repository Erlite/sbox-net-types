using System;
using System.Collections;
using System.Collections.Generic;
using Sandbox;

namespace NetworkWrappers
{
    /// <inheritdoc cref="System.Collections.Generic.Dictionary{TKey,TValue}"/>
    public class NetworkDictionaryUU<TKey, TValue> : NetworkClass, IDictionary<TKey, TValue>, IDictionary, IReadOnlyDictionary<TKey, TValue>
        where TKey : unmanaged
        where TValue : unmanaged
    {
        private Dictionary<TKey, TValue> _internalDictionary;

        #region Constructors
        public NetworkDictionaryUU()
        {
            _internalDictionary = new Dictionary<TKey, TValue>();
        }

        public NetworkDictionaryUU(int capacity)
        {
            _internalDictionary = new Dictionary<TKey, TValue>(capacity);
        }

        public NetworkDictionaryUU(IEqualityComparer<TKey>? comparer) : this(0, comparer)
        {
        }

        public NetworkDictionaryUU(int capacity, IEqualityComparer<TKey>? comparer)
        {
            _internalDictionary = new Dictionary<TKey, TValue>(capacity, comparer);
        }

        public NetworkDictionaryUU(IDictionary<TKey, TValue> dictionary) : this(dictionary, null)
        {
        }

        public NetworkDictionaryUU(IDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey>? comparer)
            : this(dictionary?.Count ?? 0, comparer)
        {
            _internalDictionary = new Dictionary<TKey, TValue>(dictionary, comparer);
        }

        public NetworkDictionaryUU(IEnumerable<KeyValuePair<TKey, TValue>> collection) : this(collection, null)
        {
        }

        public NetworkDictionaryUU(IEnumerable<KeyValuePair<TKey, TValue>> collection, IEqualityComparer<TKey>? comparer) :
            this((collection as ICollection<KeyValuePair<TKey, TValue>>)?.Count ?? 0, comparer)
        {
            _internalDictionary = new Dictionary<TKey, TValue>(collection, comparer);
        }
        #endregion // Constructors

        #region Networking
        public override bool NetRead(NetRead read)
        {
            base.NetRead(read);

            var count = read.Read<int>();
            _internalDictionary = new Dictionary<TKey, TValue>(count);
            
            for (int i = 0; i < count; i++)
            {
                var key = read.Read<TKey>();
                var value = read.Read<TValue>();

                _internalDictionary.Add(key, value);
            }

            return true;
        }

        public override bool NetWrite(NetWrite write)
        {
            base.NetWrite(write);

            write.Write(_internalDictionary.Count);
            foreach (var kv in _internalDictionary)
            {
                write.Write(kv.Key);
                write.Write(kv.Value);
            }

            return true;
        }
        #endregion // Networking

        #region Indexers
        public TValue this[TKey index]
        {
            get => _internalDictionary[index];
            set
            {
                var old = _internalDictionary[index];
                _internalDictionary[index] = value;

                if (old is IEquatable<TValue> equatable && value is IEquatable<TValue> newValue)
                {
                    if (!equatable.Equals((TValue)newValue))
                    {
                        NetworkDirty(nameof(_internalDictionary), NetVarGroup.Net);
                    }
                }
                else if (!Equals(old, value))
                {
                    NetworkDirty(nameof(_internalDictionary), NetVarGroup.Net);
                }
            }
        }

        public object? this[object key]
        {
            get => ((IDictionary)_internalDictionary)[key];
            set
            {
                var old = ((IDictionary)_internalDictionary)[key];
                ((IDictionary)_internalDictionary)[key] = value;

                if (old is IEquatable<TValue> equatable && value is IEquatable<TValue> newValue)
                {
                    if (!equatable.Equals((TValue)newValue))
                    {
                        NetworkDirty(nameof(_internalDictionary), NetVarGroup.Net);
                    }
                }
                else if (!Equals(old, value))
                {
                    NetworkDirty(nameof(_internalDictionary), NetVarGroup.Net);
                }
            }
        }
        #endregion // Indexers

        #region Interfaces
        public void Add(TKey key, TValue value)
        {
            _internalDictionary.Add(key, value);
            NetworkDirty(nameof(_internalDictionary), NetVarGroup.Net);
        }

        bool IDictionary<TKey, TValue>.ContainsKey(TKey key)
        {
            return _internalDictionary.ContainsKey(key);
        }

        bool IReadOnlyDictionary<TKey, TValue>.TryGetValue(TKey key, out TValue value)
        {
            return _internalDictionary.TryGetValue(key, out value);
        }

        public bool Remove(TKey key)
        {
            if (!_internalDictionary.Remove(key))
            {
                return false;
            }

            NetworkDirty(nameof(_internalDictionary), NetVarGroup.Net);
            return true;
        }

        bool IReadOnlyDictionary<TKey, TValue>.ContainsKey(TKey key)
        {
            return _internalDictionary.ContainsKey(key);
        }

        bool IDictionary<TKey, TValue>.TryGetValue(TKey key, out TValue value)
        {
            return _internalDictionary.TryGetValue(key, out value);
        }

        IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys => _internalDictionary.Keys;
        IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values => _internalDictionary.Values;
        ICollection<TKey> IDictionary<TKey, TValue>.Keys => _internalDictionary.Keys;
        ICollection IDictionary.Values => ((IDictionary)_internalDictionary).Values;
        ICollection IDictionary.Keys => ((IDictionary)_internalDictionary).Keys;
        ICollection<TValue> IDictionary<TKey, TValue>.Values => _internalDictionary.Values;
        #endregion // Interfaces

        public bool Contains(object key)
        {
            return ((IDictionary)_internalDictionary).Contains(key);
        }

        IDictionaryEnumerator IDictionary.GetEnumerator()
        {
            return ((IDictionary)_internalDictionary).GetEnumerator();
        }

        public void Remove(object key)
        {
            ((IDictionary)_internalDictionary).Remove(key);
            NetworkDirty(nameof(_internalDictionary), NetVarGroup.Net);
        }

        public bool IsFixedSize => ((IDictionary)_internalDictionary).IsFixedSize;
        bool IDictionary.IsReadOnly => ((IDictionary)_internalDictionary).IsReadOnly;


        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
        {
            return _internalDictionary.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_internalDictionary).GetEnumerator();
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            _internalDictionary.Add(item.Key, item.Value);
            NetworkDirty(nameof(_internalDictionary), NetVarGroup.Net);
        }

        public void Add(object key, object? value)
        {
            ((IDictionary)_internalDictionary).Add(key, value);
            NetworkDirty(nameof(_internalDictionary), NetVarGroup.Net);
        }

        void IDictionary.Clear()
        {
            var oldCount = _internalDictionary.Count;
            _internalDictionary.Clear();
            if (oldCount > 0)
            {
                NetworkDirty(nameof(_internalDictionary), NetVarGroup.Net);
            }
        }

        void ICollection<KeyValuePair<TKey, TValue>>.Clear()
        {
            var oldCount = _internalDictionary.Count;
            _internalDictionary.Clear();
            if (oldCount > 0)
            {
                NetworkDirty(nameof(_internalDictionary), NetVarGroup.Net);
            }
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return _internalDictionary.ContainsKey(item.Key) && ReferenceEquals(_internalDictionary[item.Key], item.Value);
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            throw new NotSupportedException("This is not accessible outside of Dictionary, sorry!");
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            throw new NotSupportedException("This is not accessible outside of Dictionary, please remove by key!");
        }

        public void CopyTo(Array array, int index)
        {
            ((ICollection)_internalDictionary).CopyTo(array, index);
        }

        int ICollection.Count => _internalDictionary.Count;
        public bool IsSynchronized => ((ICollection)_internalDictionary).IsSynchronized;
        public object SyncRoot => ((ICollection)_internalDictionary).SyncRoot;

        int ICollection<KeyValuePair<TKey, TValue>>.Count => _internalDictionary.Count;
        bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly => false;
        int IReadOnlyCollection<KeyValuePair<TKey, TValue>>.Count => _internalDictionary.Count;
    }
}