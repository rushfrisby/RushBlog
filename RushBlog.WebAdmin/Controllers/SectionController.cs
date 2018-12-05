using Microsoft.AspNetCore.Mvc;
using RushBlog.Common;
using System;

namespace RushBlog.WebAdmin.Controllers
{
	public class SectionController : Controller
	{
		private readonly IBlogService _blogService;

		public SectionController(IBlogService blogService)
		{
			_blogService = blogService ?? throw new ArgumentNullException(nameof(blogService));
		}

		public IActionResult Index()
		{
			var model = _blogService.ListTemplateSections();
			return View(model);
		}

		public JsonResult Get([FromBody] int id)
		{
			var section = _blogService.GetTemplateSection(id);
			return Json(section.Content);
		}

		public JsonResult Update([FromBody] TemplateSection templateSection)
		{
			_blogService.UpdateTemplateSection(templateSection.Id, templateSection.Content);
			return Json(true);
		}
	}
}