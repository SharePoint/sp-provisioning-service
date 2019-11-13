using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharePoint.Portal.Web.Business;
using SharePoint.Portal.Web.Business.Implementation;
using System;

namespace SharePoint.Portal.Web.Tests.Business
{
    [TestClass]
    public class PageTemplateServiceTests
    {
        private IPageTemplateService service;

        [TestInitialize]
        public void Init()
        {
            TestHarness.ResetDatabase();
            service = new PageTemplateService(TestHarness.GetPortalContext());
        }

        [TestCleanup]
        public void Cleanup()
        {
            service = null;
        }

        [TestMethod]
        public void GetPageTemplate_Exists()
        {
            var id = "templateId";
            var testHtml = "<span>{{ context.testing }}</span>";
            var testCss = "span { color: red }";
            using (var db = TestHarness.GetPortalContext())
            {
                db.PageTemplates.Add(new Models.PageTemplate
                {
                    Id = id,
                    Html = testHtml,
                    Css = testCss
                });
                db.SaveChanges();
            }

            var pageTemplate = service.GetPageTemplate(id).GetAwaiter().GetResult();

            Assert.IsNotNull(pageTemplate, "page template should be found");
            Assert.AreEqual(id, pageTemplate.Id);
            Assert.AreEqual(testHtml, pageTemplate.Html);
            Assert.AreEqual(testCss, pageTemplate.Css);
        }

        [TestMethod]
        public void GetPageTemplate_NotExists()
        {
            var id = "templateId";
            var badId = "badTemplateId";
            var testHtml = "<span>{{ context.testing }}</span>";
            var testCss = "span { color: red }";
            using (var db = TestHarness.GetPortalContext())
            {
                db.PageTemplates.Add(new Models.PageTemplate
                {
                    Id = id,
                    Html = testHtml,
                    Css = testCss
                });
                db.SaveChanges();
            }

            var pageTemplate = service.GetPageTemplate(badId).GetAwaiter().GetResult();
            Assert.IsNull(pageTemplate, "page template should not be found");
        }
    }
}
