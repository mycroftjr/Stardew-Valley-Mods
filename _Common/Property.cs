namespace Shockah.CommonModCode
{
	public sealed class Property<T>
	{
		public delegate void ValueChangedDelegate(T oldValue, T newValue);

		public T Value
		{
			get => _value;
			set
			{
				if (Equals(_value, value))
					return;
				T oldValue = _value;
				_value = value;
				ValueChanged?.Invoke(oldValue, value);
			}
		}

		public event ValueChangedDelegate? ValueChanged;

		private T _value;

		public Property(T initialValue)
		{
			this._value = initialValue;
		}

		public override string ToString()
			=> $"{Value}";

		public override int GetHashCode()
			=> Value?.GetHashCode() ?? 0;

		public static implicit operator T(Property<T> property) => property.Value;
	}
}
