using System;
using System.Collections.Generic;
using Sandbox;

namespace NetworkWrappers
{
	/// <summary>
	/// Wraps a type to provide a callback when the value is set or changes.
	/// </summary>
	/// <typeparam name="T"> The type of the variable to wrap. </typeparam>
	public class NetworkVar<T> : NetworkClass, IEquatable<T>
	{
		private T _value;

		/// <summary>
		/// Called when the value of this wrapper has changed.
		/// The previous value is passed.
		/// </summary>
		public event Action<T> OnValueChanged;

		/// <summary>
		/// The value of this networked variable.
		/// Setting it with a different value than the current one will raise <see cref="OnValueChanged"/>.
		/// </summary>
		public T Value
		{
			get => _value;
			set
			{
				var lastValue = _value;
				_value = value;

				if (NotifyCondition == NotifyCondition.Never) return;
				if (NotifyCondition == NotifyCondition.Always)
				{
					OnValueChanged?.Invoke( lastValue );
					NetworkDirty( nameof(Value), NetVarGroup.Net );
					return;
				}

				if (lastValue is IEquatable<T> last && value is IEquatable<T> newValue)
				{
					if (!last.Equals( (T)newValue ))
					{
						OnValueChanged?.Invoke( lastValue );
						NetworkDirty( nameof(Value), NetVarGroup.Net );
					}
				}
				else if (!value.Equals( lastValue ))
				{
					OnValueChanged?.Invoke( lastValue );
					NetworkDirty( nameof(Value), NetVarGroup.Net );
				}
			}
		}

		/// <summary>
		/// Determines when to raise the <see cref="OnValueChanged"/> event.
		/// </summary>
		public NotifyCondition NotifyCondition { get; set; } = NotifyCondition.OnChange;

		/// <summary>
		/// Create a new networked variable with a callback on change.
		/// </summary>
		public NetworkVar()
		{
			_value = default;
		}

		/// <summary>
		/// Create a new networked variable with a callback on change, with a default value.
		/// </summary>
		/// <param name="defaultValue"> Default value to set this networked variable to. </param>
		public NetworkVar( T defaultValue )
		{
			_value = defaultValue;
		}

		public override bool NetWrite( NetWrite write )
		{
			base.NetWrite( write );
			write.Write( _value );

			return true;
		}

		public override bool NetRead( NetRead read )
		{
			base.NetRead( read );
			Value = read.Read<T>();

			return true;
		}

		public bool Equals( T? other )
		{
			return Equals( (object)other );
		}

		public override bool Equals( object obj )
		{
			if (ReferenceEquals( null, obj ))
				return false;
			if (ReferenceEquals( this, obj ))
				return true;
			if (obj.GetType() != this.GetType())
				return false;
			return Equals( (NetworkVar<T>)obj );
		}

		protected bool Equals( NetworkVar<T> other )
		{
			return EqualityComparer<T>.Default.Equals( _value, other._value );
		}

		public override int GetHashCode()
		{
			return EqualityComparer<T>.Default.GetHashCode( _value );
		}

		public static implicit operator T( NetworkVar<T> networkVar ) => networkVar.Value;
	}
}
