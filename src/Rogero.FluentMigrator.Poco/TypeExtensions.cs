using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Rogero.FluentMigrator.Poco
{
    public static class TypeExtensions
    {
        // this is alternative for typeof(T).GetProperties()
        // that returns base class properties before inherited class properties
        public static PropertyInfo[] GetBasePropertiesFirst(this Type type)
        {
            var orderList     = new List<Type>();
            var iteratingType = type;
            do
            {
                orderList.Insert(0, iteratingType);
                iteratingType = iteratingType.BaseType;
            } while (iteratingType != null);

            var props = type.GetProperties()
                .OrderBy(x => orderList.IndexOf(x.DeclaringType))
                .ToArray();

            return props;
        }
    }
}