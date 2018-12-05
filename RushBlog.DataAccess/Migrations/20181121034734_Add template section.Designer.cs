﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using RushBlog.DataAccess;

namespace RushBlog.DataAccess.Migrations
{
    [DbContext(typeof(BlogContext))]
    [Migration("20181121034734_Add template section")]
    partial class Addtemplatesection
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.1.4-rtm-31024")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("RushBlog.Common.BlogPost", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("HtmlContent");

                    b.Property<bool>("IsPublished");

                    b.Property<string>("MarkdownContent");

                    b.Property<DateTimeOffset?>("PublishedOn");

                    b.Property<string>("Slug");

                    b.Property<string>("Title");

                    b.HasKey("Id");

                    b.ToTable("BlogPosts");
                });

            modelBuilder.Entity("RushBlog.Common.Tag", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<Guid?>("BlogPostId");

                    b.Property<string>("Name");

                    b.Property<string>("Slug");

                    b.HasKey("Id");

                    b.HasIndex("BlogPostId");

                    b.ToTable("Tags");
                });

            modelBuilder.Entity("RushBlog.Common.TemplateSection", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("Content");

                    b.Property<string>("Name");

                    b.HasKey("Id");

                    b.ToTable("TemplateSections");
                });

            modelBuilder.Entity("RushBlog.Common.Tag", b =>
                {
                    b.HasOne("RushBlog.Common.BlogPost")
                        .WithMany("Tags")
                        .HasForeignKey("BlogPostId");
                });
#pragma warning restore 612, 618
        }
    }
}
