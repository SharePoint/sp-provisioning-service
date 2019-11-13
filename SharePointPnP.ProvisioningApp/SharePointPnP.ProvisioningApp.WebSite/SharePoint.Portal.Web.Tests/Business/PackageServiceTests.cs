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
    public class PackageServiceTests
    {
        IPackageService service;

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
            service = new PackageService(TestHarness.GetPortalContext(), optionsMock.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {
            service = null;
        }

        [TestMethod]
        public void GetById_GetsById()
        {
            var id = Guid.NewGuid();
            var metaData = new Models.UI.MetaData
            {
                DisplayInfo = new Models.UI.DisplayInfo
                {
                    PageTemplateId = "my-template-id",
                    SystemRequirements = new List<Models.UI.SystemRequirement>
                    {
                        new Models.UI.SystemRequirement { Name = "TestRequirement", Value = "TestValue"}
                    }
                }
            };

            using (var db = TestHarness.GetPortalContext())
            {
                db.Packages.AddRange(
                    TestHarness.CreatePackageModel(id: id, metaData: metaData, platformIds: new List<string> { TestPlatformId }),
                    TestHarness.CreatePackageModel(platformIds: new List<string> { TestPlatformId })
                );

                db.SaveChanges();
            }

            var package = service.GetById(id).GetAwaiter().GetResult();

            Assert.IsNotNull(package);
            Assert.AreEqual(id, package.Id);

            Assert.IsNotNull(package.DisplayInfo);
            Assert.AreEqual(metaData.DisplayInfo.PageTemplateId, package.DisplayInfo.PageTemplateId);
            Assert.AreEqual(1, package.DisplayInfo.SystemRequirements.Count);
            Assert.AreEqual("TestRequirement", package.DisplayInfo.SystemRequirements[0].Name);
            Assert.AreEqual("TestValue", package.DisplayInfo.SystemRequirements[0].Value);
        }

        [TestMethod]
        public void GetById_FiltersOutPreviewProperly()
        {
            var id = Guid.NewGuid();
            using (var db = TestHarness.GetPortalContext())
            {
                db.Packages.AddRange(
                    TestHarness.CreatePackageModel(id: id, preview: true, platformIds: new List<string> { TestPlatformId }),
                    TestHarness.CreatePackageModel(platformIds: new List<string> { TestPlatformId })
                );
                db.SaveChanges();
            }

            var package = service.GetById(id).GetAwaiter().GetResult();
            Assert.IsNull(package, "should filter out preview package when not including preview");

            package = service.GetById(id, doIncludePreview: true).GetAwaiter().GetResult();
            Assert.IsNotNull(package, "should find package when including preview");
        }

        [TestMethod]
        public void GetById_FiltersOutPlatformProperly()
        {
            var id = Guid.NewGuid();
            var idNotInPlatform = Guid.NewGuid();
            using (var db = TestHarness.GetPortalContext())
            {
                db.Packages.AddRange(
                    TestHarness.CreatePackageModel(id: id, platformIds: new List<string> { TestPlatformId }),
                    TestHarness.CreatePackageModel(id: idNotInPlatform)
                );
                db.SaveChanges();
            }

            var package = service.GetById(id).GetAwaiter().GetResult();
            Assert.IsNotNull(package, "should find package in the platform");

            package = service.GetById(idNotInPlatform).GetAwaiter().GetResult();
            Assert.IsNull(package, "should not find package that is not in the platform");
        }

        [TestMethod]
        public void GetFromQuery_FiltersOutPreviewProperly()
        {
            var metaData = new Models.UI.MetaData
            {
                DisplayInfo = new Models.UI.DisplayInfo
                {
                    PageTemplateId = "my-template-id",
                    SystemRequirements = new List<Models.UI.SystemRequirement>
                    {
                        new Models.UI.SystemRequirement { Name = "TestRequirement", Value = "TestValue"}
                    }
                }
            };

            using (var db = TestHarness.GetPortalContext())
            {
                db.Packages.AddRange(
                    TestHarness.CreatePackageModel(preview: true, platformIds: new List<string> { TestPlatformId }),
                    TestHarness.CreatePackageModel(platformIds: new List<string> { TestPlatformId })
                );
                db.SaveChanges();
            }

            var packages = service.GetFromQuery().GetAwaiter().GetResult();
            Assert.AreEqual(1, packages.Count());

            packages = service.GetFromQuery(doIncludePreview: true).GetAwaiter().GetResult();
            Assert.AreEqual(2, packages.Count());
        }

        [TestMethod]
        public void GetFromQuery_FiltersByCategoryProperly()
        {
            var packageId1 = Guid.NewGuid();
            var packageId2 = Guid.NewGuid();
            using (var db = TestHarness.GetPortalContext())
            {
                var package1 = db.Packages.Add(TestHarness.CreatePackageModel(id: packageId1, platformIds: new List<string> { TestPlatformId }));
                var package2 = db.Packages.Add(TestHarness.CreatePackageModel(id: packageId2, platformIds: new List<string> { TestPlatformId }));
                db.Categories.AddRange(
                    new Models.Category
                    {
                        Id = "TESTONE",
                        DisplayName = "Test One",
                        PackageCategories = new List<Models.PackageCategory> {
                           new Models.PackageCategory { Package = package1.Entity },
                           new Models.PackageCategory { Package = package2.Entity },
                        }
                    },
                    new Models.Category
                    {
                        Id = "TESTTWO",
                        DisplayName = "Test Two",
                        PackageCategories = new List<Models.PackageCategory> {
                           new Models.PackageCategory { Package = package1.Entity },
                        }
                    }
                );
                db.SaveChanges();
            }

            var packages = service.GetFromQuery(categoryId: "TESTONE").GetAwaiter().GetResult();
            Assert.AreEqual(2, packages.Count());
            Assert.IsTrue(packages.Any(p => p.Id == packageId1), "should contain the first package");
            Assert.IsTrue(packages.Any(p => p.Id == packageId2), "should contain the second package");

            packages = service.GetFromQuery(categoryId: "TESTTWO").GetAwaiter().GetResult();
            Assert.AreEqual(1, packages.Count());
            Assert.IsTrue(packages.Any(p => p.Id == packageId1), "should contain the first package");
        }

        [TestMethod]
        public void GetFromQuery_FiltersOutPlatformProperly()
        {
            var packageId1 = Guid.NewGuid();
            var packageId2 = Guid.NewGuid();
            using (var db = TestHarness.GetPortalContext())
            {
                var package1 = db.Packages.Add(TestHarness.CreatePackageModel(id: packageId1, platformIds: new List<string> { TestPlatformId }));
                var package2 = db.Packages.Add(TestHarness.CreatePackageModel(id: packageId2, platformIds: new List<string> { TestPlatformId }));
                var package3 = db.Packages.Add(TestHarness.CreatePackageModel(id: Guid.NewGuid()));
                db.Platforms.Add(
                    new Models.Platform
                    {
                        Id = "PLATFORMTWO",
                        PackagePlatforms = new List<Models.PackagePlatform>
                        {
                            new Models.PackagePlatform { Package = package1.Entity },
                            new Models.PackagePlatform { Package = package2.Entity },
                            new Models.PackagePlatform { Package = package3.Entity },
                        }
                    }
                );
                db.SaveChanges();
            }

            var packages = service.GetFromQuery().GetAwaiter().GetResult();
            Assert.AreEqual(2, packages.Count());
            Assert.IsTrue(packages.Any(p => p.Id == packageId1), "should contain the first package");
            Assert.IsTrue(packages.Any(p => p.Id == packageId2), "should contain the second package");
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
