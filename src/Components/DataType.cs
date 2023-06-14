﻿using System.Xml.Linq;
using L5Sharp.Enums;

namespace L5Sharp.Components;

/// <summary>
/// A logix <c>DataType</c> component. Contains the properties that comprise the L5X DataType element.
/// </summary>
/// <footer>
/// See <a href="https://literature.rockwellautomation.com/idc/groups/literature/documents/rm/1756-rm084_-en-p.pdf">
/// `Logix 5000 Controllers Import/Export`</a> for more information.
/// </footer>
public class DataType : LogixComponent<DataType>
{
    /// <summary>
    /// Creates a new <see cref="DataType"/> with default values.
    /// </summary>
    public DataType() : base(GenerateElement())
    {
        Family = DataTypeFamily.None;
        Class = DataTypeClass.User;
    }

    /// <summary>
    /// Creates a new <see cref="DataType"/> initialized with the provided <see cref="XElement"/>.
    /// </summary>
    /// <param name="element">The <see cref="XElement"/> to initialize the type with.</param>
    public DataType(XElement element) : base(element)
    {
    }

    /// <summary>
    /// The family of the <c>DataType</c> component.
    /// </summary>
    /// <value>
    /// A <see cref="DataTypeFamily"/> option indicating the family for which the current data type belongs.
    /// This is just string for string types and none for all others.
    /// </value>
    public DataTypeFamily? Family
    {
        get => GetValue<DataTypeFamily>();
        set => SetValue(value);
    }

    /// <summary>
    /// The class of the <c>DataType</c> component.
    /// </summary>
    /// <value>
    /// A <see cref="DataTypeClass"/> option indicating the class for which the current data type belongs.
    /// L5X files will only ever contain <see cref="DataTypeClass.User"/> class types.
    /// </value>
    public DataTypeClass? Class
    {
        get => GetValue<DataTypeClass>();
        set => SetValue(value);
    }

    /// <summary>
    /// The collection of <see cref="DataTypeMember"/> that make up the structure of the <c>DataType</c> component.
    /// </summary>
    public ILogixCollection<DataTypeMember> Members
    {
        get => GetCollection<DataTypeMember>();
        set => SetCollection(value);
    }

    /// <summary>
    /// Created the default backing element for the component. 
    /// </summary>
    /// <returns>A <see cref="XElement"/> representing the underlying L5X element for the type.</returns>
    private static XElement GenerateElement() => new(nameof(DataType), new XElement(L5XName.Members));
}