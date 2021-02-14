//-----------------------------------------------------------------------
// <copyright file="Global.asax.cs" company="(none)">
//  Copyright © 2013 John Gietzen and the WebGit .NET Authors. All rights reserved.
// </copyright>
// <author>John Gietzen</author>
//-----------------------------------------------------------------------

namespace WebGitNet
{
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.IO;
    using System.Threading;
    using System.Web;
    using System.Web.Hosting;
    using System.Web.Mvc;
    using System.Web.Routing;
    using Castle.MicroKernel.Registration;
    using Castle.MicroKernel.SubSystems.Configuration;
    using Castle.Windsor;
    using Castle.Windsor.Installer;
    using WebGitNet.AspNetImplmentations;

    public partial class WebGitNetApplication : System.Web.HttpApplication
    {
        private static IWindsorContainer container;

        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }

        public static HttpContextBase GetFakeHttpContext(string url)
        {
            if (!Uri.IsWellFormedUriString(url, UriKind.Absolute))
            {
                throw new ArgumentOutOfRangeException("url", "The URL must be a well-formed absolute URI.");
            }

            var request = new HttpRequest("~" + new Uri(url).AbsolutePath, url, null);
            var response = new HttpResponse(new System.IO.StringWriter());

            var fakeHttpContext = new HttpContext(request, response);
            HttpContext.Current = fakeHttpContext;
            // use transient scope instead
            

            var fakeHttpContextWrapper = new HttpContextWrapper(fakeHttpContext);
            //var httpContext = new MyHttpContext(request);
            var routeData = RouteTable.Routes.GetRouteData(fakeHttpContextWrapper);

            request.RequestContext = new RequestContext(fakeHttpContextWrapper, routeData);
            return fakeHttpContextWrapper;
        }

        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            var routeRegisterers = container.ResolveAll<IRouteRegisterer>();
            foreach (var registerer in routeRegisterers)
            {
                registerer.RegisterRoutes(routes);
            }

            routes.MapRoute(
                "Default",
                "{controller}/{action}",
                new { controller = "Browse", action = "Index" });
        }

        protected void Application_End()
        {
            container.Dispose();
        }

        protected void Application_Start()
        {
         
            Bootstrap();

            AreaRegistration.RegisterAllAreas();

            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);

            ViewEngines.Engines.Clear();
            ViewEngines.Engines.Add(new RazorViewEngine());
            ViewEngines.Engines.Add(new ResourceRazorViewEngine());

            HostingEnvironment.RegisterVirtualPathProvider(new ResourceVirtualPathProvider());
        }

        public static void TimerAction(object state)
        {
            //var result = ((IHttpAsyncHandler)this).BeginProcessRequest(GetFakeHttpContext("http://localhost:15594/"), TheAsyncCallback, new { asdf = 1 });
            var context = GetFakeHttpContext("http://localhost:15594/");
            // todo this might allow concurrent requests to share things in scope
            // probably want to create own version of AspNetRequestScopeStorageProvider
            using (System.Web.WebPages.Scope.ScopeStorage.CreateTransientScope(new System.Web.WebPages.Scope.ScopeStorageDictionary()))
            {
                try
                {
                    var mvcHandler = new MyMvcHandler(context.Request.RequestContext);

                    var result = mvcHandler.ExposedBeginProcessRequest(context, CreateAsyncCallback(mvcHandler), new object());

                    Console.WriteLine(result.ToString());
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
                //var parameters = new object[] { context, (AsyncCallback)TheAsyncCallback };

                // var method = typeof(WebGitNetApplication).GetMethod("BeginProcessRequestNotification", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        }

        public class MyMvcHandler : MvcHandler
        {
            public MyMvcHandler(RequestContext requestContext) : base(requestContext) { }
            
            public virtual IAsyncResult ExposedBeginProcessRequest(HttpContextBase httpContext, AsyncCallback callback, object state)
            {
                return base.BeginProcessRequest(httpContext, callback, state);
            }

            public virtual void ExposedEndProcessRequest(IAsyncResult asyncResult)
            {
                base.EndProcessRequest(asyncResult);
            }
        }

        private static Timer t = new Timer(TimerAction, null, 10000, Int32.MaxValue);
        public static AsyncCallback CreateAsyncCallback(MyMvcHandler httpAsyncHandler)
        {
            return (IAsyncResult ar) => { httpAsyncHandler.ExposedEndProcessRequest(ar); Console.WriteLine(ar.IsCompleted); };
        }

        private static void Bootstrap()
        {
            var directoryFilter = new AssemblyFilter(HostingEnvironment.MapPath("~/Plugins"));

            container = new WindsorContainer()
                        .Install(new AssemblyInstaller())
                        .Install(FromAssembly.InDirectory(directoryFilter));

            var controllerFactory = new WindsorControllerFactory(container.Kernel);
            ControllerBuilder.Current.SetControllerFactory(controllerFactory);
        }

        private class AssemblyInstaller : IWindsorInstaller
        {
            public void Install(IWindsorContainer container, IConfigurationStore configurationStore)
            {
                container.Register(Component.For<IWindsorContainer>().Instance(container));

                container.Register(Classes.FromThisAssembly()
                                          .BasedOn<IRouteRegisterer>()
                                          .WithService.FromInterface());
                container.Register(Classes.FromThisAssembly()
                                          .BasedOn<IController>()
                                          .Configure(c => c.Named(c.Implementation.Name))
                                          .LifestyleTransient());
                container.Register(Classes.From(typeof(PluginContentController))
                                          .BasedOn<IController>()
                                          .Configure(c => c.Named(c.Implementation.Name))
                                          .LifestyleTransient());
            }
        }

        private static readonly object _requestScopeKey = new object();
    }
}
