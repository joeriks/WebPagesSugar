using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.WebPages;

namespace RazorTemplates
{
    public static class TemplatesHelperExtensions
    {
        public static Templates Templates(this System.Web.WebPages.Html.HtmlHelper htmlHelper, string templatePath = "/")
        {
            /*
                var currentPath = VirtualPathUtility.GetDirectory((htmlHelper.ViewDataContainer as WebPageBase).VirtualPath);

                var renderer = new Func<string,object,HelperResult>((template, model) => {
                    return new HelperResult(new Action<System.IO.TextWriter>(writer => writer.Write(htmlHelper.Partial(template, model))));
                });


                var typeNameResolver = new Func<dynamic, string>((model) =>
                {
                    if (model is Umbraco.Web.Models.RenderModel) return ((Umbraco.Web.Models.RenderModel)model).Content.DocumentTypeAlias;
                    var aliasProperty = model.GetType().GetProperty("Alias");
                    if (aliasProperty != null) return aliasProperty.GetValue(model, null);
                    return model.GetType().Name;
                });
    */

            if (WebPageContext.Current.Page.Page.CurrentTemplates != null && WebPageContext.Current.Page.Page.TemplatePath == null || WebPageContext.Current.Page.Page.TemplatePath != templatePath)
            {
                WebPageContext.Current.Page.Page.TemplatePath = templatePath;
                WebPageContext.Current.Page.Page.CurrentTemplates = new Templates(templatePath);
            }

            return WebPageContext.Current.Page.Page.CurrentTemplates;
        }

        //public static Dictionary<string, HtmlString> GetHtmlSections(this Templates templates, string htmlString, string sectionSelector = "h1", params string[] requiredSections)
        //{
        //    //var html = helperResult.ToHtmlString();
        //    var doc = new HtmlAgilityPack.HtmlDocument();
        //    doc.LoadHtml(htmlString);

        //    string currentSectionName = "";
        //    string currentSectionHtml = "";

        //    var sections = new Dictionary<string, HtmlString>();

        //    foreach (var element in doc.DocumentNode.ChildNodes)
        //    {
        //        if (element.Name == sectionSelector)
        //        {
        //            if (currentSectionName != "" || currentSectionHtml.Trim() != "")
        //            {
        //                sections.Add(currentSectionName, new HtmlString(currentSectionHtml.Trim()));
        //            }

        //            currentSectionName = element.InnerText;
        //            currentSectionHtml = "";

        //        }
        //        else
        //        {
        //            currentSectionHtml = currentSectionHtml + element.OuterHtml;
        //        }
        //    }
        //    if (currentSectionName != "" || currentSectionHtml != "")
        //    {
        //        sections.Add(currentSectionName, new HtmlString(currentSectionHtml));
        //    }

        //    foreach (var requiredSection in requiredSections)
        //    {
        //        var errorMessages = new List<string>();
        //        if (!sections.ContainsKey(requiredSection))
        //        {
        //            errorMessages.Add(String.Format("Required section in text <{0}>{1}</{0}>", sectionSelector, requiredSection));
        //        }
        //        if (errorMessages.Any())
        //        {
        //            throw new Exception(String.Join(", ", errorMessages));
        //        }
        //    }

        //    return sections;
        //}

        //public static HtmlString RenderSection(this Templates templates, string sectionName, bool required = false)
        //{
        //    var htmlString = RazorTemplates.Templates.RenderBody().ToHtmlString();
        //    if (required)
        //        return GetHtmlSections(templates, htmlString, null, sectionName)[sectionName];
        //    return GetHtmlSections(templates, htmlString)[sectionName];

        //}


    }
}
