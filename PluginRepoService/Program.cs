using Microsoft.Extensions.FileProviders;
using PluginRepoService.Thunderstore;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.AddFile(builder.Configuration.GetSection("Logging"));

var app = builder
    .Build();

var containersRoot = app.Configuration.GetValue<string>("RootPath");
var baseUrl = app.Configuration.GetValue<string>("BaseUrl");
var defaultOwner = app.Configuration.GetValue<string>("DefaultOwner");
var locator = new ThunderstoreModLocator(containersRoot, baseUrl, defaultOwner, app.Logger);

app.MapGet("/", () => "Hello!");
app.MapGet("/package", async () => locator.LocateAll());
app.MapGet("/package/empty", () => "");
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(containersRoot)),
    RequestPath = "/package/files",
    ServeUnknownFileTypes = true
});
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(containersRoot)),
    RequestPath = "/package/download",
    ServeUnknownFileTypes = true
});
app.Run();