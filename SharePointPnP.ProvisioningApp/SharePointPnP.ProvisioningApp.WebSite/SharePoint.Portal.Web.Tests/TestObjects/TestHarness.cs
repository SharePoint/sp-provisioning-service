using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SharePoint.Portal.Web.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharePoint.Portal.Web.Tests
{
    public static class TestHarness
    {
        private static SqliteConnection Connection
        {
            get
            {
                if (_connection == null)
                {
                    _connection = new SqliteConnection("DataSource=:memory:");
                    _connection.Open();
                }
                return _connection;
            }
        }

        private static SqliteConnection _connection;

        public static DbContextOptions<PortalDbContext> DbContextOptions
        {
            get
            {
                return new DbContextOptionsBuilder<PortalDbContext>()
                   .UseSqlite(Connection)
                   .Options;
            }
        }

        /// <summary>
        /// Gets the database context.
        /// </summary>
        /// <returns></returns>
        public static PortalDbContext GetPortalContext()
        {
            var context = new PortalDbContext(DbContextOptions);

            try
            {
                context.Database.EnsureCreated();
            }
            catch (Exception)
            {
                System.Diagnostics.Debugger.Break();
            }

            return context;
        }

        /// <summary>
        /// Deletes the database.
        /// </summary>
        public static void ResetDatabase()
        {
            if (_connection != null)
            {
                _connection.Close();
                _connection = null;
            }
        }

        public static Models.Package CreatePackageModel(Guid? id = null, bool preview = false, bool isVisible = true, List<string> platformIds = null, Models.UI.MetaData metaData = null)
        {
            if (!id.HasValue)
            {
                id = Guid.NewGuid();
            }

            return new Models.Package
            {
                Id = id.Value,
                Version = "",
                DisplayName = "",
                Abstract = "",
                Description = "",
                ImagePreviewUrl = "",
                PackageUrl = "",
                Preview = preview,
                PropertiesMetadata = metaData,
                Visible = isVisible,
                PackagePlatforms = platformIds?.Select(platformId => new Models.PackagePlatform { PlatformId = platformId }).ToList()
            };
        }
    }
}
