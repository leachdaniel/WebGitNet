using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Caching;
using System.Web.Instrumentation;
using System.Web.Profile;

namespace WebGitNet.AspNetImplmentations
{
    public class MyHttpContext : HttpContextBase
    { 
        public MyHttpContext(HttpRequestBase httpRequest) : base()
        {
            Request = httpRequest;
            Response = new MyHttpResponse();
            Items = new Hashtable();
        }

        // implemented 
        public override IDictionary Items { get; }
        public override HttpRequestBase Request { get; }
        public override HttpResponseBase Response { get; }
        public override Cache Cache => HttpRuntime.Cache;


        // defaulted
        public override bool IsCustomErrorEnabled => _default.IsCustomErrorEnabled;
        public override bool IsDebuggingEnabled => _default.IsDebuggingEnabled;
        public override bool IsPostNotification => _default.IsPostNotification;
        public override bool IsWebSocketRequest => _default.IsWebSocketRequest;
        public override bool IsWebSocketRequestUpgrading => _default.IsWebSocketRequestUpgrading;
        public override DateTime Timestamp => _default.Timestamp;
        public override Exception Error => _default.Error;
        public override Exception[] AllErrors => _default.AllErrors;
        //public override HttpApplicationStateBase Application => new MyHttpApplicationState();
        public override HttpServerUtilityBase Server => new HttpServerUtilityWrapper(_default.Server);
        public override HttpSessionStateBase Session => new HttpSessionStateWrapper(_default.Session);
        public override IHttpHandler CurrentHandler => _default.CurrentHandler;
        public override IHttpHandler PreviousHandler => _default.PreviousHandler;
        public override IList<string> WebSocketRequestedProtocols => _default.WebSocketRequestedProtocols;
        public override PageInstrumentationService PageInstrumentation => _default.PageInstrumentation;
        public override ProfileBase Profile => _default.Profile;
        public override RequestNotification CurrentNotification => _default.CurrentNotification;
        public override string WebSocketNegotiatedProtocol => _default.WebSocketNegotiatedProtocol;
        public override TraceContext Trace => _default.Trace;
        

        private readonly HttpContext _default = new HttpContext(new HttpRequest("~" + new Uri("http://localhost").AbsolutePath, "http://localhost", null), new HttpResponse(new StringWriter()));
    }
}