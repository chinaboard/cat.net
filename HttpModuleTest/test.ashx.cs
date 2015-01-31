using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HttpModuleTest
{
    /// <summary>
    /// test 的摘要说明
    /// </summary>
    public class test : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "text/plain";
            context.Response.Write("test");
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