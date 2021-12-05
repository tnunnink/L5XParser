﻿using System;
using System.Xml.Serialization;
using L5Sharp.Enums;

namespace L5Sharp.Core
{
    /// <inheritdoc cref="IMember{TDataType}" />
    public sealed class Member<TDataType> : IMember<TDataType>, IEquatable<Member<TDataType>>
        where TDataType : IDataType
    {
        internal Member(string name, TDataType dataType, Radix? radix,
            ExternalAccess? externalAccess, string? description)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            DataType = dataType;
            Radix = radix ?? Radix.Default(dataType);
            ExternalAccess = externalAccess ?? ExternalAccess.ReadWrite;
            Description = description ?? string.Empty;
        }

        /// <inheritdoc />
        public string Name { get; }

        /// <inheritdoc />
        public string Description { get; }

        /// <inheritdoc />
        public TDataType DataType { get; }

        /// <inheritdoc />
        [XmlAttribute("Dimension")]
        public Dimensions Dimensions => Dimensions.Empty;

        /// <inheritdoc />
        public Radix Radix { get; }

        /// <inheritdoc />
        public ExternalAccess ExternalAccess { get; }
        
        /// <inheritdoc />
        public IMember<TDataType> Copy()
        {
            return new Member<TDataType>(string.Copy(Name), (TDataType)DataType.Instantiate(), Radix, ExternalAccess,
                string.Copy(Description));
        }

        /// <inheritdoc />
        public bool Equals(Member<TDataType>? other)
        {
            if (ReferenceEquals(other, null)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Name == other.Name && Equals(DataType, other.DataType) && Dimensions == other.Dimensions &&
                   Equals(Radix, other.Radix) && Equals(ExternalAccess, other.ExternalAccess) &&
                   Description == other.Description;
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(obj, null)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((Member<TDataType>)obj);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HashCode.Combine(Name, DataType, Dimensions, Radix, ExternalAccess, Description);
        }

        /// <summary>
        /// Determines whether two objects are equal. 
        /// </summary>
        /// <param name="left">The left object to compare.</param>
        /// <param name="right">The right object to compare.</param>
        /// <returns>True if the left object is equal to the right object. Otherwise, False</returns>
        public static bool operator ==(Member<TDataType> left, Member<TDataType> right)
        {
            return Equals(left, right);
        }

        /// <summary>
        /// Determines whether two objects are not equal. 
        /// </summary>
        /// <param name="left">The left object to compare.</param>
        /// <param name="right">The right object to compare.</param>
        /// <returns>True if the left object is not equal to the right object. Otherwise, False</returns>
        public static bool operator !=(Member<TDataType> left, Member<TDataType> right)
        {
            return !Equals(left, right);
        }
    }
}