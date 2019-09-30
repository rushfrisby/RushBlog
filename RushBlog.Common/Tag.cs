﻿using System;

namespace RushBlog.Common
{
	public class Tag
	{
		public int Id { get; set; }

		public string Name { get; set; }

		public string Slug { get; set; }

        public Guid BlogPostId { get; set; }
	}
}
