using System;
using System.Collections.Generic;
using System.Text;

namespace Jetpack.Core
{
    public class TypeManifest
    {
        public static Dictionary<Type, FieldType> FieldTypes = new Dictionary<Type, FieldType>()
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
    }
}
