using System;

namespace RushBlog.StaticSiteGenerator.Models
{
    public class BlogPost
    {
        public string Title { get; set; }

        public bool IsPublished { get; set; }

        public DateTimeOffset? PublishedOn { get; set; }

        public string Slug { get; set; }

        public string[] Tags { get; set; }

        public bool IsPage { get; set; }
    }
}
