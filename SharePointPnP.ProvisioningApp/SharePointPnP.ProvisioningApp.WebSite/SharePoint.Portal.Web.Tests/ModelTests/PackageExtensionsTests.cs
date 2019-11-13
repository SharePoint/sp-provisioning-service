using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharePoint.Portal.Web.Models.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace SharePoint.Portal.Web.Tests.ModelTests
{
    [TestClass]
    public class PackageExtensionsTests
    {
        [TestMethod]
        public void ToUiFormat_ProvideCorrectProvisioningFormUrl()
        {
            var stringGuid = "B4F93552-53C5-4EF1-BBA6-0AF52C3B785B";
            var testId = Guid.Parse(stringGuid);
            var package = TestHarness.CreatePackageModel(id: testId);

            package.PackageType = Models.PackageType.Tenant;
            var uiPackage = PackageExtensions.ToUiFormat(package, "https://example.com/testing", false);
            Assert.AreEqual(
                "https://example.com/testing/tenant/home/provision?packageId=B4F93552-53C5-4EF1-BBA6-0AF52C3B785B".ToLower(),
                uiPackage.ProvisioningFormUrl.ToLower());

            package.PackageType = Models.PackageType.Tenant;
            uiPackage = PackageExtensions.ToUiFormat(package, "https://example.com", false);
            Assert.AreEqual(
                "https://example.com/tenant/home/provision?packageId=B4F93552-53C5-4EF1-BBA6-0AF52C3B785B".ToLower(),
                uiPackage.ProvisioningFormUrl.ToLower());

            package.PackageType = Models.PackageType.SiteCollection;
            uiPackage = PackageExtensions.ToUiFormat(package, "https://example.com/testing", false);
            Assert.AreEqual(
                "https://example.com/testing/site/home/provision?packageId=B4F93552-53C5-4EF1-BBA6-0AF52C3B785B".ToLower(),
                uiPackage.ProvisioningFormUrl.ToLower());

            package.PackageType = Models.PackageType.SiteCollection;
            uiPackage = PackageExtensions.ToUiFormat(package, "https://example.com", false);
            Assert.AreEqual(
                "https://example.com/site/home/provision?packageId=B4F93552-53C5-4EF1-BBA6-0AF52C3B785B".ToLower(),
                uiPackage.ProvisioningFormUrl.ToLower());
        }
    }
}
