namespace NetworkWrappers
{
	/// <summary>
	/// Decides when to raise the <see cref="NetworkVar{T}.OnValueChanged"/> event of a NetworkVar.
	/// </summary>
	public enum NotifyCondition
	{
		/// <summary>
		/// Do not raise any change event.
		/// </summary>
		Never,
		/// <summary>
		/// Raise it when the value changes.
		/// </summary>
		OnChange,
		/// <summary>
		/// Raise it when the value is set.
		/// </summary>
		Always,
	}
}
