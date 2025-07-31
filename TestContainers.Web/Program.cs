using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using System.Text.Json;
using TestContainers.Web;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient();
// Add services to the container.
builder.Services.AddRazorPages();

builder.Services.AddHealthChecks()
    .AddUrlGroup(new Uri($"{builder.Configuration["ApiUrl"]}/health"), "MessageApi")
    .AddResourceUtilizationHealthCheck();

builder.Services.AddScoped<MessageClient>((sp) => {
    var config = sp.GetRequiredService<IConfiguration>();
    return new MessageClient(config.GetValue<string>("ApiUrl"), sp.GetRequiredService<HttpClient>());
});
    

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}



app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";

        var result = new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(entry => new
            {
                name = entry.Key,
                status = entry.Value.Status.ToString(),
                description = entry.Value.Description,
                exception = entry.Value.Exception?.Message,
                duration = entry.Value.Duration.ToString()
            })
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(result));
    }
});

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();
