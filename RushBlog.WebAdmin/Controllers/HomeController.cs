using System;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using RushBlog.Common;
using RushBlog.WebAdmin.Models;

namespace RushBlog.WebAdmin.Controllers
{
	public class HomeController : Controller
	{
		private readonly IBlogService _blogService;

		public HomeController(IBlogService blogService)
		{
			_blogService = blogService ?? throw new ArgumentNullException(nameof(blogService));
		}

		public IActionResult Index()
		{
			return View();
		}

		public IActionResult Add()
		{
			return View();
		}

		[HttpPost]
		public IActionResult Add(BlogPost blogPost)
		{
			blogPost = _blogService.AddBlogPost(blogPost);
			return Redirect("/Home/Index");
		}

		public IActionResult Edit(Guid id)
		{
			var blogPost = _blogService.GetBlogPost(id);
			return View(blogPost);
		}

		[HttpPost]
		public IActionResult Edit(BlogPost blogPost)
		{
			blogPost = _blogService.UpdateBlogPost(blogPost);
			return Redirect("/Home/Index");
		}

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}

		public IActionResult Search([FromBody] BlogPostSearchCriteria criteria)
		{
			return Json(_blogService.BlogPostSearch(criteria));
		}
	}
}
