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
        public static Action<WritableBuffer, object> BuildAlphaSerializer(Type type)
        {
            var fields = type.GetRuntimeFields()
                .Where(x => x.IsPublic || x.IsPrivate || x.IsInitOnly)
                .OrderBy(x => x.Name);

            var writableBuffer = Expression.Parameter(typeof(WritableBuffer), "buffer");
            var param = Expression.Parameter(typeof(object), "obj");
            var item = Expression.Variable(type, "item");

            var fieldSerializers = new List<Expression>();
            fieldSerializers.Add(Expression.Assign(item, Expression.Convert(param, type)));
            foreach(var field in fields)
            {
                fieldSerializers.Add(WriteValue(writableBuffer, Expression.Field(item, field), field.FieldType));
            }

            var expression = Expression.Lambda<Action<WritableBuffer, object>>(Expression.Block(new[] { item }, fieldSerializers), new[] { writableBuffer, param });
            return expression.Compile();
        }

        public static Expression WriteValue(Expression writableBuffer, Expression field, Type valueType)
        {
            MethodInfo writeValueMethod;
            if(TypeManifest.FieldTypes.ContainsKey(valueType))
            {
                writeValueMethod = typeof(WritableBuffer)
                    .GetRuntimeMethod("WriteValue", new[] { valueType });
            }
            else
            {
                writeValueMethod = typeof(WritableBuffer)
                    .GetRuntimeMethod("WriteObjectHeader", new[] { typeof(object) });
            }

            return Expression.Call(writableBuffer, writeValueMethod, new[] { field });
        }
    }
}
