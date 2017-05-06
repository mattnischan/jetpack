using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Jetpack.Core
{
    public static class DecoderExtensions
    {
        private unsafe struct MarshalledParams
        {
            public char* chars;
            public int charCount;
            public byte* bytes;
            public int byteCount;
            public bool flush;
            public Decoder decoder;

            public MarshalledParams(char* chars, int charCount, byte* bytes, int byteCount, bool flush, Decoder decoder)
            {
                this.chars = chars;
                this.charCount = charCount;
                this.bytes = bytes;
                this.byteCount = byteCount;
                this.flush = flush;
                this.decoder = decoder;
            }
        }

        private delegate void ConvertDelegate(MarshalledParams parameters, out int bytesUsed, out int charsUsed, out bool completed);
        private static ConvertDelegate _convertMethod = CreateConvertDelegate();

        private static ConvertDelegate CreateConvertDelegate()
        {
            //Input parameters
            var parameters = Expression.Parameter(typeof(MarshalledParams), "parameters");

            //Marshalled fields
            var decoderField = typeof(MarshalledParams).GetRuntimeField("decoder");
            var charsField = typeof(MarshalledParams).GetRuntimeField("chars");
            var charCountField = typeof(MarshalledParams).GetRuntimeField("charCount");
            var bytesField = typeof(MarshalledParams).GetRuntimeField("bytes");
            var byteCountField = typeof(MarshalledParams).GetRuntimeField("byteCount");
            var flushField = typeof(MarshalledParams).GetRuntimeField("flush");

            var decoder = Expression.Convert(Expression.MakeMemberAccess(parameters, decoderField), Type.GetType("System.Text.DecoderNLS"));
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
                typeof(byte*),
                typeof(int),
                typeof(char*),
                typeof(int),
                typeof(bool),
                typeof(int).MakeByRefType(),
                typeof(int).MakeByRefType(),
                typeof(bool).MakeByRefType()
            };

            var methodParams = new Expression[]
            {
                bytes,
                byteCount,
                chars,
                charCount,
                flush,
                outBytesUsed,
                outCharsUsed,
                outCompleted
            };

            //Method to use
            var method = Encoding.UTF8.GetDecoder().GetType().GetRuntimeMethod("Convert", parameterSignature);
            var call = Expression.Call(decoder, method, methodParams);

            return Expression.Lambda<ConvertDelegate>(call, parameters, outCharsUsed, outBytesUsed, outCompleted).Compile();
        }

        public static unsafe void Convert(this Decoder encoder, byte* bytes, int byteCount,
            char* chars, int charCount, bool flush,
            out int charsUsed, out int bytesUsed, out bool completed)
        {
            _convertMethod(new MarshalledParams(chars, charCount, bytes, byteCount, flush, encoder), out bytesUsed, out charsUsed, out completed);
        }
    }
}
