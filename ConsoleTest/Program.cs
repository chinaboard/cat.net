using Com.Dianping.Cat;
using Com.Dianping.Cat.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConsoleTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Cat.Initialize();

            

            Com.Dianping.Cat.Util.CatHelper.CatHelperMsg catResponseMessage = null;
            var tran = CatHelper.NewTransaction(out catResponseMessage, "index", "test");
            var catReqmsg = catResponseMessage;
            tran = CatHelper.NewTransaction(out catResponseMessage, "index", "test", catRequestMessage: catReqmsg);
            catReqmsg = catResponseMessage;
            tran = CatHelper.NewTransaction(out catResponseMessage, "index", "test", catRequestMessage: catReqmsg);
            catReqmsg = catResponseMessage;
            tran = CatHelper.NewTransaction(out catResponseMessage, "index", "test", catRequestMessage: catReqmsg);
            catReqmsg = catResponseMessage;
            tran = CatHelper.NewTransaction(out catResponseMessage, "index", "test", catRequestMessage: catReqmsg);

            


            Console.ReadLine();
        }
    }
}
