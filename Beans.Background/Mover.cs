using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;

using Newtonsoft.Json;

using Serilog;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Beans.Background;

public class Mover
{
    private readonly FunctionSettings _settings;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public Mover(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        var section = _configuration.GetSection("FunctionSettings");
        _settings = section.Get<FunctionSettings>();
        Log.Logger = !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("WEBSITE_INSTANCE_ID"))
            ? new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.AzureBlobStorage(connectionString: _settings.LogConnectionString,
                    storageContainerName: _settings.ContainerName,
                    storageFileName: "{yyyyMMdd}-" + _settings.LogFilenameBase)
                .CreateLogger()
            : (Serilog.ILogger)new LoggerConfiguration().WriteTo.Console().CreateLogger();
        Log.Information("Beans Background function initialized");
    }

    private Uri CreateUri(int version = 1, string controller = null, string action = null, params object[] parms)
    {
        var sb = new StringBuilder(_settings.ApiBase);
        if (version > 0)
        {
            sb.Append($"/api/v{version}/");
        }
        if (!string.IsNullOrWhiteSpace(controller))
        {
            sb.Append($"{controller}/");
        }
        if (!string.IsNullOrWhiteSpace(action))
        {
            sb.Append($"{action}/");
        }
        if (parms is not null && parms.Any())
        {
            foreach (var parm in parms)
            {
                sb.Append(parm);
                sb.Append('/');
            }
        }
        return new Uri(sb.ToString());
    }

    private string Innermost(Exception ex)
    {
        if (ex is null)
        {
            return string.Empty;
        }
        if (ex.InnerException is null)
        {
            return ex.Message;
        }
        return Innermost(ex.InnerException);
    }

    // runs daily at 3 am

    [FunctionName("Move")]
    public async Task Run([TimerTrigger("0 0 3 * * *")] TimerInfo myTimer)
    {
        Log.Information("Beans background function started");
        Log.Information("Beans background function - timer info: Past Due = {pastdue}", myTimer.IsPastDue);
        var uri = CreateUri(controller: "Movement", action: "CatchUp");
        using var client = _httpClientFactory.CreateClient();
        using var response = await client.PostAsJsonAsync(uri, _settings.ApiKey);
        if (response.IsSuccessStatusCode)
        {
            Log.Information("Beans background function - CatchUp completed with no errors");
        }
        else
        {
            var json = await response.Content.ReadAsStringAsync();
            try
            {
                var apierror = JsonConvert.DeserializeObject<ApiError>(json);
                Log.Error("Beans background function - CatchUp error: {error}", apierror.Message);
            }
            catch (Exception ex)
            {
                Log.Error("Beans background function - Error deserializing response: {error}", Innermost(ex));
            }
        }
        Log.CloseAndFlush();
    }
}
