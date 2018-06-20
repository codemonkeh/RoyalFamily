using System.IO;
using RoyalFamily.Common.Data.Repositories;
using RoyalFamily.Data;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace RoyalFamily.Web
{
    public class Program
    {
        public static IConfiguration Configuration { get; set; }

        public static void Main(string[] args)
        {
            var host = BuildWebHost(args);

            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;

                var builder = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json");
                Configuration = builder.Build();

                // As we are using an in-memory database for this exercise we must seed it on start-up.
                // In a real-world application this would be unnecessary
                var dataFilename = Configuration["Data:TestDataFilename"];
                var personRepository = services.GetService<IPersonRepository>();
                var importer = new JsonDataImporter(personRepository);
                importer.LoadPeopleFromJsonFile(dataFilename).Wait();
            }

            host.Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .Build();
    }
}
