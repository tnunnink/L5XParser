﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace L5Sharp.Core;

/// <summary>
/// A generic collection that provides operations over an underlying <see cref="XElement"/> container
/// of <see cref="LogixElement"/> objects.
/// </summary>
/// <typeparam name="TObject">The type inheriting <see cref="LogixElement"/>.</typeparam>
/// <remarks>
/// <para>
/// This class represents a wrapper around a L5X element that contains a sequence of child elements of the same type.
/// This class exposes collection methods for querying and modifying child elements of the container.
/// Note that a container could potentially contain more than one type of child element, but this container class will
/// only operate over the specified element type.
/// </para>
/// <para>
/// The class is designed to only offer very basic operations, allowing it to be applicable to all container type elements,
/// However, the user can extended the API for any container type using extension methods and <see cref="Serialize"/>
/// to get the underlying <see cref="XElement"/> container object. See <see cref="ContainerExtensions"/> for examples.
/// </para>
/// </remarks>
public class LogixContainer<TObject> : IEnumerable<TObject>, ILogixSerializable where TObject : LogixObject
{
    /// <summary>
    /// The underlying <see cref="XElement"/> representing the backing data for the container. Use this object to store
    /// and retrieve data for the the collection.
    /// </summary>
    private readonly XElement _element;

    /// <summary>
    /// The type name of the child elements in the container. This is needed as we support containers with different
    /// element types, so we need to know the name of the correct type to return and deserialize.
    /// This also allows types with secondary element names to be synced as they are added to the container. 
    /// </summary>
    private readonly XName _type;

    /// <summary>
    /// Creates a empty <see cref="LogixContainer{TElement}"/> with the default type name.
    /// </summary>
    public LogixContainer()
    {
        _element = new XElement(typeof(TObject).L5XContainer());
        _type = typeof(TObject).L5XType();
    }

    /// <summary>
    /// Creates a new <see cref="LogixContainer{TComponent}"/> initialized with the provided <see cref="XElement"/>. 
    /// </summary>
    /// <param name="container">The <see cref="XElement"/> containing a collection of elements.</param>
    /// <param name="type">Optionally the type name of the child elements of the container. Will default to the
    /// configured L5XType name if not provided.</param>
    /// <exception cref="ArgumentNullException"><c>container</c> is null.</exception>
    public LogixContainer(XElement container, XName? type = null)
    {
        _element = container ?? throw new ArgumentNullException(nameof(container));
        _type = type ?? typeof(TObject).L5XType();
    }

    /// <summary>
    /// Creates a empty <see cref="LogixContainer{TElement}"/> with the specified type name.
    /// </summary>
    /// <param name="name">The name of the container element.</param>
    /// <param name="type">Optionally the type name of the child elements of the container. Will default to the
    /// configured L5XType name if not provided.</param>
    public LogixContainer(XName name, XName? type = null)
    {
        _element = new XElement(name);
        _type = type ?? typeof(TObject).L5XType();
    }

    /// <summary>
    /// Creates a new <see cref="LogixContainer{TComponent}"/> initialized with the provided collection.
    /// </summary>
    /// <param name="elements">The collection of elements to initialize.</param>
    public LogixContainer(IEnumerable<TObject> elements) : this()
    {
        if (elements is null)
            throw new ArgumentNullException(nameof(elements));

        foreach (var element in elements)
        {
            if (element is null)
                throw new ArgumentNullException(nameof(element));

            var xml = element.L5XType == _type
                ? element.Serialize()
                : element.Convert<TObject>(_type.LocalName).Serialize();

            _element.Add(xml);
        }
    }

    /// <summary>
    /// Indicates whether this container is attached to a L5X document.
    /// </summary>
    /// <value><c>true</c> if this is an attached container; Otherwise, <c>false</c>.</value>
    /// <remarks>
    /// This simply looks to see if the element has a ancestor with the root RSLogix5000Content element or not.
    /// If so we will assume this element is attached to an overall L5X document.
    /// </remarks>
    public bool IsAttached => _element.Ancestors(L5XName.RSLogix5000Content).Any();

    /// <summary>
    /// Returns the <see cref="L5X"/> instance this <see cref="LogixElement"/> is attached to if it is attached. 
    /// </summary>
    /// <returns>
    /// If the current element is attached to a L5X document (i.e. has the root content element),
    /// then the <see cref="L5X"/> instance; Otherwise, <c>null</c>.
    /// </returns>
    /// <remarks>
    /// This allows attached logix elements to reach up to the L5X file in order to traverse or retrieve
    /// other elements in the L5X. This is helpful/used for other extensions and cross referencing functions.
    /// </remarks>
    public L5X? L5X => _element.Ancestors(L5XName.RSLogix5000Content).FirstOrDefault()?.Annotation<L5X>();

    /// <summary>
    /// Accesses a single element at the specified index of the container collection.
    /// </summary>
    /// <param name="index">The zero based index of the element to retrieve.</param>
    /// <returns>The element at the specified position in the source container.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><c>index</c> is less than zero or greater than or equal to the
    /// number of elements in the collection.</exception>
    /// <exception cref="ArgumentNullException"><c>value</c> is null when setting index.</exception>
    public TObject this[int index]
    {
        get => _element.Elements(_type).ElementAt(index).Deserialize<TObject>();
        set
        {
            if (value is null)
                throw new ArgumentNullException(nameof(value),
                    $"Can not set container element of type {typeof(TObject)} null instance.");

            var xml = value.L5XType == _type
                ? value.Serialize()
                : value.Convert<TObject>(_type.LocalName).Serialize();

            _element.Elements(_type).ElementAt(index).ReplaceWith(xml);
        }
    }

    /// <summary>
    /// Adds the provided element to the logix container at the end of the collection.
    /// </summary>
    /// <param name="element">The element to add.</param>
    /// <exception cref="ArgumentNullException"><c>element</c> is null.</exception>
    public void Add(TObject element)
    {
        if (element is null)
            throw new ArgumentNullException(nameof(element));

        var xml = element.L5XType == _type
            ? element.Serialize()
            : element.Convert<TObject>(_type.LocalName).Serialize();

        var last = _element.Elements(_type).LastOrDefault();

        if (last is null)
        {
            _element.Add(xml);
            return;
        }

        last.AddAfterSelf(xml);
    }

    /// <summary>
    /// Adds the provided elements to the logix container at the end of the collection.
    /// </summary>
    /// <param name="elements">The collection of elements to add.</param>
    /// <exception cref="ArgumentNullException"><c>elements</c> or any element in <c>elements</c> is null.</exception>
    public void AddRange(IEnumerable<TObject> elements)
    {
        if (elements is null)
            throw new ArgumentNullException(nameof(elements));

        foreach (var element in elements)
        {
            if (element is null)
                throw new ArgumentNullException(nameof(element));

            var xml = element.L5XType == _type
                ? element.Serialize()
                : element.Convert<TObject>(_type.LocalName).Serialize();

            var last = _element.Elements(_type).LastOrDefault();

            if (last is null)
            {
                _element.Add(xml);
                continue;
            }

            last.AddAfterSelf(xml);
        }
    }

    /// <summary>
    /// Gets the number of elements in the collection.
    /// </summary>
    /// <returns>A <see cref="int"/> representing the number of elements in the collection.</returns>
    public int Count() => _element.Elements(_type).Count();

    /// <summary>
    /// Inserts the provided element at the specified index of the container collection.
    /// </summary>
    /// <param name="index">The zero based index at which to insert the element.</param>
    /// <param name="element">The element to insert.</param>
    /// <exception cref="ArgumentNullException"><c>element</c> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><c>index</c> is less than zero or greater than or equal to the
    /// number of elements in the collection.</exception>
    public void Insert(int index, TObject element)
    {
        if (element is null)
            throw new ArgumentNullException(nameof(element));

        var xml = element.L5XType == _type
            ? element.Serialize()
            : element.Convert<TObject>(_type.LocalName).Serialize();

        _element.Elements(_type).ElementAt(index).AddBeforeSelf(xml);
    }

    /// <summary>
    /// Removes all elements in the container collection.
    /// </summary>
    public void RemoveAll() => _element.Elements(_type).Remove();

    /// <summary>
    /// Removes all elements that satisfy the provided condition predicate.
    /// </summary>
    /// <param name="condition">The condition for which to remove elements.</param>
    /// <exception cref="ArgumentNullException"><c>condition</c> is null.</exception>
    public void RemoveAll(Func<TObject, bool> condition)
    {
        if (condition is null) throw new ArgumentNullException(nameof(condition));
        _element.Elements(_type).Where(e => condition.Invoke(e.Deserialize<TObject>())).Remove();
    }

    /// <summary>
    /// Removes a element at the specified index of the collection.
    /// </summary>
    /// <param name="index">The zero based index of the element to remove.</param>
    /// <exception cref="ArgumentOutOfRangeException"><c>index</c> is less than zero or greater than or equal to the
    /// number of elements in the collection.</exception>
    public void RemoveAt(int index)
    {
        _element.Elements(_type).ElementAt(index).Remove();
    }

    /// <summary>
    /// Updates all elements in the container by applying the provided update action delegate.
    /// </summary>
    /// <param name="update">A update to apply to each element.</param>
    /// <exception cref="ArgumentNullException"><c>update</c> is null.</exception>
    public void Update(Action<TObject> update)
    {
        if (update is null) throw new ArgumentNullException(nameof(update));

        foreach (var child in _element.Elements(_type))
        {
            var element = child.Deserialize<TObject>();
            update.Invoke(element);
        }
    }

    /// <summary>
    /// Updates all elements in the container that satisfy the provided condition predicate by applying the provided
    /// update action delegate.
    /// </summary>
    /// <param name="update">A update to apply to each element.</param>
    /// <param name="condition">The condition for which to update elements.</param>
    /// <exception cref="ArgumentNullException"><c>update</c> or <c>condition</c> is null.</exception>
    public void Update(Action<TObject> update, Func<TObject, bool> condition)
    {
        if (update is null) throw new ArgumentNullException(nameof(update));
        if (condition is null) throw new ArgumentNullException(nameof(condition));

        foreach (var child in _element.Elements(_type))
        {
            var element = child.Deserialize<TObject>();
            if (condition.Invoke(element))
                update.Invoke(element);
        }
    }

    /// <inheritdoc />
    public XElement Serialize() => _element;

    /// <inheritdoc />
    public IEnumerator<TObject> GetEnumerator() =>
        _element.Elements(_type).Select(e => e.Deserialize<TObject>()).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

/// <summary>
/// Extensions methods to the <see cref="LogixContainer{TElement}"/> class.
/// </summary>
public static class ContainerExtensions
{
    /// <summary>
    /// Determines if a component with the specified name exists in the container.
    /// </summary>
    /// <param name="container">The logix container of component objets.</param>
    /// <param name="name">The name of the component to find.</param>
    /// <returns><c>true</c> if a component with the specified name exists; otherwise, <c>false</c>.</returns>
    public static bool Contains<TComponent>(this LogixContainer<TComponent> container, string name)
        where TComponent : LogixComponent
    {
        return container.Serialize().Elements().Any(e => e.LogixName() == name);
    }

    /// <summary>
    /// Returns a component with the specified name if it exists in the container, otherwise returns <c>null</c>.
    /// </summary>
    /// <param name="container">The logix container of component objets.</param>
    /// <param name="name">The name of the component to find.</param>
    /// <typeparam name="TComponent">The component type to return.</typeparam>
    /// <returns>A <see cref="LogixComponent"/> of the specified type if found; Otherwise, <c>null</c>.</returns>
    public static TComponent? Find<TComponent>(this LogixContainer<TComponent> container, string name)
        where TComponent : LogixComponent
    {
        return container.Serialize().Elements().FirstOrDefault(e => e.LogixName() == name)?.Deserialize<TComponent>();
    }

    /// <summary>
    /// Returns a component with the specified name from the container.
    /// </summary>
    /// <param name="container">The logix container of component objets.</param>
    /// <param name="name">The name of the component to find.</param>
    /// <typeparam name="TComponent">The component type to return.</typeparam>
    /// <returns>A <see cref="LogixComponent"/> of the specified type.</returns>
    /// <exception cref="InvalidOperationException">No component having <c>name</c> exists in the container.</exception>
    public static TComponent Get<TComponent>(this LogixContainer<TComponent> container, string name)
        where TComponent : LogixComponent
    {
        var element = container.Serialize();
        var component = element.Elements().SingleOrDefault(e => e.LogixName() == name);
        return component is not null
            ? component.Deserialize<TComponent>()
            : throw new InvalidOperationException($"No component with name {name} was found in container.");
    }

    /// <summary>
    /// Removes a component with the specified name from the container.
    /// </summary>
    /// <param name="container">The logix container of component objets.</param>
    /// <param name="name">The name of the component to remove.</param>
    public static void Remove<TComponent>(this LogixContainer<TComponent> container, string name)
        where TComponent : LogixComponent
    {
        container.Serialize().Elements().SingleOrDefault(c => string.Equals(c.LogixName(), name))?.Remove();
    }
}