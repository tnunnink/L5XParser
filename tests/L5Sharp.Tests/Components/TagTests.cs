﻿using FluentAssertions;
using L5Sharp.Components;
using L5Sharp.Core;
using L5Sharp.Enums;
using L5Sharp.Tests.Types.Custom;
using L5Sharp.Types;
using L5Sharp.Types.Atomics;
using L5Sharp.Types.Predefined;

namespace L5Sharp.Tests.Components
{
    [TestFixture]
    public class TagTests
    {
        [Test]
        public void ValueTesting()
        {
            var tag = new Tag<TIMER>
            {
                Name = "Test",
                Value = new TIMER(),
                ["PRE"] = { Value = 5000 },
                ["ACC"] = { Value = 1234 },
                ["EN"] = { Value = true }
            };

            tag.Value.PRE.Should().Be(5000);
            tag.Value.ACC.Should().Be(1234);
            tag.Value.DN.Should().Be(0);
            tag.Value.TT.Should().Be(0);
            tag.Value.EN.Should().Be(1);
        }

        [Test]
        public void ArrayTags()
        {
            var array = new Tag
            {
                Name = "Test",
                Value = new DINT[] { 1, 2, 3, 4, 5 }
            };

            array[0].Value.As<DINT>().Should().Be(1);
        }

        [Test]
        public void New_Default_ShouldNotBeNull()
        {
            var tag = new Tag();

            tag.Should().NotBeNull();
        }

        [Test]
        public void New_Atomic_ShouldNotBeNull()
        {
            var tag = new Tag { Name = "Test", Value = new BOOL() };

            tag.Should().NotBeNull();
            tag.Name.Should().Be("Test");
            tag.Value.Should().BeOfType<BOOL>();
        }

        [Test]
        public void New_Structure_ShouldNotBeNull()
        {
            var tag = new Tag { Name = "Test", Value = new TIMER() };

            tag.Should().NotBeNull();
            tag.Name.Should().Be("Test");
            tag.Value.Should().BeOfType<TIMER>();
            tag.Value.To<TIMER>().DN.Should().Be(0);
        }

        [Test]
        public void New_Array_ShouldNotBeNull()
        {
            var tag = new Tag { Name = "Test", Value = new BOOL[] { 0, 0, 1, 0, 1 } };

            tag.Should().NotBeNull();
            tag.Name.Should().Be("Test");
            tag.Value.Should().BeOfType<ArrayType>();
            tag.Dimensions.Should().Be(new Dimensions(5));
        }

        [Test]
        public void New_Default_ShouldHaveDefaultValues()
        {
            var tag = new Tag();

            tag.Name.Should().BeEmpty();
            tag.Value.Should().Be(LogixType.Null);
            tag.DataType.Should().Be("NULL");
            tag.Dimensions.Should().Be(Dimensions.Empty);
            tag.Radix.Should().Be(Radix.Null);
            tag.ExternalAccess.Should().Be(ExternalAccess.ReadWrite);
            tag.Description.Should().BeEmpty();
            tag.Constant.Should().BeFalse();
            tag.TagType.Should().Be(TagType.Base);
            tag.TagName.Should().Be(TagName.Empty);
            tag.Usage.Should().BeNull();
            tag.AliasFor.Should().BeNull();
        }

        [Test]
        public void New_Overloaded_ShouldHaveExpectedProperties()
        {
            var tag = new Tag
            {
                Name = "Test",
                Description = "This is a test",
                Value = new BOOL(),
                ExternalAccess = ExternalAccess.ReadOnly,
                TagType = TagType.Alias,
                Usage = TagUsage.Local,
                AliasFor = new TagName("SomeOtherTag"),
                Constant = true
            };

            tag.Name.Should().Be("Test");
            tag.Value.Should().BeOfType<BOOL>();
            tag.DataType.Should().Be("BOOL");
            tag.Dimensions.Should().Be(Dimensions.Empty);
            tag.Radix.Should().Be(Radix.Decimal);
            tag.ExternalAccess.Should().Be(ExternalAccess.ReadOnly);
            tag.Description.Should().Be("This is a test");
            tag.Constant.Should().BeTrue();
            tag.Usage.Should().Be(TagUsage.Local);
            tag.TagType.Should().Be(TagType.Alias);
            tag.AliasFor.Should().Be("SomeOtherTag");
            tag.TagName.Should().Be("Test");
        }

        [Test]
        public void SetValue_IsAtomicNewAtomic_ShouldBeExpected()
        {
            var tag = new Tag { Name = "Test", Value = new DINT(12) };

            tag.Value = 21;

            tag.Value.As<DINT>().Should().Be(21);

            var bits = tag.Value.To<DINT>().ToBitArray();

            bits.Length.Should().Be(32);
        }

        [Test]
        public void Member_ValidMember_ShouldBeExpectedTagMember()
        {
            var tag = new Tag
            {
                Name = "Test",
                Value = new TIMER()
            };

            var member = tag.Member("DN");

            member.Should().NotBeNull();
            member?.TagName.Should().Be("Test.DN");
            member?.Value.Should().BeOfType<BOOL>();
            member?.DataType.Should().Be("BOOL");
            member?.Dimensions.Should().Be(Dimensions.Empty);
            member?.Radix.Should().Be(Radix.Decimal);
            member?.Description.Should().BeEmpty();
            //member?.Unit.Should().BeEmpty();
        }

        [Test]
        public void Member_NestedType_ShouldBeExpected()
        {
            var tag = new Tag
            {
                Name = "Test",
                Value = new MyNestedType()
            };

            var member = tag.Member("Simple.M1");

            member.Should().NotBeNull();
            member?.TagName.Should().Be("Test.Simple.M1");
        }

        [Test]
        public void Member_ChainedCalls_ShouldBeExpected()
        {
            var tag = new Tag
            {
                Name = "Test",
                Value = new MyNestedType()
            };

            var nested = tag.Member("Simple")?.Member("M1");

            nested.Should().NotBeNull();
            nested?.TagName.Should().Be("Test.Simple.M1");
        }

        [Test]
        public void Member_NonExisting_ShouldReturnNull()
        {
            var tag = new Tag
            {
                Name = "Test",
                Value = new TIMER()
            };

            var member = tag.Member("Fake");

            member.Should().BeNull();
        }

        [Test]
        public void Members_SimpleStructure_ShouldHaveExpectedCount()
        {
            var tag = new Tag
            {
                Name = "Test",
                Value = new TIMER()
            };

            var members = tag.Members().ToList();

            members.Should().HaveCountGreaterThan(0);
        }

        [Test]
        public void Members_SimpleStructure_ShouldHaveContainExpectedTagNames()
        {
            var tag = new Tag
            {
                Name = "Test",
                Value = new TIMER()
            };

            var members = tag.Members().ToList();

            members.Should().Contain(t => t.TagName == "Test.DN");
            members.Should().Contain(t => t.TagName == "Test.EN");
            members.Should().Contain(t => t.TagName == "Test.TT");
            members.Should().Contain(t => t.TagName == "Test.ACC");
            members.Should().Contain(t => t.TagName == "Test.PRE");
        }

        [Test]
        public void Members_NestedStructure_ShouldHaveExpectedCount()
        {
            var tag = new Tag
            {
                Name = "Test",
                Value = new MyNestedType()
            };

            var members = tag.Members().ToList();

            members.Should().NotBeEmpty();
        }

        [Test]
        public void Members_NestedStructure_AllDataTypesShouldNotBeNull()
        {
            var tag = new Tag
            {
                Name = "Test",
                Value = new MyNestedType()
            };

            var result = tag.Members().ToList().Select(m => m.Value).ToList();

            result.Should().AllBeAssignableTo<LogixType>();
        }

        [Test]
        public void Members_NestedStructure_ShouldHaveExpectedTagNames()
        {
            var tag = new Tag
            {
                Name = "Test",
                Value = new MyNestedType()
            };

            var members = tag.Members().ToList();

            members.Should().Contain(t => t.TagName == "Test.Simple.M1");
            members.Should().Contain(t => t.TagName == "Test.Simple.M2");
            members.Should().Contain(t => t.TagName == "Test.Simple.M3");
            members.Should().Contain(t => t.TagName == "Test.Simple.M4");
            members.Should().Contain(t => t.TagName == "Test.Simple.M5");
            members.Should().Contain(t => t.TagName == "Test.Simple.M6");
            members.Should().Contain(t => t.TagName == "Test.Tmr.DN");
            members.Should().Contain(t => t.TagName == "Test.Tmr.EN");
            members.Should().Contain(t => t.TagName == "Test.Tmr.TT");
            members.Should().Contain(t => t.TagName == "Test.Tmr.ACC");
            members.Should().Contain(t => t.TagName == "Test.Tmr.PRE");
            members.Should().Contain(t => t.TagName == "Test.Str.LEN");
            members.Should().Contain(t => t.TagName == "Test.Str.Value");
            members.Should().Contain(t => t.TagName == "Test.Str.Value[0]");
        }

        [Test]
        public void Members_TagNameContainsTmr_ShouldReturnExpectedCount()
        {
            var tag = new Tag
            {
                Name = "Test",
                Value = new MyNestedType()
            };

            var members = tag.Members(t => t.Contains("Tmr"));

            members.Should().HaveCountGreaterThan(0);
        }

        [Test]
        public void Members_TagNameWithMoreThanThreeMembers_ShouldReturnExpectedCount()
        {
            var tag = new Tag
            {
                Name = "Test",
                Value = new MyNestedType()
            };

            var members = tag.Members(t => t.Members.Count() > 4);

            members.Should().NotBeEmpty();
        }

        [Test]
        public void Members_TagNameEqualToMemberWithMemberNameComparer_ShouldReturnExpectedCount()
        {
            var tag = new Tag
            {
                Name = "Test",
                Value = new MyNestedType()
            };

            var members = tag.Members(t => TagName.Equals(t, "M1", TagNameComparer.MemberName));

            members.Should().HaveCount(1);
        }

        [Test]
        public void Members_TagMemberWithBOOLDataTypeName_ShouldReturnExpectedCount()
        {
            var tag = new Tag
            {
                Name = "Test",
                Value = new MyNestedType()
            };

            var members = tag.Members(t => t.DataType == "BOOL").ToList();

            members.Should().NotBeEmpty();
        }

        [Test]
        public void MembersOf_NestedStructure_ShouldHaveExpectedCount()
        {
            var tag = new Tag
            {
                Name = "Test",
                Value = new MyNestedType()
            };

            var members = tag.MembersOf("Tmr").ToList();

            members.Should().HaveCountGreaterThan(0);
        }

        [Test]
        public void MembersOf_NonExistingMember_ShouldBeEmpty()
        {
            var tag = new Tag
            {
                Name = "Test",
                Value = new MyNestedType()
            };

            var members = tag.MembersOf("Fake").ToList();

            members.Should().BeEmpty();
        }

        [Test]
        public void Value_GetAtomic_ShouldWork()
        {
            var tag = new Tag { Name = "Test", Value = new DINT(33) };

            var value = tag.Value;

            value.As<DINT>().Should().Be(33);
        }

        [Test]
        public void Value_SetAtomic_ShouldWork()
        {
            var tag = new Tag { Name = "Test", Value = new DINT() };

            tag.Value = new DINT(43);

            tag.Value.As<DINT>().Should().Be(43);
        }

        [Test]
        public void Value_GetTimer_ShouldBeNull()
        {
            var tag = new Tag { Name = "Test", Value = new TIMER() };

            var value = tag.Value;

            value.Should().BeNull();
        }

        [Test]
        public void Value_SetTimer_ShouldThrowException()
        {
            var tag = new Tag { Name = "Test", Value = new TIMER() };

            FluentActions.Invoking(() => tag.Value = new REAL(43)).Should().Throw<InvalidOperationException>();
        }

        [Test]
        public void Root_FromDescendantMember_ShouldNotBeNullAndSameAsTag()
        {
            var tag = new Tag { Name = "Test", Value = new TIMER() };

            var member = tag.Member("DN");

            var root = member?.Root;

            root.Should().NotBeNull();
            root.Should().BeSameAs(tag);
        }

        [Test]
        public void Root_FromNestedDescendantMember_ShouldNotBeNullAndSameAsTag()
        {
            var tag = new Tag { Name = "Test", Value = new MyNestedType() };

            var member = tag["Simple.M1"];

            var root = member.Root;

            root.Should().NotBeNull();
            root.Should().BeSameAs(tag);
        }

        [Test]
        public void Parent_WhenCalled_ShouldBeExpected()
        {
            var tag = new Tag { Name = "Test", Value = new MyNestedType() };

            var member = tag["Simple.M1"];

            var parent = member.Parent;

            parent.Should().NotBeNull();
            parent?.TagName.Should().Be("Test.Simple");
        }

        [Test]
        public void Member_GenericNestedComplex_ShouldNotBeNullAndOfType()
        {
            var tag = new Tag
            {
                Name = "Test",
                Value = new MyNestedType()
            };

            var tmr = tag.Member<TIMER>("Tmr");

            tmr?.Value.PRE.Should().Be(0);
        }
    }
}