using Jetpack.Core;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Jetpack.Tests
{
    public class BufferTests
    {
        private WritableBuffer _wBuf;
        private ReadableBuffer _rBuf;

        public BufferTests()
        {
            var buffer = new byte[16];
            _wBuf = new WritableBuffer(buffer);
            _rBuf = new ReadableBuffer(buffer);
        }

        [Fact]
        public void TestBool()
        {
            _wBuf.WriteValue(true);
            _rBuf.ReadValue(out bool value);
            Assert.True(value);
        }

        [Fact]
        public void TestDecimal()
        {
            _wBuf.WriteValue(15.1234M);
            _rBuf.ReadValue(out decimal value);
            Assert.Equal(15.1234M, value);
        }

        [Fact]
        public void TestString()
        {
            _wBuf.WriteValue("Hello World!", out var charsWritten);
            _rBuf.ReadValue(out string value, 12);
            Assert.Equal("Hello World!", value);
        }

        [Fact]
        public void TestStringSplit()
        {
            var buf = new byte[128];
            var writer = new WritableBuffer(buf);

            writer.WriteValue("Hello World! Hello World!", out var charsWritten);

            var newBuf = new byte[1];
            Array.Copy(buf, newBuf, 1);

            var reader = new ReadableBuffer(newBuf);
            var result = reader.ReadValue(out string value, 25);

            Assert.False(result.IsRead);

            var nextBuf = new byte[32];
            Array.Copy(buf, result.BytesRead, nextBuf, 0, 32);
            reader.Reset(nextBuf);

            result = reader.ReadValue(ref value, result.CharsRead, true);

            Assert.True(result.IsRead);
            Assert.Equal("Hello World! Hello World!", value);
        }
    }
}
