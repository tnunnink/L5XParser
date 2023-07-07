﻿using System.Diagnostics;
using FluentAssertions;
using L5Sharp.Components;
using L5Sharp.Core;
using L5Sharp.Elements;

namespace L5Sharp.Tests;

[TestFixture]
public class LogixContentTemplateTests
{
    [Test]
    public void DataTypes_WhenCalled_ShouldNotBeEmpty()
    {
        var content = LogixContent.Load(Known.Template);

        var components = content.DataTypes.ToList();

        components.Should().NotBeEmpty();
    }

    [Test]
    public void Modules_WhenCalled_ShouldNotBeEmpty()
    {
        var content = LogixContent.Load(Known.Template);

        var components = content.Modules.ToList();

        components.Should().NotBeEmpty();
    }

    [Test]
    public void Instructions_WhenCalled_ShouldNotBeEmpty()
    {
        var content = LogixContent.Load(Known.Template);

        var components = content.Instructions.ToList();

        components.Should().NotBeEmpty();
    }

    [Test]
    public void Tags_WhenCalled_ShouldNotBeEmpty()
    {
        var content = LogixContent.Load(Known.Template);

        var components = content.Tags.ToList();

        components.Should().NotBeEmpty();
    }

    [Test]
    public void TagsAll_WhenEnumerated_ShouldWork()
    {
        var content = LogixContent.Load(Known.Template);

        var stopwatch = new Stopwatch();
        stopwatch.Start();
        var components = content.Find<Tag>().SelectMany(t => t.Members()).ToList();
        stopwatch.Stop();

        Console.WriteLine(stopwatch.Elapsed);
        components.Should().NotBeEmpty();
    }

    [Test]
    public void Programs_WhenCalled_ShouldNotBeEmpty()
    {
        var content = LogixContent.Load(Known.Template);

        var components = content.Programs.ToList();

        components.Should().NotBeEmpty();
    }

    [Test]
    public void Tasks_WhenCalled_ShouldNotBeEmpty()
    {
        var content = LogixContent.Load(Known.Template);

        var components = content.Tasks.ToList();

        components.Should().NotBeEmpty();
    }

    [Test]
    public void InstructionsNames()
    {
        var content = LogixContent.Load(Known.Template);

        var components = content.Instructions.Select(i => i.Name).ToList();

        components.Should().NotBeEmpty();
    }

    [Test]
    public void ProgramsWithNullType()
    {
        var content = LogixContent.Load(Known.Template);

        var components = content.Programs.Where(p => p.Type is null).ToList();

        components.Should().BeEmpty();
    }

    [Test]
    public void FindReferencesDifferentApproach()
    {
        var content = LogixContent.Load(Known.Template);

        var references = content.Find<Rung>()
            .Select(r => r.Text)
            .SelectMany(t => t.References()).ToList();

        references.Should().NotBeEmpty();
    }

    [Test]
    public void FindInstructionAndReplaceWithSignature()
    {
        var content = LogixContent.Load(Known.Template);

        var aoiLookup = content.Instructions.ToDictionary(k => k.Name, v => v);

        var aoiReferences = content.Find<Rung>()
            .Select(r => r.Text)
            .SelectMany(t => aoiLookup.SelectMany(l => t.SplitByKey(l.Key)))
            .ToList();

        var flattened = new List<NeutralText>();

        foreach (var reference in aoiReferences)
        {
            var key = reference.Keys().First();
            var instruction = aoiLookup[key];
            var logic = instruction.Logic(reference);
            flattened.AddRange(logic);
        }

        flattened.Should().NotBeEmpty();
    }
}