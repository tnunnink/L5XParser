﻿using System;
using System.Xml.Linq;
using L5Sharp.Components;
using L5Sharp.Core;

namespace L5Sharp.Elements;

/// <summary>
/// A component of a <see cref="Module"/> that represents the means for connecting devices on a network or in a chassis.
/// </summary>
/// <remarks>
/// A Port is a component that helps define the structure of the IO tree.
/// Each port may contain the slot on the chassis or backplane where the <see cref="Module"/> resides,
/// or the the network address (IP) of the device. Each port may (or may not) have a <see cref="BusSize"/>.
/// Each port is identifiable by the <see cref="Id"/> property. 
/// </remarks>
public sealed class Port : LogixElement
{
    /// <summary>
    /// Creates a new <see cref="Port"/> with default values.
    /// </summary>
    public Port()
    {
        Id = 0;
        Type = string.Empty;
        Address = Address.None;
        Upstream = false;
    }

    /// <summary>
    /// Creates a new <see cref="Port"/> initialized with the provided <see cref="XElement"/>.
    /// </summary>
    /// <param name="element">The <see cref="XElement"/> to initialize the type with.</param>
    /// <exception cref="ArgumentNullException"><c>element</c> is null.</exception>
    public Port(XElement element) : base(element)
    {
    }
    
    /// <summary>
    /// Gets the Id of the <see cref="Port"/>.
    /// </summary>
    /// <remarks>
    /// All Modules have at least one port. Each is identified by the Id property. Typically Modules will have one
    /// or two ports with Ids '1' and '2', respectively.
    /// </remarks>
    public int Id
    {
        get => GetValue<int>();
        set => SetValue(value);
    }

    /// <summary>
    /// Gets the value that represents the <see cref="Port"/> type.
    /// </summary>
    /// <remarks>
    /// This value appears to be specific to the product. Ports with IP will have 'Network' for their type. This
    /// property is required for importing a <see cref="Module"/> correctly.
    /// </remarks>
    public string? Type
    {
        get => GetValue<string>();
        set => SetValue(value);
    }

    /// <summary>
    /// Gets the address of the <see cref="Port"/>.
    /// </summary>
    /// <remarks>
    /// The Address of a port represents the slot or IP address for the port. This value is used in the
    /// <see cref="Module"/> to determine the slot and IP properties. All ports must have an Address.
    /// </remarks>
    public Address Address
    {
        get => GetValue<Address>() ?? Address.None;
        set => SetValue(value);
    }

    /// <summary>
    /// Gets the value indicating whether the there are devices upstream of the current <see cref="Port"/>.
    /// </summary>
    /// <remarks>
    /// From examining the L5X examples, the upstream flag seems to indicate that the port is one connected to a
    /// parent module living 'upstream' of the current device. Non-upstream ports seem to indicate they contain
    /// child modules, or connect to devices living 'downstream' of the current Module. This property must be set
    /// correctly to be able to import L5X.
    /// </remarks>
    public bool Upstream
    {
        get => GetValue<bool>();
        set => SetValue(value);
    }

    /// <summary>
    /// Gets the bus size or rack size of the port.
    /// </summary>
    /// <remarks>
    /// A port's bus represents the chassis or network on which Modules are accessible to the port.
    /// Only downstream modules will have a valid Bus. 
    /// </remarks>
    public byte? BusSize
    {
        get => Element.Element(L5XName.Bus)?.Attribute(L5XName.Size)?.Value.Parse<byte>();
        set
        {
            if (value is null)
            {
                Element.Element(L5XName.Bus)?.Remove();
                return;
            }
            
            var bus = Element.Element(L5XName.Bus);
            if (bus is null)
            {
                bus = new XElement(L5XName.Bus);
                Element.Add(bus);
            }
            
            bus.SetAttributeValue(L5XName.Size, value);
        }
    }
}