using System.Collections.Generic;
using System.Linq;

using HtmlAgilityPack;

namespace SimpleTv.Sdk.Http
{
    public static class HttpDocumentExtensions
    {
        public static HtmlDocument AsHtmlDocument(this string rawResponse)
        {
            var html = new HtmlDocument();
            html.LoadHtml(rawResponse);
            return html;
        }

        public static IEnumerable<HtmlNode> SelectClass(this IEnumerable<HtmlNode> nodes, string className) {
            return nodes.Where(n => 
                n.GetAttributeValue("class", "").Equals(className));
        }

        public static IEnumerable<HtmlNode> SelectClass(this HtmlNode node, string className)
        {
            return node.Descendants().SelectClass(className);
        }

        public static IEnumerable<HtmlNode> SelectClass(this HtmlDocument doc, string className)
        {
            return doc.DocumentNode.Descendants().SelectClass(className);
        }

        public static IEnumerable<HtmlNode> SelectTag(this IEnumerable<HtmlNode> nodes, string tag)
        {
            return nodes.SelectMany(n => n.Descendants(tag));
        }

        public static IEnumerable<HtmlNode> SelectTag(this HtmlNode node, string tag)
        {
            return node.Descendants(tag);
        }
    }
}
