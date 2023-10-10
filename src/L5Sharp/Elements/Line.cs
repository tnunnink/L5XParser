﻿using System;
using System.Collections.Generic;
using System.Xml.Linq;
using L5Sharp.Common;
using L5Sharp.Enums;

namespace L5Sharp.Elements;

/// <summary>
/// A Logix <c>Line</c> element containing the properties for a L5X Line component.
/// </summary>
public class Line : LogixCode
{
    /// <summary>
    /// Creates a new <see cref="Line"/> with default values.
    /// </summary>
    public Line()
    {
        Text = NeutralText.Empty;
    }

    /// <summary>
    /// Creates a new <see cref="Line"/> initialized with the provided <see cref="XElement"/>.
    /// </summary>
    /// <param name="element">The <see cref="XElement"/> to initialize the type with.</param>
    /// <exception cref="ArgumentNullException"><c>element</c> is null.</exception>
    public Line(XElement element) : base(element)
    {
    }

    /// <summary>
    /// The logic of the <see cref="Line"/> as a <see cref="NeutralText"/> value.
    /// </summary>
    public NeutralText Text
    {
        get => Element.Value;
        set => Element.SetValue(new XCData(value.ToString()));
    }

    /// <inheritdoc />
    public override string ToString() => Text;

    /// <inheritdoc />
    public override IEnumerable<TagName> TagNames()
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public override IEnumerable<Instruction> Instructions()
    {
        throw new NotImplementedException();
    }
}