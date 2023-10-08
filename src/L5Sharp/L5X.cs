﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using L5Sharp.Common;
using L5Sharp.Components;
using L5Sharp.Elements;
using L5Sharp.Enums;
using L5Sharp.Utilities;

namespace L5Sharp;

/// <summary>
/// This is the primary entry point for interacting with the L5X file.
/// Provides access to query and manipulate logix components, elements, containers, and more. 
/// </summary>
/// <remarks>
/// </remarks>
public class L5X : ILogixSerializable
{
    /// <summary>
    /// The date/time format for the L5X content.
    /// </summary>
    public const string DateTimeFormat = "ddd MMM d HH:mm:ss yyyy";

    /// <summary>
    /// The underlying root RSLogix5000Content element of the L5X file.
    /// </summary>
    private readonly XElement _content;

    /// <summary>
    /// An index of all logix components in the L5X file for fast lookups.
    /// </summary>
    private readonly Dictionary<ComponentKey, XElement> _componentIndex = new();

    /// <summary>
    /// The list of top level component containers for a L5X content or controller element in order of which
    /// they should appear in the L5X file.
    /// </summary>
    private static readonly List<string> Containers = new()
    {
        L5XName.DataTypes,
        L5XName.Modules,
        L5XName.AddOnInstructionDefinitions,
        L5XName.Tags,
        L5XName.Programs,
        L5XName.Tasks,
        L5XName.ParameterConnections,
        L5XName.Trends,
        L5XName.QuickWatchLists
    };

    /// <summary>
    /// Creates a new <see cref="L5X"/> instance wrapping the provided <see cref="XElement"/> content object.
    /// </summary>
    /// <param name="content">The root <see cref="XElement"/> object representing the RSLogix5000Content element of the
    /// L5X file.</param>
    /// <exception cref="ArgumentNullException"><c>content</c> is null.</exception>
    /// <exception cref="ArgumentException"><c>content</c> name is not expected <c>RSLogix5000Content</c>.</exception>
    public L5X(XElement content)
    {
        if (content is null)
            throw new ArgumentNullException(nameof(content));

        if (content.Name != L5XName.RSLogix5000Content)
            throw new ArgumentException(
                $"Expecting root element name of {L5XName.RSLogix5000Content} to initialize L5X.");

        _content = content;

        // We will "normalize" (ensure consistent root controller element and component containers) for all
        // files so that we won't have issues getting top level containers. When saving we can remove unused containers.
        NormalizeContent();

        //Index all components for quick lookup from child elements or from top level L5X.
        IndexComponents();

        //Detect changes to keep index up to date.
        _content.Changed += ContentOnChanged;

        //This stores L5X object as in-memory object for the root XElement,
        //allowing child elements to retrieve the object locally without creating a new instance (and reindexing of content).
        //This allows them to reference to root L5X for index or other operations.
        _content.AddAnnotation(this);
    }

    /// <summary>
    /// Creates a new <see cref="L5X"/> by loading the contents of the provide file name.
    /// </summary>
    /// <param name="fileName">The full path, including file name, to the L5X file to load.</param>
    /// <returns>A new <see cref="L5X"/> containing the contents of the specified file.</returns>
    /// <exception cref="ArgumentException">The string is null or empty.</exception>
    /// <remarks>
    /// This factory method uses the <see cref="XElement.Load(string)"/> to load the contents of the XML file into
    /// memory. This means that this method is subject to the same exceptions that could be generated by loading the
    /// XElement.
    /// </remarks>
    public static L5X Load(string fileName) => new(XElement.Load(fileName));

    /// <summary>
    /// Creates a new <see cref="L5X"/> file with the standard root content and controller elements, and configures them
    /// with the provided controller name, processor, and revision. 
    /// </summary>
    /// <param name="name">The name of the controller.</param>
    /// <param name="processor">The processor catalog number.</param>
    /// <param name="revision">The optional software revision of the processor.</param>
    /// <returns>A new default <see cref="L5X"/> with the specified controller properties.</returns>
    public static L5X New(string name, string processor, Revision? revision) =>
        new(NewContent(name, nameof(Controller), revision));

    /// <summary>
    /// 
    /// </summary>
    /// <param name="component"></param>
    /// <param name="revision"></param>
    /// <typeparam name="TComponent"></typeparam>
    /// <returns></returns>
    public static L5X New<TComponent>(TComponent component, Revision? revision = null)
        where TComponent : LogixComponent => new(NewContent(component.Name, typeof(TComponent).L5XType(), revision));

    /// <summary>
    /// Creates a new <see cref="L5X"/> with the provided L5X string content.
    /// </summary>
    /// <param name="text">The string that contains the L5X content to parse.</param>
    /// <returns>A new <see cref="L5X"/> containing the contents of the specified string.</returns>
    /// <exception cref="ArgumentException">The string is null or empty.</exception>
    /// <remarks>
    /// This factory method uses the <see cref="XElement.Parse(string)"/> to load the contents of the XML file into
    /// memory. This means that this method is subject to the same exceptions that could be generated by parsing the
    /// XElement.
    /// </remarks>
    public static L5X Parse(string text) => new(XElement.Parse(text));

    /// <summary>
    /// The <see cref="L5XInfo"/> representing the L5X content export information.
    /// </summary>
    public L5XInfo Info => new(_content);

    /// <summary>
    /// The root <see cref="Components.Controller"/> component of the L5X file.
    /// </summary>
    /// <value>A <see cref="Components.Controller"/> component object.</value>
    /// <remarks>If the L5X does not <c>ContainContext</c>, meaning it is a project export, this will contain all the
    /// relevant controller properties and configurations. Otherwise most data will be null as the controller serves as
    /// just a root container for other component objects.</remarks>
    public Controller Controller => new(GetController());

    /// <summary>
    /// The container collection of <see cref="DataType"/> components found in the L5X file.
    /// </summary>
    /// <value>A <see cref="LogixContainer{TComponent}"/> of <see cref="DataType"/> components.</value>
    public LogixContainer<DataType> DataTypes => new(GetContainer(L5XName.DataTypes));

    /// <summary>
    /// Gets the collection of <see cref="AddOnInstruction"/> components found in the L5X file.
    /// </summary>
    /// <value>A <see cref="LogixContainer{TComponent}"/> of <see cref="AddOnInstruction"/> components.</value>
    public LogixContainer<AddOnInstruction> Instructions => new(GetContainer(L5XName.AddOnInstructionDefinitions));

    /// <summary>
    /// Gets the collection of <see cref="Module"/> components found in the L5X file.
    /// </summary>
    /// <value>A <see cref="LogixContainer{TComponent}"/> of <see cref="Module"/> components.</value>
    public LogixContainer<Module> Modules => new(GetContainer(L5XName.Modules));

    /// <summary>
    /// Gets the collection of Controller <see cref="Tags"/> components found in the L5X file.
    /// </summary>
    /// <value>A <see cref="LogixContainer{TComponent}"/> of <see cref="Tags"/> components.</value>
    /// <remarks>To access program specific tag collection user the <see cref="Programs"/> collection.</remarks>
    public LogixContainer<Tag> Tags => new(GetContainer(L5XName.Tags));

    /// <summary>
    /// Gets the collection of <see cref="Program"/> components found in the L5X file.
    /// </summary>
    /// <value>A <see cref="LogixContainer{TComponent}"/> of <see cref="Program"/> components.</value>
    public LogixContainer<Program> Programs => new(GetContainer(L5XName.Programs));

    /// <summary>
    /// Gets the collection of <see cref="Task"/> components found in the L5X file.
    /// </summary>
    /// <value>A <see cref="LogixContainer{TComponent}"/> of <see cref="Task"/> components.</value>
    public LogixContainer<Task> Tasks => new(GetContainer(L5XName.Tasks));

    /// <summary>
    /// The container collection of <see cref="ParameterConnection"/> components found in the L5X file.
    /// </summary>
    /// <value>A <see cref="LogixContainer{TComponent}"/> of <see cref="ParameterConnection"/> components.</value>
    public LogixContainer<ParameterConnection> ParameterConnections =>
        new(GetContainer(L5XName.ParameterConnections));

    /// <summary>
    /// The container collection of <see cref="Trend"/> components found in the L5X file.
    /// </summary>
    /// <value>A <see cref="LogixContainer{TComponent}"/> of <see cref="Trend"/> components.</value>
    public LogixContainer<Trend> Trends => new(GetContainer(L5XName.Trends));

    /// <summary>
    /// The container collection of <see cref="WatchList"/> components found in the L5X file.
    /// </summary>
    /// <value>A <see cref="LogixContainer{TComponent}"/> of <see cref="WatchList"/> components.</value>
    public LogixContainer<WatchList> WatchLists => new(GetContainer(L5XName.QuickWatchLists));

    /// <summary>
    /// Adds the given logix component to the first found container within the L5X tree. 
    /// </summary>
    /// <param name="component">The component to add to the L5X.</param>
    /// <typeparam name="TComponent">The type of component to add to the L5X.</typeparam>
    /// <exception cref="InvalidOperationException">No container was found in the L5X tree for the specified type.</exception>
    public void Add<TComponent>(TComponent component) where TComponent : LogixComponent
    {
        var containerType = typeof(TComponent).L5XContainerType();
        var container = _content.Descendants(containerType).FirstOrDefault();
        if (container is null) throw new InvalidOperationException($"Container '{containerType}' not found in L5X.");
        container.Add(component.Serialize());
    }

    /// <summary>
    /// Gets the number of elements of the specified type in the L5X.
    /// </summary>
    /// <typeparam name="TElement">The logix element type to get the count for.</typeparam>
    /// <returns>A <see cref="int"/> representing the number of elements of the specified type.</returns>
    public int Count<TElement>() where TElement : LogixElement
    {
        var type = typeof(TElement).L5XType();
        return _content.Descendants(type).Count();
    }

    /// <summary>
    /// Finds elements of the specified type across the entire L5X and returns as a flat <see cref="IEnumerable{T}"/> of objects.
    /// </summary>
    /// <typeparam name="TElement">The element type to find.</typeparam>
    /// <returns>A <see cref="IEnumerable{T}"/> containing all found objects of the specified type.</returns>
    /// <remarks>
    /// This methods provides a flexible and simple way to query the entire L5X for a specific type. Since
    /// it returns an <see cref="IEnumerable{T}"/>, you can make use of LINQ and the strongly typed objects to build
    /// more complex queries. This method does not make use of any optimized searching, so if you want to find items quickly,
    /// see <see cref="Find{TComponent}(string,string?)"/> or <see cref="FindTag"/> and <see cref="FindTags"/>.
    /// </remarks>
    public IEnumerable<TElement> Find<TElement>() where TElement : LogixElement
    {
        //var typeNames = typeof(TElement).L5XTypes().ToList();
        return _content.Descendants(typeof(TElement).L5XType()).Select(LogixSerializer.Deserialize<TElement>);
    }

    /// <summary>
    /// Gets a component with the specified name and option container name using the internal component index.
    /// </summary>
    /// <param name="name">The name of the component to get.</param>
    /// <param name="container">The optional name of the container in which to search for the component.
    /// This really only applies to tags and routines since they are scoped components.</param>
    /// <typeparam name="TComponent">The type of component to find.</typeparam>
    /// <returns>A single <see cref="LogixComponent"/> with the specified component name.</returns>
    /// <remarks>
    /// Since components have unique names, we can find and index them for fast lookup when needed. This might
    /// be helpful for certain functions that need to repeatedly find references to other components to perform
    /// certain tasks. Note that for tags this only find the root tag component. If you want to further find nested
    /// tag components, look at using <see cref="FindTag"/> or <see cref="FindTags"/>.
    /// </remarks>
    public TComponent? Find<TComponent>(string name, string? container = null) where TComponent : LogixComponent
    {
        var type = typeof(TComponent).L5XType();
        container ??= GetControllerName();
        var key = new ComponentKey(type, container, name);

        return _componentIndex.TryGetValue(key, out var element)
            ? LogixSerializer.Deserialize<TComponent>(element)
            : default;
    }

    /// <summary>
    /// Finds all tags with the specified name and optional container in the L5X using internal component index.
    /// </summary>
    /// <param name="tagName">The <see cref="TagName"/> of the tag to find in the L5X file.</param>
    /// <param name="container">The optional container of the tag to find. Will default to controller container name
    /// (i.e., controller scoped tags) unless otherwise specified.</param>
    /// <returns>A <see cref="Tag"/> with the specified tag name if found; Otherwise, <c>null</c>.</returns>
    /// <remarks>
    /// The point of this method is to provide an optimized way to retrieve a tag with a specific name in the L5X without
    /// having to iterate all tags in the file. This method uses the underlying component index to search for tags.
    /// If you want to all tags with a given tag name regardless of container, see <see cref="FindTags"/> 
    /// </remarks>
    public Tag? FindTag(TagName tagName, string? container = null)
    {
        container ??= Controller.Name;
        var key = new ComponentKey(typeof(Tag).L5XType(), container, tagName.Root);

        if (!_componentIndex.TryGetValue(key, out var element)) return default;

        var tag = LogixSerializer.Deserialize<Tag>(element);

        return tagName.Depth == 0 ? tag : tag.Member(tagName.Path);
    }

    /// <summary>
    /// Finds all tags with the specified name and optional scope in the L5X using internal component index
    /// for optimized lookups.
    /// </summary>
    /// <param name="tagName">The <see cref="TagName"/> of the tags to find in the L5X file.</param>
    /// <param name="scope">The optional scope (program or controller) to limit the search to.</param>
    /// <returns>
    /// A <see cref="IEnumerable{T}"/> containing all <see cref="Tag"/> elements found in the L5X file that
    /// have the provided tag name.
    /// </returns>
    /// <remarks>
    /// The point of this method is to provide an optimized way to retrieve tags with a specific name in the L5X without
    /// having to iterate all tags in the file. This method uses the underlying component index to search for tags.
    /// If you want to find a tag specific to a container or program, see <see cref="FindTag"/>.
    /// </remarks>
    public IEnumerable<Tag> FindTags(TagName tagName, Scope? scope = null)
    {
        var results = new List<Tag>();
        scope ??= Scope.Null;

        var containers = new List<string>();

        if (scope == Scope.Null || scope == Scope.Controller)
            GetControllerName();

        if (scope == Scope.Null || scope == Scope.Program)
            containers.AddRange(GetContainer(L5XName.Programs).Elements().Select(e => e.LogixName()));

        // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator Prefer loop for debugging.
        foreach (var container in containers)
        {
            var key = new ComponentKey(typeof(Tag).L5XType(), container, tagName.Root);
            if (!_componentIndex.TryGetValue(key, out var element)) continue;

            var tag = LogixSerializer.Deserialize<Tag>(element);
            if (tagName.Depth == 0)
            {
                results.Add(tag);
                continue;
            }

            var member = tag.Member(tagName.Path);
            if (member is not null)
            {
                results.Add(member);
            }
        }

        return results;
    }

    /// <summary>
    /// Gets a component with the specified name and optional container name using the internal component index.
    /// </summary>
    /// <param name="name">The name of the component to get.</param>
    /// <param name="container">The optional name of the container in which to search for the component.
    /// This really only applies to tags and routines since they are scoped components.</param>
    /// <typeparam name="TComponent">The type of component to find.</typeparam>
    /// <returns>A single <see cref="LogixComponent"/> with the specified component name.</returns>
    /// <exception cref="KeyNotFoundException">A component with <c>name</c> was not found in the L5X.</exception>
    /// <remarks>
    /// Since components have unique names, we can find and index them for fast lookup when needed. This might
    /// be helpful for certain functions that need to repeatedly find references to other components to perform
    /// certain tasks. 
    /// </remarks>
    public TComponent Get<TComponent>(string name, string? container = null) where TComponent : LogixComponent
    {
        var type = typeof(TComponent).L5XType();
        container ??= GetControllerName();

        var key = new ComponentKey(type, container, name);

        return _componentIndex.TryGetValue(key, out var element)
            ? LogixSerializer.Deserialize<TComponent>(element)
            : throw new KeyNotFoundException($"Component not found in L5X: {key}");
    }

    /// <summary>
    /// Merges the specified L5X file with the current <see cref="L5X"/> L5X by adding or overwriting logix components.
    /// </summary>
    /// <param name="fileName">The file name of L5X to merge.</param>
    /// <param name="overwrite">A bit indicating whether to overwrite incoming components of the same name.</param>
    /// <exception cref="ArgumentException"><c>fileName</c> is null or empty.</exception>
    public void Import(string fileName, bool overwrite = true)
    {
        if (string.IsNullOrEmpty(fileName))
            throw new ArgumentException("FileName can not be null or empty.", nameof(fileName));
        var content = Load(fileName);
        MergeContent(content, overwrite);
    }

    /// <summary>
    /// Merges another <see cref="L5X"/> file into the current L5X by adding or overwriting logix components.
    /// </summary>
    /// <param name="content">The <see cref="L5X"/> to merge.</param>
    /// <param name="overwrite">A bit indicating whether to overwrite incoming components of the same name.</param>
    /// <exception cref="ArgumentNullException"><c>content</c> is null.</exception>
    public void Import(L5X content, bool overwrite = true)
    {
        if (content is null) throw new ArgumentNullException(nameof(content));
        MergeContent(content, overwrite);
    }

    /// <summary>
    /// Serialize this <see cref="L5X"/> to a file, overwriting an existing file, if it exists.
    /// </summary>
    /// <param name="fileName">A string that contains the name of the file.</param>
    public void Save(string fileName) => SaveContent(fileName);

    /// <inheritdoc />
    public XElement Serialize() => _content;

    /// <inheritdoc />
    public override string ToString() => _content.ToString();

    #region Internal

    private void ContentOnChanged(object sender, XObjectChangeEventArgs e)
    {
        switch (e.ObjectChange)
        {
            case XObjectChange.Add:
            {
                var element = sender as XElement;
                //todo process addition of element and if its a component element we need to add to index.
                break;
            }
            case XObjectChange.Remove:
                //todo process removal of element and if its a component element we need to remove from index.
                break;
            case XObjectChange.Name:
                //Not really sure we need to handle this change
                break;
            case XObjectChange.Value:
                //todo if a value changed it could potentially change references to a component.
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    /// <summary>
    /// Gets a top level container element from the root controller element of the L5X.
    /// </summary>
    /// <param name="name">The name of the container to retrieve.</param>
    /// <returns>A <see cref="XElement"/> representing the container with the provided name.</returns>
    /// <exception cref="InvalidOperationException">The element does not exist.</exception>
    private XElement GetContainer(string name) => GetController().Element(name) ?? throw _content.L5XError(name);

    /// <summary>
    /// Gets all primary/top level L5X component containers in the current L5X file.
    /// </summary>
    /// <returns>A <see cref="IEnumerable{T}"/> of <see cref="XElement"/> representing the L5X component containers.</returns>
    private IEnumerable<XElement> GetContainers() => Containers.Select(name => GetController().Element(name)).ToList();

    /// <summary>
    /// Gets the root controller element of the L5X file. We expect this to always exist if the L5X is constructed
    /// due to the normalization process. 
    /// </summary>
    private XElement GetController() =>
        _content.Element(L5XName.Controller) ?? throw _content.L5XError(L5XName.Controller);

    /// <summary>
    /// Gets the name of the controller element of the L5X file. Will default to empty string if not found. 
    /// </summary>
    private string GetControllerName() => GetController().LogixName();

    /// <summary>
    /// Finds all logix component elements and indexes them into a local dictionary for fast lookups.
    /// </summary>
    private void IndexComponents()
    {
        IndexControllerScopedComponents();
        IndexProgramScopedComponents();
        IndexModuleDefinedTagComponents();
    }

    private void IndexControllerScopedComponents()
    {
        //The container for all controller scoped components will be the name of the controller.
        var containerName = GetControllerName();

        //Only consider component elements with a valid name attribute.
        var components = GetContainers().SelectMany(c =>
            c.Elements().Where(e => e.Attribute(L5XName.Name) is not null));

        foreach (var component in components)
        {
            var type = component.Name.LocalName;
            var name = component.LogixName();
            var key = new ComponentKey(type, containerName, name);
            if (!_componentIndex.TryAdd(key, component))
                throw new InvalidOperationException($"Duplicate component found: {key}");
        }
    }

    /// <summary>
    /// Handles iterating each program component element in the L5X and index each tag and routine with the correct scoped keys.
    /// </summary>
    private void IndexProgramScopedComponents()
    {
        var programs = GetContainer(L5XName.Programs).Elements();

        foreach (var program in programs)
        {
            var container = program.LogixName();

            foreach (var component in program.Descendants()
                         .Where(d => d.L5XType() is L5XName.Tag or L5XName.Routine))
            {
                var key = new ComponentKey(component.L5XType(), container, component.LogixName());
                if (!_componentIndex.TryAdd(key, component))
                    throw new InvalidOperationException($"Duplicate component found: {key}");
            }
        }
    }

    /// <summary>
    /// Handles iterating each module defined tag component element in the L5X and indexes each tag.
    /// </summary>
    private void IndexModuleDefinedTagComponents()
    {
        var container = GetControllerName();

        foreach (var component in GetContainer(L5XName.Modules).Descendants().Where(e =>
                     e.L5XType() is L5XName.ConfigTag or L5XName.InputTag or L5XName.OutputTag))
        {
            var key = new ComponentKey(L5XName.Tag, container, component.ModuleTagName());
            if (!_componentIndex.TryAdd(key, component))
                throw new InvalidOperationException($"Duplicate component found: {key}");
        }
    }

    /// <summary>
    /// If no root controller element exists, adds new context controller and moves all root elements into that controller
    /// element. Then adds missing top level containers to ensure consistent structure of the root L5X.
    /// </summary>
    private void NormalizeContent()
    {
        if (_content.Element(L5XName.Controller) is null)
        {
            var context = new XElement(L5XName.Controller, new XAttribute(L5XName.Use, Use.Context));
            context.Add(_content.Elements());
            _content.RemoveNodes();
            _content.Add(context);
        }

        var controller = _content.Element(L5XName.Controller)!;

        foreach (var container in from container in Containers
                 let existing = controller.Element(container)
                 where existing is null
                 select container)
        {
            controller.Add(new XElement(container));
        }
    }

    /// <summary>
    /// Merges all top level containers and their immediate child elements between the current L5X content and the
    /// provided L5X content. Will overwrite if specified.
    /// </summary>
    /// <param name="l5X">The L5X element to merge with the current target element.</param>
    /// <param name="overwrite">A flag to indicate whether to overwrite child elements of matching name.</param>
    private void MergeContent(L5X l5X, bool overwrite)
    {
        if (l5X is null) throw new ArgumentNullException(nameof(l5X));

        var containerPairs = GetContainers()
            .Join(l5X.GetContainers(), e => e.Name, e => e.Name, (a, b) => new {a, b})
            .ToList();

        foreach (var pair in containerPairs)
            MergeContainers(pair.a, pair.b, overwrite);
    }

    /// <summary>
    /// Given to top level containers, adds or replaces all child elements matching based on the logix name of the elements.
    /// </summary>
    private static void MergeContainers(XContainer target, XContainer source, bool overwrite)
    {
        foreach (var element in source.Elements())
        {
            var match = target.Elements().FirstOrDefault(e => e.LogixName() == element.LogixName());

            if (match is null)
            {
                target.Add(element);
                continue;
            }

            if (overwrite)
                match.ReplaceWith(element);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="targetName"></param>
    /// <param name="targetType"></param>
    /// <param name="softwareRevision"></param>
    /// <returns></returns>
    private static XElement NewContent(string targetName, string targetType, Revision? softwareRevision)
    {
        var content = new XElement(L5XName.RSLogix5000Content);
        content.Add(new XAttribute(L5XName.SchemaRevision, new Revision()));
        if (softwareRevision is not null) content.Add(new XAttribute(L5XName.SoftwareRevision, softwareRevision));
        content.Add(new XAttribute(L5XName.TargetName, targetName));
        content.Add(new XAttribute(L5XName.TargetType, targetType));
        content.Add(new XAttribute(L5XName.ContainsContext, targetType != nameof(Controller)));
        content.Add(new XAttribute(L5XName.Owner, Environment.UserName));
        content.Add(new XAttribute(L5XName.ExportDate, DateTime.Now.ToString(DateTimeFormat)));

        return content;
    }

    /// <summary>
    /// Create document, adds default declaration, and saves the current L5X content to the specified file name.
    /// </summary>
    /// <param name="fileName">A string that contains the name of the file.</param>
    private void SaveContent(string fileName)
    {
        //This will sanitize containers that were perhaps added when normalizing that went unused.
        foreach (var container in GetContainers().Where(c => !c.HasElements))
            container.Remove();

        var declaration = new XDeclaration("1.0", "UTF-8", "yes");
        var document = new XDocument(declaration);
        document.Add(_content);
        document.Save(fileName);
    }

    #endregion
}