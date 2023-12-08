﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace L5Sharp.Core;

/// <summary>
/// A Logix <c>Line</c> element containing the properties for a L5X Line component.
/// </summary>
public sealed class Line : LogixCode
{
    private NeutralText Text => new(Element.Value);
    
    /// <summary>
    /// Creates a new <see cref="Line"/> with default values.
    /// </summary>
    public Line()
    {
        Element.ReplaceNodes(new XCData(NeutralText.Empty));
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
    /// Creates a new <see cref="Line"/> initialized with the provided <see cref="NeutralText"/>.
    /// </summary>
    /// <param name="text">The <see cref="NeutralText"/> representing the line of structured text logic.</param>
    /// <remarks>This will initialize ...
    /// When importing, Logix ignores the rung number and imports Rung's in order of the container sequence,
    /// meaning, its really only necessary to specify valid text, which is why this constructor is available,
    /// allowing concise construction of a <c>Rung</c> object.</remarks>
    public Line(NeutralText text)
    {
        Element.ReplaceNodes(new XCData(text));
    }

    /// <inheritdoc />
    public override IEnumerable<CrossReference> References()
    {
        var references = new List<CrossReference>();

        var instructions = Text.Instructions().ToList();
        
        foreach (var instruction in instructions)
        {
            references.Add(new CrossReference(Element, L5XName.Instruction, instruction.Key));
            
            if (instruction.IsRoutineCall)
            {
                var routine = instruction.Arguments.FirstOrDefault()?.ToString() ?? string.Empty;
                references.Add(new CrossReference(Element, L5XName.Routine, routine, instruction.Key));
                
                var parameters = instruction.Arguments.Skip(1).Where(a => a.IsTag).Select(t => t.ToString());
                references.AddRange(parameters.Select(p => new CrossReference(Element, L5XName.Tag, p, instruction.Key)));
                continue;
            }

            if (instruction.IsTaskCall)
            {
                var task = instruction.Arguments.FirstOrDefault()?.ToString() ?? string.Empty;
                references.Add(new CrossReference(Element, L5XName.Task, task, instruction.Key));
                continue;
            }
            
            //todo other instructions like GSV SSV

            references.AddRange(instruction.Tags()
                .Select(t => new CrossReference(Element, L5XName.Tag, t.ToString(), instruction.Key)));
        }

        return references;
    }

    /// <inheritdoc />
    public override string ToString() => Text;

    /// <summary>
    /// Implicitly converts the <see cref="Rung"/> object to a <see cref="NeutralText"/>.
    /// </summary>
    /// <param name="rung">The <c>Rung</c> to convert.</param>
    /// <returns>A <see cref="NeutralText"/> instance representing the contents of the <c>Rung</c>.</returns>
    public static implicit operator NeutralText(Line rung) => new(rung.Text);

    /// <summary>
    /// Implicitly converts the <see cref="NeutralText"/> object to a <see cref="Rung"/>.
    /// </summary>
    /// <param name="text">The <c>NeutralText</c> to convert.</param>
    /// <returns>A <see cref="Rung"/> instance representing the contents of the <c>NeutralText</c>.</returns>
    public static implicit operator Line(NeutralText text) => new(text);
}