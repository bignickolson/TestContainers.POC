using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using TestContainers.Api.Data;
using TestContainers.Api.Request;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connString = builder.Configuration.GetConnectionString("Default");
builder.Services.AddDbContext<DataContext>(opt =>
    opt.UseSqlServer(connString));

builder.Services.AddControllers();

builder.Services.AddHealthChecks()
    .AddResourceUtilizationHealthCheck()
    .AddDbContextCheck<DataContext>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using var scope = app.Services.CreateScope();
using var ctx = scope.ServiceProvider.GetRequiredService<DataContext>();
if (ctx.Database.GetPendingMigrations().Any())
{
    await ctx.Database.MigrateAsync();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

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

app.MapGet("/messages", async (DataContext ctx) =>
{
    return Results.Ok(await ctx.Messages.Select(i => new MessageSummary { Id = i.Id, Subject = i.Subject, CreatedOn = i.CreatedOn }).ToListAsync());
})
    .Produces<IEnumerable<MessageSummary>>(StatusCodes.Status200OK);
;

app.MapGet("/message", async (int id, DataContext ctx) =>
{
    var m = await ctx.Messages.FirstOrDefaultAsync(i => i.Id == id);
    return m != null ? Results.Ok(m) : Results.NotFound();
})
    .Produces<Message>(StatusCodes.Status200OK);

app.MapPost("/messages", async (CreateMessageRequest req, DataContext ctx) =>
{
    var m = new Message { Subject = req.Subject, Content = req.Content, CreatedOn = DateTime.Now };
    ctx.Messages.Add(m);
    await ctx.SaveChangesAsync();
    return Results.Created($"/message/{m.Id}", m);
}).Accepts<CreateMessageRequest>("application/json")
.Produces<Message>(StatusCodes.Status201Created);


app.MapDelete("/message", async (int id, DataContext ctx) =>
{
    var m = await ctx.Messages.FirstOrDefaultAsync(i => i.Id == id);
    if (m == null)
        return Results.NotFound();
    ctx.Messages.Remove(m);
    await ctx.SaveChangesAsync();
    return Results.NoContent();
}).Produces(StatusCodes.Status204NoContent).Produces(StatusCodes.Status404NotFound);


app.Run();
