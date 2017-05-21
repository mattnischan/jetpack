using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Jetpack.Core
{
    /// <summary>
    /// A wrapper around a byte buffer that provides the ability to write
    /// values to the buffer.
    /// </summary>
    /// <remarks>
    /// This class is not guaranteed to be concurrency-safe.
    /// </remarks>
    public unsafe struct WritableBuffer : IDisposable
    {
        private int _bufferSize;

        private byte* _bufferPtr;

        public byte[] Buffer;

        private GCHandle _handle;

        public int CurrentIndex;

        private bool isDisposed;

        private Encoder _encoder;

        private Func<byte[]> _allocate;
        private Action<byte[]> _flush;

        public WritableBuffer(byte[] buffer, Func<byte[]> allocate, Action<byte[]> flush)
        {
            Buffer = buffer;
            _bufferSize = buffer.Length;

            _handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            _bufferPtr = (byte*)_handle.AddrOfPinnedObject().ToPointer();

            _allocate = allocate;
            _flush = flush;

            _encoder = null;
            isDisposed = false;
            CurrentIndex = 0;
        }

        private void Allocate()
        {
            var newBuffer = _allocate();
            Array.Copy(Buffer, CurrentIndex, newBuffer, 0, _bufferSize - CurrentIndex);

            var oldBuffer = Buffer;
            Reset(newBuffer);
            _flush(Buffer);
        }

        public void WriteValue(bool value)
        {
            while (true)
            {
                var newPos = CurrentIndex + 1;
                if (newPos <= _bufferSize)
                {
                    *(bool*)_bufferPtr = value;
                    _bufferPtr += 1;
                    CurrentIndex += 1;
                    break;
                }
                Allocate();
            }
        }

        public void WriteValue(byte value)
        {
            while (true)
            {
                var newPos = CurrentIndex + 1;
                if (newPos <= _bufferSize)
                {
                    *_bufferPtr = value;
                    _bufferPtr += 1;
                    CurrentIndex += 1;

                    break;
                }
                Allocate();
            }
        }

        public void WriteValue(sbyte value)
        {
            while (true)
            {
                var newPos = CurrentIndex + 1;
                if (newPos <= _bufferSize)
                {
                    *(sbyte*)_bufferPtr = value;
                    _bufferPtr += 1;
                    CurrentIndex += 1;

                    break;
                }
                Allocate();
            }
        }

        public void WriteValue(char value)
        {
            while (true)
            {
                var newPos = CurrentIndex + 2;
                if (newPos <= _bufferSize)
                {
                    *(char*)_bufferPtr = value;
                    _bufferPtr += 2;
                    CurrentIndex += 2;

                    break;
                }
                Allocate();
            }
        }

        public void WriteValue(decimal value)
        {
            while (true)
            {
                var newPos = CurrentIndex + 16;
                if (newPos <= _bufferSize)
                {
                    *(decimal*)_bufferPtr = value;
                    _bufferPtr += 16;
                    CurrentIndex += 16;

                    break;
                }
                Allocate();
            }
        }

        public void WriteValue(double value)
        {
            while (true)
            {
                var newPos = CurrentIndex + 8;
                if (newPos <= _bufferSize)
                {
                    *(double*)_bufferPtr = value;
                    _bufferPtr += 8;
                    CurrentIndex += 8;

                    break;
                }
                Allocate();
            }
        }

        public void WriteValue(float value)
        {
            while (true)
            {
                var newPos = CurrentIndex + 4;
                if (newPos <= _bufferSize)
                {
                    *(float*)_bufferPtr = value;
                    _bufferPtr += 4;
                    CurrentIndex += 4;

                    break;
                }
                Allocate();
            }
        }

        public void WriteValue(int value)
        {
            while (true)
            {
                var newPos = CurrentIndex + 5;
                if (newPos <= _bufferSize)
                {
                    *_bufferPtr = (byte)FieldType.Int;
                    _bufferPtr++;

                    *(int*)_bufferPtr = value;
                    _bufferPtr += 4;
                    CurrentIndex += 5;

                    break;
                }
                Allocate();
            }
        }

        public void WriteValue(uint value)
        {
            while (true)
            {
                var newPos = CurrentIndex + 4;
                if (newPos <= _bufferSize)
                {
                    *(uint*)_bufferPtr = value;
                    _bufferPtr += 4;
                    CurrentIndex += 4;

                    break;
                }
                Allocate();
            }
        }

        public void WriteValue(long value)
        {
            while (true)
            {
                var newPos = CurrentIndex + 8;
                if (newPos <= _bufferSize)
                {
                    *(long*)_bufferPtr = value;
                    _bufferPtr += 8;
                    CurrentIndex += 8;

                    break;
                }
                Allocate();
            }
        }

        public void WriteValue(ulong value)
        {
            while (true)
            {
                var newPos = CurrentIndex + 8;
                if (newPos <= _bufferSize)
                {
                    *(ulong*)_bufferPtr = value;
                    _bufferPtr += 8;
                    CurrentIndex += 8;

                    break;
                }
                Allocate();
            }
        }

        public void WriteValue(short value)
        {
            while (true)
            {
                var newPos = CurrentIndex + 2;
                if (newPos <= _bufferSize)
                {
                    *(short*)_bufferPtr = value;
                    _bufferPtr += 2;
                    CurrentIndex += 2;

                    break;
                }
                Allocate();
            }
        }

        public void WriteValue(ushort value)
        {
            while (true)
            {
                var newPos = CurrentIndex + 2;
                if (newPos <= _bufferSize)
                {
                    *(ushort*)_bufferPtr = value;
                    _bufferPtr += 2;
                    CurrentIndex += 2;

                    break;
                }
                Allocate();
            }
        }

        public void WriteValue(DateTime value)
        {
            while (true)
            {
                var newPos = CurrentIndex + 9;
                if (newPos <= _bufferSize)
                {
                    *_bufferPtr = (byte)FieldType.DateTime;
                    _bufferPtr++;

                    *(long*)_bufferPtr = value.Ticks;
                    _bufferPtr += 8;
                    CurrentIndex += 9;

                    break;
                }
                Allocate();
            }
        }

        public void WriteValue(Guid value)
        {
            while (true)
            {
                var newPos = CurrentIndex + 17;
                if (newPos <= _bufferSize)
                {
                    *_bufferPtr = (byte)FieldType.Guid;
                    _bufferPtr++;

                    *(Guid*)_bufferPtr = value;
                    _bufferPtr += 16;
                    CurrentIndex += 17;

                    break;
                }
                Allocate();
            }
        }

        public void WriteValue(string value)
        {
            int index = 0;
            if(!WriteString(value, out var charsWritten))
            {
                var completed = false;
                do
                {
                    Allocate();
                    completed = WriteString(value, index, out charsWritten);
                    index += charsWritten;
                } while (!completed);
            }
        }

        private bool WriteString(string value, out int charsWritten)
        {
            *_bufferPtr = (byte)FieldType.String;
            _bufferPtr++;
            CurrentIndex++;

            fixed(char* charPtr = value)
            {
                if(_encoder == null)
                {
                    _encoder = Encoding.UTF8.GetEncoder();
                }

                _encoder.Convert(charPtr, value.Length, _bufferPtr, (_bufferSize - 1) - CurrentIndex, false, out var charsUsed, out var bytesUsed, out var completed);

                if(charsUsed == value.Length)
                {
                    _encoder.Reset();
                }

                _bufferPtr += bytesUsed;
                CurrentIndex += bytesUsed;
                charsWritten = charsUsed;

                return completed;
            }
        }

        private bool WriteString(string value, int startIndex, out int charsWritten)
        {
            fixed (char* charPtrOrigin = value)
            {
                char* charPtr = charPtrOrigin + startIndex;

                if (_encoder == null)
                {
                    _encoder = Encoding.UTF8.GetEncoder();
                }

                _encoder.Convert(charPtr, value.Length - startIndex, _bufferPtr, (_bufferSize - 1) - CurrentIndex, false, out var charsUsed, out var bytesUsed, out var completed);

                if (charsUsed == (value.Length - startIndex))
                {
                    _encoder.Reset();
                }

                _bufferPtr += bytesUsed;
                CurrentIndex += bytesUsed;
                charsWritten = charsUsed;

                return completed;
            }
        }

        public void Reset(byte[] buffer)
        {
            if (_handle != default(GCHandle))
            {
                _handle.Free();
            }

            _handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            _bufferPtr = (byte*)_handle.AddrOfPinnedObject().ToPointer();
            CurrentIndex = 0;
        }

        public void Reset(byte* buffer)
        {
            if (_handle != default(GCHandle))
            {
                _handle.Free();
            }

            _handle = default(GCHandle);
            _bufferPtr = buffer;
            CurrentIndex = 0;
        }

        public void Reset()
        {
            _bufferPtr -= CurrentIndex;
            CurrentIndex = 0;
        }

        public void Dispose(bool disposing)
        {
            if (!isDisposed)
            {
                if (_handle != default(GCHandle))
                {
                    _handle.Free();
                }
                _flush(Buffer);
                isDisposed = true;
            }
        }


        public void Dispose()
        {
            Dispose(true);
        }
    }
}
