﻿using System;
using L5Sharp.Configurations;

namespace L5Sharp
{
    public interface IDataTypeMembers : IComponentCollection<IDataTypeMember<IDataType>>
    {
        void Add(string name, Action<IDataTypeMemberConfiguration> config = null);
    }
}