﻿using System;

namespace L5Sharp.Core;

/// <summary>
/// Represents a <b>LREAL</b> Logix atomic data type, or a type analogous to a <see cref="double"/>.
/// </summary>
[L5XType(nameof(LREAL))]
public sealed class LREAL : AtomicType, IComparable, IConvertible, ILogixParsable<LREAL>
{
    /// <summary>
    /// The value of the underlying data parsed to the corresponding primitive value type.
    /// </summary>
    private new double Value
    {
        get
        {
            var value = Radix.ParseValue(base.Value);
            return value is double typed ? typed : (double)Convert.ChangeType(value, typeof(double));
        }
    }

    /// <summary>
    /// Creates a new default <see cref="LREAL"/> type.
    /// </summary>
    public LREAL() : base(CreateElement(nameof(LREAL), Radix.Float, 0))
    {
    }

    /// <summary>
    /// Creates a new <see cref="LREAL"/> with the provided value.
    /// </summary>
    /// <param name="value">The value to initialize the type with.</param>
    public LREAL(double value) : base(CreateElement(nameof(LREAL), Radix.Float, value))
    {
    }

    /// <summary>
    /// Creates a new <see cref="LREAL"/> value with the provided radix format.
    /// </summary>
    /// <param name="radix">The <see cref="Core.Radix"/> number format of the value.</param>
    public LREAL(Radix radix) : base(CreateElement(nameof(LREAL), radix, 0))
    {
    }

    /// <summary>
    /// Creates a new <see cref="INT"/> with the provided value.
    /// </summary>
    /// <param name="value">The value to initialize the type with.</param>
    /// <param name="radix">The optional radix format of the value.</param>
    public LREAL(double value, Radix radix) : base(CreateElement(nameof(LREAL), radix, value))
    {
    }

    /// <inheritdoc />
    public int CompareTo(object? obj)
    {
        return obj switch
        {
            null => 1,
            LREAL typed => Value.CompareTo(typed.Value),
            AtomicType atomic => Value.CompareTo((double)Convert.ChangeType(atomic, typeof(double))),
            ValueType value => Value.CompareTo((double)Convert.ChangeType(value, typeof(double))),
            _ => throw new ArgumentException($"Cannot compare logix type {obj.GetType().Name} with {GetType().Name}.")
        };
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return obj switch
        {
            LREAL value => Math.Abs(Value - value.Value) < double.Epsilon,
            AtomicType atomic => Value.Equals((double)Convert.ChangeType(atomic, typeof(double))),
            ValueType value => Value.Equals(Convert.ChangeType(value, typeof(double))),
            _ => false
        };
    }

    /// <inheritdoc />
    public override int GetHashCode() => Value.GetHashCode();
    
    /// <summary>
    /// Parses a string into a <see cref="LREAL"/> value.
    /// </summary>
    /// <param name="value">The string to parse.</param>
    /// <returns>A <see cref="LREAL"/> representing the parsed value.</returns>
    /// <exception cref="FormatException">The <see cref="Radix"/> format can not be inferred from <c>value</c>.</exception>
    public new static LREAL Parse(string value)
    {
        if (value.Contains("QNAN")) return new LREAL(double.NaN);
        
        if (double.TryParse(value, out var result))
            return new LREAL(result);

        var radix = Radix.Infer(value);
        var atomic = radix.ParseValue(value);
        var converted = (double)Convert.ChangeType(atomic, typeof(double));
        return new LREAL(converted, radix);
    }

    /// <summary>
    /// Tries to parse a string into a <see cref="LREAL"/> value.
    /// </summary>
    /// <param name="value">The string to parse.</param>
    /// <returns>The parsed <see cref="LREAL"/> value if successful; Otherwise, <c>null</c>.</returns>
    public new static LREAL? TryParse(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return default;
        
        if (value.Contains("QNAN")) return new LREAL(double.NaN);

        if (double.TryParse(value, out var primitive))
            return new LREAL(primitive);

        if (!Radix.TryInfer(value, out var radix))
            return default;

        var parsed = radix.ParseValue(value);
        var converted = (double)Convert.ChangeType(parsed, typeof(double));
        return new LREAL(converted, radix);
    }
    
    // Contains the implicit .NET conversions for the type.

    #region Conversions

    /// <summary>
    /// Converts the provided <see cref="double"/> to a <see cref="LREAL"/> value.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns>A <see cref="LREAL"/> value.</returns>
    public static implicit operator LREAL(double value) => new(value);

    /// <summary>
    /// Converts the provided <see cref="LREAL"/> to a <see cref="double"/> value.
    /// </summary>
    /// <param name="atomic">The value to convert.</param>
    /// <returns>A <see cref="double"/> type value.</returns>
    public static implicit operator double(LREAL atomic) => atomic.Value;

    /// <summary>
    /// Implicitly converts a <see cref="string"/> to a <see cref="LREAL"/> value.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns>A new <see cref="LREAL"/> value.</returns>
    public static implicit operator LREAL(string value) => Parse(value);

    /// <summary>
    /// Implicitly converts the provided <see cref="LREAL"/> to a <see cref="string"/> value.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns>A new <see cref="string"/> value.</returns>
    public static implicit operator string(LREAL value) => value.ToString();

    #endregion
    
    // Contains the IConvertible implementation for the type. I am explicitly implementing this interface for each
    // atomic type to avoid polluting the API, and to have the implementation as performant as possible.
    // To perform conversion, use the recommended .NET Convert.ChangeType() method and specify the target type.

    #region Convertible

    /// <inheritdoc />
    TypeCode IConvertible.GetTypeCode() => TypeCode.Object;

    /// <inheritdoc />
    bool IConvertible.ToBoolean(IFormatProvider? provider) => Value != 0;

    /// <inheritdoc />
    byte IConvertible.ToByte(IFormatProvider? provider) => (byte)Value;

    /// <inheritdoc />
    char IConvertible.ToChar(IFormatProvider? provider) =>  
        throw new InvalidCastException($"Conversion from {Name} to {nameof(Char)} is not supported.");

    /// <inheritdoc />
    DateTime IConvertible.ToDateTime(IFormatProvider? provider) =>
        throw new InvalidCastException($"Conversion from {Name} to {nameof(DateTime)} is not supported.");

    /// <inheritdoc />
    decimal IConvertible.ToDecimal(IFormatProvider? provider) =>
        throw new InvalidCastException($"Conversion from {Name} to {nameof(Decimal)} is not supported.");

    /// <inheritdoc />
    double IConvertible.ToDouble(IFormatProvider? provider) => Value;

    /// <inheritdoc />
    short IConvertible.ToInt16(IFormatProvider? provider) => (short)Value;

    /// <inheritdoc />
    int IConvertible.ToInt32(IFormatProvider? provider) => (int)Value;

    /// <inheritdoc />
    long IConvertible.ToInt64(IFormatProvider? provider) => (long)Value;

    /// <inheritdoc />
    sbyte IConvertible.ToSByte(IFormatProvider? provider) => (sbyte)Value;

    /// <inheritdoc />
    float IConvertible.ToSingle(IFormatProvider? provider) => (float)Value;

    /// <inheritdoc />
    string IConvertible.ToString(IFormatProvider? provider) => ToString();

    /// <inheritdoc />
    object IConvertible.ToType(Type conversionType, IFormatProvider? provider)
    {
        var convertible = (IConvertible)this;

        return Type.GetTypeCode(conversionType) switch
        {
            TypeCode.Boolean => convertible.ToBoolean(provider),
            TypeCode.Byte => convertible.ToByte(provider),
            TypeCode.Char => convertible.ToChar(provider),
            TypeCode.DateTime => convertible.ToDateTime(provider),
            TypeCode.Decimal => convertible.ToDecimal(provider),
            TypeCode.Double => convertible.ToDouble(provider),
            TypeCode.Empty => throw new ArgumentNullException(nameof(conversionType)),
            TypeCode.Int16 => convertible.ToInt16(provider),
            TypeCode.Int32 => convertible.ToInt32(provider),
            TypeCode.Int64 => convertible.ToInt64(provider),
            TypeCode.Object => ToAtomic(conversionType),
            TypeCode.SByte => convertible.ToSByte(provider),
            TypeCode.Single => convertible.ToSingle(provider),
            TypeCode.String => ToString(),
            TypeCode.UInt16 => convertible.ToUInt16(provider),
            TypeCode.UInt32 => convertible.ToUInt32(provider),
            TypeCode.UInt64 => convertible.ToUInt64(provider),
            TypeCode.DBNull => throw new InvalidCastException(
                "Conversion for type code 'DbNull' not supported by AtomicType."),
            _ => throw new InvalidCastException($"Conversion for {conversionType.Name} not supported by AtomicType.")
        };
    }

    /// <inheritdoc />
    ushort IConvertible.ToUInt16(IFormatProvider? provider) => (ushort)Value;

    /// <inheritdoc />
    uint IConvertible.ToUInt32(IFormatProvider? provider) => (uint)Value;

    /// <inheritdoc />
    ulong IConvertible.ToUInt64(IFormatProvider? provider) => (ulong)Value;

    /// <summary>
    /// Converts the current atomic type to the specified atomic type.
    /// </summary>
    /// <param name="conversionType">The atomic type to convert to.</param>
    /// <returns>A <see cref="object"/> representing the converted atomic type value.</returns>
    /// <exception cref="InvalidCastException">The specified type is not a valid atomic type.</exception>
    private object ToAtomic(Type conversionType)
    {
        if (conversionType == typeof(BOOL))
            return new BOOL(Value != 0);
        if (conversionType == typeof(SINT))
            return new SINT((sbyte)Value);
        if (conversionType == typeof(INT))
            return new INT((short)Value);
        if (conversionType == typeof(DINT))
            return new DINT((int)Value);
        if (conversionType == typeof(LINT))
            return new LINT((long)Value);
        if (conversionType == typeof(REAL))
            return new REAL((float)Value);
        if (conversionType == typeof(LREAL))
            return new LREAL(Value);
        if (conversionType == typeof(USINT))
            return new USINT((byte)Value);
        if (conversionType == typeof(UINT))
            return new UINT((ushort)Value);
        if (conversionType == typeof(UDINT))
            return new UDINT((uint)Value);
        if (conversionType == typeof(ULINT))
            return new ULINT((ulong)Value);

        throw new InvalidCastException($"Cannot convert from {GetType().Name} to {conversionType.Name}.");
    }

    #endregion
}