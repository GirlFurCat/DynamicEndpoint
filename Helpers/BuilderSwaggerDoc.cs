
using Microsoft.AspNetCore.Routing;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Writers;

namespace DynamicEndpoint.Helpers
{
    public class BuilderSwaggerDoc(EndpointDataSource endpointData)
    {
        public async Task InvokeAsync()
        {
            //检查Json文件
            string filePath = Path.Combine("wwwroot", "dynamic");
            if (!Directory.Exists(filePath))
                Directory.CreateDirectory(filePath);
            filePath = Path.Combine(filePath, "swagger.json");
            if (File.Exists(filePath))
                File.Delete(filePath);

            // 创建 OpenAPI 文档对象
            var document = new OpenApiDocument
            {
                Info = new OpenApiInfo
                {
                    Title = "Dynamic API",
                    Version = "v1"
                },
                Paths = new OpenApiPaths()
            };

            // 获取终结点
            var endpoints = endpointData.Endpoints.Where(x => x.Metadata.GetMetadata<OpenApiOperation>()?.Tags.Any(x => x.Name == "DynamicEndpoint") ?? false);
            foreach (RouteEndpoint routeEndpoint in endpoints)
            {
                string routePattern = routeEndpoint.RoutePattern.RawText ?? "/unknown";
                OpenApiOperation? openApiOperation = routeEndpoint.Metadata.GetMetadata<OpenApiOperation>()!;

                var parameters = new List<OpenApiParameter>();

                // 提取参数
                if (openApiOperation is not null)
                {
                    foreach (var param in openApiOperation.Parameters)
                    {
                        parameters.Add(new OpenApiParameter
                        {
                            Name = param.Name,
                            In = param.In,
                            Required = param.Required,
                        });
                    }
                }
                var body = openApiOperation?.RequestBody ?? default;

                // 添加到文档中
                if (!document.Paths.ContainsKey(routePattern))
                {
                    document.Paths.Add(routePattern, new OpenApiPathItem
                    {
                        Operations =
                        {
                            [GetOperationType(routeEndpoint.Metadata.GetMetadata<HttpMethodMetadata>()?.HttpMethods.FirstOrDefault() ?? string.Empty)] = GetOpenApiOperation(parameters, body, openApiOperation?.Description??null)
                        }
                    });
                }
                else
                {
                    // 如果路径已经存在，更新现有的操作或做其他处理
                    var existingPathItem = document.Paths[routePattern];
                    existingPathItem.Operations[GetOperationType(routeEndpoint.Metadata.GetMetadata<HttpMethodMetadata>()!.HttpMethods.FirstOrDefault() ?? string.Empty)] = GetOpenApiOperation(parameters, body, openApiOperation?.Description ?? null);
                }
            }

            // 序列化 OpenAPI 文档到 JSON
            // 使用 using 语句确保所有资源正确释放
            using (var stream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write))
            {
                using (var streamWriter = new StreamWriter(stream))
                {
                    var writer = new OpenApiJsonWriter(streamWriter);
                    document.SerializeAsV3(writer);

                    await stream.FlushAsync();
                    writer.Flush();
                }
            }
        }

        private OperationType GetOperationType(string Method)
        {
            switch (Method)
            {
                case "GET": return OperationType.Get;
                case "POST": return OperationType.Post;
                case "PUT": return OperationType.Put;
                case "DELETE": return OperationType.Delete;
                default: return OperationType.Get;
            }
        }

        private OpenApiOperation GetOpenApiOperation(List<OpenApiParameter>? parameters, OpenApiRequestBody? requestBody, string? Description = null)
        {
            return new OpenApiOperation
            {
                Summary = Description ?? "无描述",
                Responses = new OpenApiResponses
                {
                    ["200"] = new OpenApiResponse
                    {
                        Description = "Success",
                        Content =
                        {
                            ["application/json"] = new OpenApiMediaType()
                            {
                                Schema = new OpenApiSchema
                                {
                                    Type = "object"
                                    // 你可以根据需求详细描述返回结构
                                }
                            }
                        }
                    }
                },
                Parameters = parameters ?? new List<OpenApiParameter>(),
                RequestBody = requestBody ?? default
            };
        }
    }
}
