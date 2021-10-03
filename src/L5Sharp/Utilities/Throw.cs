﻿using System;
using L5Sharp.Abstractions;
using L5Sharp.Enumerations;
using L5Sharp.Exceptions;

namespace L5Sharp.Utilities
{
    internal static class Throw
    {
        public static void ArgumentNullOrEmptyException(string paramName)
            => throw new ArgumentException("Argument cannot be null or empty.", paramName);

        public static void InvalidNameException(string tagName) =>
            throw new InvalidNameException(
                $"Name {tagName} is not valid. " +
                $"Must contain alphanumeric, start with letter, and contains only '_' special characters");
        
        public static void PredefinedCollisionException(string dataType) =>
            throw new PredefinedCollisionException(
                $"Data type {dataType} already exists either as a predefined type on in the current controller context");
        
        public static void DataTypeNotFoundException(string dataType) =>
            throw new DataTypeNotFoundException(
                $"Data type '{dataType}' does not exist in the current controller context");

        public static void NameCollisionException(string name, Type type) =>
            throw new NameCollisionException(
                $"Name {name} already exists for {type}. Name must be unique.");
        
        public static void ItemNotFoundException(string name) =>
            throw new ItemNotFoundException(
                $"Item {name} does not exists on this type");

        public static void NotConfigurableException(string propertyName, string baseType, string reason) =>
            throw new NotConfigurableException(
                $"The property {propertyName} is not not configurable for this instance of {baseType}. {reason}");
        
        public static void RadixNotSupportedException(Radix radix, IDataType dataType) =>
            throw new RadixNotSupportedException($"Radix {radix.Name} not supported by type {dataType.Name}");
    }
}