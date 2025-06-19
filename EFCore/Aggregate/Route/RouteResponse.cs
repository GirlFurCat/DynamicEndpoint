using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicEndpoint.EFCore.Aggregate.Route
{
    public enum RouteResponse
    {
        /// <summary>
        /// 返回列表
        /// </summary>
        list = 0,

        /// <summary>
        /// 返回结果
        /// </summary>
        singleton = 1
    }
}
