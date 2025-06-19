using Dapper;
using DynamicEndpoint.Configuration;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace DynamicEndpoint.Helpers
{
    public class DapperHelper : IDisposable
    {
        private AppSetting AppSetting;
        private IDbConnection _writeConn;
        private IDbConnection _readConn;

        public DapperHelper(AppSetting AppSetting)
        {
            this.AppSetting = AppSetting;
            _writeConn = new SqlConnection(AppSetting.WriteConnectionStr);
            _readConn = new SqlConnection(AppSetting.ReadConnectionStr);
            _writeConn.Open();
            _readConn.Open();
        }

        public async Task<dynamic?> GetAsync<T>(string sql, T Entity)
        {
            try
            {
                return await _readConn.QueryFirstOrDefaultAsync(sql, Entity);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error executing SQL: {sql}. Error: {ex.Message}");
            }
        }

        public async Task<dynamic> GetPageAsync<T>(string sql, T Entity)
        {
            try
            {
                PropertyInfo? pageIndexInfo = Entity!.GetType().GetProperty("pageIndex");
                PropertyInfo? pageSizeInfo = Entity!.GetType().GetProperty("pageSize");
                if (pageIndexInfo is null || pageSizeInfo is null)
                    return await _readConn.QueryAsync(sql, Entity);

                var result = await _readConn.QueryAsync(PageSql(sql, "Id", (int)pageIndexInfo.GetValue(Entity)!, (int)pageSizeInfo.GetValue(Entity)!), Entity);
                return result;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error executing SQL: {sql}. Error: {ex.Message}");
            }
        }

        private static string PageSql(string sql, string? sort, int pageIndex, int pageSize)
        {
            string pageSql = @"SELECT TOP {0} * FROM (SELECT ROW_NUMBER() OVER (ORDER BY {1}) _row_number_,*  FROM 
              ({2})temp )temp1 WHERE temp1._row_number_>{3} ORDER BY _row_number_ ASC";
            return string.Format(pageSql, pageSize, sort, sql, pageSize * (pageIndex - 1));
        }

        public async Task<dynamic> ExecuteAsync<T>(string sql, T Entity)
        {
            try
            {
                var result = await _writeConn.ExecuteAsync(sql, Entity);
                return result;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error executing SQL: {sql}. Error: {ex.Message}");
            }
        }

        public void Dispose()
        {
            _writeConn.Close();
            _readConn.Close();
            _writeConn.Dispose();
            _readConn.Dispose();
        }
    }
}
