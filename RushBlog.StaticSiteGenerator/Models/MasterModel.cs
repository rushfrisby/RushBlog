using System.Collections.Generic;
using System.Linq;

namespace RushBlog.StaticSiteGenerator.Models
{
    public class MasterModel
    {
        public IEnumerable<BlogPostDetail> SourceData { get; set; }

        public IEnumerable<BlogPostDetail> PublishedPosts
        {
            get
            {
                return SourceData?.Where(x => !x.IsPage && x.IsPublished && x.PublishedOn.HasValue).OrderByDescending(x => x.PublishedOn.Value);
            }
        }

        public IEnumerable<BlogPostDetail> UnpublishedPosts
        {
            get
            {
                return SourceData?.Where(x => !x.IsPage && (!x.IsPublished || !x.PublishedOn.HasValue)).OrderBy(x => x.Title);
            }
        }

        public IEnumerable<BlogPostDetail> PublishedPages
        {
            get
            {
                return SourceData?.Where(x => x.IsPage && x.IsPublished && x.PublishedOn.HasValue).OrderByDescending(x => x.PublishedOn.Value);
            }
        }

        public IEnumerable<BlogPostDetail> UnpublishedPages
        {
            get
            {
                return SourceData?.Where(x => x.IsPage && (!x.IsPublished || !x.PublishedOn.HasValue)).OrderBy(x => x.Title);
            }
        }

        public IEnumerable<TagSummary> Tags { get; set; }
    }
}
