using BrainstormSessions.Core.Interfaces;
using BrainstormSessions.Core.Model;
using BrainstormSessions.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
builder.Host.UseSerilog((context, loggerConfig) =>
{
    loggerConfig
        .ReadFrom.Configuration(context.Configuration) 
        .Enrich.FromLogContext()
        .WriteTo.Console();
});

// Add services to the container
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseInMemoryDatabase("InMemoryDb"));

builder.Services.AddControllersWithViews();

builder.Services.AddScoped<IBrainstormSessionRepository, EFStormSessionRepository>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    // Initialize database
    using var scope = app.Services.CreateScope();
    var repository = scope.ServiceProvider.GetRequiredService<IBrainstormSessionRepository>();
    await InitializeDatabaseAsync(repository);
}

app.UseStaticFiles();

app.UseRouting();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

// Helper methods
async Task InitializeDatabaseAsync(IBrainstormSessionRepository repo)
{
    var sessionList = await repo.ListAsync();
    if (!sessionList.Any())
    {
        await repo.AddAsync(GetTestSession());
    }
}

BrainstormSession GetTestSession()
{
    var session = new BrainstormSession
    {
        Name = "Test Session 1",
        DateCreated = new DateTime(2016, 8, 1)
    };
    var idea = new Idea
    {
        DateCreated = new DateTime(2016, 8, 1),
        Description = "Totally awesome idea",
        Name = "Awesome idea"
    };
    session.AddIdea(idea);
    return session;
}