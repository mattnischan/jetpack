using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Jetpack.Core
{
    public static class SerializerCompiler
    {
        public static Dictionary<Type, FieldType> TypeManifest = new Dictionary<Type, FieldType>()
        {
            { typeof(bool), FieldType.Bool },
            { typeof(byte), FieldType.Byte },
            { typeof(sbyte), FieldType.SByte },
            { typeof(char), FieldType.Char },
            { typeof(decimal), FieldType.Decimal },
            { typeof(double), FieldType.Double },
            { typeof(float), FieldType.Float },
            { typeof(int), FieldType.Int },
            { typeof(uint), FieldType.UInt },
            { typeof(long), FieldType.Long },
            { typeof(ulong), FieldType.ULong },
            { typeof(short), FieldType.Short },
            { typeof(ushort), FieldType.UShort },
            { typeof(string), FieldType.String },
            { typeof(DateTime), FieldType.DateTime },
            { typeof(Guid), FieldType.Guid }
        };

        public static Action<WritableBuffer, JetpackSession, T> BuildAlphaSerializer<T>()
        {
            var fields = typeof(T).GetRuntimeFields()
                .Where(x => x.IsPublic || x.IsPrivate || x.IsInitOnly)
                .OrderBy(x => x.Name);

            var session = Expression.Parameter(typeof(JetpackSession), "session");
            var writableBuffer = Expression.Parameter(typeof(WritableBuffer), "buffer");
            var item = Expression.Parameter(typeof(T), "item");

            var fieldSerializers = new List<Expression>();
            foreach(var field in fields)
            {
                fieldSerializers.Add(WriteValue(writableBuffer, Expression.Constant((byte)TypeManifest[field.FieldType]), typeof(byte)));
                fieldSerializers.Add(WriteValue(writableBuffer, Expression.Field(item, field), field.FieldType));
            }

            var expression = Expression.Lambda<Action<WritableBuffer, JetpackSession, T>>(Expression.Block(fieldSerializers), new[] { session, writableBuffer, item });
            return expression.Compile();
        }

        public static Expression WriteValue(Expression writableBuffer, Expression field, Type valueType)
        {
            var writeValueMethod = typeof(WritableBuffer)
                .GetRuntimeMethod("WriteValue", new[] { valueType });

            var writeValue = Expression.Call(writableBuffer, writeValueMethod, new[] { field });
            var writeValueNotEqual = Expression.NotEqual(writeValue, Expression.Constant(true));

            return Expression.IfThen(writeValueNotEqual, Expression.Block(new Expression[] 
            {
                GetBuffer,
                writeValue
            }));
        }

        public static Expression<Func<JetpackSession, byte[]>> GetBuffer = (JetpackSession session) => session.GetBuffer();
    }
}
