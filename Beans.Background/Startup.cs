using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using System.IO;
using System.Reflection;

[assembly: FunctionsStartup(typeof(Beans.Background.Startup))]
namespace Beans.Background;

public class Startup : FunctionsStartup
{
    public override void Configure(IFunctionsHostBuilder builder)
    {
        builder.Services.AddHttpClient();
        var fileinfo = new FileInfo(Assembly.GetExecutingAssembly().Location);
        var path = fileinfo.Directory.Parent.FullName;
        var config = new ConfigurationBuilder()
            .SetBasePath(path)
            .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();
        builder.Services.AddSingleton<IConfiguration>(config);
    }
}
