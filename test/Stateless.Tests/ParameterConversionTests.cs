using System;
using Xunit;

namespace Stateless.Tests
{
    public class ParameterConversionTests
    {
        private readonly object[] args = { 5, 10, 15 };

        [Fact]
        public void Unpack_ShouldReturnArg_WhenValidationSucceeds()
        {
            Assert.Equal(5, ParameterConversion.Unpack(args, typeof(int), 0));
            Assert.Equal(10, ParameterConversion.Unpack(args, typeof(int), 1));
            Assert.Equal(15, ParameterConversion.Unpack(args, typeof(int), 2));
            Assert.Null(ParameterConversion.Unpack(new object[] { "John", null, "Doe" }, typeof(string), 1));
        }

        [Fact]
        public void Unpack_ShouldThrowArgumentNullException_WhenArgsIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => ParameterConversion.Unpack(null, null, 0));
        }

        [Fact]
        public void Unpack_ShouldReturnNull_WhenArgsLengthIsZero()
        {
            Assert.Null(ParameterConversion.Unpack(Array.Empty<object>(), null, 0));
        }

        [Fact]
        public void Unpack_ShouldThrowArgumentException_WhenArgsLengthLessThanIndex()
        {
            Assert.Throws<ArgumentException>(() => ParameterConversion.Unpack(args, typeof(int), 5));
        }

        [Fact]
        public void Unpack_ShouldThrowArgumentException_WhenIndexLessThanZero()
        {
            Assert.Throws<ArgumentException>(() => ParameterConversion.Unpack(args, typeof(int), -1));
        }

        [Fact]
        public void Unpack_ShouldThrowArgumentException_WhenTypeIsNotAssignable()
        {
            Assert.Throws<ArgumentException>(() => ParameterConversion.Unpack(args, typeof(char), 0));
        }

        [Fact]
        public void Unpack_ShouldReturnDefault_When2ParameterMethodCalled()
        {
            Assert.Equal(0, ParameterConversion.Unpack<int>(Array.Empty<object>(), 0));
            Assert.Equal(char.MinValue, ParameterConversion.Unpack<char>(Array.Empty<object>(), 0));
            Assert.Equal(false, ParameterConversion.Unpack<bool>(Array.Empty<object>(), 0));
        }

        [Fact]
        public void Validate_ShouldThrowArgumentException_WhenArgsGreaterThanExpected()
        {
            Assert.Throws<ArgumentException>(() => ParameterConversion.Validate(args, new Type[] { null, null }));
        }

        [Fact]
        public void Validate_ShouldWorkWithoutException_WhenEverythingExpectedIsProvided()
        {
            try
            {
                ParameterConversion.Validate(args, new Type[] { typeof(int), typeof(int), typeof(int) });
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
