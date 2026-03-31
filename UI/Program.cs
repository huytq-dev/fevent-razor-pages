using Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using UI.Models.Chatbot;
using UI.Services.Implementations;
using UI.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add DbContext for SQL Server
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("SqlServerConnection")));

// Identity đã được loại bỏ, chỉ dùng User custom và BCrypt.

builder.Services
    .AddInfrastructureServices(builder.Configuration)
    .AddApplicationServices(builder.Configuration)
    .AddApiServices(builder.Configuration);

builder.Services.Configure<AiChatbotOptions>(builder.Configuration.GetSection("AiChatbot"));
builder.Services.AddHttpClient<IAiChatbotService, AiChatbotService>();

// Add services to the container.
builder.Services.AddRazorPages();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.Name = ".FEvents.Session";
});

var app = builder.Build();

var uploadsRootPath = Path.Combine(app.Environment.ContentRootPath, "uploads");
Directory.CreateDirectory(Path.Combine(uploadsRootPath, "avatars"));

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

// Seed database
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await db.Database.MigrateAsync();
    await AppData.SeedAsync(db);
}

app.UseHttpsRedirection();

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(uploadsRootPath),
    RequestPath = "/uploads"
});

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.UseSession();

app.MapRazorPages()
   .WithStaticAssets();


app.MapPost("/api/chatbot/message", async (
    IAiChatbotService chatbotService,
    ChatbotRequest request,
    CancellationToken ct) =>
{
    if (string.IsNullOrWhiteSpace(request.Message))
    {
        return Results.BadRequest(new { error = "Message is required." });
    }

    var result = await chatbotService.GetReplyAsync(request.Message, ct);
    return Results.Ok(new { reply = result.Reply, errorDetail = result.ErrorDetail });
});

app.Run();

public sealed record ChatbotRequest(string Message);
