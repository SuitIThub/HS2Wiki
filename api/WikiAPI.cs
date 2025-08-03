using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HS2Wiki.api
{
    public class WikiAPI
    {
        public class PageInfo
        {
            public string Category;
            public string PageName;
            public Action PageContentCallback;
        }

        private readonly List<PageInfo> _pages = new();

        public void RegisterPage(string category, string name, Action contentCallback)
        {
            _pages.Add(new PageInfo
            {
                Category = category,
                PageName = name,
                PageContentCallback = contentCallback
            });
        }

        public IReadOnlyList<PageInfo> GetPages() => _pages;
    }

}
