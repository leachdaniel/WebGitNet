using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Web;

namespace WebGitNet.AspNetImplmentations
{
    // https://github.com/aspnet/AspNetWebStack/blob/1a987f8/test/System.Web.WebPages.Test/WebPage/Utils.cs#L53
    public class MyHttpRequest : HttpRequestBase
    {
        public MyHttpRequest(HttpRequest request) : base()
        {
            _request = request;
        }

        public override Stream InputStream => _request.InputStream;
        public override string Path => _request.Path;
        public override string RawUrl => _request.RawUrl;
        public override bool IsLocal => false;
        public override string UserAgent => String.Empty;
        public override NameValueCollection QueryString => _request.QueryString;
        public override HttpBrowserCapabilitiesBase Browser => new MyHttpBrowserCapabilities();
        public override string AppRelativeCurrentExecutionFilePath => "~";
        public override string HttpMethod => _request.HttpMethod;

        private readonly HttpRequest _request;
    }
    public class MyHttpResponse : HttpResponseBase
    {
        public override HttpCookieCollection Cookies => new HttpCookieCollection();
    }
}