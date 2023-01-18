﻿using System.Collections.Generic;
using L5Sharp.Attributes;
using L5Sharp.Core;
using L5Sharp.Enums;
using L5Sharp.Extensions;
using L5Sharp.Serialization;
using L5Sharp.Types.Atomics;

namespace L5Sharp.Types
{
    /// <summary>
    /// A logix type that represents a string structure type.
    /// </summary>
    [LogixSerializer(typeof(StringSerializer))]
    public class StringType : StructureType
    {
        /// <summary>
        /// Creates a new <see cref="StringType"/> instance with the provided data.
        /// </summary>
        /// <param name="name">The name of the string type.</param>
        /// <param name="value">The predefined string length of the type.</param>
        /// <param name="description">The optional description of the string type.</param>
        public StringType(string name, string value, string? description = null) : base(name, description)
        {
            LEN = new DINT(value.Length);
            DATA = value.ToSintArray();
        }

        /// <inheritdoc />
        public sealed override DataTypeFamily Family => DataTypeFamily.String;

        /// <inheritdoc />
        public override IEnumerable<Member> Members()
        {
            return new List<Member>
            {
                new(nameof(LEN), LEN),
                new(nameof(DATA), new ArrayType<SINT>(DATA), radix: Radix.Ascii)
            };
        }

        /// <summary>
        /// Gets the character length value of the string. 
        /// </summary>
        /// <returns>A <see cref="DINT"/> logix atomic value representing the integer length of the string.</returns>
        public DINT LEN { get; }

        /// <summary>
        /// Gets the array of bytes that represent the ASCII encoded string value.
        /// </summary>
        /// <returns>An array of <see cref="SINT"/> logix atomic values representing the bytes of the string.</returns>
        public SINT[] DATA { get; }
    }
}