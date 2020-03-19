using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Niusys.Extensions.AspNetCore.Sessions;

namespace Niusys.Extensions.AspNetCore
{
    public abstract class SeoRazorPage<T> : RazorPage<T>
    {
        public string Host => RequestSession.Host;

        public IRequestSession RequestSession => Context.RequestServices.GetService<IRequestSession>();

        public IWebHostEnvironment HostEnvironment => Context.RequestServices.GetService<IWebHostEnvironment>();

        public string Title
        {
            get
            {
                var title = ViewBag.Title?.ToString();
                if (string.IsNullOrWhiteSpace(title))
                {
                    title = string.Empty;
                }
                return title;
            }
        }

        public string Keywords
        {
            get
            {
                var keywords = ViewBag.Keywords?.ToString();
                if (string.IsNullOrWhiteSpace(keywords))
                {
                    keywords = string.Empty;
                }
                return keywords;
            }
        }

        public string Description
        {
            get
            {
                var description = ViewBag.Description?.ToString();
                if (string.IsNullOrWhiteSpace(description))
                {
                    description = string.Empty;
                }
                return description;
            }
        }

        public string CanonicalUrl
        {
            get
            {
                var canonicalUrl = ViewBag.CanonicalUrl ?? string.Empty;
                if (string.IsNullOrEmpty(canonicalUrl))
                {
                    canonicalUrl = this.Context.Request.Path.Value;
                }
                return GCU(canonicalUrl).TrimEnd('/');
            }
        }

        public bool IsDevelopment()
        {
            return HostEnvironment.IsDevelopment();
        }

        /// <summary>
        /// 在某个页面上Gtag是否被禁止, 不允许ga分析
        /// </summary>
        public bool IsGtagDisabled
        {
            get
            {
                var strValue = ViewBag.DisableGtag?.ToString();
                bool.TryParse(strValue ?? false.ToString(), out bool isGtagDisabled);
                return isGtagDisabled;
            }
        }

        public string GCU(string url)
        {
            var urlHelper = urlHelperFactory.GetUrlHelper(this.ViewContext);
            return $"{Host}{urlHelper.Content(url)}";
        }

        public IUrlHelperFactory urlHelperFactory => Context.RequestServices.GetService<IUrlHelperFactory>();
    }
}
