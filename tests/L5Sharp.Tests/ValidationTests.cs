﻿using FluentAssertions;
using L5Sharp.Extensions;
using L5Sharp.Types;
using L5Sharp.Types.Atomics;

namespace L5Sharp.Tests;

[TestFixture]
public class ValidationTests
{
    [Test]
    public void ValidateSomeTagValue()
    {
        var content = LogixContent.Load(Known.Test);

        var tags = content.Tags().Where(t => t.DataType == "DINT" && t.Data is AtomicType);

        foreach (var tag in tags)
        {
            tag.Data.ToType<DINT>().Should().BeGreaterOrEqualTo(0);
        }
    }

    [Test]
    public void UnusedTags()
    {
        var content = LogixContent.Load(Known.Test);

        var referencedTags = content.Logic().SelectMany(t => t.Tags());

        var unused = content.Tags().SelectMany(t => t.TagName).Where(t => referencedTags.All(r => r != t)).ToList();

        foreach (var tagName in unused)
        {
            Console.WriteLine(tagName);
        }
    }
}