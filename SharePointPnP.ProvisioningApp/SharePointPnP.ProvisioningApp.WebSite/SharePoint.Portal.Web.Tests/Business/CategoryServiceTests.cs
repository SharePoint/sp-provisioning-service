using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SharePoint.Portal.Web.Business;
using SharePoint.Portal.Web.Business.DependencyInjection;
using SharePoint.Portal.Web.Business.Implementation;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SharePoint.Portal.Web.Tests.Business
{
    [TestClass]
    public class CategoryServiceTests
    {
        ICategoryService service;

        Mock<IOptionsMonitor<GlobalOptions>> optionsMock;

        readonly string TestPlatformId = "TESTPLATFORM";

        [TestInitialize]
        public void Init()
        {
            optionsMock = new Mock<IOptionsMonitor<GlobalOptions>>();
            optionsMock.SetupGet(m => m.CurrentValue)
                .Returns(new GlobalOptions
                {
                    ProvisioningPageBaseUrl = "https://example.com",
                    PlatformId = TestPlatformId
                });

            TestHarness.ResetDatabase();
            InsertTestData();
            service = new CategoryService(TestHarness.GetPortalContext(), optionsMock.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {
            service = null;
        }

        [TestMethod]
        public void GetAllWithPackages()
        {
            using (var db = TestHarness.GetPortalContext())
            {
                db.Categories.AddRange(
                    new Models.Category
                    {
                        Id = "TESTONE",
                        DisplayName = "Test one",
                        PackageCategories = new List<Models.PackageCategory>
                        {
                            new Models.PackageCategory { Package = TestHarness.CreatePackageModel(platformIds: new List<string> { TestPlatformId }) },
                            new Models.PackageCategory { Package = TestHarness.CreatePackageModel(platformIds: new List<string> { TestPlatformId }) },
                            new Models.PackageCategory { Package = TestHarness.CreatePackageModel(platformIds: new List<string> { TestPlatformId }) },
                        }
                    },
                    new Models.Category
                    {
                        Id = "TESTTWO",
                        DisplayName = "Test Two",
                        PackageCategories = new List<Models.PackageCategory>
                        {
                            new Models.PackageCategory {  Package = TestHarness.CreatePackageModel(platformIds: new List<string> { TestPlatformId }) },
                        }
                    }
                );
                db.SaveChanges();
            }

            var categories = service.GetAllWithPackages().GetAwaiter().GetResult();

            Assert.AreEqual(2, categories.Count(), "should have the correct number of categories");

            Assert.AreEqual(3, categories[0].Packages.Count, "first category should have the correct number of packages");

            Assert.AreEqual(1, categories[1].Packages.Count, "second category should have the correct number of packages");
        }

        [TestMethod]
        public void GetAllWithPackages_FiltersOutPreviewProperly()
        {
            using (var db = TestHarness.GetPortalContext())
            {
                db.Categories.AddRange(
                    new Models.Category
                    {
                        Id = "TESTONE",
                        DisplayName = "Test one",
                        PackageCategories = new List<Models.PackageCategory>
                        {
                            new Models.PackageCategory { Package = TestHarness.CreatePackageModel(preview: true, platformIds: new List<string> { TestPlatformId }) },
                            new Models.PackageCategory { Package = TestHarness.CreatePackageModel(preview: true, platformIds: new List<string> { TestPlatformId }) },
                            new Models.PackageCategory { Package = TestHarness.CreatePackageModel(platformIds: new List<string> { TestPlatformId }) },
                        }
                    },
                    new Models.Category
                    {
                        Id = "TESTTWO",
                        DisplayName = "Test Two",
                        PackageCategories = new List<Models.PackageCategory>
                        {
                            new Models.PackageCategory {  Package = TestHarness.CreatePackageModel(preview: true, platformIds: new List<string> { TestPlatformId }) },
                        }
                    }
                );
                db.SaveChanges();
            }

            var categories = service.GetAllWithPackages().GetAwaiter().GetResult();
            Assert.AreEqual(1, categories.Count(), "should have filtered out category with only preview packages");
            Assert.AreEqual(1, categories[0].Packages.Count, "first category should have only non preview packages");

            categories = service.GetAllWithPackages(doIncludePreview: true).GetAwaiter().GetResult();
            Assert.AreEqual(2, categories.Count(), "should have all categories");
            Assert.AreEqual(3, categories[0].Packages.Count, "first category should have the correct number of packages");
            Assert.AreEqual(1, categories[1].Packages.Count, "second category should have the correct number of packages");
        }

        [TestMethod]
        public void GetAllWithPackages_SortsProperly()
        {
            using (var db = TestHarness.GetPortalContext())
            {
                db.Categories.AddRange(
                    new Models.Category
                    {
                        Id = "TESTONE",
                        DisplayName = "Test One",
                        Order = 2,
                        PackageCategories = new List<Models.PackageCategory> { new Models.PackageCategory { Package = TestHarness.CreatePackageModel(platformIds: new List<string> { TestPlatformId }) } }
                    },
                    new Models.Category
                    {
                        Id = "TESTTWO",
                        DisplayName = "Test Two",
                        Order = 0,
                        PackageCategories = new List<Models.PackageCategory> { new Models.PackageCategory { Package = TestHarness.CreatePackageModel(platformIds: new List<string> { TestPlatformId }) } }
                    },
                    new Models.Category
                    {
                        Id = "TESTTHREE",
                        DisplayName = "Test Three",
                        Order = 1,
                        PackageCategories = new List<Models.PackageCategory> { new Models.PackageCategory { Package = TestHarness.CreatePackageModel(platformIds: new List<string> { TestPlatformId }) } }
                    }
                );
                db.SaveChanges();
            }

            var categories = service.GetAllWithPackages().GetAwaiter().GetResult();
            Assert.AreEqual("TESTTWO", categories.ElementAt(0).Id);
            Assert.AreEqual("TESTTHREE", categories.ElementAt(1).Id);
            Assert.AreEqual("TESTONE", categories.ElementAt(2).Id);
        }

        [TestMethod]
        public void GetAllWithPackages_FiltersOutVisibleProperly()
        {
            using (var db = TestHarness.GetPortalContext())
            {
                db.Categories.AddRange(
                    new Models.Category
                    {
                        Id = "TESTONE",
                        DisplayName = "Test one",
                        PackageCategories = new List<Models.PackageCategory>
                        {
                            new Models.PackageCategory { Package = TestHarness.CreatePackageModel(isVisible: false,platformIds: new List<string> { TestPlatformId }) },
                            new Models.PackageCategory { Package = TestHarness.CreatePackageModel(isVisible: false, platformIds: new List<string> { TestPlatformId }) },
                            new Models.PackageCategory { Package = TestHarness.CreatePackageModel(platformIds: new List<string> { TestPlatformId }) },
                        }
                    },
                    new Models.Category
                    {
                        Id = "TESTTWO",
                        DisplayName = "Test Two",
                        PackageCategories = new List<Models.PackageCategory>
                        {
                            new Models.PackageCategory {  Package = TestHarness.CreatePackageModel(isVisible: false, platformIds: new List<string> { TestPlatformId }) },
                        }
                    }
                );
                db.SaveChanges();
            }

            var categories = service.GetAllWithPackages().GetAwaiter().GetResult();
            Assert.AreEqual(1, categories.Count(), "should have filtered out category with no visible packages");
            Assert.AreEqual(1, categories[0].Packages.Count, "first category should have only visble packages");
        }

        [TestMethod]
        public void GetAllWithPackages_FiltersOutPlatformProperly()
        {
            var packageId = Guid.NewGuid();
            using (var db = TestHarness.GetPortalContext())
            {
                db.Platforms.Add(new Models.Platform { Id = "PLATFORMTWO" });
                db.Categories.AddRange(
                    new Models.Category
                    {
                        Id = "TESTONE",
                        DisplayName = "Test one",
                        PackageCategories = new List<Models.PackageCategory>
                        {
                            new Models.PackageCategory { Package = TestHarness.CreatePackageModel() },
                            new Models.PackageCategory { Package = TestHarness.CreatePackageModel(platformIds: new List<string> { "PLATFORMTWO" }) },
                            new Models.PackageCategory { Package = TestHarness.CreatePackageModel(platformIds: new List<string> { TestPlatformId }, id: packageId) },
                        }
                    },
                    new Models.Category
                    {
                        Id = "TESTTWO",
                        DisplayName = "Test Two",
                        PackageCategories = new List<Models.PackageCategory>
                        {
                            new Models.PackageCategory {  Package = TestHarness.CreatePackageModel(platformIds: new List<string> { "PLATFORMTWO" }) },
                        }
                    }
                );
                db.SaveChanges();
            }

            var categories = service.GetAllWithPackages().GetAwaiter().GetResult();
            Assert.AreEqual(1, categories.Count(), "should have filtered out category with no packages for the platform");
            Assert.AreEqual(1, categories[0].Packages.Count, "first category should have only packages for the platform");
            Assert.AreEqual(packageId, categories[0].Packages[0].Id);
        }

        private void InsertTestData()
        {
            using (var db = TestHarness.GetPortalContext())
            {
                db.Platforms.Add(new Models.Platform { Id = TestPlatformId });
                db.SaveChanges();
            }
        }
    }
}
