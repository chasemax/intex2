using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Intex2.Models.ViewModels;

namespace Intex2.Infrastructure
{
    [HtmlTargetElement("div", Attributes = "page-model")]
    public class PaginationTagHelper : TagHelper
    {
        private IUrlHelperFactory uhf;

        public PaginationTagHelper(IUrlHelperFactory temp)
        {
            uhf = temp;
        }

        [ViewContext]
        [HtmlAttributeNotBound]
        public ViewContext vc { get; set; }

        public PageInfo PageModel { get; set; }

        public string PageAction { get; set; }

        public string PageClass { get; set; }

        public bool PageClassesEnabled { get; set; }

        public string PageClassNormal { get; set; }

        public string PageClassSelected { get; set; }

        public override void Process(TagHelperContext thc, TagHelperOutput tho)
        {
            IUrlHelper uh = uhf.GetUrlHelper(vc);

            TagBuilder final = new TagBuilder("div");

            //for (int i = 1; i <= PageModel.TotalPages; i++)
            //{
            //    TagBuilder tb = new TagBuilder("a");

            //    tb.Attributes["href"] = uh.Action(PageAction, new { pageNum = i });

            //    if (PageClassesEnabled)
            //    {
            //        tb.AddCssClass(PageClass);
            //        tb.AddCssClass(i == PageModel.CurrentPage ? PageClassSelected : PageClassNormal);
            //    }
            //    tb.AddCssClass(PageClass);

            //    tb.InnerHtml.Append(i.ToString());

            //    final.InnerHtml.AppendHtml(tb);
            //}
            TagBuilder first = new TagBuilder("a");

            first.Attributes["href"] = uh.Action(PageAction, new { pageNum = 1 });

            if (PageClassesEnabled)
            {
                first.AddCssClass(PageClass);
                first.AddCssClass(PageClassNormal);
            }
            first.AddCssClass(PageClass);

            first.InnerHtml.Append("First");

            final.InnerHtml.AppendHtml(first);

            for (int i = (PageModel.CurrentPage - 1); i <= (PageModel.CurrentPage + 1); i++)
            {
                while (i <= 0)
                {
                    i += 1;
                }
                if (i == (PageModel.TotalPages + 1))
                {
                    break;
                }
                TagBuilder tb = new TagBuilder("a");

                tb.Attributes["href"] = uh.Action(PageAction, new { pageNum = i });

                if (PageClassesEnabled)
                {
                    tb.AddCssClass(PageClass);
                    tb.AddCssClass(i == PageModel.CurrentPage ? PageClassSelected : PageClassNormal);
                }
                tb.AddCssClass(PageClass);

                tb.InnerHtml.Append(i.ToString());

                final.InnerHtml.AppendHtml(tb);
            }

            TagBuilder last = new TagBuilder("a");

            last.Attributes["href"] = uh.Action(PageAction, new { pageNum = PageModel.TotalPages });

            if (PageClassesEnabled)
            {
                last.AddCssClass(PageClass);
                last.AddCssClass(PageClassNormal);
            }
            last.AddCssClass(PageClass);

            last.InnerHtml.Append("Last");

            final.InnerHtml.AppendHtml(last);

            tho.Content.AppendHtml(final.InnerHtml);
        }
    }
}