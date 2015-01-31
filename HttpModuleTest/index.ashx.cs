﻿using Com.Dianping.Cat.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;

namespace HttpModuleTest
{
    /// <summary>
    /// index 的摘要说明
    /// </summary>
    public class index : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "text/plain";
            context.Response.Write("root : " + CatHelper.GetRootMessageId());
            context.Response.Write(Environment.NewLine);
            context.Response.Write("msg  : " + CatHelper.GetMessageId());
            context.Response.Write(Environment.NewLine);

            HttpWebRequest httpRequest = (HttpWebRequest)WebRequest.Create("http://localhost:64198/index.ashx");
            Com.Dianping.Cat.Util.CatHelper.CatHelperMsg catResponseMessage = null;
            var tran = CatHelper.NewTransaction(out catResponseMessage, "index", "test", httpRequest);
            //httpRequest.Timeout = 2000;
            httpRequest.Method = "GET";
            HttpWebResponse httpResponse = (HttpWebResponse)httpRequest.GetResponse();
            StreamReader sr = new StreamReader(httpResponse.GetResponseStream());
            string result = sr.ReadToEnd();
            sr.Close();
            context.Response.Write(Environment.NewLine);
            context.Response.Write("root : " + httpResponse.Headers[CatHelper.CatRootIdTag]);
            context.Response.Write(Environment.NewLine);
            context.Response.Write("parent:" + httpResponse.Headers[CatHelper.CatParentIdTag]);
            context.Response.Write(Environment.NewLine);
            context.Response.Write("msg  : " + httpResponse.Headers[CatHelper.CatIdTag]);
            context.Response.Write(Environment.NewLine);
            context.Response.Write(Environment.NewLine);

            var catid = httpResponse.Headers[CatHelper.CatIdTag].ToString();
            var catpar = httpResponse.Headers[CatHelper.CatParentIdTag].ToString();
            var catroot = httpResponse.Headers[CatHelper.CatRootIdTag].ToString();
            tran.Complete();

            for (int i = 0; i < 10; i++)
            {
                var catReqmsg = catResponseMessage;
                httpRequest = (HttpWebRequest)WebRequest.Create("http://localhost:64198/index.ashx");
                //httpRequest.Headers[CatHelper.CatParentIdTag] = catpar;
                //httpRequest.Headers[CatHelper.CatIdTag] = catid;
                //httpRequest.Headers[CatHelper.CatRootIdTag] = catroot;
                catResponseMessage = null;
                tran = CatHelper.NewTransaction(out catResponseMessage, "index", "test", httpRequest);
                //httpRequest.Timeout = 2000;
                httpRequest.Method = "GET";
                httpResponse = (HttpWebResponse)httpRequest.GetResponse();
                sr = new StreamReader(httpResponse.GetResponseStream());
                result = sr.ReadToEnd();
                sr.Close();
                context.Response.Write(Environment.NewLine);
                context.Response.Write("root : " + httpResponse.Headers[CatHelper.CatRootIdTag]);
                context.Response.Write(Environment.NewLine);
                context.Response.Write("parent:" + httpResponse.Headers[CatHelper.CatParentIdTag]);
                context.Response.Write(Environment.NewLine);
                context.Response.Write("msg  : " + httpResponse.Headers[CatHelper.CatIdTag]);
                context.Response.Write(Environment.NewLine);
                context.Response.Write(Environment.NewLine);
                tran.Complete();

                catid = httpResponse.Headers[CatHelper.CatIdTag].ToString();
                catpar = httpResponse.Headers[CatHelper.CatParentIdTag].ToString();
                catroot = httpResponse.Headers[CatHelper.CatRootIdTag].ToString();

            }
            //context.Response.Write(result);
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