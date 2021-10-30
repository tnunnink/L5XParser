﻿using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using L5Sharp.Abstractions;
using L5Sharp.Extensions;
using L5Sharp.Factories;

namespace L5Sharp.Repositories
{
    public class ReadOnlyRepository<T> : IReadOnlyRepository<T> where T : ILogixComponent
    {
        protected readonly LogixContext Context;
        protected readonly XElement Container;
        internal readonly IComponentFactory<T> Factory;

        protected ReadOnlyRepository(LogixContext context)
        {
            Context = context;
            Container = context.GetContainer<T>();
            Factory = context.GetFactory<T>();
        }

        public virtual bool Exists(string name)
        {
            return Container.Contains<T>(name);
        }

        public virtual T Get(string name)
        {
            return Factory.Create(Container.GetFirst<T>(name));
        }

        public virtual IEnumerable<T> GetAll()
        {
            return Container.GetAll<T>().Select(e => Factory.Create(e));
        }
    }
}