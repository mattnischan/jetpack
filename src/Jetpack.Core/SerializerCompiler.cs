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

        public static Action<WritableBuffer, T> BuildAlphaSerializer<T>()
        {
            var fields = typeof(T).GetRuntimeFields()
                .Where(x => x.IsPublic || x.IsPrivate || x.IsInitOnly)
                .OrderBy(x => x.Name);

            var writableBuffer = Expression.Parameter(typeof(WritableBuffer), "buffer");
            var item = Expression.Parameter(typeof(T), "item");

            var fieldSerializers = new List<Expression>();
            foreach(var field in fields)
            {
                fieldSerializers.Add(WriteValue(writableBuffer, Expression.Field(item, field), field.FieldType));
            }

            var expression = Expression.Lambda<Action<WritableBuffer, T>>(Expression.Block(fieldSerializers), new[] { writableBuffer, item });
            return expression.Compile();
        }

        public static Expression WriteValue(Expression writableBuffer, Expression field, Type valueType)
        {
            var writeValueMethod = typeof(WritableBuffer)
                .GetRuntimeMethod("WriteValue", new[] { valueType });

            return Expression.Call(writableBuffer, writeValueMethod, new[] { field });
        }
    }
}
