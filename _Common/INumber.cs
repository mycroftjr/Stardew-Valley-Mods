using System;

namespace Shockah.CommonModCode
{
	public interface INumberDomain<T> where T: notnull, INumber<T>, IEquatable<T>, IComparable<T>
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Nested in another interface")]
		public interface WithZero: INumberDomain<T>
		{
			T Zero { get; }
		}
	}
	
	public interface INumber<T>: IEquatable<T>, IComparable<T> where T: notnull, INumber<T>, IEquatable<T>, IComparable<T>
	{
		public static bool operator <(INumber<T> left, INumber<T> right)
			=> left.CompareTo((T)right) < 0;

		public static bool operator <=(INumber<T> left, INumber<T> right)
			=> left.CompareTo((T)right) <= 0;

		public static bool operator >(INumber<T> left, INumber<T> right)
			=> left.CompareTo((T)right) > 0;

		public static bool operator >=(INumber<T> left, INumber<T> right)
			=> left.CompareTo((T)right) >= 0;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Nested in another interface")]
		public interface WithZeroDomain: INumber<T>
		{
			INumberDomain<T>.WithZero Domain { get; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Nested in another interface")]
		public interface WithAddition: INumber<T>
		{
			T Add(T other);
			T Subtract(T other);
			T Negate();

			public static T operator +(WithAddition lhs, T rhs)
				=> lhs.Add(rhs);

			public static T operator -(WithAddition lhs, T rhs)
				=> lhs.Subtract(rhs);

			public static T operator -(WithAddition v)
				=> v.Negate();
		}
	}

	public readonly struct IntNumberDomain: INumberDomain<IntNumber>.WithZero
	{
		public static IntNumberDomain Instance => new();
		public readonly IntNumber Zero => new(0);
	}

	public readonly struct IntNumber: INumber<IntNumber>.WithZeroDomain, INumber<IntNumber>.WithAddition
	{
		public INumberDomain<IntNumber>.WithZero Domain
			=> IntNumberDomain.Instance;

		public readonly int Value;

		public IntNumber(int value)
		{
			this.Value = value;
		}

		public int CompareTo(IntNumber other)
			=> Value.CompareTo(other.Value);

		public bool Equals(IntNumber other)
			=> Value == other.Value;

		public override bool Equals(object? obj)
			=> obj is IntNumber other && Equals(other);

		public override int GetHashCode()
			=> Value;

		public IntNumber Add(IntNumber other)
			=> new(Value + other.Value);

		public IntNumber Subtract(IntNumber other)
			=> new(Value - other.Value);

		public IntNumber Negate()
			=> new(-Value);

		public static bool operator ==(IntNumber left, IntNumber right)
			=> left.Equals(right);

		public static bool operator !=(IntNumber left, IntNumber right)
			=> !(left == right);

		public static bool operator <(IntNumber left, IntNumber right)
			=> left.CompareTo(right) < 0;

		public static bool operator <=(IntNumber left, IntNumber right)
			=> left.CompareTo(right) <= 0;

		public static bool operator >(IntNumber left, IntNumber right)
			=> left.CompareTo(right) > 0;

		public static bool operator >=(IntNumber left, IntNumber right)
			=> left.CompareTo(right) >= 0;

		public static implicit operator int(IntNumber v)
			=> v.Value;

		public static implicit operator IntNumber(int v)
			=> new(v);
	}
}
