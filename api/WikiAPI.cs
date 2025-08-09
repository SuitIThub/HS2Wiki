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
        private WikiPlugin _plugin;

        // Referenz auf das Plugin speichern
        public void Initialize(WikiPlugin plugin)
        {
            _plugin = plugin;
        }

        public void OpenImage(string path)
        {
            _plugin?.OpenImage(path);
        }

        public void OpenPage(string category, string name)
        {
            var page = _pages.FirstOrDefault(p => p.Category == category && p.PageName == name);
            if (page != null)
            {
                _plugin?.OpenPage(page);
            }
        }

        public void RegisterPage(string category, string name, Action contentCallback)
        {
            _pages.Add(new PageInfo
            {
                Category = category,
                PageName = name,
                PageContentCallback = contentCallback
            });
            
            // Nach Registrierung einer neuen Seite den Kategoriebaum neu erstellen
            _plugin?.RebuildCategoryTree();
        }

        public IReadOnlyList<PageInfo> GetPages() => _pages;
    }

}
