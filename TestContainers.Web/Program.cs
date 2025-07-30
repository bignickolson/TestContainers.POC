using TestContainers.Web;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient();
// Add services to the container.
builder.Services.AddRazorPages();

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



app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();
