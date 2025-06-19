using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DynamicEndpoint.EFCore.Migrations
{
    /// <inheritdoc />
    public partial class initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "t_Route",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    path = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    method = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    sql = table.Column<string>(type: "varchar(max)", nullable: false),
                    parameter = table.Column<string>(type: "varchar(max)", nullable: false),
                    response = table.Column<int>(type: "int", nullable: false),
                    authorization = table.Column<bool>(type: "bit", nullable: false),
                    version = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: false),
                    introduction = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    createdBy = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    createdAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_t_Route", x => x.id);
                });

            // 添加查询路由接口
            migrationBuilder.Sql(@"INSERT INTO t_Route ([path], [method], [sql], [parameter], [authorization], [response], [version], [introduction], [createdBy], [createdAt]) VALUES ('/api/admin/{version}/route', 'GET', 'select * from t_Route WHERE (path = @path OR @path IS NULL) AND (response = @response OR @response IS NULL) AND (method LIKE @method OR @method IS NULL)', '{""path"":""string?|like"",""method"":""string|like"",""response"":""string?""}', 'true', 0, 'v1', '查询路由', 'System', getdate());");

            // 添加修改路由接口
            //migrationBuilder.Sql(@"INSERT INTO t_Route ([path], [method], [sql], [parameter], [authorization], [response], [version], [introduction], [createdBy], [createdAt]) VALUES ('/api/admin/{version}/{id}/route', 'PUT', 'UPDATE t_Route SET [path] = ISNULL(@path, [path]), [method] = ISNULL(@method, [method]), [sql] = ISNULL(@sql, [sql]), [parameter] = ISNULL(@parameter, [parameter]), [authorization] = ISNULL(@authorization, [authorization]), [response] = ISNULL(@response, [response]), [introduction] = ISNULL(@introduction, [introduction]), WHERE id=@id', '{""path"":""string?"",""method"":""string?"",""sql"":""string?"",""parameter"":""string?"",""authorization"":""bool?"",""response"":""string?"",""introduction"":""string?"",""id"":""int""}', 'true', 'singleton', 'v1', '更新路由', 'System', getdate());");

            // 添加添加路由接口
            //migrationBuilder.Sql(@"INSERT INTO t_Route ([path], [method], [sql], [parameter], [authorization], [response], [version], [introduction], [createdBy], [createdAt]) VALUES ('/api/admin/{version}/route', 'POST', 'INSERT t_Route(path, method, sql, parameter, authorization, response, introduction, createdBy, createdAt) VALUES(@path, @method, @sql, @parameter, @authorization, @response, @introduction, ''System'', GETDATE())', '{""path"":""string"",""method"":""string"",""sql"":""string"",""parameter"":""string"",""authorization"":""bool"",""response"":""string"",""introduction"":""string""}', 'true', 'singleton', 'v1', '添加路由', 'System', getdate());");

            // 添加删除路由接口
            //migrationBuilder.Sql(@"INSERT INTO t_Route ([path], [method], [sql], [parameter], [authorization], [response], [version], [introduction], [createdBy], [createdAt]) VALUES ('/api/admin/{version}/{id}/route', 'DELETE', 'delete from t_Route where id=@id', '{ ""id"":""int"" }', 'true', 'singleton', 'v1', '删除路由', 'System', getdate());");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "t_Route");
        }
    }
}
