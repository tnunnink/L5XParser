﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using L5Sharp.Common;
using L5Sharp.Utilities;

namespace L5Sharp.Elements;

/// <summary>
/// A <c>DiagramBlock</c> type that defines the properties for instances of an Add-On Instruction within a
/// Function Block Diagram (FBD).
/// </summary>
/// <remarks>
/// </remarks>
/// <footer>
/// See <a href="https://literature.rockwellautomation.com/idc/groups/literature/documents/rm/1756-rm084_-en-p.pdf">
/// `Logix 5000 Controllers Import/Export`</a> for more information.
/// </footer>
[L5XType(L5XName.AddOnInstruction, L5XName.Sheet)]
public class AOI : FunctionBlock
{
    /// <summary>
    /// Creates a new <see cref="AOI"/> with default values.
    /// </summary>
    public AOI()
    {
    }

    /// <summary>
    /// Creates a new <see cref="AOI"/> initialized with the provided <see cref="XElement"/>.
    /// </summary>
    /// <param name="element">The <see cref="XElement"/> to initialize the type with.</param>
    /// <exception cref="ArgumentNullException"><c>element</c> is null.</exception>
    public AOI(XElement element) : base(element)
    {
    }

    /// <summary>
    /// The name of the Add-On Instruction this <c>AoiBlock</c> represents.
    /// </summary>
    /// <value>A <see cref="string"/> containing the name if it exists; Otherwise, <c>null</c>.</value>
    public string? Name
    {
        get => GetValue<string>();
        set => SetValue(value);
    }

    /// <summary>
    /// The backing tag name for the AOI block element.
    /// </summary>
    /// <value>A <see cref="string"/> containing the tag name if it exists; Otherwise, <c>null</c>.</value>
    public TagName? Operand
    {
        get => GetValue<TagName>();
        set => SetValue(value);
    }

    /// <summary>
    /// A collection of pin names that are visible for the <c>AoiBlock</c>.
    /// </summary>
    /// <value>A <see cref="IEnumerable{T}"/> containing the names of the pins if found. If not found then an
    /// empty collection.</value>
    /// <remarks>To update the property, you must assign a new collection of pin names.</remarks>
    public Params? VisiblePins
    {
        get => Element.Attribute(L5XName.VisiblePins) is not null
            ? new Params(Element.Attribute(L5XName.VisiblePins)!)
            : default;
        set => SetValue(value is not null ? string.Join(" ", value) : null);
    }

    /// <summary>
    /// The collection of input/output parameters for the <c>AoiBlock</c> instance.
    /// </summary>
    /// <value>A <see cref="IEnumerable{T}"/> containing <see cref="KeyValuePair"/> objects where the key
    /// is the name of the parameter and the value is the argument for the parameter. The argument refers to a tag name
    /// reference.</value>
    /// <remarks>To update the property, you must assign a new collection of <see cref="KeyValuePair"/> parameters.</remarks>
    public IEnumerable<KeyValuePair<string, string>> Parameters
    {
        get =>
            Element.Elements(L5XName.InOutParameter)
                .Where(e => e.Attribute(L5XName.Name) is not null && e.Attribute(L5XName.Argument) is not null)
                .Select(e => new KeyValuePair<string, string>(e.Get(L5XName.Name), e.Get(L5XName.Argument)));
        set =>
            Element.ReplaceNodes(
                value.Select(
                    p => new XElement(L5XName.InOutParameter,
                        new XAttribute(L5XName.Name, p.Key),
                        new XAttribute(L5XName.Argument, p.Value))));
    }
    
    /// <inheritdoc />
    public override IEnumerable<CrossReference> References()
    {
        if (Operand is not null)
            yield return new CrossReference(Element, Operand, L5XName.Tag);
        
        if (Name is not null)
            yield return new CrossReference(Element, Name, L5XName.AddOnInstructionDefinition);
        
        foreach (var parameter in Parameters)
            if (parameter.Value.IsTag())
                yield return new CrossReference(Element, parameter.Value, L5XName.Tag);
    }

    /// <inheritdoc />
    protected override IEnumerable<Argument> GetArguments(KeyValuePair<uint, string?> endpoint)
    {
        //todo we need to think about how could this be an invalid call (i.e. why check pins)
        yield return Operand is not null && endpoint.Value is not null && VisiblePins?.Contains(endpoint.Value) == true
            ? TagName.Concat(Operand, endpoint.Value)
            : Argument.Empty;
    }
}