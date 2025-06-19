using DynamicEndpoint.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace DynamicEndpoint.apis
{
    public class ApiTemplate(DapperHelper dapper)
    {
        public async Task<ActionResult> GetAsync<T>(T Entity, string sql)
        {
            var result = new JsonResult(await dapper.GetAsync(sql, Entity));
            return result;
        }

        public async Task<ActionResult> GetPagesAsync<T>(T Entity, string sql)
        {
            var result = new JsonResult(await dapper.GetPageAsync(sql, Entity));
            return result;
        }

        public async Task<ActionResult> PostAsync<T>(T Entity, string sql)
        {
            var result = new JsonResult((await dapper.ExecuteAsync(sql, Entity)) > 0 ? true : false);
            return result;
        }

        public async Task<ActionResult> PutAsync<T>(T Entity, string sql)
        {
            var result = new JsonResult((await dapper.ExecuteAsync(sql, Entity)) > 0 ? true : false);
            return result;
        }

        public async Task<ActionResult> DeleteAsync<T>(T Entity, string sql)
        {
            var result = new JsonResult((await dapper.ExecuteAsync(sql, Entity)) > 0 ? true : false);
            return result;
        }
    }
}
