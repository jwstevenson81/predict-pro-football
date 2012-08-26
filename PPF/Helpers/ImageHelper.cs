using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace PPF.Helpers
{
    public static class ImageHelper
    {
        public static HtmlString Image(this HtmlHelper helper, string id, string url, string alternateText)
        {
            return Image(helper, id, url, alternateText, null);
        }

        public static HtmlString Image(this HtmlHelper helper, string id, string url, string alternateText, object htmlAttributes)
        {
            // Instantiate a UrlHelper 
            var urlHelper = new UrlHelper(helper.ViewContext.RequestContext);

            // Create tag builder
            var builder = new TagBuilder("img");

            // Create valid id
            builder.GenerateId(id);

            // Add attributes
            builder.MergeAttribute("src", urlHelper.Content(url));
            builder.MergeAttribute("alt", alternateText);
            builder.MergeAttributes(new RouteValueDictionary(htmlAttributes));

            // Render tag
            return new HtmlString(builder.ToString(TagRenderMode.SelfClosing));
        }

    }
}