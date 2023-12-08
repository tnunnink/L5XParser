﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Xml.Linq;

namespace L5Sharp.Core;

/// <summary>
/// A static deserialization class for <see cref="LogixElement"/> objects and their derivatives.
/// This class uses a dictionary to cache deserialization functions for types deriving from LogixElement.
/// We are using compiled expression functions as they are more performant that invoking constructors via reflection.
/// We are also caching them for reuse so we don't have to build them each time we call <see cref="Deserialize"/>.
/// Users can register custom implementations of a <c>LogixElement</c> using <see cref="Register{TElement}"/>, which will
/// allow the type to be deserialized as calls to the type are made throughout the library.
/// </summary>
public static class LogixSerializer
{
    /// <summary>
    /// The global cache for all <see cref="LogixElement"/> object deserializer delegate functions.
    /// </summary>
    private static readonly Lazy<Dictionary<string, Func<XElement, LogixElement>>> Deserializers = new(() =>
            Introspect(typeof(LogixSerializer).Assembly).ToDictionary(k => k.Key, v => v.Value),
        LazyThreadSafetyMode.ExecutionAndPublication);

    /// <summary>
    /// Deserializes a <see cref="XElement"/> into the specified object type.
    /// </summary>
    /// <param name="element">The <see cref="XElement"/> to deserialize.</param>
    /// <typeparam name="TElement">The return type of the deserialized element.</typeparam>
    /// <returns>A new object of the specified type representing the deserialized element.</returns>
    /// <remarks>
    /// The return object must specify a public constructor accepting a <see cref="XElement"/> parameter for
    /// deserialization to work.
    /// </remarks>
    public static TElement Deserialize<TElement>(this XElement element) where TElement : LogixElement =>
        (TElement)Deserialize(element);
    
    /// <summary>
    /// Deserializes a <see cref="XElement"/> into the first matching <see cref="LogixElement"/> type found in the
    /// element hierarchy.
    /// </summary>
    /// <param name="element">The XML element to deserialize.</param>
    /// <returns>If the element is or has a parent of a known deserializable type, then a new <see cref="LogixElement"/>
    /// of the first found type in the XML tree; Otherwise, <c>null</c>.</returns>
    /// <remarks>
    /// This method will traverse the XML tree until it reaches a <c>XElement</c> with a name matching
    /// a known deserializable logic element type. Once it finds that type/element pair, it will deserialize it as that
    /// type and return the result. This is important for any feature that would require deserialization of
    /// logix elements when the type is not known at compile type or perhaps returns a collection of different element
    /// types. It is up to the caller to infer or cast the resulting element type appropriately. 
    /// </remarks>
    public static LogixElement Deserialize(this XElement element)
    {
        if (element is null) throw new ArgumentNullException(nameof(element));

        while (true)
        {
            if (Deserializers.Value.TryGetValue(element.L5XType(), out var deserializer))
                return deserializer.Invoke(element);
            element = element.Parent ??
                      throw new InvalidOperationException(
                          $"Could not find deserializable type for element {element.Name}.");
        }
    }

    /// <summary>
    /// Registers the specified logix element type to the global logix serialization cache to be used for
    /// deserializing the type.
    /// </summary>
    /// <typeparam name="TElement">The type to register.</typeparam>
    /// <exception cref="ArgumentException">The specified type is not deserializable, meaning it does not inherit
    /// from <see cref="LogixElement"/>, is not public, is abstract, or does not have a constructor taking a single
    /// <see cref="XElement"/> parameter.</exception>
    /// <exception cref="InvalidOperationException">A type with the same name (L5XType) is already registered.</exception>
    /// <remarks>
    /// Not that this is common, but this gives external users a means to register custom <see cref="LogixElement"/>
    /// derived types with the library so that they can be deserialized and used thought other parts of the library.
    /// </remarks>
    public static void Register<TElement>() where TElement : LogixElement
    {
        var type = typeof(TElement);

        if (!IsDeserializableType(type))
        {
            var explanation =
                $"The type must derive from {typeof(LogixElement)}, be public, non-abstract," +
                $" and have a constructor accepting a single {typeof(XElement)}";
            throw new ArgumentException(
                $"Type '{typeof(TElement)} is not a valid deserializable logix type. {explanation}");            
        }

        var deserializer = type.Deserializer<LogixElement>();
        var deserializers = type.L5XTypes()
            .Select(t => new KeyValuePair<string, Func<XElement, LogixElement>>(t, deserializer));

        foreach (var pair in deserializers)
        {
            if (!Deserializers.Value.TryAdd(pair.Key, pair.Value))
                throw new InvalidOperationException($"Type '{pair.Key}' is already registered with another function.");
        }
    }

    /// <summary>
    /// Performs reflection scanning of provided <see cref="Assembly"/> to get all public non abstract types
    /// inheriting from <see cref="LogixElement"/> that have the supported deserialization constructor,
    /// and returns the <c>L5XType</c> and compiled deserialization delegate pair. This is used to initialize the
    /// set of concrete deserializer functions for all known logix element objects.
    /// </summary>
    private static IEnumerable<KeyValuePair<string, Func<XElement, LogixElement>>> Introspect(Assembly assembly)
    {
        var deserializers = new List<KeyValuePair<string, Func<XElement, LogixElement>>>();

        var types = assembly.GetTypes().Where(IsDeserializableType);

        foreach (var type in types)
        {
            var deserializer = type.Deserializer<LogixElement>();
            deserializers.AddRange(type.L5XTypes()
                .Select(t => new KeyValuePair<string, Func<XElement, LogixElement>>(t, deserializer)));
        }

        return deserializers;
    }

    /// <summary>
    /// Checks whether the type is deserializable by this library. This means that it inherits <see cref="LogixElement"/>,
    /// is a public non-abstract type, and has a constructor accepting a single <see cref="XElement"/> parameter.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <returns><c>true</c> if the type is deserializable; otherwise, <c>false</c>.</returns>
    private static bool IsDeserializableType(Type type)
    {
        return typeof(LogixElement).IsAssignableFrom(type) &&
               type is { IsAbstract: false, IsPublic: true } &&
               type.GetConstructor(BindingFlags.Public | BindingFlags.Instance, null, new[] { typeof(XElement) },
                   null) is not null;
    }
}