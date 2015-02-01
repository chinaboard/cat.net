using Com.Dianping.Cat.Message;
using Com.Dianping.Cat.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;

namespace Com.Dianping.Cat.Util
{
    public static class CatHelper
    {
        public class CatHelperMsg
        {
            public string CatRootId { get; private set; }
            public string CatParentId { get; private set; }
            public string CatId { get; private set; }
            public string RequestMothed { get; private set; }

            public CatHelperMsg(string catRootId, string catParentId, string catId, string requestMothed = null)
            {
                CatRootId = catRootId;
                CatParentId = catParentId;
                CatId = catId;
                RequestMothed = requestMothed ?? string.Empty;
            }
        }

        public const string CatRootIdTag = "X-Cat-RootId";
        public const string CatParentIdTag = "X-Cat-ParentId";
        public const string CatIdTag = "X-Cat-Id";

        public static ITransaction NewTransaction(out CatHelperMsg catResponseMessage, string type, string name, WebRequest webRequest = null, HttpResponse webResponse = null, CatHelperMsg catRequestMessage = null, bool isRequest = false)
        {
            var tran = Cat.NewTransaction(type, name);
            tran.Status = "0";
            if (System.Web.HttpContext.Current != null)
            {
                //本地request本地response
                if (webRequest == null && webResponse == null)
                    catResponseMessage = LocalHttpContextRequest(type, name, isRequest);
                //指定request本地response或指定response
                else if (webRequest != null)
                    catResponseMessage = LocalHttpContextWebRequest(type, name, webRequest, webResponse, isRequest);
                //本地request指定response
                else if (webResponse != null)
                    catResponseMessage = LocalHttpContextWebResponse(type, name, webResponse, isRequest);
                //本地request本地response指定catRequestMessage
                else
                    catResponseMessage = LocalHttpContextCatRequestMessage(type, name, catRequestMessage, isRequest);
            }
            //指定catrequestmessage
            else
                catResponseMessage = LocalCatRequestMessage(type, name, catRequestMessage, isRequest);

#if DEBUG
            PrintDebugInfo();
#endif
            return tran;
        }

        private static CatHelperMsg LocalHttpContextRequest(string type, string name, bool isRequest = false)
        {
            var ctx = System.Web.HttpContext.Current;

            var request = ctx.Request;
            var response = ctx.Response;

            var tree = Cat.GetManager().ThreadLocalMessageTree;
            if (tree == null)
            {
                Cat.GetManager().Setup();
                tree = Cat.GetManager().ThreadLocalMessageTree;
            }

            var catRootId = string.IsNullOrWhiteSpace(request.Headers[CatHelper.CatRootIdTag]) ? string.IsNullOrWhiteSpace(response.Headers[CatHelper.CatRootIdTag]) ? tree.RootMessageId ?? tree.MessageId : response.Headers[CatHelper.CatRootIdTag] : request.Headers[CatHelper.CatRootIdTag];
            var catParentId = string.IsNullOrWhiteSpace(request.Headers[CatHelper.CatParentIdTag]) ? string.IsNullOrWhiteSpace(response.Headers[CatHelper.CatParentIdTag]) ? tree.MessageId : response.Headers[CatHelper.CatParentIdTag] : request.Headers[CatHelper.CatParentIdTag];
            var catId = string.IsNullOrWhiteSpace(request.Headers[CatHelper.CatIdTag]) ? string.IsNullOrWhiteSpace(request.Headers[CatHelper.CatIdTag]) ? Cat.GetProducer().CreateMessageId() : request.Headers[CatHelper.CatIdTag] : request.Headers[CatHelper.CatIdTag];

            tree.RootMessageId = catRootId;
            tree.ParentMessageId = catParentId;
            tree.MessageId = catId;

            response.Headers[CatHelper.CatRootIdTag] = catRootId;
            response.Headers[CatHelper.CatParentIdTag] = catParentId;
            response.Headers[CatHelper.CatIdTag] = response.Headers[CatHelper.CatIdTag] ?? catId;

            request.Headers[CatHelper.CatRootIdTag] = catRootId;
            request.Headers[CatHelper.CatParentIdTag] = catParentId;
            request.Headers[CatHelper.CatIdTag] = catId;

            Cat.LogEvent(type, type + ".Type", "0", isRequest ? "Request" : "Response");
            Cat.LogEvent(type, type + ".Server", "0", GetURLServerValue(request));
            Cat.LogEvent(type, type + ".Method", "0", GetURLMethodValue(request));
            Cat.LogEvent(type, type + ".Client", "0", AppEnv.GetClientIp(request));

            if (isRequest)
                Cat.LogEvent("RemoteCall", "HttpRequest", "0", catId);

            return new CatHelperMsg(catRootId, catParentId, catId, name);
        }

        private static CatHelperMsg LocalHttpContextWebRequest(string type, string name, WebRequest request, HttpResponse webResponse = null, bool isRequest = false)
        {
            var ctx = System.Web.HttpContext.Current;

            var response = webResponse ?? ctx.Response;

            var tree = Cat.GetManager().ThreadLocalMessageTree;
            if (tree == null)
            {
                Cat.GetManager().Setup();
                tree = Cat.GetManager().ThreadLocalMessageTree;
            }

            var catRootId = string.IsNullOrWhiteSpace(request.Headers[CatHelper.CatRootIdTag]) ? string.IsNullOrWhiteSpace(response.Headers[CatHelper.CatRootIdTag]) ? tree.RootMessageId ?? tree.MessageId : response.Headers[CatHelper.CatRootIdTag] : request.Headers[CatHelper.CatRootIdTag];
            var catParentId = string.IsNullOrWhiteSpace(request.Headers[CatHelper.CatParentIdTag]) ? string.IsNullOrWhiteSpace(response.Headers[CatHelper.CatParentIdTag]) ? tree.MessageId : response.Headers[CatHelper.CatParentIdTag] : request.Headers[CatHelper.CatParentIdTag];
            var catId = string.IsNullOrWhiteSpace(request.Headers[CatHelper.CatIdTag]) ? string.IsNullOrWhiteSpace(request.Headers[CatHelper.CatIdTag]) ? Cat.GetProducer().CreateMessageId() : request.Headers[CatHelper.CatIdTag] : request.Headers[CatHelper.CatIdTag];

            tree.RootMessageId = catRootId;
            tree.ParentMessageId = catParentId;
            tree.MessageId = catId;

            response.Headers[CatHelper.CatRootIdTag] = catRootId;
            response.Headers[CatHelper.CatParentIdTag] = catParentId;
            response.Headers[CatHelper.CatIdTag] = webResponse != null ? ctx.Response.Headers[CatHelper.CatIdTag] : catId;

            request.Headers[CatHelper.CatRootIdTag] = catRootId;
            request.Headers[CatHelper.CatParentIdTag] = catParentId;
            request.Headers[CatHelper.CatIdTag] = catId;

            Cat.LogEvent(type, type + ".Type", "0", isRequest ? "Request" : "Response");
            Cat.LogEvent(type, type + ".Server", "0", "LoaclCustomHttpRequest");
            Cat.LogEvent(type, type + ".Method", "0", string.Format("{0} {1}", request.Method, request.RequestUri));
            Cat.LogEvent(type, type + ".Client", "0", AppEnv.IP);

            if (isRequest)
                Cat.LogEvent("RemoteCall", "HttpRequest", "0", catId);

            return new CatHelperMsg(catRootId, catParentId, catId, name);
        }

        private static CatHelperMsg LocalHttpContextWebResponse(string type, string name, HttpResponse webResponse, bool isRequest = false)
        {
            var ctx = System.Web.HttpContext.Current;
            var request = ctx.Request;
            var response = webResponse;

            var tree = Cat.GetManager().ThreadLocalMessageTree;
            if (tree == null)
            {
                Cat.GetManager().Setup();
                tree = Cat.GetManager().ThreadLocalMessageTree;
            }

            var catRootId = string.IsNullOrWhiteSpace(request.Headers[CatHelper.CatRootIdTag]) ? string.IsNullOrWhiteSpace(response.Headers[CatHelper.CatRootIdTag]) ? tree.RootMessageId ?? tree.MessageId : response.Headers[CatHelper.CatRootIdTag] : request.Headers[CatHelper.CatRootIdTag];
            var catParentId = string.IsNullOrWhiteSpace(request.Headers[CatHelper.CatParentIdTag]) ? string.IsNullOrWhiteSpace(response.Headers[CatHelper.CatParentIdTag]) ? tree.MessageId : response.Headers[CatHelper.CatParentIdTag] : request.Headers[CatHelper.CatParentIdTag];
            var catId = string.IsNullOrWhiteSpace(request.Headers[CatHelper.CatIdTag]) ? string.IsNullOrWhiteSpace(request.Headers[CatHelper.CatIdTag]) ? Cat.GetProducer().CreateMessageId() : request.Headers[CatHelper.CatIdTag] : request.Headers[CatHelper.CatIdTag];

            tree.RootMessageId = catRootId;
            tree.ParentMessageId = catParentId;
            tree.MessageId = catId;

            response.Headers[CatHelper.CatRootIdTag] = catRootId;
            response.Headers[CatHelper.CatParentIdTag] = catParentId;
            response.Headers[CatHelper.CatIdTag] = ctx.Response.Headers[CatHelper.CatIdTag];

            request.Headers[CatHelper.CatRootIdTag] = catRootId;
            request.Headers[CatHelper.CatParentIdTag] = catParentId;
            request.Headers[CatHelper.CatIdTag] = catId;

            Cat.LogEvent(type, type + ".Type", "0", isRequest ? "Request" : "Response");
            Cat.LogEvent(type, type + ".Server", "0", GetURLServerValue(request));
            Cat.LogEvent(type, type + ".Method", "0", GetURLMethodValue(request));
            Cat.LogEvent(type, type + ".Client", "0", AppEnv.GetClientIp(request));

            if (isRequest)
                Cat.LogEvent("RemoteCall", "HttpRequest", "0", catId);

            return new CatHelperMsg(catRootId, catParentId, catId, name);
        }

        private static CatHelperMsg LocalHttpContextCatRequestMessage(string type, string name, CatHelperMsg catRequestMessage = null, bool isRequest = false)
        {
            var ctx = System.Web.HttpContext.Current;

            var request = ctx.Request;
            var response = ctx.Response;

            var tree = Cat.GetManager().ThreadLocalMessageTree;
            if (tree == null)
            {
                Cat.GetManager().Setup();
                tree = Cat.GetManager().ThreadLocalMessageTree;
            }

            var catRootId = catRequestMessage != null ? catRequestMessage.CatRootId : tree.RootMessageId ?? tree.MessageId;
            var catParentId = catRequestMessage != null ? catRequestMessage.CatParentId : tree.MessageId;
            var catId = catRequestMessage != null ? catRequestMessage.CatId : Cat.GetProducer().CreateMessageId();

            tree.RootMessageId = catRootId;
            tree.ParentMessageId = catParentId;
            tree.MessageId = catId;

            response.Headers[CatHelper.CatRootIdTag] = catRootId;
            response.Headers[CatHelper.CatParentIdTag] = catParentId;
            response.Headers[CatHelper.CatIdTag] = response.Headers[CatHelper.CatIdTag] ?? catId;

            request.Headers[CatHelper.CatRootIdTag] = catRootId;
            request.Headers[CatHelper.CatParentIdTag] = catParentId;
            request.Headers[CatHelper.CatIdTag] = catId;


            Cat.LogEvent(type, type + ".Server", "0", GetURLServerValue(request));
            Cat.LogEvent(type, type + ".Method", "0", GetURLMethodValue(request));
            Cat.LogEvent(type, type + ".Client", "0", AppEnv.GetClientIp(request));

            if (isRequest)
                Cat.LogEvent("RemoteCall", "HttpRequest", "0", catId);

            return new CatHelperMsg(catRootId, catParentId, catId, name);
        }

        private static CatHelperMsg LocalCatRequestMessage(string type, string name, CatHelperMsg catRequestMessage = null, bool isRequest = false)
        {
            var tree = Cat.GetManager().ThreadLocalMessageTree;
            if (tree == null)
            {
                Cat.GetManager().Setup();
                tree = Cat.GetManager().ThreadLocalMessageTree;
            }

            var catRootId = catRequestMessage != null ? catRequestMessage.CatRootId : tree.RootMessageId ?? tree.MessageId;
            var catParentId = catRequestMessage != null ? catRequestMessage.CatParentId : tree.MessageId;
            var catId = catRequestMessage != null ? catRequestMessage.CatId : Cat.GetProducer().CreateMessageId();

            tree.RootMessageId = catRootId;
            tree.ParentMessageId = catParentId;
            tree.MessageId = catId;

            Cat.LogEvent(type, type + ".Type", "0", isRequest ? "Request" : "Response");
            Cat.LogEvent(type, name, "0", catRequestMessage == null ? string.Format("Mothed.Request : {0}", name) : string.Format("Mothed.Response : {0}", catRequestMessage.RequestMothed));

            if (isRequest)
                Cat.LogEvent("RemoteCall", "Request", "0", catId);

            return new CatHelperMsg(catRootId, catId, Cat.GetProducer().CreateMessageId(), name);
        }

        public static string GetRootMessageId()
        {
            if (!Cat.IsInitialized() || !Cat.GetManager().CatEnabled)
                return string.Empty;

            var tree = Cat.GetManager().ThreadLocalMessageTree;
            if (tree == null)
            {
                Cat.GetManager().Setup();
                tree = Cat.GetManager().ThreadLocalMessageTree;
            }

            return string.IsNullOrEmpty(tree.RootMessageId) ? tree.MessageId : tree.RootMessageId;
        }

        public static string GetMessageId()
        {
            if (!Cat.IsInitialized() || !Cat.GetManager().CatEnabled)
                return string.Empty;
            var tree = Cat.GetManager().ThreadLocalMessageTree;
            if (tree == null)
            {
                Cat.GetManager().Setup();
                tree = Cat.GetManager().ThreadLocalMessageTree;
            }
            return tree.MessageId;
        }

        public static void PrintDebugInfo()
        {
            var tree = Cat.GetManager().ThreadLocalMessageTree;
            if (tree == null)
            {
                Cat.GetManager().Setup();
                tree = Cat.GetManager().ThreadLocalMessageTree;
            }
            Console.WriteLine("------------DebugInfo-----------");
            Console.WriteLine("root : {0}", tree.RootMessageId);
            Console.WriteLine("par  : {0}", tree.ParentMessageId);
            Console.WriteLine("msg  : {0}", tree.MessageId);
        }

        #region url info
        private static string GetURLServerValue(HttpRequest request)
        {
            if (request == null)
                return string.Empty;
            StringBuilder sb = new StringBuilder();
            sb.Append("IPS=").Append(AppEnv.GetClientIp(request));
            sb.Append("&VirtualIP=").Append(AppEnv.GetRemoteIp(request));
            sb.Append("&Server=").Append(AppEnv.IP);
            sb.Append("&Referer=").Append(request.GetHeader("Referer"));
            sb.Append("&Agent=").Append(request.GetHeader("User-Agent"));
            return sb.ToString();
        }

        private static string GetURLMethodValue(HttpRequest request)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(request.GetServerVariables("Request_Method")).Append(" ");
            sb.Append(request.Url.ToString());
            return sb.ToString();
        }

        private static string GetHeader(this HttpRequest request, string key)
        {
            if (request == null)
                request = HttpContext.Current.Request;
            var headerKey = request.Headers.AllKeys.Where(p => p.Equals(key, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
            return request.Headers[headerKey] ?? "null";
        }

        private static string GetServerVariables(this HttpRequest request, string key)
        {
            if (request == null)
                request = HttpContext.Current.Request;
            var headerKey = request.ServerVariables.AllKeys.Where(p => p.Equals(key, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
            return request.ServerVariables[headerKey] ?? "null";
        }
        #endregion

    }
}
