﻿using System;
using System.Xml.Linq;
using L5Sharp.Utilities;

namespace L5Sharp.Elements;

/// <summary>
/// A <c>DiagramBlock</c> type that defines the properties for nested function block elements in a
/// Function Block Diagram (FBD).
/// </summary>
/// <remarks>
/// A <c>Function</c> represents a function block within the FBD. These are blocks that represent
/// specific logix built-in instructions as opposed to AOIs. A <c>Function</c> differs from a <c>Block</c>
/// in that it does not require a backing tag to operate over. Rather, a <c>Function</c> represents a simple logic gate or
/// operation that takes inputs and produces an output without the need for a backing tag. Use the static factory method
/// to create any known logix <see cref="Function"/>.
/// </remarks>
/// <footer>
/// See <a href="https://literature.rockwellautomation.com/idc/groups/literature/documents/rm/1756-rm084_-en-p.pdf">
/// `Logix 5000 Controllers Import/Export`</a> for more information.
/// </footer>
[L5XType(L5XName.Function, L5XName.Sheet)]
public class Function : FunctionBlock
{
    /// <summary>
    /// Creates a new <see cref="FunctionBlock"/> with default values.
    /// </summary>
    public Function()
    {
    }

    /// <summary>
    /// Creates a new <see cref="FunctionBlock"/> initialized with the provided <see cref="XElement"/>.
    /// </summary>
    /// <param name="element">The <see cref="XElement"/> to initialize the type with.</param>
    /// <exception cref="ArgumentNullException"><c>element</c> is null.</exception>
    public Function(XElement element) : base(element)
    {
    }

    /// <summary>
    /// The mnemonic name specifying the type of function for the <c>DiagramBlock</c> instance.
    /// </summary>
    /// <value>A <see cref="string"/> containing the type of the function if it exists; Otherwise, <c>null</c>.</value>
    public string? Type
    {
        get => GetValue<string>();
        set => SetValue(value);
    }
}