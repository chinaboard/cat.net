﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.SessionState;
using Com.Dianping.Cat.Util;
using System.Threading;

namespace Com.Dianping.Cat.Web
{
    public class CatHttpHandler : IHttpHandler, IRequiresSessionState
    {
        public CatHttpHandler(IHttpHandler httpHandler)
        {
            this.handler = httpHandler;
        }

        private IHttpHandler handler;

        public bool IsReusable { get { return handler.IsReusable; } }

        public void ProcessRequest(HttpContext context)
        {
            var tran = CatHelper.BeginServerTransaction("URL", response: context.Response);
            try
            {
                handler.ProcessRequest(context);
            }
            catch (Exception ex)
            {
                var baseEx = ex.GetBaseException();
                CatHelper.SetTrancationStatus(tran, baseEx);
                CatHelper.AddLogEvent("Exception", baseEx.StackTrace);
                CatHelper.CompleteTrancation(tran);
                throw ex;
            }
            CatHelper.CompleteTrancation(tran);
        }
    }
}
