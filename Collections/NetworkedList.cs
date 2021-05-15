using System;
using System.Collections;
using System.Collections.Generic;
using Sandbox;

namespace NetworkWrappers
{
	/// <inheritdoc cref="List{T}"/>
	public class NetworkedList<T> : NetworkClass, IList<T>, IReadOnlyList<T> where T : class
	{
		private List<T> _internalList;

		/// <inheritdoc cref="List{T}.IndexOf(T)"/>
		public int IndexOf( T item )
		{
			return _internalList.IndexOf( item );
		}

		/// <inheritdoc cref="List{T}.Insert(int, T)"/>
		public void Insert( int index, T item )
		{
			_internalList.Insert( index, item );
			NetworkDirty( nameof(_internalList), NetVarGroup.Net ); // TODO: Apparently this is bad, FP help?
		}

		/// <inheritdoc cref="List{T}.RemoveAt(int)"/>
		public void RemoveAt( int index )
		{
			_internalList.RemoveAt( index );
			NetworkDirty( nameof(_internalList), NetVarGroup.Net );
		}

		public T this[ int index ]
		{
			get => _internalList[index];
			set
			{
				var old = _internalList[index];
				_internalList[index] = value;

				if (old is IEquatable<T> equatable && value is IEquatable<T> newValue)
				{
					if (!equatable.Equals( (T)newValue ))
					{
						NetworkDirty( nameof(_internalList), NetVarGroup.Net );
					}
				}
				else if (!Equals( old, value ))
				{
					NetworkDirty( nameof(_internalList), NetVarGroup.Net );
				}
			}
		}

		/// <inheritdoc cref="List{T}"/>
		public NetworkedList()
		{
			_internalList = new List<T>();
		}

		/// <inheritdoc cref="List{T}(int)"/>
		public NetworkedList( int capacity )
		{
			_internalList = new List<T>( capacity );
		}

		/// <inheritdoc cref="List{T}(IEnumerable{T})"/>
		public NetworkedList( IEnumerable<T> collection )
		{
			_internalList = new List<T>( collection );
		}

		public override bool NetRead( NetRead read )
		{
			base.NetRead( read );

			var count = read.Read<int>();
			_internalList = new List<T>( count );

			for (int i = 0; i < count; i++)
			{
				_internalList.Add( read.ReadClass<T>( null ) );
			}

			return true;
		}

		public override bool NetWrite( NetWrite write )
		{
			base.NetWrite( write );

			write.Write( _internalList.Count );
			foreach (var element in _internalList)
			{
				write.Write( element );
			}

			return true;
		}

		/// <inheritdoc cref="List{T}.GetEnumerator()"/>
		public IEnumerator<T> GetEnumerator()
		{
			return _internalList.GetEnumerator();
		}

		/// <inheritdoc cref="List{T}.GetEnumerator()"/>
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable)_internalList).GetEnumerator();
		}

		/// <inheritdoc cref="List{T}.Add(T)"/>
		public void Add( T item )
		{
			_internalList.Add( item );
			NetworkDirty( nameof(_internalList), NetVarGroup.Net );
		}

		/// <inheritdoc cref="List{T}.Clear()"/>
		public void Clear()
		{
			var count = _internalList.Count;
			_internalList.Clear();
			if (count > 0)
			{
				NetworkDirty( nameof(_internalList), NetVarGroup.Net );
			}
		}

		/// <inheritdoc cref="List{T}.Contains(T)"/>
		public bool Contains( T item )
		{
			return _internalList.Contains( item );
		}

		/// <inheritdoc cref="List{T}.CopyTo(T[], int)"/>
		public void CopyTo( T[] array, int arrayIndex )
		{
			_internalList.CopyTo( array, arrayIndex );
		}

		/// <inheritdoc cref="List{T}.Remove(T)"/>
		public bool Remove( T item )
		{
			if (!_internalList.Remove( item ))
			{
				return false;
			}

			NetworkDirty( nameof(_internalList), NetVarGroup.Net );
			return true;
		}

		/// <inheritdoc cref="List{T}.Count"/>
		int ICollection<T>.Count => _internalList.Count;

		public bool IsReadOnly => false;

		/// <inheritdoc cref="List{T}.Count"/>
		int IReadOnlyCollection<T>.Count => _internalList.Count;
	}
}