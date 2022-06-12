using ServiceLifetimeDemonstration;

var builder = WebApplication.CreateBuilder(args);
//builder.Host.UseDefaultServiceProvider(options => options.ValidateScopes = false);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddSingleton<IGuidService, GuidService>();
builder.Services.AddSingleton<IGuidTrimmer, GuidTrimmer>();
builder.Services.AddSingleton<DisposableService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}
app.UseStaticFiles();

app.UseRouting();

app.UseMiddleware<CustomMiddleware>();

app.MapRazorPages();

app.Run();
