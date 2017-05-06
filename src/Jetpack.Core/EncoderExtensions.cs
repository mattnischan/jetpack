using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Jetpack.Core
{
    public static class EncoderExtensions
    {
        private unsafe struct MarshalledParams
        {
            public char* chars;
            public int charCount;
            public byte* bytes;
            public int byteCount;
            public bool flush;
            public Encoder encoder;

            public MarshalledParams(char* chars, int charCount, byte* bytes, int byteCount, bool flush, Encoder encoder)
            {
                this.chars = chars;
                this.charCount = charCount;
                this.bytes = bytes;
                this.byteCount = byteCount;
                this.flush = flush;
                this.encoder = encoder;
            }
        }

        private delegate void ConvertDelegate(MarshalledParams parameters, out int charsUsed, out int bytesUsed, out bool completed);
        private static ConvertDelegate _convertMethod = CreateConvertDelegate();

        private static ConvertDelegate CreateConvertDelegate()
        {
            //Input parameters
            var parameters = Expression.Parameter(typeof(MarshalledParams), "parameters");

            //Marshalled fields
            var encoderField = typeof(MarshalledParams).GetRuntimeField("encoder");
            var charsField = typeof(MarshalledParams).GetRuntimeField("chars");
            var charCountField = typeof(MarshalledParams).GetRuntimeField("charCount");
            var bytesField = typeof(MarshalledParams).GetRuntimeField("bytes");
            var byteCountField = typeof(MarshalledParams).GetRuntimeField("byteCount");
            var flushField = typeof(MarshalledParams).GetRuntimeField("flush");

            var encoder = Expression.Convert(Expression.MakeMemberAccess(parameters, encoderField), Type.GetType("System.Text.EncoderNLS"));
            var chars = Expression.MakeMemberAccess(parameters, charsField);
            var charCount = Expression.MakeMemberAccess(parameters, charCountField);
            var bytes = Expression.MakeMemberAccess(parameters, bytesField);
            var byteCount = Expression.MakeMemberAccess(parameters, byteCountField);
            var flush = Expression.MakeMemberAccess(parameters, flushField);

            //Out parameters
            var outCharsUsed = Expression.Parameter(typeof(int).MakeByRefType(), "charsUsed");
            var outBytesUsed = Expression.Parameter(typeof(int).MakeByRefType(), "bytesUsed");
            var outCompleted = Expression.Parameter(typeof(bool).MakeByRefType(), "completed");

            var parameterSignature = new[]
            {
                typeof(char*),
                typeof(int),
                typeof(byte*),
                typeof(int),
                typeof(bool),
                typeof(int).MakeByRefType(),
                typeof(int).MakeByRefType(),
                typeof(bool).MakeByRefType()
            };

            var methodParams = new Expression[]
            {
                chars,
                charCount,
                bytes,
                byteCount,
                flush,
                outCharsUsed,
                outBytesUsed,
                outCompleted
            };

            //Method to use
            var method = Encoding.UTF8.GetEncoder().GetType().GetRuntimeMethod("Convert", parameterSignature);
            var call = Expression.Call(encoder, method, methodParams);

            return Expression.Lambda<ConvertDelegate>(call, parameters, outCharsUsed, outBytesUsed, outCompleted).Compile();
        }

        public static unsafe void Convert(this Encoder encoder, char* chars, int charCount,
            byte* bytes, int byteCount, bool flush,
            out int charsUsed, out int bytesUsed, out bool completed)
        {
            _convertMethod(new MarshalledParams(chars, charCount, bytes, byteCount, flush, encoder), out charsUsed, out bytesUsed, out completed);
        }
    }
}
