using DynamicEndpoint.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using System.Threading.Tasks;

namespace DynamicEndpoint.Helpers
{
    public class DtoLoadContext : AssemblyLoadContext
    {
        public DtoLoadContext() : base(isCollectible: true) { }

        public string AssemblyPaths = string.Empty;
        public Assembly? Assembly { get; set; }

        public Assembly Load(string path)
        {
            AssemblyPaths = path;
            Assembly = LoadFromAssemblyPath(path);
            return Assembly;
        }
    }

    public static class DtoLoadContextAggregate
    {
        public static Dictionary<string, DtoLoadContext> Contexts { get; } = new();

        public static Dictionary<string, ClassTypeModel> ClassTypeDic = new();

        /// <summary>
        /// 加载程序集
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static Assembly Load(string path, ClassTypeModel classType)
        {
            if(ClassTypeDic.TryGetValue(path, out ClassTypeModel? typeModel))
            {
                if (classType.Parameter == typeModel.Parameter && classType.ParameterType == typeModel.ParameterType)
                    return Contexts[path].Assembly!;
            }

            DtoLoadContext dtoLoad = new();
            Assembly assembly = dtoLoad.Load(path);
            Contexts.Add(path, dtoLoad);
            return assembly;
        }

        /// <summary>
        /// 卸载指定前缀程序集
        /// </summary>
        public static void UnloadPrefix(string Prefix)
        {
            var unloadAssembly = Contexts.Where(x => x.Key.StartsWith(Prefix)).ToDictionary();
            foreach (var context in unloadAssembly)
            {
                try
                {
                    context.Value.Unload();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to unload assembly at {context.Key}: {ex.Message}");
                }
                Contexts.Remove(context.Key);
                ClassTypeDic.Remove(context.Key);
            }
        }

        /// <summary>
        /// 卸载所有已加载的程序集
        /// </summary>
        public static void UnloadAll()
        {
            foreach (var context in Contexts)
            {
                try
                {
                    context.Value.Unload();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to unload assembly at {context.Key}: {ex.Message}");
                }
                Contexts.Remove(context.Key);
                ClassTypeDic.Remove(context.Key);
            }
        }
    }
}
