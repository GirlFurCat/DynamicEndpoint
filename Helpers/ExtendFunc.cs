using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection.Emit;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DynamicEndpoint.Helpers
{
    public static class ExtendFunc
    {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="strings"></param>
        /// <param name="Symbol"></param>
        /// <returns></returns>
        public static string ToString(this IEnumerable<string> strings, char Symbol)
        {
            return string.Join(Symbol, strings);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="strings"></param>
        /// <param name="Symbol"></param>
        /// <returns></returns>
        public static string ToString(this IList<string> strings, char Symbol)
        {
            return string.Join(Symbol, strings);
        }

        /// <summary>
        /// 填充默认值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="_this"></param>
        /// <param name="num">返回的列表长度</param>
        /// <returns></returns>
        public static IList<int> FillDefaults(this IList<int> _this, int num)
        {
            IList<int> value = new List<int>();
            if (num > _this.Count)
            {
                for (int i = 0; i < num - _this.Count; i++)
                {
                    value.Add(0);
                }
            }
            else
            {
                for (int i = 0; i < _this.Count - num; i++)
                {
                    _this.RemoveAt(num);

                }
            }
            IList<int> result = new List<int>();
            foreach (int temp in _this)
            {
                result.Add(temp);
            }

            foreach (int temp in value)
            {
                result.Add(temp);
            }
            return result;
        }

        /// <summary>
        /// 填充默认值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="_this"></param>
        /// <param name="num">返回的列表长度</param>
        /// <returns></returns>
        public static IList<string> FillDefaults(this IEnumerable<string> __this, int num)
        {
            IList<string> value = new List<string>();
            IList<string> _this = __this.ToList();
            if (num > _this.Count)
            {
                for (int i = 0; i < num - _this.Count; i++)
                {
                    value.Add(string.Empty);
                }
            }
            else
            {
                for (int i = 0; i < _this.Count - num; i++)
                {
                    _this.RemoveAt(num);

                }
            }
            IList<string> result = new List<string>();
            foreach (string temp in _this)
            {
                result.Add(temp);
            }

            foreach (string temp in value)
            {
                result.Add(temp);
            }
            return result;
        }


        public static IQueryable<TSource> Where<TSource>(this IQueryable<TSource> source, bool isUse, Expression<Func<TSource, bool>> predicate)
        {
            if (isUse)
            {
                return source.Where(predicate);
            }
            return source;
        }

        public static IEnumerable<TSource> Where<TSource>(this IEnumerable<TSource> source, bool isUse, Func<TSource, bool> predicate)
        {
            if (isUse)
            {
                return source.Where(predicate);
            }
            return source;
        }

        public static IList<TSource> Where<TSource>(this IList<TSource> source, bool isUse, Expression<Func<TSource, bool>> predicate)
        {
            return source.AsQueryable().Where(isUse, predicate).ToList();
        }

        public static double Sum<TSource>(this IQueryable<TSource> source, bool isUse, Expression<Func<TSource, double>> predicate)
        {
            if (isUse)
            {
                return source.Sum(predicate);
            }
            return 0;
        }

        public static double Sum<TSource>(this IList<TSource> source, bool isUse, Expression<Func<TSource, double>> predicate)
        {
            return source.AsQueryable().Sum(isUse, predicate);
        }

        public static decimal Sum<TSource>(this IQueryable<TSource> source, bool isUse, Expression<Func<TSource, decimal>> predicate)
        {
            if (isUse)
            {
                return source.Sum(predicate);
            }
            return 0;
        }

        public static decimal Sum<TSource>(this IList<TSource> source, bool isUse, Expression<Func<TSource, decimal>> predicate)
        {
            return source.AsQueryable().Sum(isUse, predicate);
        }

        public static IEnumerable<TSource> Distinct<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> predicate)
        {
            var groupedItems = source.GroupBy(predicate);
            foreach (var group in groupedItems)
            {
                yield return group.First();
            }
        }

        public static IEnumerable<TSource> Distinct<TSource, TKey>(this IQueryable<TSource> source, Func<TSource, TKey> predicate)
        {
            var groupedItems = source.AsQueryable().GroupBy(predicate);
            foreach (var group in groupedItems)
            {
                yield return group.First();
            }
        }

        public static string Sum<TSource>(this IEnumerable<TSource> source, Func<TSource, string> predicate)
        {
            string result = string.Join(',', source.Select(x=> predicate(x)));
            return result;
        }

      
        /// <summary>
        /// 排序
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="source"></param>
        /// <param name="Sort">排序字段（区分大小写） 排序方式(不区分大小写)（PropertyName ASE）</param>
        /// <returns></returns>
        public static IEnumerable<TSource> OrderBy<TSource>(this IEnumerable<TSource> source, string Sort)
        {
            if (string.IsNullOrEmpty(Sort))
                return source;

            string propertyName = Sort.Split(' ')[0];
            string sortType = Sort.Split(' ')[1];

            PropertyInfo? property = typeof(TSource).GetProperty(propertyName);
            if (property == null)
                throw new ArgumentNullException(nameof(property));

            var propertyType = property.PropertyType;
            var underlyingType = Nullable.GetUnderlyingType(propertyType);

            if (sortType.ToLower() == "asc")
            {
                return source.OrderBy(x => underlyingType != null ? (property.GetValue(x, null) != null ?
                            Convert.ChangeType(property.GetValue(x, null), Nullable.GetUnderlyingType(propertyType)!) : null) :
                            Convert.ChangeType(property.GetValue(x, null), propertyType)).ToList();
                //return source.OrderBy(x => property.GetValue(x, null) != null ? Convert.ChangeType(property.GetValue(x, null), property.PropertyType) : null).ToList();
            }
            return source.OrderByDescending(x => underlyingType != null ? (property.GetValue(x, null) != null ?
                            Convert.ChangeType(property.GetValue(x, null), Nullable.GetUnderlyingType(propertyType)!) : null) :
                            Convert.ChangeType(property.GetValue(x, null), propertyType));
        }

        /// <summary>
        /// 排序
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="source"></param>
        /// <param name="Sort">排序字段（区分大小写） 排序方式(不区分大小写)（PropertyName ASC）</param>
        /// <returns></returns>
        public static IList<TSource> OrderBy<TSource>(this IList<TSource> source, string Sort)
        {
            string propertyName = Sort.Split(' ')[0];
            string sortType = Sort.Split(' ')[1];

            PropertyInfo? property = typeof(TSource).GetProperty(propertyName);
            if (property == null)
                throw new ArgumentNullException(nameof(property));

            var propertyType = property.PropertyType;
            var underlyingType = Nullable.GetUnderlyingType(propertyType);

            if (sortType.ToLower() == "asc")
            {

                return source.OrderBy(x => underlyingType != null ? (property.GetValue(x, null) != null ?
                            Convert.ChangeType(property.GetValue(x, null), Nullable.GetUnderlyingType(propertyType)!) : null) :
                            Convert.ChangeType(property.GetValue(x, null), propertyType)).ToList();
                //return source.OrderBy(x => property.GetValue(x, null) != null ? Convert.ChangeType(property.GetValue(x, null), property.PropertyType) : null).ToList();
            }
            return source.OrderByDescending(x => underlyingType != null ? (property.GetValue(x, null) != null ?
                            Convert.ChangeType(property.GetValue(x, null), Nullable.GetUnderlyingType(propertyType)!) : null) :
                            Convert.ChangeType(property.GetValue(x, null), propertyType)).ToList();
        }


        /// <summary>
        /// 判断日期是否在指定范围内
        /// </summary>
        /// <param name="source"></param>
        /// <param name="startDate">开始日期</param>
        /// <param name="endDate">结束日期</param>
        /// <returns></returns>
        public static bool DateRange(this DateTime source, DateTime startDate, DateTime endDate, bool isEquals)
        {
            if (isEquals)
            {
                if (source >= startDate && source <= endDate) { return true; }
                return false;
            }
            else
            {
                if (source > startDate && source < endDate) { return true; }
                return false;
            }
        }

        /// <summary>
        /// 判断日期是否在指定范围内
        /// </summary>
        /// <param name="source"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="isEquals"></param>
        /// <returns></returns>
        public static bool DateRange(this DateTime source, DateTime startDate, DateTime endDate)
        {
           return source.DateRange(startDate, endDate, true);
        }

        /// <summary>
        /// 判断日期是否在指定范围内
        /// </summary>
        /// <param name="source"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        public static bool DateRange(this DateTime? source, DateTime startDate, DateTime endDate)
        {
            if (source != null)
            {
                return ((DateTime)source).DateRange(startDate, endDate, true);
            }
            return false;
        }

        /// <summary>
        /// 判断日期是否在指定范围内
        /// </summary>
        /// <param name="source"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        public static bool DateRange(this DateTime? source, DateTime startDate, DateTime endDate, bool isEquals)
        {
            if (source != null)
            {
                return ((DateTime)source).DateRange(startDate, endDate, isEquals);
            }
            return false;
        }

        /// <summary>
        /// 将DataTable转化为List
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        public static List<dynamic> ToList(this DataTable table)
        {
            // 使用动态类型生成器创建一个新的类型
            var typeBuilder = GetTypeBuilder();

            // 根据数据表中的每个列添加属性到动态类型中
            foreach (DataColumn column in table.Columns)
            {
                AddProperty(typeBuilder, column.ColumnName, column.DataType);
            }

            // 创建动态类型并生成类型信息
            var objectType = typeBuilder.CreateType();

            // 将每一行数据转换为动态类型对象
            var objectList = new List<dynamic>();
            foreach (DataRow row in table.Rows)
            {
                dynamic obj = Activator.CreateInstance(objectType);

                // 为每个属性赋值
                foreach (DataColumn column in table.Columns)
                {
                    ((object)obj).GetType().GetProperty(column.ColumnName).SetValue(obj, row[column], null);
                }

                objectList.Add(obj);
            }

            return objectList;
        }

        /// <summary>
        /// DataTable转成实体
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="table"></param>
        /// <returns></returns>
        public static List<T> ToList<T>(this DataTable table)
        {
            // 使用动态类型生成器创建一个新的类型
            var typeBuilder = GetTypeBuilder();

            // 根据数据表中的每个列添加属性到动态类型中
            foreach (DataColumn column in table.Columns)
            {
                AddProperty(typeBuilder, column.ColumnName, column.DataType);
            }

            // 将每一行数据转换为动态类型对象
            List<T> objectList = new List<T>();
            foreach (DataRow row in table.Rows)
            {
                T obj = Activator.CreateInstance<T>();

                // 为每个属性赋值
                foreach (DataColumn column in table.Columns)
                {
                    obj.GetType().GetProperty(column.ColumnName).SetValue(obj, row[column], null);
                }

                objectList.Add(obj);
            }

            return objectList;
        }

        private static TypeBuilder GetTypeBuilder()
        {
            var typeSignature = "DynamicObjectType";
            var an = new AssemblyName(typeSignature);
            var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(Guid.NewGuid().ToString()), AssemblyBuilderAccess.Run);
            var moduleBuilder = assemblyBuilder.DefineDynamicModule("MainModule");
            var typeBuilder = moduleBuilder.DefineType(typeSignature,
                                                       TypeAttributes.Public |
                                                       TypeAttributes.Class |
                                                       TypeAttributes.AutoClass |
                                                       TypeAttributes.AnsiClass |
                                                       TypeAttributes.BeforeFieldInit |
                                                       TypeAttributes.AutoLayout,
                                                       null);
            return typeBuilder;
        }
        private static void AddProperty(TypeBuilder typeBuilder, string propertyName, Type propertyType)
        {
            var fieldBuilder = typeBuilder.DefineField("_" + propertyName, propertyType, FieldAttributes.Private);

            var propertyBuilder = typeBuilder.DefineProperty(propertyName, PropertyAttributes.HasDefault, propertyType, null);
            var getMethodBuilder = typeBuilder.DefineMethod("get_" + propertyName,
                                                            MethodAttributes.Public |
                                                            MethodAttributes.SpecialName |
                                                            MethodAttributes.HideBySig,
                                                            propertyType,
                                                            Type.EmptyTypes);
            var getMethodIL = getMethodBuilder.GetILGenerator();

            getMethodIL.Emit(OpCodes.Ldarg_0);
            getMethodIL.Emit(OpCodes.Ldfld, fieldBuilder);
            getMethodIL.Emit(OpCodes.Ret);

            var setMethodBuilder = typeBuilder.DefineMethod("set_" + propertyName,
                                                            MethodAttributes.Public |
                                                            MethodAttributes.SpecialName |
                                                            MethodAttributes.HideBySig,
                                                            null,
                                                            new Type[] { propertyType });
            var setMethodIL = setMethodBuilder.GetILGenerator();

            setMethodIL.Emit(OpCodes.Ldarg_0);
            setMethodIL.Emit(OpCodes.Ldarg_1);
            setMethodIL.Emit(OpCodes.Stfld, fieldBuilder);
            setMethodIL.Emit(OpCodes.Ret);

            propertyBuilder.SetGetMethod(getMethodBuilder);
            propertyBuilder.SetSetMethod(setMethodBuilder);
        }
    }
}
