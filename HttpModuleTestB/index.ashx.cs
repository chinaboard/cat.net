using Com.Dianping.Cat.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;

namespace HttpModuleTestB
{
    /// <summary>
    /// index 的摘要说明
    /// </summary>
    public class index : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {
            HttpWebRequest httpRequest = (HttpWebRequest)WebRequest.Create("http://localhost:60145/test.ashx");
            Com.Dianping.Cat.Util.CatHelper.CatHelperMsg catResponseMessage = null;
            var tran = CatHelper.NewTransaction(out catResponseMessage, "index", "test", httpRequest);
            //httpRequest.Timeout = 2000;
            httpRequest.Method = "GET";
            HttpWebResponse httpResponse = (HttpWebResponse)httpRequest.GetResponse();
            StreamReader sr = new StreamReader(httpResponse.GetResponseStream());
            string result = sr.ReadToEnd();
            sr.Close();
            context.Response.Write(Environment.NewLine);
            context.Response.Write("Broot : " + httpResponse.Headers[CatHelper.CatRootIdTag]);
            context.Response.Write(Environment.NewLine);
            context.Response.Write("Bparent:" + httpResponse.Headers[CatHelper.CatParentIdTag]);
            context.Response.Write(Environment.NewLine);
            context.Response.Write("Bmsg  : " + httpResponse.Headers[CatHelper.CatIdTag]);
            tran.Complete();
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