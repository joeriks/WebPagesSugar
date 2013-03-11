using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.WebPages;
using System.Web.WebPages.Html;

namespace RazorTemplates
{

    /// <summary>
    ///
    /// Sugar for RenderPage - an efficient way of rendering razor templates
    /// It picks up changes immediately and requires no app restarts	
    ///
    /// Usage:
    ///
    /// @using RazorTemplates
    /// @{ dynamic tmpl = new Templates(); }
    /// @tmpl.Render(someType)
    ///
    /// It checks typename - or NodeTypeAlias if that property exists
    /// and renders the .cshtml with that name 	
    /// 
    /// Alternative usage:
    ///
    /// @tmpl.SomeRazorFileName(someModel) 
    ///
    /// The optional parameter(s) can be used in the .cshtml by the use of PageData[], for example:
    /// @{var viewModel = (MyModelType)PageData[0];}
    ///
    /// Also - RenderPage keeps the context, so you can use the existing Model
    ///
    /// This means you can save templates for your Document types in Umbraco and render the correct template		
    /// just by using @template.Render(myNode)
    /// renders /macroScripts/MyNodeTypeAlias.cshtml
    ///
    /// If you like to render different templates at different times, just specify template folder to find the templates in
    /// 	
    /// Templates("~/wherever") keeps the path as specified
    /// Templates("/relativeToTemplateRoot") uses path relative to /macroScripts
    /// Templates("relativeToCurrent") uses path relative to current path (i.e. subtemplates in same folder)
    /// Templates() uses /macroScripts
    /// Templates("") uses current path
    /// 	
    /// </summary>

    public class Templates : System.Dynamic.DynamicObject
    {
        private System.Web.WebPages.WebPageRenderingBase page;
        private HtmlHelper htmlHelper;
        private string defaultRootPath = "~/templates";

        private string appendExtension = ".cshtml";
        private string[] propertiesAsTemplateNames = new[] { "NodeTypeAlias", "Alias" };
        private string templatePath;
        private string[] fallbackTemplatePaths = new[] { "~/templates/shared/" };
        private string fallbackFileName = "_defaultTemplate";
        private bool useFallbacks = true;
        private string currentPath;
        private bool useMvc;
        private Func<string, object, HelperResult> renderer;
        private Func<dynamic, string> typeNameResolver;
        private object dynamicModel;
        public Dictionary<string, string> TagTemplates = new Dictionary<string, string>();

        public Templates(HtmlHelper htmlHelper, string templatePath, string rootPath, string currentPath, Func<string, object, HelperResult> renderer, Func<dynamic, string> typeNameResolver)
        {
            this.htmlHelper = htmlHelper;
            this.useMvc = true;
            this.defaultRootPath = rootPath;
            this.currentPath = currentPath;
            this.renderer = renderer;
            this.typeNameResolver = typeNameResolver;
            setTemplatePath(templatePath);
        }
        private void setTemplatePath(string templatePath)
        {
            if (templatePath != "")
            {
                var currentPath = VirtualPathUtility.GetDirectory(page.VirtualPath);
                if (VirtualPathUtility.IsAbsolute(templatePath)) templatePath = defaultRootPath + templatePath;
                templatePath = VirtualPathUtility.AppendTrailingSlash(templatePath);
            }
            this.templatePath = templatePath;

        }
        private void initialize(string templatePath, object dynamicModel = null, string fallbackTemplatePath = "/shared")
        {
            page = System.Web.WebPages.WebPageContext.Current.Page;
            setTemplatePath(templatePath);

            if (dynamicModel != null)
                this.dynamicModel = dynamicModel;

        }
        public Templates(string templatePath, object dynamicModel = null, string fallbackTemplatePath = "/shared")
        {
            initialize(templatePath, dynamicModel, fallbackTemplatePath);

        }




        public Templates(object arg = null)
        {
            if (arg == null) arg = "/";
            if (arg is string)
            {
                initialize((string)arg);
            }
            else
            {
                initialize("/" + resolveTemplateName(arg), arg);
            }
        }

        public dynamic AsDynamic()
        {
            return this;
        }


        /// <summary>
        /// conventions:
        /// 
        /// RenderTemplate("foo",model)
        /// RenderTemplate("foo/",model)
        /// RenderTemplate("/foo",model)
        /// RenderTemplate("~/foo",model)
        /// RenderTemplate(model)
        /// 
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>

        public static HelperResult RenderTemplate(params object[] args)
        {

            var templatePath = "/";
            var page = WebPageContext.Current.Page.Page;
            if (page.TemplatePath != null) { templatePath = page.TemplatePath; }

            //if (args.Length==1 && args[0] is string) return templates.Render((string)args[0]);
            if (page.CurrentTemplates == null || WebPageContext.Current.Page.Page.TemplatePath != templatePath)
            {
                page.TemplatePath = templatePath;
                page.CurrentTemplates = new Templates(templatePath);
            }

            return page.CurrentTemplates.Render(args);

        }

        public HelperResult Render(params object[] args)
        {
            var templateName = "";
            object model = null;

            if (args.Length > 0 && args[0] is string)
            {
                templateName = (string)args[0];
                if (args.Length == 1)
                    model = args[0];
                else
                    model = args[1];
            }

            if (templateName == "")
            {
                templateName = resolveTemplateName(args[0]);
                model = args[0];
            }

            var template = templatePath + templateName + appendExtension;

            if (renderer != null)
            {
                if (args.Length == 1)
                    return renderer(template, null);
                else
                    return renderer(template, args[0]);
            }

            if (useFallbacks)
            {
                return RenderWithFallbacks(template, templateName, model);
            }

            //var verifiedPath = tryFindVirtualFileWithFallbacks(template, templateName);

            return page.RenderPage(template, model);

        }
        public static dynamic TemplateModel
        {
            get
            {
                return WebPageContext.Current.Page.Page.PageData[0];
            }
        }
        private bool TryRender(string path, object model, out HelperResult result)
        {
            try
            {
                var htmlString = page.RenderPage(path, model).ToHtmlString();
                result = new HelperResult((writer) =>
                {
                    writer.Write(htmlString);
                });
                return true;
            }
            catch (HttpException ex)
            {
                HttpContext.Current.Trace.Warn(ex.ToString());
            }
            result = null;
            return false;

        }
        private HelperResult RenderWithFallbacks(string template, string templateName, object model)
        {
            var path = template;
            HelperResult result;

            if (TryRender(path, model, out result)) return result;

            var notFoundPaths = new List<string>();
            notFoundPaths.Add(path);

            foreach (var fallbackPath in fallbackTemplatePaths)
            {
                path = fallbackPath + templateName + appendExtension;
                if (TryRender(path, model, out result)) return result;
                notFoundPaths.Add(path);
            }

            path = templatePath + fallbackFileName + appendExtension;
            if (TryRender(path, model, out result)) return result;
            notFoundPaths.Add(path);

            foreach (var fallbackPath in fallbackTemplatePaths)
            {
                path = fallbackPath + fallbackFileName + appendExtension;
                if (TryRender(path, model, out result)) return result;
                notFoundPaths.Add(path);
            }
            return new HelperResult((writer) =>
            {
                throw new Exception("Template not found on any path : " + string.Join(", ", notFoundPaths));
            });

        }

        private bool virtualFileExists(string virtualPath)
        {
            var exists = false;
            try
            {
                WebPageBase.CreateInstanceFromVirtualPath(virtualPath);
                exists = true;
            }
            catch (Exception)
            {

            }

            return exists;

        }

        public System.Web.WebPages.HelperResult RenderWith(string templateName, Func<object, HelperResult> itemTemplate)
        {
            return Render(templateName, new[] { itemTemplate });
        }
        public System.Web.WebPages.HelperResult RenderWith(string templateName, object viewModel, Func<object, HelperResult> itemTemplate)
        {
            return Render(templateName, new[] { viewModel, itemTemplate });
        }
        public System.Web.WebPages.HelperResult RenderWith(string templateName, object viewModel, Func<object, HelperResult> itemTemplate, Func<object, HelperResult> alternativeItemTemplate)
        {
            return Render(templateName, new[] { viewModel, itemTemplate, alternativeItemTemplate });
        }

        private string tidyName(string name)
        {
            return name.Replace(" ", "");
        }
        private string resolveTemplateName(dynamic value)
        {
            if (typeNameResolver != null)
                return typeNameResolver(value);
            //foreach (var propertyName in propertiesAsTemplateNames)
            //{
            //    var tryGetNodeTypeAlias = value.GetType().GetProperty(propertyName);
            //    if (tryGetNodeTypeAlias != null) return tidyName(tryGetNodeTypeAlias.GetValue(value, null).ToString());
            //}

            return value.GetType().Name;
        }

        public HelperResult RenderEach(IEnumerable<object> enumerable)
        {
            return new HelperResult((writer) =>
            {
                try
                {
                    foreach (var item in enumerable)
                    {
                        var htmlString = Render(item).ToHtmlString();
                        writer.Write(htmlString);
                    }

                }
                catch (Exception ex)
                {
                    throw ex;
                }
            });

        }

        //public System.Web.WebPages.HelperResult Render(dynamic value)
        //{
        //    var templateName = resolveTemplateName(value);
        //    return Render(templateName, new[] { value });
        //}
        public static Func<object, HelperResult> ItemTemplate(Func<object, HelperResult> template) { return template; }

        private static List<Func<object, HelperResult>> getItemTemplates()
        {
            var itemTemplates = new List<Func<object, HelperResult>>();

            foreach (dynamic pageData in System.Web.WebPages.WebPageContext.Current.PageData)
            {
                if (pageData.Value is Func<object, HelperResult>)
                {
                    itemTemplates.Add(pageData.Value);
                }
            }

            return itemTemplates;
        }

        public static HelperResult RenderBody(dynamic item = null, int templateNumber = 0)
        {
            if (item == null) item = "";
            var itemTemplates = getItemTemplates();
            if (itemTemplates.Any() && itemTemplates.Count() > templateNumber)
                return itemTemplates[templateNumber](item);
            return null;
        }

        public override bool TryInvokeMember(System.Dynamic.InvokeMemberBinder binder, object[] args, out object result)
        {
            if (binder.Name != "Render")
            {
                result = Render(binder.Name, args[0]);
                return true;
            }
            if (binder.Name == "RenderEach")
            {
                result = RenderEach(args[0] as IEnumerable<dynamic>);
                return true;
            }
            if (args.Length == 0)
            {
                result = Render(binder.Name);
                return true;
            }
            if (args.Length == 1)
            {
                result = Render(args[0]);
                return true;
            }
            result = "Invalid call";
            return false;
        }
        public override bool TryGetMember(System.Dynamic.GetMemberBinder binder, out object result)
        {

            if (dynamicModel != null)
            {
                //if (dynamicModel is umbraco.MacroEngines.DynamicNode)
                //{
                //    var model = dynamicModel as umbraco.MacroEngines.DynamicNode;
                //    var tryGetMember = model.TryGetMember(binder, out result);
                //    if (tryGetMember)
                //    {
                //        result = Render(binder.Name, new[] { result });
                //        return true;
                //    }
                //}
                if (dynamicModel is System.Dynamic.ExpandoObject)
                {
                    var dynamicModelAsDictionary = (IDictionary<string, object>)dynamicModel;
                    var item = dynamicModelAsDictionary[binder.Name];



                }
                var property = dynamicModel.GetType().GetProperty(binder.Name);
                if (property != null)
                {
                    var value = property.GetValue(dynamicModel, null);
                    result = Render(binder.Name, new[] { value });
                    return true;
                }
            }
            result = null;
            return false;
        }

    }

}