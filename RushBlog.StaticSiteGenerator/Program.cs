using HandlebarsDotNet;
using Markdig;
using Newtonsoft.Json;
using RushBlog.StaticSiteGenerator.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RushBlog.StaticSiteGenerator
{
    class Program
    {
        private const string DataDirectoryName = ".blogdata";
        private const string TemplatesDirectoryName = "templates";
        private const string PostsDirectoryName = "posts";
        private const string PartialsDirectoryName = "partials";
        private const string TagDirectoryName = "tag";
        private const string DefaultMarkdownContentFormat = "[//]: # ({0})\r\n";
        private const string PostsJsonFileName = "posts.json";
        private const string TagsJsonFileName = "tags.json";
        private const string PostHtmlFileName = "post.html";
        private const string PageHtmlFileName = "page.html";
        private const string TagHtmlFileName = "tag.html";

        private static DirectoryInfo BaseDirectory;
        private static DirectoryInfo DataDirectory;
        private static DirectoryInfo TemplatesDirectory;
        private static DirectoryInfo PostsDirectory;
        private static DirectoryInfo PartialsDirectory;
        private static DirectoryInfo TagDirectory;
        private static FileInfo PostsFile;
        private static FileInfo TagsFile;
        private static FileInfo PostTemplateFile;
        private static FileInfo PageTemplateFile;
        private static FileInfo TagTemplateFile;
        private static Dictionary<int, Func<object, string>> CompiledTemplateCache;

        static async Task Main(string[] args)
		{
            if (args != null && args.Length > 0)
            {
                if (string.Equals("help", args[0], StringComparison.InvariantCultureIgnoreCase))
                {
                    PrintHelp();
                    return;
                }
                if (string.Equals("init", args[0], StringComparison.InvariantCultureIgnoreCase))
                {
                    await SetIoInfo(args);
                    return;
                }
                if (string.Equals("new", args[0], StringComparison.InvariantCultureIgnoreCase))
                {
                    await CreateNewPost(args);
                    return;
                }
                if (string.Equals("gen", args[0], StringComparison.InvariantCultureIgnoreCase))
                {
                    await GenerateSite(args);
                    return;
                }
            }
            PrintHelp();
        }

        private static async Task SetIoInfo(string[] args)
        {
            var baseDirectoryPath = Directory.GetCurrentDirectory();
            if (args.Length > 1)
            {
                baseDirectoryPath = args[1];
            }

            BaseDirectory = new DirectoryInfo(baseDirectoryPath);
            if (!BaseDirectory.Exists)
            {
                Console.WriteLine($"{BaseDirectory.FullName} doesn't exist");
                return;
            }

            DataDirectory = new DirectoryInfo(Path.Combine(BaseDirectory.FullName, DataDirectoryName));
            if (!DataDirectory.Exists)
            {
                DataDirectory.Create();
                Console.WriteLine($"Created directory {DataDirectory.FullName}");
            }

            PostsDirectory = new DirectoryInfo(Path.Combine(DataDirectory.FullName, PostsDirectoryName));
            if(!PostsDirectory.Exists)
            {
                PostsDirectory.Create();
                Console.WriteLine($"Created directory {PostsDirectory.FullName}");
            }

            TemplatesDirectory = new DirectoryInfo(Path.Combine(DataDirectory.FullName, TemplatesDirectoryName));
            if (!TemplatesDirectory.Exists)
            {
                TemplatesDirectory.Create();
                Console.WriteLine($"Created directory {TemplatesDirectory.FullName}");
            }

            PartialsDirectory = new DirectoryInfo(Path.Combine(DataDirectory.FullName, PartialsDirectoryName));
            if (!PartialsDirectory.Exists)
            {
                PartialsDirectory.Create();
                Console.WriteLine($"Created directory {PartialsDirectory.FullName}");
            }

            TagDirectory = new DirectoryInfo(Path.Combine(BaseDirectory.FullName, TagDirectoryName));
            if (!TagDirectory.Exists)
            {
                TagDirectory.Create();
                Console.WriteLine($"Created directory {TagDirectory.FullName}");
            }

            PostsFile = new FileInfo(Path.Combine(DataDirectory.FullName, PostsJsonFileName));
            if (!PostsFile.Exists)
            {
                var blogPost = new BlogPost
                {
                    PublishedOn = DateTimeOffset.UtcNow,
                    IsPublished = false,
                    IsPage = false,
                    Tags = new string[] { "blogging" },
                    Title = "My First Blog Post",
                    Slug = "my-first-blog-post"
                };
                var json = JsonConvert.SerializeObject(new[] { blogPost }, Formatting.Indented);
                await File.WriteAllTextAsync(PostsFile.FullName, json);
                Console.WriteLine($"Created file {PostsFile.FullName}");

                var mdFileName = Path.Combine(PostsDirectory.FullName, $"{blogPost.Slug}.md");
                await File.WriteAllTextAsync(mdFileName, string.Format(DefaultMarkdownContentFormat, blogPost.Title));
                Console.WriteLine($"Created file {mdFileName}");
            }

            TagsFile = new FileInfo(Path.Combine(DataDirectory.FullName, TagsJsonFileName));
            if (!TagsFile.Exists)
            {
                var d = new Dictionary<string, string>
                {
                    { "blogging", "Blogging" }
                };
                var json = JsonConvert.SerializeObject(d, Formatting.Indented);
                await File.WriteAllTextAsync(TagsFile.FullName, json);

                Console.WriteLine($"Created file {TagsFile.FullName}");
            }


            PostTemplateFile = new FileInfo(Path.Combine(TemplatesDirectory.FullName, PostHtmlFileName));
            if(!PostTemplateFile.Exists)
            {
                await File.WriteAllTextAsync(PostTemplateFile.FullName, "");
                Console.WriteLine($"Created file {PostTemplateFile.FullName}");
            }

            PageTemplateFile = new FileInfo(Path.Combine(TemplatesDirectory.FullName, PageHtmlFileName));
            if (!PageTemplateFile.Exists)
            {
                await File.WriteAllTextAsync(PageTemplateFile.FullName, "");
                Console.WriteLine($"Created file {PageTemplateFile.FullName}");
            }

            TagTemplateFile = new FileInfo(Path.Combine(TemplatesDirectory.FullName, TagHtmlFileName));
            if (!TagTemplateFile.Exists)
            {
                await File.WriteAllTextAsync(TagTemplateFile.FullName, "");
                Console.WriteLine($"Created file {TagTemplateFile.FullName}");
            }
        }

        private static async Task<MasterModel> GetMasterModel()
        {
            var model = new MasterModel();

            var postsJson = await File.ReadAllTextAsync(PostsFile.FullName);
            model.SourceData = JsonConvert.DeserializeObject<BlogPostDetail[]>(postsJson);

            foreach(var item in model.SourceData)
            {
                var fileName = Path.Combine(PostsDirectory.FullName, $"{item.Slug}.md");
                if (File.Exists(fileName))
                {
                    var markdownContent = await File.ReadAllTextAsync(fileName);
                    item.Content = Markdown.ToHtml(markdownContent);
                }
                else
                {
                    item.Content = "";
                }
                item.MasterModel = model;
            }

            var tags = model.PublishedPosts.SelectMany(x => x.Tags).Distinct();
            var tagsJson = await File.ReadAllTextAsync(TagsFile.FullName);
            var tagDisplayNames = JsonConvert.DeserializeObject<Dictionary<string, string>>(tagsJson);
            var tagSummaries = new List<TagSummary>();
            foreach(var tag in tags)
            {
                var displayName = tag;
                if(tagDisplayNames.ContainsKey(tag))
                {
                    displayName = tagDisplayNames[tag];
                }
                var taggedBlogPosts = model.PublishedPosts.Where(x => x.Tags.Contains(tag)).OrderByDescending(x => x.PublishedOn.Value);
                tagSummaries.Add(new TagSummary
                {
                    Tag = tag,
                    DisplayName = displayName,
                    TimesUsed = taggedBlogPosts.Count(),
                    BlogPosts = taggedBlogPosts,
                    MasterModel = model
                });
            }
            model.Tags = tagSummaries;

            return model;
        }

        private static void PrintHelp()
        {
            Console.WriteLine("Commands:");
            Console.WriteLine($"\t init [directory] \t Initializes the current directory, or the directory passed in using the second parameter.");
            Console.WriteLine($"\t new \t Adds an entry to posts.json and creates a new Markdown file.");
            Console.WriteLine($"\t gen \t Generates the site from source within the `{DataDirectoryName}` directory.");
        }

        private static async Task CreateNewPost(string[] args)
        {
            await SetIoInfo(args);

            var blogPost = new BlogPost
            {
                PublishedOn = DateTimeOffset.UtcNow,
                IsPublished = false,
                IsPage = false,
                Tags = new string[0]
            };

            Console.Write("Post Title: ");
            blogPost.Title = Console.ReadLine().Trim();
            blogPost.Slug = GenerateSlug(blogPost.Title);

            var postsJsonFileName = Path.Combine(DataDirectory.FullName, "posts.json");
            var postsJson = await File.ReadAllTextAsync(postsJsonFileName);
            var allBlogPosts = JsonConvert.DeserializeObject<List<BlogPost>>(postsJson);

            allBlogPosts.Insert(0, blogPost);

            postsJson = JsonConvert.SerializeObject(allBlogPosts, Formatting.Indented);
            await File.WriteAllTextAsync(postsJsonFileName, postsJson);

            var mdFileName = Path.Combine(PostsDirectory.FullName, $"{blogPost.Slug}.md");
            await File.WriteAllTextAsync(mdFileName, string.Format(DefaultMarkdownContentFormat, blogPost.Title));
        }

        private static string GenerateSlug(string phrase)
        {
            var slug = phrase.ToLower();
            slug = Regex.Replace(slug, @"[^a-z0-9\s-]", ""); // invalid chars
            slug = Regex.Replace(slug, @"\s+", " ").Trim(); // convert multiple spaces into one space
            slug = Regex.Replace(slug, @"\s", "-"); // hyphens   
            return slug;
        }

        private static async Task GenerateSite(string[] args)
        {
            await SetIoInfo(args);
            var model = await GetMasterModel();

            await RegisterPartials();
            var templates = await GetTemplates();

            var postTemplateKey = PostHtmlFileName.Replace(PostTemplateFile.Extension, "");
            var pageTemplateKey = PageHtmlFileName.Replace(PageTemplateFile.Extension, "");
            var tagTemplateKey = TagHtmlFileName.Replace(TagTemplateFile.Extension, "");

            var postTemplate = templates.FirstOrDefault(x => x.Key == postTemplateKey);
            var pageTemplate = templates.FirstOrDefault(x => x.Key == pageTemplateKey);
            var tagTemplate = templates.FirstOrDefault(x => x.Key == tagTemplateKey);

            templates.Remove(postTemplateKey);
            templates.Remove(pageTemplateKey);
            templates.Remove(tagTemplateKey);
            
            foreach(var template in templates)
            {
                var fileName = Path.Combine(BaseDirectory.FullName, $"{template.Key}.html");
                await CompileTemplateAndSaveFile(fileName, template.Value, model);
            }

            foreach (var post in model.SourceData.Where(x => !x.IsPage))
            {
                var fileName = Path.Combine(BaseDirectory.FullName, $"{post.Slug}.html");
                await CompileTemplateAndSaveFile(fileName, postTemplate.Value, post);
            }

            foreach (var page in model.SourceData.Where(x => x.IsPage))
            {
                var fileName = Path.Combine(BaseDirectory.FullName, $"{page.Slug}.html");
                await CompileTemplateAndSaveFile(fileName, pageTemplate.Value, page);
            }

            foreach (var tag in model.Tags)
            {
                var fileName = Path.Combine(TagDirectory.FullName, $"{tag.Tag}.html");
                await CompileTemplateAndSaveFile(fileName, tagTemplate.Value, tag);
            }
        }

        private static async Task CompileTemplateAndSaveFile(string fileName, string template, object data)
        {
            var content = "";
            Func<object, string> compiledTemplate = x => "";
            var cacheKey = template.GetHashCode();
            if(CompiledTemplateCache == null)
            {
                CompiledTemplateCache = new Dictionary<int, Func<object, string>>();
            }

            if(CompiledTemplateCache.ContainsKey(cacheKey))
            {
                compiledTemplate = CompiledTemplateCache[cacheKey];
            }
            else
            {
                try
                {
                    if (template != null)
                    {
                        compiledTemplate = Handlebars.Compile(template);
                        CompiledTemplateCache.Add(cacheKey, compiledTemplate);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("");
                    Console.WriteLine("*** Error compiling template ***");
                    Console.WriteLine(fileName);
                    Console.WriteLine(ex.Message);
                    Console.WriteLine("");

                    content = "Error compiling template: " + ex.Message;
                }
            }

            try
            {
                content = compiledTemplate(data);
            }
            catch (Exception ex)
            {
                Console.WriteLine("");
                Console.WriteLine("*** Error applying template ***");
                Console.WriteLine(fileName);
                Console.WriteLine(ex.Message);
                Console.WriteLine("");

                content = "Error applying template: " + ex.Message;
            }

            try
            {
                await File.WriteAllTextAsync(fileName, content);
            }
            catch(Exception ex)
            {
                Console.WriteLine("");
                Console.WriteLine("*** Error writing file ***");
                Console.WriteLine(fileName);
                Console.WriteLine(ex.Message);
                Console.WriteLine("");
            }
        }

        private static async Task<IDictionary<string, string>> GetTemplates()
        {
            var d = new Dictionary<string, string>();
            foreach (var file in TemplatesDirectory.GetFiles())
            {
                var content = await File.ReadAllTextAsync(file.FullName);
                d.Add(file.Name.Replace(file.Extension, ""), content);
            }
            return d;
        }

        private static async Task RegisterPartials()
        {
            foreach (var file in PartialsDirectory.GetFiles())
            {
                var name = file.Name.Replace(file.Extension, "");
                var content = await File.ReadAllTextAsync(file.FullName);
                Handlebars.RegisterTemplate(name, content);
            }
        }
    }
}
