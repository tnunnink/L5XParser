﻿using System;
using System.Globalization;
using System.Xml.Linq;
using L5Sharp.Core;
using L5Sharp.Extensions;
using L5Sharp.Helpers;

namespace L5Sharp.Serialization.Components
{
    internal class ControllerSerializer : IL5XSerializer<IController>
    {
        private static readonly XName ElementName = L5XElement.Controller.ToXName();

        public XElement Serialize(IController component)
        {
            if (component == null)
                throw new ArgumentNullException(nameof(component));

            var element = new XElement(ElementName);

            element.AddAttribute(component, c => c.Name);
            element.AddElement(component, c => c.Description);
            element.AddAttribute(component, c => c.ProcessorType);
            element.AddAttribute(component, c => c.Revision.Major, nameOverride: "MajorRev");
            element.AddAttribute(component, c => c.Revision.Minor, nameOverride: "MinorRev");

            element.Add(new XAttribute(nameof(component.ProjectCreationDate),
                component.ProjectCreationDate.ToString("ddd MMM d HH:mm:ss yyyy")));

            element.Add(new XAttribute(nameof(component.LastModifiedDate),
                component.ProjectCreationDate.ToString("ddd MMM d HH:mm:ss yyyy")));

            return element;
        }

        public IController Deserialize(XElement element)
        {
            if (element == null)
                throw new ArgumentNullException(nameof(element));

            if (element.Name != ElementName)
                throw new ArgumentException($"Element '{element.Name}' not valid for the serializer {GetType()}.");

            var name = element.GetComponentName();
            var processorType = element.GetAttribute<IController, string>(c => c.ProcessorType);
            var description = element.GetAttribute<IController, string>(c => c.Description);
            var major = element.GetAttribute<IController, ushort>(c => c.Revision.Major);
            var minor = element.GetAttribute<IController, ushort>(c => c.Revision.Minor);
            var revision = new Revision(major, minor);

            var creationDate = DateTime.ParseExact(element.Attribute("ProjectCreationDate")?.Value,
                "ddd MMM d HH:mm:ss yyyy", CultureInfo.CurrentCulture);

            var modifiedDate = DateTime.ParseExact(element.Attribute("LastModifiedDate")?.Value,
                "ddd MMM d HH:mm:ss yyyy", CultureInfo.CurrentCulture);

            return new Controller(name, processorType!, revision, creationDate, modifiedDate, description);
        }
    }
}