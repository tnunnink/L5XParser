﻿using System;
using System.Collections.Generic;
using System.Linq;
using L5Sharp.Builders;
using L5Sharp.Enums;

namespace L5Sharp.Core
{
    /// <inheritdoc cref="L5Sharp.ITask" />
    public class Task : ITask, IEquatable<Task>
    {
        private readonly HashSet<string> _programs = new HashSet<string>();

        internal Task(ComponentName name, TaskType type = null,
            TaskPriority priority = null, ScanRate rate = null, Watchdog watchdog = null,
            bool inhibitTask = false, bool disableUpdateOutputs = false, string description = null)
        {
            Name = name;
            Description = description;
            Type = type ?? TaskType.Periodic;
            Priority = priority ?? new TaskPriority(10);
            Rate = rate ?? new ScanRate(10);
            Watchdog = watchdog ?? new Watchdog(500);
            InhibitTask = inhibitTask;
            DisableUpdateOutputs = disableUpdateOutputs;
        }

        /// <inheritdoc />
        public ComponentName Name { get; }

        /// <inheritdoc />
        public string Description { get; }

        /// <inheritdoc />
        public TaskType Type { get; }

        /// <inheritdoc />
        public TaskPriority Priority { get; }

        /// <inheritdoc />
        public ScanRate Rate { get; }

        /// <inheritdoc />
        public Watchdog Watchdog { get; }

        /// <inheritdoc />
        public bool InhibitTask { get; }

        /// <inheritdoc />
        public bool DisableUpdateOutputs { get; }

        /// <inheritdoc />
        public IEnumerable<string> ScheduledPrograms => _programs.AsEnumerable();

        /// <summary>
        /// Creates a new instance of a <see cref="ITask"/> with default properties.
        /// </summary>
        /// <param name="name">The name of the task to create.</param>
        /// <returns>A new instance of <see cref="ITask"/></returns>
        public static ITask Create(string name)
        {
            return new Task(name);
        }

        /// <summary>
        /// Builds a new instance of <see cref="ITask"/> using the fluent builder API.
        /// </summary>
        /// <param name="name">The name of the task to create.</param>
        /// <returns>A new instance of <see cref="ITaskBuilder"/></returns>
        public static ITaskBuilder Build(string name)
        {
            return new TaskBuilder(name);
        }

        /// <inheritdoc />
        /// <exception cref="ArgumentNullException">Thrown when the name is null.</exception>
        public void ScheduleProgram(string name)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentException("Can not be null or empty", nameof(name));
            if (_programs.Contains(name)) return;
            _programs.Add(name);
        }

        /// <inheritdoc />
        /// <exception cref="ArgumentNullException">Thrown when the name is null.</exception>
        public void RemoveProgram(string name)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentException("Can not be null or empty", nameof(name));
            if (!_programs.Contains(name)) return;
            _programs.Remove(name);
        }

        /// <inheritdoc />
        public bool Equals(Task other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return _programs.SequenceEqual(other._programs) && Equals(Name, other.Name) &&
                   Description == other.Description &&
                   Equals(Type, other.Type) && Equals(Priority, other.Priority) && Equals(Rate, other.Rate) &&
                   Equals(Watchdog, other.Watchdog) && InhibitTask == other.InhibitTask &&
                   DisableUpdateOutputs == other.DisableUpdateOutputs;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((Task)obj);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            var hashCode = new HashCode();
            hashCode.Add(_programs);
            hashCode.Add(Name);
            hashCode.Add(Description);
            hashCode.Add(Type);
            hashCode.Add(Priority);
            hashCode.Add(Rate);
            hashCode.Add(Watchdog);
            hashCode.Add(InhibitTask);
            hashCode.Add(DisableUpdateOutputs);
            return hashCode.ToHashCode();
        }

        /// <summary>
        /// Indicates whether one object is equal to another object of the same type.
        /// </summary>
        /// <param name="left">The left instance of the object.</param>
        /// <param name="right">The right instance of the object.</param>
        /// <returns>True if the two objects are equal, otherwise false.</returns>
        public static bool operator ==(Task left, Task right)
        {
            return Equals(left, right);
        }

        /// <summary>
        /// Indicates whether one object is not equal to another object of the same type.
        /// </summary>
        /// <param name="left">The left instance of the object.</param>
        /// <param name="right">The right instance of the object.</param>
        /// <returns>True if the two objects are not equal, otherwise false.</returns>
        public static bool operator !=(Task left, Task right)
        {
            return !Equals(left, right);
        }
    }
}