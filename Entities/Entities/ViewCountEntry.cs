using System;

namespace Core.Entities
{
    public class ViewCountEntry
    {
        private String domain;
        private String pageTitle;
        private int viewCount;

        public ViewCountEntry(string domain, string pageTitle, int viewCount)
        {
            this.domain = domain;
            this.pageTitle = pageTitle;
            this.viewCount = viewCount;
        }

        public string GetDomain()
        {
            return domain;
        }

        public string GetPageTitle()
        {
            return pageTitle;
        }

        public int GetViewCount()
        {
            return viewCount;
        }
    }
}
