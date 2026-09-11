using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using MedAdvice.Controllers;
using MedAdvice.Models;
using MedAdvice.Tests.Infrastructure;
using MedAdvice.viewmodel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace MedAdvice.Tests.Homepage
{
    /// The homepage became data driven in phase four. These assert the rules that decide what
    /// a visitor actually sees.
    public class HomepageSectionTests : IDisposable
    {
        readonly TestDatabase database = new TestDatabase();
        readonly IdentityTestHost identity;

        public HomepageSectionTests()
        {
            identity = new IdentityTestHost(database);
        }

        HomeController Controller()
        {
            IConfiguration configuration = new ConfigurationBuilder().AddInMemoryCollection().Build();
            IHttpClientFactory clients = new ServiceCollection()
                .AddHttpClient().BuildServiceProvider()
                .GetRequiredService<IHttpClientFactory>();

            HomeController controller = new HomeController(identity.Db, identity.Users, configuration, clients);
            TestContext.Prepare(controller);
            return controller;
        }

        async Task AddSectionAsync(string key, bool visible, int order)
        {
            identity.Db.Add(new HomepageSection
            {
                SectionKey = key,
                DisplayName = key,
                IsVisible = visible,
                SortOrder = order
            });
            await identity.Db.SaveChangesAsync();
        }

        async Task<HomepageViewModel> RenderAsync()
        {
            ViewResult view = Assert.IsType<ViewResult>(await Controller().Home());
            return Assert.IsType<HomepageViewModel>(view.Model);
        }

        [Fact]
        public async Task Hidden_sections_are_not_rendered()
        {
            await AddSectionAsync("banner", visible: true, order: 1);
            await AddSectionAsync("work", visible: false, order: 2);

            HomepageViewModel model = await RenderAsync();

            Assert.Single(model.Sections);
            Assert.Equal("banner", model.Sections[0].SectionKey);
        }

        [Fact]
        public async Task Sections_come_back_in_sort_order_not_insertion_order()
        {
            await AddSectionAsync("work", visible: true, order: 3);
            await AddSectionAsync("banner", visible: true, order: 1);
            await AddSectionAsync("about", visible: true, order: 2);

            HomepageViewModel model = await RenderAsync();

            Assert.Equal(new[] { "banner", "about", "work" }, model.Sections.Select(x => x.SectionKey).ToArray());
        }

        [Fact]
        public async Task A_missing_content_row_still_yields_a_bindable_model()
        {
            // Every partial binds Model.Content without a null guard, so the controller must
            // never hand them null.
            await AddSectionAsync("banner", visible: true, order: 1);

            HomepageViewModel model = await RenderAsync();

            Assert.NotNull(model.Content);
        }

        [Fact]
        public async Task Stored_hero_copy_reaches_the_view()
        {
            await AddSectionAsync("banner", visible: true, order: 1);
            identity.Db.Add(new HomepageContent
            {
                HeroTagline = "Tagline",
                HeroTitle = "Title",
                HeroText = "Text"
            });
            await identity.Db.SaveChangesAsync();

            HomepageViewModel model = await RenderAsync();

            Assert.Equal("Title", model.Content.HeroTitle);
            Assert.Equal("Tagline", model.Content.HeroTagline);
        }

        [Fact]
        public async Task A_pinned_blog_becomes_a_featured_article_pointing_at_the_blog()
        {
            await AddSectionAsync("featured", visible: true, order: 1);

            BlogCategory category = new BlogCategory { BlogCategoryname = "c" };
            identity.Db.Add(category);
            await identity.Db.SaveChangesAsync();

            Blog blog = new Blog { BlogTitle = "Pinned blog", BlogBriefText = "brief", BlogCategoryId = category.Id };
            identity.Db.Add(blog);
            await identity.Db.SaveChangesAsync();

            identity.Db.Add(new FeaturedArticle { BlogId = blog.Id, SortOrder = 1 });
            await identity.Db.SaveChangesAsync();

            HomepageViewModel model = await RenderAsync();

            FeaturedArticleViewModel featured = Assert.Single(model.Featured);
            Assert.Equal("Pinned blog", featured.Title);
            Assert.Equal("BlogDetails", featured.Action);
            Assert.Equal(blog.Id, featured.RouteId);
        }

        [Fact]
        public async Task A_pinned_advice_becomes_a_featured_article_pointing_at_the_advice()
        {
            await AddSectionAsync("featured", visible: true, order: 1);

            AdviceCategory category = new AdviceCategory { AdviceCategoryname = "c" };
            identity.Db.Add(category);
            await identity.Db.SaveChangesAsync();

            Advice advice = new Advice { AdviceTitle = "Pinned advice", AdviceBriefText = "brief", AdviceCategoryId = category.Id };
            identity.Db.Add(advice);
            await identity.Db.SaveChangesAsync();

            identity.Db.Add(new FeaturedArticle { AdviceId = advice.Id, SortOrder = 1 });
            await identity.Db.SaveChangesAsync();

            HomepageViewModel model = await RenderAsync();

            FeaturedArticleViewModel featured = Assert.Single(model.Featured);
            Assert.Equal("Pinned advice", featured.Title);
            Assert.Equal("ViewAdviceDetails", featured.Action);
            Assert.Equal(advice.Id, featured.RouteId);
        }

        [Fact]
        public async Task Featured_articles_respect_their_sort_order()
        {
            await AddSectionAsync("featured", visible: true, order: 1);

            BlogCategory category = new BlogCategory { BlogCategoryname = "c" };
            identity.Db.Add(category);
            await identity.Db.SaveChangesAsync();

            Blog first = new Blog { BlogTitle = "First", BlogCategoryId = category.Id };
            Blog second = new Blog { BlogTitle = "Second", BlogCategoryId = category.Id };
            identity.Db.Add(first);
            identity.Db.Add(second);
            await identity.Db.SaveChangesAsync();

            identity.Db.Add(new FeaturedArticle { BlogId = second.Id, SortOrder = 1 });
            identity.Db.Add(new FeaturedArticle { BlogId = first.Id, SortOrder = 2 });
            await identity.Db.SaveChangesAsync();

            HomepageViewModel model = await RenderAsync();

            Assert.Equal(new[] { "Second", "First" }, model.Featured.Select(x => x.Title).ToArray());
        }

        [Fact]
        public async Task An_empty_homepage_configuration_renders_nothing_rather_than_failing()
        {
            HomepageViewModel model = await RenderAsync();

            Assert.Empty(model.Sections);
            Assert.Empty(model.Featured);
            Assert.NotNull(model.Content);
        }

        public void Dispose()
        {
            identity.Dispose();
            database.Dispose();
        }
    }
}
