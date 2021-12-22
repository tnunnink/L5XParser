﻿using System;
using System.Collections;
using FluentAssertions;
using L5Sharp.Components;
using L5Sharp.Enums;
using L5Sharp.Types;
using NUnit.Framework;
using String = L5Sharp.Types.String; 

namespace L5Sharp.Core.Tests
{
    [TestFixture]
    public class MemberTests
    {
        [Test]
        public void Create_ValidNameAndType_ShouldNotBeNull()
        {
            var type = new UserDefined("Test");
            var member = Member.Create("Test", type);

            member.Should().NotBeNull();
        }

        [Test]
        public void Create_TypedValidNameAndType_ShouldNotBeNull()
        {
            var member = Member.Create("Test", new Bool());

            member.Should().NotBeNull();
        }

        [Test]
        public void Create_TypedValidName_ShouldNotBeNull()
        {
            var member = Member.Create<Bool>("Test");

            member.Should().NotBeNull();
        }

        [Test]
        public void New_NullName_ShouldThrowArgumentNullException()
        {
            FluentActions.Invoking(() => Member.Create<Dint>(null!)).Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void New_NullType_ShouldHaveNullType()
        {
            FluentActions.Invoking(() => Member.Create("Name", null!)).Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void New_Overloaded_ShouldHaveExpectedValues()
        {
            var member = Member.Create("Member", (IDataType)new Real(), Dimensions.Empty, Radix.Exponential,
                ExternalAccess.ReadOnly, "Test");

            member.Should().NotBeNull();
            member.Name.Should().Be("Member");
            member.DataType.Should().BeOfType<Real>();
            member.Dimension.Should().Be(Dimensions.Empty);
            member.Radix.Should().Be(Radix.Exponential);
            member.ExternalAccess.Should().Be(ExternalAccess.ReadOnly);
            member.Description.Should().Be("Test");
        }

        [Test]
        public void Name_GetValue_ShouldBeExpected()
        {
            var member = Member.Create<Real>("Member");

            var name = member.Name;

            name.Should().Be("Member");
        }

        [Test]
        public void Description_GetValue_ShouldBeExpected()
        {
            var member = Member.Create<Real>("Member");

            var description = member.Description;

            description.Should().BeEmpty();
        }

        [Test]
        public void DataType_GetValue_ShouldBeExpected()
        {
            var member = Member.Create<Real>("Member");

            var dataType = member.DataType;

            dataType.Should().Be(new Real());
        }

        [Test]
        public void Dimension_GetValue_ShouldBeExpected()
        {
            var member = Member.Create<Real>("Member");

            var dimension = member.Dimension;

            dimension.Length.Should().Be(0);
        }

        [Test]
        public void Radix_GetValue_ShouldBeExpected()
        {
            var member = Member.Create<Real>("Member");

            var radix = member.Radix;

            radix.Should().Be(Radix.Float);
        }

        [Test]
        public void ExternalAccess_GetValue_ShouldBeExpected()
        {
            var member = Member.Create<Real>("Member");

            var access = member.ExternalAccess;

            access.Should().Be(ExternalAccess.ReadWrite);
        }

        [Test]
        public void Index_Get_ShouldReturnValidElement()
        {
            var member = Member.Create<Dint>("Test", new Dimensions(10));

            var element = member[5];

            element.Should().NotBeNull();
            element.Name.Should().Be("[5]");
            element.DataType.Should().BeOfType<Dint>();
            element.Radix.Should().Be(Radix.Decimal);
            element.ExternalAccess.Should().Be(ExternalAccess.ReadWrite);
            element.Description.Should().BeEmpty();
        }

        [Test]
        public void HasValue_IsAtomicMember_ShouldBeTrue()
        {
            var member = Member.Create<Bool>("Test");

            member.HasValue.Should().BeTrue();
        }
        
        [Test]
        public void HasValue_IsComplexMember_ShouldBeFalse()
        {
            var member = Member.Create<String>("Test");

            member.HasValue.Should().BeFalse();
        }
        
        [Test]
        public void HasStructure_IsAtomicMember_ShouldBeFalse()
        {
            var member = Member.Create<Bool>("Test");

            member.HasStructure.Should().BeFalse();
        }
        
        [Test]
        public void HasStructure_IsComplexMember_ShouldBeTrue()
        {
            var member = Member.Create<String>("Test");

            member.HasStructure.Should().BeTrue();
        }
        
        [Test]
        public void HasArray_DimensionsEmpty_ShouldBeFalse()
        {
            var member = Member.Create<Bool>("Test");

            member.HasArray.Should().BeFalse();
        }
        
        [Test]
        public void HasArray_DimensionsNotEmpty_ShouldBeTrue()
        {
            var member = Member.Create<Bool>("Test", new Dimensions(5));

            member.HasArray.Should().BeTrue();
        }
        
        [Test]
        public void IterateCollection_ShouldAllNotBeNull()
        {
            var member = Member.Create<Dint>("Test", new Dimensions(10));

            foreach (var element in member)
            {
                element.Should().NotBeNull();
            }
        }

        [Test]
        public void Copy_WhenCalled_ShouldNotBeSameButEqual()
        {
            var member = Member.Create<Dint>("Test", Dimensions.Empty, Radix.Binary, ExternalAccess.ReadOnly,
                "This is a test");

            var copy = member.Copy();

            copy.Should().NotBeSameAs(member);
            copy.Name.Should().NotBeSameAs(member.Name);
            copy.DataType.Should().NotBeSameAs(member.DataType);
            copy.Dimension.Should().NotBeSameAs(member.Dimension);
            copy.Description.Should().NotBeSameAs(member.Description);
        }

        [Test]
        public void TypedEquals_AreEqual_ShouldBeTrue()
        {
            var first = (Member<Bool>)Member.Create("Test", new Bool());
            var second = (Member<Bool>)Member.Create("Test", new Bool());

            var result = first.Equals(second);

            result.Should().BeTrue();
        }

        [Test]
        public void TypedEquals_AreSame_ShouldBeTrue()
        {
            var first = (Member<Bool>)Member.Create("Test", new Bool());
            var second = first;

            var result = first.Equals(second);

            result.Should().BeTrue();
        }


        [Test]
        public void TypedEquals_Null_ShouldBeFalse()
        {
            var first = (Member<Bool>)Member.Create("Test", new Bool());

            var result = first.Equals(null);

            result.Should().BeFalse();
        }

        [Test]
        public void ObjectEquals_AreEqual_ShouldBeTrue()
        {
            var first = Member.Create("Test", new Bool());
            var second = Member.Create("Test", new Bool());

            var result = first.Equals(second);

            result.Should().BeTrue();
        }

        [Test]
        public void ObjectEquals_AreSame_ShouldBeTrue()
        {
            var first = Member.Create("Test", new Bool());
            var second = first;

            var result = first.Equals(second);

            result.Should().BeTrue();
        }


        [Test]
        public void ObjectEquals_Null_ShouldBeFalse()
        {
            var first = Member.Create("Test", new Bool());

            var result = first.Equals(null);

            result.Should().BeFalse();
        }

        [Test]
        public void OperatorEquals_AreEqual_ShouldBeTrue()
        {
            var first = (Member<Bool>)Member.Create("Test", new Bool());
            var second = (Member<Bool>)Member.Create("Test", new Bool());

            var result = first == second;

            result.Should().BeTrue();
        }

        [Test]
        public void OperatorNotEquals_AreEqual_ShouldBeFalse()
        {
            var first = (Member<Bool>)Member.Create("Test", new Bool());
            var second = (Member<Bool>)Member.Create("Test", new Bool());

            var result = first != second;

            result.Should().BeFalse();
        }

        [Test]
        public void GetHashCode_WhenCalled_ShouldNotBeZero()
        {
            var first = Member.Create("Test", new Bool());

            var hash = first.GetHashCode();

            hash.Should().NotBe(0);
        }

        [Test]
        public void GetEnumerator_Object_ShouldNotBeNull()
        {
            var first = (IEnumerable)Member.Create<Dint>("Test", new Dimensions(10));

            var enumerator = first.GetEnumerator();

            enumerator.Should().NotBeNull();
        }
    }
}