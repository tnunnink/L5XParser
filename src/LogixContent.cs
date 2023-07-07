﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using L5Sharp.Components;
using L5Sharp.Core;
using L5Sharp.Elements;
using L5Sharp.Enums;

namespace L5Sharp;

/// <summary>
/// A simple wrapper around a given <see cref="XElement"/>, which is expected to be the root RSLogix5000Content element
/// of the L5X file.
/// </summary>
public class LogixContent
{
    /// <summary>
    /// Creates a new <see cref="LogixContent"/> instance with the provided root <see cref="XElement"/>.
    /// </summary>
    /// <param name="element">The root RSLogix5000Content element of the L5X.</param>
    /// <exception cref="ArgumentNullException"><c>element</c> is null.</exception>
    /// <exception cref="ArgumentException"><c>element</c> name is not root RSLogix5000Content name.</exception>
    public LogixContent(XElement element)
    {
        L5X = new L5X(element);
    }

    /// <summary>
    /// Creates a new <see cref="LogixContent"/> by loading the contents of the provide file name.
    /// </summary>
    /// <param name="fileName">The full path, including file name, to the L5X file to load.</param>
    /// <returns>A new <see cref="LogixContent"/> containing the contents of the specified file.</returns>
    /// <exception cref="ArgumentException">The string is null or empty.</exception>
    /// <remarks>
    /// This factory method uses the <see cref="XDocument.Load(string)"/> to load the contents of the xml file into
    /// memory. This means that this method is subject to the same exceptions that could be generated by loading the
    /// XDocument. Once loaded, validation is performed to ensure the content adheres to the specified L5X Schema files.
    /// </remarks>
    public static LogixContent Load(string fileName) => new(XElement.Load(fileName));

    /// <summary>
    /// Creates a new <see cref="LogixContent"/> with the provided L5X string content.
    /// </summary>
    /// <param name="text">The string that contains the L5X content to parse.</param>
    /// <returns>A new <see cref="LogixContent"/> containing the contents of the specified string.</returns>
    /// <exception cref="ArgumentException">The string is null or empty.</exception>
    /// <remarks>
    /// This factory method uses the <see cref="XDocument.Parse(string)"/> to load the contents of the xml file into
    /// memory. This means that this method is subject to the same exceptions that could be generated by parsing the
    /// XDocument. Once parsed, validation is performed to ensure the content adheres to the specified L5X Schema files.
    /// </remarks>
    public static LogixContent Parse(string text) => new(XElement.Parse(text));

    /// <summary>
    /// Creates a new <see cref="LogixContent"/> with the provided logix component as the target type.
    /// </summary>
    /// <param name="target">The L5X target component of the resulting content.</param>
    /// <param name="softwareRevision">The optional software revision, or version of Studio to export the component as.</param>
    /// <returns>A <see cref="LogixContent"/> containing the component as the target of the L5X.</returns>
    public static LogixContent Export<TComponent>(LogixComponent<TComponent> target, Revision? softwareRevision = null)
        where TComponent : LogixComponent<TComponent>
    {
        var content = new XElement(L5XName.RSLogix5000Content);
        content.Add(new XAttribute(L5XName.SchemaRevision, new Revision()));
        if (softwareRevision is not null) content.Add(new XAttribute(L5XName.SoftwareRevision, softwareRevision));
        content.Add(new XAttribute(L5XName.TargetName, target.Name));
        content.Add(new XAttribute(L5XName.TargetType, target.TypeName));
        content.Add(new XAttribute(L5XName.ContainsContext, target.GetType() != typeof(Controller)));
        content.Add(new XAttribute(L5XName.Owner, Environment.UserName));
        content.Add(new XAttribute(L5XName.ExportDate, DateTime.Now.ToString(L5X.DateTimeFormat)));

        target.Use = Use.Target;
        content.Add(target.Serialize());

        return new LogixContent(content);
    }

    /// <summary>
    /// The root L5X content containing all raw XML data for the <see cref="LogixContent"/>.
    /// </summary>
    /// <remarks>
    /// <see cref="L5X"/> inherits from <see cref="XElement"/> and adds some helper properties and methods
    /// for interacting with the root content of the L5X file.
    /// </remarks>
    public L5X L5X { get; }

    /// <summary>
    /// The root <see cref="Components.Controller"/> component of the L5X file.
    /// </summary>
    /// <value>A <see cref="Components.Controller"/> component object.</value>
    /// <remarks>If the L5X does not <c>ContainContext</c>, meaning it is a project export, this will container all the
    /// relevant controller properties and configurations. Otherwise most data will be null as the controller serves as
    /// just a root container for other component objects.</remarks>
    public Controller Controller => new(L5X.GetController());

    /// <summary>
    /// The container collection of <see cref="DataType"/> components found in the L5X file.
    /// </summary>
    /// <value>A <see cref="LogixContainer{TComponent}"/> of <see cref="DataType"/> components.</value>
    public LogixContainer<DataType> DataTypes => new(L5X.GetContainer(L5XName.DataTypes));

    /// <summary>
    /// Gets the collection of <see cref="AddOnInstruction"/> components found in the L5X file.
    /// </summary>
    /// <value>A <see cref="LogixContainer{TComponent}"/> of <see cref="AddOnInstruction"/> components.</value>
    public LogixContainer<AddOnInstruction> Instructions => new(L5X.GetContainer(L5XName.AddOnInstructionDefinitions));

    /// <summary>
    /// Gets the collection of <see cref="Module"/> components found in the L5X file.
    /// </summary>
    /// <value>A <see cref="LogixContainer{TComponent}"/> of <see cref="Module"/> components.</value>
    public LogixContainer<Module> Modules => new(L5X.GetContainer(L5XName.Modules));

    /// <summary>
    /// Gets the collection of Controller <see cref="Tags"/> components found in the L5X file.
    /// </summary>
    /// <value>A <see cref="LogixContainer{TComponent}"/> of <see cref="Tags"/> components.</value>
    /// <remarks>To access program specific tag collection user the <see cref="Programs"/> collection.</remarks>
    public LogixContainer<Tag> Tags => new(L5X.GetContainer(L5XName.Tags));

    /// <summary>
    /// Gets the collection of <see cref="Program"/> components found in the L5X file.
    /// </summary>
    /// <value>A <see cref="LogixContainer{TComponent}"/> of <see cref="Program"/> components.</value>
    public LogixContainer<Program> Programs => new(L5X.GetContainer(L5XName.Programs));

    /// <summary>
    /// Gets the collection of <see cref="LogixTask"/> components found in the L5X file.
    /// </summary>
    /// <value>A <see cref="LogixContainer{TComponent}"/> of <see cref="LogixTask"/> components.</value>
    public LogixContainer<LogixTask> Tasks => new(L5X.GetContainer(L5XName.Tasks));

    /// <summary>
    /// The container collection of <see cref="ParameterConnection"/> components found in the L5X file.
    /// </summary>
    /// <value>A <see cref="LogixContainer{TComponent}"/> of <see cref="ParameterConnection"/> components.</value>
    public LogixContainer<ParameterConnection> ParameterConnections =>
        new(L5X.GetContainer(L5XName.ParameterConnections));

    /// <summary>
    /// The container collection of <see cref="Trend"/> components found in the L5X file.
    /// </summary>
    /// <value>A <see cref="LogixContainer{TComponent}"/> of <see cref="Trend"/> components.</value>
    public LogixContainer<Trend> Trends => new(L5X.GetContainer(L5XName.Trends));

    /// <summary>
    /// The container collection of <see cref="WatchList"/> components found in the L5X file.
    /// </summary>
    /// <value>A <see cref="LogixContainer{TComponent}"/> of <see cref="WatchList"/> components.</value>
    public LogixContainer<WatchList> WatchLists => new(L5X.GetContainer(L5XName.QuickWatchLists));

    /// <summary>
    /// Finds elements of the specified type across the entire L5X and returns as a flat <see cref="IEnumerable{T}"/> of objects.
    /// </summary>
    /// <typeparam name="TElement">The element type to find.</typeparam>
    /// <returns>A <see cref="IEnumerable{T}"/> containing all found objects of the specified type.</returns>
    public IEnumerable<TElement> Find<TElement>() where TElement : LogixElement<TElement> =>
        L5X.Descendants(typeof(TElement).LogixTypeName()).Select(LogixSerializer.Deserialize<TElement>);

    /// <summary>
    /// Merges the specified L5X file with the current <see cref="LogixContent"/> L5X by adding or overwriting logix components.
    /// </summary>
    /// <param name="fileName">The file name of L5X to merge.</param>
    /// <param name="overwrite">A bit indicating whether to overwrite incoming components of the same name.</param>
    /// <exception cref="ArgumentException"><c>fileName</c> is null or empty.</exception>
    public void Merge(string fileName, bool overwrite = true)
    {
        if (string.IsNullOrEmpty(fileName))
            throw new ArgumentException("FileName can not be null or empty.", nameof(fileName));
        var content = Load(fileName);
        Merge(content, overwrite);
    }

    /// <summary>
    /// Merges another <see cref="LogixContent"/> file into the current L5X by adding or overwriting logix components.
    /// </summary>
    /// <param name="content">The <see cref="LogixContent"/> to merge.</param>
    /// <param name="overwrite">A bit indicating whether to overwrite incoming components of the same name.</param>
    /// <exception cref="ArgumentNullException"><c>content</c> is null.</exception>
    public void Merge(LogixContent content, bool overwrite = true)
    {
        if (content is null)
            throw new ArgumentNullException(nameof(content));
        L5X.Merge(content.L5X, overwrite);
    }

    /// <summary>
    /// Serialize this <see cref="LogixContent"/> to a file, overwriting an existing file, if it exists.
    /// </summary>
    /// <param name="fileName">A string that contains the name of the file.</param>
    public void Save(string fileName)
    {
        var declaration = new XDeclaration("1.0", "UTF-8", "yes");
        var document = new XDocument(declaration);
        document.Add(L5X);
        document.Save(fileName);
    }

    /// <inheritdoc />
    public override string ToString() => L5X.ToString();
}