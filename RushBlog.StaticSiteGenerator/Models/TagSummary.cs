using System.Collections.Generic;

namespace RushBlog.StaticSiteGenerator.Models
{
    public class TagSummary
    {
        public string Tag { get; set; }

        public string DisplayName { get; set; }

        public int TimesUsed { get; set; }

        public IEnumerable<BlogPostDetail> BlogPosts { get; set; }
    }
}
