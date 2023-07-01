﻿using System;
using System.ComponentModel;
using L5Sharp.Enums;
using L5Sharp.Types.Atomics.Converters;

namespace L5Sharp.Types.Atomics;

/// <summary>
/// Represents a <b>USINT</b> Logix atomic data type, or a type analogous to a <see cref="byte"/>.
/// </summary>
[TypeConverter(typeof(USintConverter))]
public sealed class USINT : AtomicType, IEquatable<USINT>, IComparable<USINT>, IComparable
{
    private byte Number => ToBytes()[0];

    /// <summary>
    /// Creates a new default <see cref="USINT"/> type.
    /// </summary>
    public USINT() : base(nameof(USINT), Radix.Decimal, new[] { default(byte) })
    {
    }

    /// <summary>
    /// Creates a new <see cref="USINT"/> value with the provided radix format.
    /// </summary>
    /// <param name="radix">The <see cref="Enums.Radix"/> number format of the value.</param>
    public USINT(Radix radix) : base(nameof(USINT), radix, new[] { default(byte) })
    {
    }

    /// <summary>
    /// Creates a new <see cref="USINT"/> with the provided value.
    /// </summary>
    /// <param name="value">The value to initialize the type with.</param>
    /// <param name="radix">The optional radix format of the value.</param>
    public USINT(byte value, Radix? radix = null) : base(nameof(USINT), radix ?? Radix.Decimal, new[] { value })
    {
    }

    /// <summary>
    /// Gets a <see cref="BOOL"/> at the specified bit index.
    /// </summary>
    /// <param name="bit">The bit index to access</param>
    public BOOL this[int bit] => new(ToBits()[bit]);

    /// <summary>
    /// Represents the largest possible value of <see cref="USINT"/>.
    /// </summary>
    public const byte MaxValue = byte.MaxValue;

    /// <summary>
    /// Represents the smallest possible value of <see cref="USINT"/>.
    /// </summary>
    public const byte MinValue = byte.MinValue;

    /// <summary>
    /// Parses the provided string value to a new <see cref="USINT"/>.
    /// </summary>
    /// <param name="value">The string value to parse.</param>
    /// <returns>A <see cref="USINT"/> representing the parsed value.</returns>
    /// <exception cref="FormatException">The <see cref="Radix"/> format can not be inferred from <c>value</c>.</exception>
    public static USINT Parse(string value)
    {
        if (byte.TryParse(value, out var result))
            return new USINT(result);

        var radix = Radix.Infer(value);
        var atomic = radix.Parse(value);
        var converted = (TypeDescriptor.GetConverter(typeof(USINT)).ConvertFrom(atomic) as USINT)!;
        return new USINT(converted, radix);
    }

    /// <inheritdoc />
    public bool Equals(USINT? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return Number == other.Number;
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        switch (obj)
        {
            case USINT value:
                return Number.Equals(value.Number);
            case AtomicType atomic:
                var converted = TypeDescriptor.GetConverter(GetType()).ConvertFrom(atomic) as USINT;
                return Number.Equals(converted?.Number);
            default:
                return false;
        }
    }

    /// <inheritdoc />
    public override int GetHashCode() => Number.GetHashCode();

    /// <summary>
    /// Determines whether the objects are equal.
    /// </summary>
    /// <param name="left">An object to compare.</param>
    /// <param name="right">An object to compare.</param>
    /// <returns>true if the objects are equal, otherwise, false.</returns>
    public static bool operator ==(USINT left, USINT right) => Equals(left, right);

    /// <summary>
    /// Determines whether the objects are not equal.
    /// </summary>
    /// <param name="left">An object to compare.</param>
    /// <param name="right">An object to compare.</param>
    /// <returns>true if the objects are not equal, otherwise, false.</returns>
    public static bool operator !=(USINT left, USINT right) => !Equals(left, right);

    /// <inheritdoc />
    public int CompareTo(USINT? other)
    {
        if (ReferenceEquals(this, other)) return 0;
        return ReferenceEquals(null, other) ? 1 : Number.CompareTo(other.Number);
    }
    
    /// <inheritdoc />
    public int CompareTo(object obj)
    {
        switch (obj)
        {
            case null:
                return 1;
            case USINT value:
                return Number.CompareTo(value.Number);
            case AtomicType atomic:
                var converted = TypeDescriptor.GetConverter(GetType()).ConvertFrom(atomic) as USINT;
                return Number.CompareTo(converted?.Number);
            default:
                throw new ArgumentException($"Cannot compare object of type {obj.GetType()} with {GetType()}.");
        }
    }

    #region Conversions

    /// <summary>
    /// Converts the provided <see cref="byte"/> to a <see cref="SINT"/> value.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns>A <see cref="SINT"/> value.</returns>
    public static implicit operator USINT(byte value) => new(value);

    /// <summary>
    /// Converts the provided <see cref="SINT"/> to a <see cref="byte"/> value.
    /// </summary>
    /// <param name="atomic">The value to convert.</param>
    /// <returns>A <see cref="byte"/> type value.</returns>
    public static implicit operator byte(USINT atomic) => atomic.Number;

    /// <summary>
    /// Implicitly converts a <see cref="string"/> to a <see cref="ULINT"/> value.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns>A new <see cref="ULINT"/> value.</returns>
    public static implicit operator USINT(string value) => Parse(value);

    /// <summary>
    /// Implicitly converts the provided <see cref="ULINT"/> to a <see cref="string"/> value.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns>A new <see cref="string"/> value.</returns>
    public static implicit operator string(USINT value) => value.ToString();

    /// <summary>
    /// Converts the provided <see cref="USINT"/> to a <see cref="BOOL"/> value.
    /// </summary>
    /// <param name="atomic">The value to convert.</param>
    /// <returns>A <see cref="BOOL"/> type value.</returns>
    public static explicit operator BOOL(USINT atomic) => new(atomic.Number != 0);

    /// <summary>
    /// Converts the provided <see cref="USINT"/> to a <see cref="SINT"/> value.
    /// </summary>
    /// <param name="atomic">The value to convert.</param>
    /// <returns>A <see cref="SINT"/> type value.</returns>
    public static explicit operator SINT(USINT atomic) => new((sbyte)atomic.Number);

    /// <summary>
    /// Converts the provided <see cref="USINT"/> to a <see cref="INT"/> value.
    /// </summary>
    /// <param name="atomic">The value to convert.</param>
    /// <returns>A <see cref="INT"/> type value.</returns>
    public static implicit operator INT(USINT atomic) => new(atomic.Number);

    /// <summary>
    /// Converts the provided <see cref="USINT"/> to a <see cref="UINT"/> value.
    /// </summary>
    /// <param name="atomic">The value to convert.</param>
    /// <returns>A <see cref="UINT"/> type value.</returns>
    public static implicit operator UINT(USINT atomic) => new(atomic.Number);

    /// <summary>
    /// Converts the provided <see cref="USINT"/> to a <see cref="DINT"/> value.
    /// </summary>
    /// <param name="atomic">The value to convert.</param>
    /// <returns>A <see cref="DINT"/> type value.</returns>
    public static implicit operator DINT(USINT atomic) => new(atomic.Number);

    /// <summary>
    /// Converts the provided <see cref="USINT"/> to a <see cref="UDINT"/> value.
    /// </summary>
    /// <param name="atomic">The value to convert.</param>
    /// <returns>A <see cref="UDINT"/> type value.</returns>
    public static implicit operator UDINT(USINT atomic) => new(atomic.Number);

    /// <summary>
    /// Converts the provided <see cref="USINT"/> to a <see cref="LINT"/> value.
    /// </summary>
    /// <param name="atomic">The value to convert.</param>
    /// <returns>A <see cref="LINT"/> type value.</returns>
    public static implicit operator LINT(USINT atomic) => new(atomic.Number);

    /// <summary>
    /// Converts the provided <see cref="USINT"/> to a <see cref="ULINT"/> value.
    /// </summary>
    /// <param name="atomic">The value to convert.</param>
    /// <returns>A <see cref="ULINT"/> type value.</returns>
    public static implicit operator ULINT(USINT atomic) => new(atomic.Number);

    /// <summary>
    /// Converts the provided <see cref="USINT"/> to a <see cref="REAL"/> value.
    /// </summary>
    /// <param name="atomic">The value to convert.</param>
    /// <returns>A <see cref="REAL"/> type value.</returns>
    public static implicit operator REAL(USINT atomic) => new(atomic.Number);

    #endregion
}