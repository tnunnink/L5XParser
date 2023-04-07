﻿using System;
using System.Globalization;
using L5Sharp.Components;

namespace L5Sharp.Core;

/// <summary>
/// A configurable property of a <see cref="LogixTask"/> that controls the rate at which the task will be evaluated or scanned.
/// </summary>
/// <remarks>
/// <see cref="ScanRate"/> is a simple float value that must be between 0.1 and 2,000,000.0ms.
/// Attempting to set the <see cref="ScanRate"/> to a value outside that range will result in an
/// <see cref="ArgumentOutOfRangeException"/>.
/// This parameter will control the rate at which the <see cref="LogixTask"/> component is scanned.
/// </remarks>
public readonly struct ScanRate : IEquatable<ScanRate>
{
    private readonly float _rate;

    /// <summary>
    /// Creates a new instance of <see cref="ScanRate"/> with the specified rate value.
    /// </summary>
    /// <param name="rate">The scan rate value in milliseconds. Valid range is between 0.1 and 2M</param>
    /// <exception cref="ArgumentOutOfRangeException">Throw when the provided rate is outside the specified range</exception>
    public ScanRate(float rate)
    {
        if (rate is < 0.1f or > 2000000.0f)
            throw new ArgumentOutOfRangeException(nameof(rate),
                "Rate must be value between 0.1 and 2,000,000.0 ms");

        _rate = rate;
    }

    /// <summary>
    /// Converts a <see cref="ScanRate"/> to a <see cref="float"/>.
    /// </summary>
    /// <param name="rate">The value to convert.</param>
    /// <returns>A <see cref="float"/> value.</returns>
    public static implicit operator float(ScanRate rate) => rate._rate;

    /// <summary>
    /// Converts a <see cref="float"/> to a <see cref="ScanRate"/>.
    /// </summary>
    /// <param name="rate">The value to convert.</param>
    /// <returns>A <see cref="ScanRate"/> value.</returns>
    public static implicit operator ScanRate(float rate) => new(rate);

    /// <summary>
    /// Parses a string value into a <see cref="ScanRate"/>.
    /// </summary>
    /// <param name="str">The string to parse.</param>
    /// <returns>A <see cref="ScanRate"/> value if the parse was successful; otherwise; the default value.</returns>
    public static ScanRate Parse(string str) =>
        float.TryParse(str, out var result) ? new ScanRate(result) : default;

    /// <inheritdoc />
    public override string ToString() => _rate.ToString(CultureInfo.CurrentCulture);

    /// <inheritdoc />
    public bool Equals(ScanRate other) => _rate.Equals(other._rate);

    /// <inheritdoc />
    public override bool Equals(object obj) => obj is ScanRate other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => _rate.GetHashCode();

    /// <summary>
    /// Determines if the provided objects are equal.
    /// </summary>
    /// <param name="left">An object to compare.</param>
    /// <param name="right">An object to compare.</param>
    /// <returns>true if the provided objects are equal; otherwise, false.</returns>
    public static bool operator ==(ScanRate left, ScanRate right) => Equals(left, right);

    /// <summary>
    /// Determines if the provided objects are not equal.
    /// </summary>
    /// <param name="left">An object to compare.</param>
    /// <param name="right">An object to compare.</param>
    /// <returns>true if the provided objects are not equal; otherwise, false.</returns>
    public static bool operator !=(ScanRate left, ScanRate right) => !Equals(left, right);
}