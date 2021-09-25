﻿using System;
using System.Linq;
using AutoFixture;
using FluentAssertions;
using LogixHelper.Enumerations;
using LogixHelper.Exceptions;
using LogixHelper.Primitives;
using NUnit.Framework;

namespace LogixHelper.Tests
{
    [TestFixture]
    public class DataTypeTests
    {
        [Test]
        public void New_EmptyTagName_ShouldThrowArgumentNullException()
        {
            FluentActions.Invoking(() => new DataType(string.Empty)).Should()
                .Throw<ArgumentException>();
        }

        [Test]
        public void New_InvalidTagName_ShouldThrowInvalidTagNameException()
        {
            var fixture = new Fixture();
            FluentActions.Invoking(() => new DataType(fixture.Create<string>())).Should()
                .Throw<InvalidTagNameException>();
        }

        [Test]
        public void New_ValidNameAndDescription_ShouldBeExpected()
        {
            var fixture = new Fixture();
            var description = fixture.Create<string>();

            var type = new DataType("Test_Type_001", description);

            type.Should().NotBeNull();
            type.Name.Should().Be("Test_Type_001");
            type.Class.Should().Be(DataTypeClass.User);
            type.Family.Should().Be(DataTypeFamily.None);
            type.Description.Should().Be(description);
            type.IsAtomic.Should().BeFalse();
            type.Members.Should().BeEmpty();
        }

        [Test]
        public void StringType_ValidNameAndLength_ShouldHaveExpectedProperties()
        {
            var fixture = new Fixture();
            var desc = fixture.Create<string>();

            var type = DataType.StringType("Test_String_001", 100, desc);

            type.Should().NotBeNull();
            type.Name.Should().Be("Test_String_001");
            type.Class.Should().Be(DataTypeClass.User);
            type.Family.Should().Be(DataTypeFamily.String);
            type.Description.Should().Be(desc);
            type.Members.Should().Contain(m => m.Name == "LEN");
            type.Members.Should().Contain(m => m.Name == "DATA");
            var data = type.Members.Single(m => m.Name == "DATA");
            data.Dimension.Should().Be(100);
            data.Radix.Should().Be(Radix.Ascii);
            data.ExternalAccess.Should().Be(ExternalAccess.ReadWrite);
        }

        [Test]
        public void StringType_InvalidLength_ShouldThrowException()
        {
            var fixture = new Fixture();
            var desc = fixture.Create<string>();

            FluentActions.Invoking(() => DataType.StringType("Test_String_001", 0, desc)).Should()
                .Throw<ArgumentException>();
        }

        [Test]
        public void StringType_AddMember_ShouldThrowNotConfigurableException()
        {
            var fixture = new Fixture();
            var desc = fixture.Create<string>();

            var type = DataType.StringType("Test_String_001", 100, desc);

            FluentActions.Invoking(() => type.AddMember("Test", DataType.Dint)).Should()
                .Throw<NotConfigurableException>();
        }
        
        [Test]
        public void StringType_RemoveMember_ShouldThrowNotConfigurableException()
        {
            var fixture = new Fixture();
            var desc = fixture.Create<string>();

            var type = DataType.StringType("Test_String_001", 100, desc);

            FluentActions.Invoking(() => type.RemoveMember("LEN")).Should()
                .Throw<NotConfigurableException>();
        }
        
        [Test]
        public void StringType_UpdateMember_ShouldThrowNotConfigurableException()
        {
            var fixture = new Fixture();
            var desc = fixture.Create<string>();

            var type = DataType.StringType("Test_String_001", 100, desc);

            FluentActions.Invoking(() => type.UpdateMember("LEN").WithDataType(DataType.Bool)).Should()
                .Throw<NotConfigurableException>();
        }
        
        [Test]
        public void StringType_RenameMember_ShouldThrowNotConfigurableException()
        {
            var fixture = new Fixture();
            var desc = fixture.Create<string>();

            var type = DataType.StringType("Test_String_001", 100, desc);

            FluentActions.Invoking(() => type.RenameMember("Data", "Test")).Should()
                .Throw<NotConfigurableException>();
        }

        [Test]
        public void SetName_ValidName_ShouldBeExpectedValue()
        {
            var fixture = new Fixture();
            var description = fixture.Create<string>();
            var type = new DataType("Test", description);

            type.Name = "NewName";

            type.Name.Should().Be("NewName");
        }
        
        [Test]
        public void SetName_InvalidName_ShouldThrowInvalidTagNameException()
        {
            var fixture = new Fixture();
            var description = fixture.Create<string>();
            var type = new DataType("Test", description);

            FluentActions.Invoking(() => type.Name = "Not.Valid%01").Should().Throw<InvalidTagNameException>();
        }
        
        [Test]
        public void SetDescription_WhenCalled_ShouldBeExpectedValue()
        {
            var fixture = new Fixture();
            var description = fixture.Create<string>();
            var newDescription = fixture.Create<string>();
            var type = new DataType("Test", description);

            type.Description = newDescription;

            type.Description.Should().Be(newDescription);
        }
        
        [Test]
        public void AddMember_ExistingMember_ShouldThrowMemberNameCollisionException()
        {
            var type = new DataType("Test_Type_001");
            type.AddMember("Member", DataType.Dint);

            FluentActions.Invoking(() => type.AddMember("Member", DataType.Int)).Should()
                .Throw<MemberNameCollisionException>();
        }

        [Test]
        public void AddMember_InvalidTagName_ShouldThrowInvalidTagNameException()
        {
            var type = new DataType("Test_Type_001");

            FluentActions.Invoking(() => type.AddMember("This_is Not_It", DataType.Dint)).Should()
                .Throw<InvalidTagNameException>();
        }
        
        [Test]
        public void AddMember_NullDataType_ShouldThrowArgumentNullException()
        {
            var type = new DataType("Test_Type_001");

            FluentActions.Invoking(() => type.AddMember("Member", null)).Should()
                .Throw<ArgumentNullException>();
        }
        
        [Test]
        public void AddMember_NullRadix_ShouldThrowArgumentNullException()
        {
            var type = new DataType("Test_Type_001");

            FluentActions.Invoking(() => type.AddMember("Member", DataType.Int).WithRadix(null)).Should()
                .Throw<ArgumentNullException>();
        }
        
        [Test]
        public void AddMember_InvalidRadixForType_ShouldThrowRadixNotSupportedException()
        {
            var type = new DataType("Test_Type_001");

            FluentActions.Invoking(() => type.AddMember("Member", DataType.Int).WithRadix(Radix.Float)).Should()
                .Throw<RadixNotSupportedException>();
        }
        
        [Test]
        public void AddMember_NullExternalAccess_ShouldThrowArgumentNullException()
        {
            var type = new DataType("Test_Type_001");

            FluentActions.Invoking(() => type.AddMember("Member", DataType.Int).WithAccess(null)).Should()
                .Throw<ArgumentNullException>();
        }
        
        [Test]
        public void AddMember_ValidArguments_ShouldHaveExpectedMember()
        {
            var type = new DataType("Test_Type_001");

            type.AddMember("Member", DataType.Dint);

            type.Members.Should().HaveCount(1);
            var result = type.Members.Single(m => m.Name == "Member");
            result.Should().NotBeNull();
            result.Name.Should().Be("Member");
            result.DataType.Should().Be(DataType.Dint);
            result.Dimension.Should().Be(0);
            result.Description.Should().Be(string.Empty);
            result.Radix.Should().Be(Radix.Decimal);
            result.ExternalAccess.Should().Be(ExternalAccess.ReadWrite);
        }

        [Test]
        public void AddMember_WithProperties_ShouldHaveExpectedProperties()
        {
            var type = new DataType("Test_Type_001");

            type.AddMember("Member", DataType.Dint)
                .WithDataType(DataType.Int)
                .WithDimension(4)
                .WithDescription("This is a test description")
                .WithRadix(Radix.Hex)
                .WithAccess(ExternalAccess.None);

            var result = type.Members.SingleOrDefault(m => m.Name == "Member");
            result.Should().NotBeNull();
            result?.Name.Should().Be("Member");
            result?.DataType.Should().Be(DataType.Int);
            result?.Dimension.Should().Be(4);
            result?.Description.Should().Be("This is a test description");
            result?.Radix.Should().Be(Radix.Hex);
            result?.ExternalAccess.Should().Be(ExternalAccess.None);
        }

        [Test]
        public void UpdateMember_ExistingMemberWithProperties_ShouldHaveExpectedProperties()
        {
            var type = new DataType("Test_Type_001");
            type.AddMember("Member", DataType.Dint);

            type.UpdateMember("Member")
                .WithDataType(DataType.Int)
                .WithDimension(4)
                .WithDescription("This is a test description")
                .WithRadix(Radix.Hex)
                .WithAccess(ExternalAccess.None);

            var result = type.Members.SingleOrDefault(m => m.Name == "Member");
            result.Should().NotBeNull();
            result?.Name.Should().Be("Member");
            result?.DataType.Should().Be(DataType.Int);
            result?.Dimension.Should().Be(4);
            result?.Description.Should().Be("This is a test description");
            result?.Radix.Should().Be(Radix.Hex);
            result?.ExternalAccess.Should().Be(ExternalAccess.None);
        }
        
        [Test]
        public void UpdateMember_NonExistingMember_ShouldThrowMemberNotFoundException()
        {
            var type = new DataType("Test_Type_001");
            type.AddMember("Member", DataType.Dint);

            FluentActions.Invoking(() => type.UpdateMember("Test")).Should().Throw<MemberNotFoundException>();
        }
        
        [Test]
        public void RemoveMember_ExistingMember_MembersShouldEmpty()
        {
            var type = new DataType("Test_Type_001");
            type.AddMember("Member", DataType.Dint);

            type.RemoveMember("Member");

            type.Members.Should().BeEmpty();
        }
        
        [Test]
        public void RemoveMember_NonExistingMember_MembersShouldSame()
        {
            var type = new DataType("Test_Type_001");
            type.AddMember("Member", DataType.Dint);

            type.RemoveMember("Test");

            type.Members.Should().HaveCount(1);
        }
        
        [Test]
        public void GetMember_ExistingName_ShouldNotBeNullAndExpected()
        {
            var fixture = new Fixture();
            var description = fixture.Create<string>();
            var type = new DataType("Test", description);
            type.AddMember("Member", DataType.Dint);

            var member = type.GetMember("Member");

            member.Should().NotBeNull();
            member.Name.Should().Be("Member");
            member.DataType.Should().Be(DataType.Dint);
        }
    }
}