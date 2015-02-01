using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HttpModuleTestC
{
    /// <summary>
    /// index 的摘要说明
    /// </summary>
    public class index : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "text/plain";
            context.Response.Write("-C");
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}