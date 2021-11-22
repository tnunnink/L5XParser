﻿using System.Collections.Generic;
using L5Sharp.Core;
using L5Sharp.Enums;

namespace L5Sharp
{
    public interface IModule : ILogixComponent
    {
        string CatalogNumber { get; }
        ushort Vendor { get; }
        ushort ProductType { get; }
        ushort ProductCode { get; }
        Revision Revision { get; }
        Module Parent { get; }
        int ParentModPortId { get; }
        string ParentModule { get; }
        bool Inhibited { get; }
        bool MajorFault { get; }
        bool SafetyEnabled { get; }
        KeyingState State { get; }
        IEnumerable<Port> Ports { get; }
        IEnumerable<Connection> Connections { get; }
        ITag<IModuleDefined> Config { get; }
        ITag<IModuleDefined> Input { get; }
        ITag<IModuleDefined> Output { get; }
    }
}