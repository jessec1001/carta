using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CartaWeb
{
    /// <summary>
    /// Represents the entry point of the program.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Starts the web application.
        /// </summary>
        /// <param name="args">The command line arguments.</param>
        public static void Main(string[] args) => CreateHostBuilder(args).Build().Run();

        /// <summary>
        /// Creates a web host builder using the command line arguments. 
        /// </summary>
        /// <param name="args">The command line arguments.</param>
        /// <returns>The host builder,</returns>
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureLogging(logging =>
                {
                    logging.AddConsole();
                })
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config.AddJsonFile("appsettings.CDK.json", optional: true, reloadOnChange: false);
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
