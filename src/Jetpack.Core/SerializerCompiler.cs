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
                if (field.FieldType != typeof(string))
                {
                    fieldSerializers.Add(WriteValue(writableBuffer, Expression.Constant((byte)TypeManifest[field.FieldType]), typeof(byte)));
                    fieldSerializers.Add(WriteValue(writableBuffer, Expression.Field(item, field), field.FieldType));
                }
                else
                {
                    fieldSerializers.Add(WriteValue(writableBuffer, Expression.Constant((byte)TypeManifest[field.FieldType]), typeof(byte)));
                    fieldSerializers.Add(WriteString(writableBuffer, Expression.Field(item, field)));
                }
            }

            var expression = Expression.Lambda<Action<WritableBuffer, JetpackSession, T>>(Expression.Block(fieldSerializers), new[] { writableBuffer, session, item });
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

        public static Expression WriteString(Expression writableBuffer, Expression field)
        {
            var writeStringMethod = typeof(WritableBuffer)
                .GetRuntimeMethod("WriteValue", new[] { typeof(string), typeof(int).MakeByRefType() });

            var writeStringContMethod = typeof(WritableBuffer)
                .GetRuntimeMethod("WriteValue", new[] { typeof(string), typeof(int), typeof(int).MakeByRefType() });

            var isCompleted = Expression.Variable(typeof(bool), "isCompleted");
            var outCharsUsed = Expression.Variable(typeof(int), "outCharsUsed");
            var charIndex = Expression.Variable(typeof(int), "charsUsed");
            var breakLabel = Expression.Label("break");

            var expr = Expression.Block(
                new ParameterExpression[] { isCompleted, outCharsUsed },
                new Expression[]
            {
                Expression.Assign(
                    isCompleted,
                    Expression.Call(writableBuffer, writeStringMethod, new[] { field, outCharsUsed })
                ),
                Expression.IfThen(Expression.IsTrue(isCompleted),
                    Expression.Loop(
                        Expression.IfThenElse(
                            Expression.NotEqual(
                                Expression.Call(writableBuffer, writeStringContMethod, new[] { field, outCharsUsed, outCharsUsed }),
                                Expression.Constant(true)
                            ),
                            GetBuffer,
                            Expression.Goto(breakLabel)
                        )
                    , breakLabel)
                )
            });

            return expr;
        }

        public static Expression<Func<JetpackSession, byte[]>> GetBuffer = (JetpackSession session) => session.GetBuffer();
    }
}
