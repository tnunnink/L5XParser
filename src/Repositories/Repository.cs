﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using L5Sharp.Core;
using L5Sharp.Exceptions;
using L5Sharp.Extensions;

namespace L5Sharp.Repositories
{
    /// <summary>
    /// Generic abstract implementation of the read only repository.
    /// </summary>
    /// <typeparam name="TComponent"></typeparam>
    public abstract class Repository<TComponent> : IRepository<TComponent> where TComponent : ILogixComponent
    {
        /// <summary>
        /// The current logix context for the repository.
        /// </summary>
        protected readonly LogixContext Context;

        /// <summary>
        /// Creates a new instance of the <see cref="Repository{TComponent}"/> with the provided context.
        /// </summary>
        /// <param name="context"></param>
        protected Repository(LogixContext context)
        {
            Context = context;
        }

        /// <inheritdoc />
        public virtual bool Contains(string name) =>
            Context.L5X.GetComponents<TComponent>().Any(x => x.GetComponentName() == name);

        /// <inheritdoc />
        public virtual TComponent? Get(string name)
        {
            var element = Context.L5X.GetComponents<TComponent>().FirstOrDefault(x => x.GetComponentName() == name);

            return element is not null ? element.Deserialize<TComponent>() : default;
        }

        /// <inheritdoc />
        public virtual IEnumerable<TComponent> GetAll() => 
            Context.L5X.GetComponents<TComponent>().Select(e => e.Deserialize<TComponent>());

        /// <inheritdoc />
        public virtual TComponent? Find(Expression<Func<TComponent, bool>> predicate)
        {
            var filter = predicate.ToXExpression();

            var element = Context.L5X.GetComponents<TComponent>().FirstOrDefault(filter);

            return element is not null ? element.Deserialize<TComponent>() : default;
        }

        /// <inheritdoc />
        public virtual IEnumerable<TComponent> FindAll(Expression<Func<TComponent, bool>> predicate)
        {
            var filter = predicate.ToXExpression();

            var elements = Context.L5X.GetComponents<TComponent>().Where(filter);

            return elements.Select(e => e.Deserialize<TComponent>());
        }

        /// <inheritdoc />
        public virtual void Add(TComponent component)
        {
            if (component == null)
                throw new ArgumentNullException(nameof(component));

            if (Contains(component.Name))
                throw new ComponentNameCollisionException(component.Name, component.GetType());

            var element = component.Serialize();

            Context.L5X.GetContainer<TComponent>().Add(element);
        }

        /// <inheritdoc />
        public virtual void Update(TComponent component)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public virtual void Remove(ComponentName name)
        {
            Context.L5X.GetComponents<TComponent>().FirstOrDefault(x => x.GetComponentName() == name)?.Remove();
        }
    }
}