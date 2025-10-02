using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace DataAccessLayer
{
    public class MyAppDbContextFactory : IDesignTimeDbContextFactory<MyAppDbContext>
    {
        public MyAppDbContext CreateDbContext(string[] args)
        {
            // Get the base directory (solution root)
            var basePath = Directory.GetCurrentDirectory();
            
            // If we're in the DataAccessLayer directory, we need to go up to find the solution root
            if (basePath.EndsWith("DataAccessLayer"))
            {
                basePath = Directory.GetParent(basePath)?.FullName ?? basePath;
            }
            
            // Try to find the Garage_pro_api directory
            var apiProjectPath = Path.Combine(basePath, "Garage_pro_api");
            if (!Directory.Exists(apiProjectPath))
            {
                // If we can't find it, try going up one more level
                var parentPath = Directory.GetParent(basePath)?.FullName;
                if (parentPath != null)
                {
                    apiProjectPath = Path.Combine(parentPath, "Garage_pro_api");
                }
            }
            
            // If still not found, use the current path
            if (!Directory.Exists(apiProjectPath))
            {
                apiProjectPath = basePath;
            }
            
            var builder = new ConfigurationBuilder()
                .SetBasePath(apiProjectPath)
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development"}.json", optional: true);

            IConfigurationRoot configuration = builder.Build();

            var dbContextBuilder = new DbContextOptionsBuilder<MyAppDbContext>();
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            dbContextBuilder.UseSqlServer(connectionString);

            return new MyAppDbContext(dbContextBuilder.Options);
        }
    }
}