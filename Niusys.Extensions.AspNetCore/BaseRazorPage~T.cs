using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Niusys.Extensions.AspNetCore.Sessions;

namespace Niusys.Extensions.AspNetCore
{
    public abstract class BaseRazorPage<T> : RazorPage<T>
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
                var canonicalUrl = ViewBag.CanonicalUrl?.ToString();
                return GCU(canonicalUrl);
            }
        }

        public bool IsDevelopment()
        {
            return HostEnvironment.IsDevelopment();
        }

        public string GCU(string url = "")
        {
            return $"{Host}{url}";
        }
    }
}
