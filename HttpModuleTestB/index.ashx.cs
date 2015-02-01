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
            HttpWebRequest httpRequest = (HttpWebRequest)WebRequest.Create("http://localhost:7339/index.ashx");
            Com.Dianping.Cat.Util.CatHelper.CatHelperMsg catResponseMessage = null;
            var tran = CatHelper.NewTransaction(out catResponseMessage, "index", "localtest", httpRequest, isRequest: true);
            httpRequest.Method = "GET";
            HttpWebResponse httpResponse = (HttpWebResponse)httpRequest.GetResponse();
            StreamReader sr = new StreamReader(httpResponse.GetResponseStream());
            string result = sr.ReadToEnd();
            sr.Close();
            context.Response.Write("-B" + result);
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