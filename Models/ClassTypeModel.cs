using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using DynamicEndpoint.Helpers;

namespace DynamicEndpoint.Models
{
    public class ClassTypeModel
    {
        public string ClassName { get; set; } = null!;

        public string ClassCode { get; set; } = null!;

        public string Parameter { get; set; } = null!;

        public string ParameterType { get; set; } = null!;

        public string? AssemblyName { get; set; }

        public string AssemblyDir = Path.Combine(AppContext.BaseDirectory, "DynamicAssembly");

        public string AssemblyPath => Path.Combine(AssemblyDir, $"{this.AssemblyName}.dll");

        public Assembly? Assembly { get; set; }

        public byte[]? AssemblyBytes { get; set; }

        /// <summary>
        /// 生成代码
        /// </summary>
        /// <returns></returns>
        public async Task<Assembly> BuildCodeAsync()
        {
            AssemblyName = $"DynamicDtoAssembly_{ClassName}_{Guid.NewGuid().ToString()}";
            var syntaxTree = CSharpSyntaxTree.ParseText(ClassCode);

            var references = AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => !a.IsDynamic && !string.IsNullOrWhiteSpace(a.Location))
                .Select(a => MetadataReference.CreateFromFile(a.Location))
                .ToList();

            var compilation = CSharpCompilation.Create(
                AssemblyName,
                new[] { syntaxTree },
                references,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
            );

            using var ms = new MemoryStream();
            var result = compilation.Emit(ms);

            if (!result.Success)
            {
                var errors = string.Join("\n", result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
                throw new Exception($"编译失败:\n{errors}");
            }

            ms.Seek(0, SeekOrigin.Begin);
            AssemblyBytes = ms.ToArray();

            //保存到磁盘中
            ExistsAssemblyDir();
            await File.WriteAllBytesAsync(AssemblyPath, AssemblyBytes);
            Assembly = DtoLoadContextAggregate.Load(AssemblyPath, this);

            return Assembly;
        }

        private void ExistsAssemblyDir()
        {
            if (!Directory.Exists(AssemblyDir))
                Directory.CreateDirectory(AssemblyDir);
        }
    }
}
