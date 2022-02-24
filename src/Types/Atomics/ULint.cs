﻿using System;
using System.ComponentModel;
using L5Sharp.Converters;
using L5Sharp.Enums;

namespace L5Sharp.Types.Atomics
{
    /// <summary>
    /// Represents a <b>ULINT</b> Logix atomic data type, or a type analogous to a <see cref="ulong"/>.
    /// </summary>
    [TypeConverter(typeof(ULintConverter))]
    public class ULint : IAtomicType<ulong>, IEquatable<ULint>, IComparable<ULint>
    {
        /// <summary>
        /// Creates a new default <see cref="ULint"/> type.
        /// </summary>
        public ULint()
        {
            Name = nameof(ULint).ToUpper();
            Value = default;
        }

        /// <summary>
        /// Creates a new <see cref="ULint"/> with the provided value.
        /// </summary>
        /// <param name="value">The value to initialize the type with.</param>
        public ULint(ulong value) : this()
        {
            Value = value;
        }

        /// <inheritdoc />
        public string Name { get; }

        /// <inheritdoc />
        public string Description => $"Logix representation of a {typeof(ulong)}";

        /// <inheritdoc />
        public DataTypeFamily Family => DataTypeFamily.None;

        /// <inheritdoc />
        public DataTypeClass Class => DataTypeClass.Atomic;

        /// <inheritdoc />
        public ulong Value { get; private set; }

        object IAtomicType.Value => Value;

        /// <inheritdoc />
        public void SetValue(ulong value) => Value = value;

        /// <inheritdoc />
        public void SetValue(object value)
        {
            if (value is null)
                throw new ArgumentNullException(nameof(value));

            var converter = TypeDescriptor.GetConverter(GetType());

            if (!converter.CanConvertFrom(value.GetType()))
                throw new ArgumentException($"Value of type '{value.GetType()}' is not a valid for {GetType()}");

            Value = (ULint)converter.ConvertFrom(value)!;
        }

        /// <inheritdoc />
        public string Format(Radix? radix = null) =>
            radix is not null ? radix.Format(this) : Radix.Default(this).Format(this);

        /// <inheritdoc />
        public IDataType Instantiate() => new ULint();

        /// <summary>
        /// Converts the provided <see cref="ulong"/> to a <see cref="ULint"/> value.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>A <see cref="ULint"/> value.</returns>
        public static implicit operator ULint(ulong value) => new(value);

        /// <summary>
        /// Converts the provided <see cref="ULint"/> to a <see cref="ulong"/> value.
        /// </summary>
        /// <param name="atomic">The value to convert.</param>
        /// <returns>A <see cref="ulong"/> type value.</returns>
        public static implicit operator ulong(ULint atomic) => atomic.Value;

        /// <summary>
        /// Converts the provided <see cref="string"/> to a <see cref="ULint"/> value. 
        /// </summary>
        /// <param name="input">The string value to convert.</param>
        /// <returns>
        /// If the string value is able to be parsed, a new instance of a <see cref="ULint"/> with the value
        /// provided. If not, then a default instance value.
        /// </returns>
        public static implicit operator ULint(string input) =>
            ulong.TryParse(input, out var result) ? new ULint(result) : Radix.ParseValue<ULint>(input);

        /// <inheritdoc />
        public bool Equals(ULint? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Value == other.Value;
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((ULint)obj);
        }

        /// <inheritdoc />
        public override int GetHashCode() => Name.GetHashCode();

        /// <inheritdoc />
        public override string ToString() => Name;

        /// <summary>
        /// Determines whether the objects are equal.
        /// </summary>
        /// <param name="left">An object to compare.</param>
        /// <param name="right">An object to compare.</param>
        /// <returns>true if the objects are equal, otherwise, false.</returns>
        public static bool operator ==(ULint left, ULint right) => Equals(left, right);

        /// <summary>
        /// Determines whether the objects are not equal.
        /// </summary>
        /// <param name="left">An object to compare.</param>
        /// <param name="right">An object to compare.</param>
        /// <returns>true if the objects are not equal, otherwise, false.</returns>
        public static bool operator !=(ULint left, ULint right) => !Equals(left, right);

        /// <inheritdoc />
        public int CompareTo(ULint? other)
        {
            if (ReferenceEquals(this, other)) return 0;
            return ReferenceEquals(null, other) ? 1 : Value.CompareTo(other.Value);
        }
    }
}