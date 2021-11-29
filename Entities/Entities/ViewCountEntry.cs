using System;

namespace Core.Entities
{
    public class ViewCountEntry
    {
        public String domain;
        public String pageTitle;
        public int viewCount;

        public ViewCountEntry(string domain, string pageTitle, int viewCount)
        {
            this.domain = domain;
            this.pageTitle = pageTitle;
            this.viewCount = viewCount;
        }

        public void AddViewCount(int viewCount)
        {
            this.viewCount += viewCount;
        }
    }
}
