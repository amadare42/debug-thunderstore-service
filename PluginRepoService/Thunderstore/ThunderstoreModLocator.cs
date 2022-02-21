using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using PluginRepoService.Bepinex;
using PluginRepoService.Thunderstore.Model;

namespace PluginRepoService.Thunderstore;

public class ThunderstoreModLocator
{
    private readonly string rootPath;
    private readonly string baseUrl;
    private readonly string defaultOwner;
    private readonly ILogger logger;
    private readonly BepinexPluginLocator pluginLocator;

    public ThunderstoreModLocator(string rootPath, string baseUrl, string defaultOwner, ILogger logger)
    {
        this.pluginLocator = new BepinexPluginLocator(logger);
        this.rootPath = rootPath;
        this.baseUrl = baseUrl;
        this.defaultOwner = defaultOwner;
        this.logger = logger;
    }

    public List<ThunderstoreResponseModel> LocateAll()
    {
        return Directory.EnumerateDirectories(this.rootPath)
            .SelectMany(Directory.EnumerateDirectories)
            .Select(TryMapLocalMod)
            .Where(m => m != null)
            .ToList();
    }

    private ThunderstoreResponseModel TryMapLocalMod(string path)
    {
        try
        {
            return MapMod(path);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, $"Error on mapping local mod @ '{path}'");
        }

        return null;
    }

    private ThunderstoreResponseModel MapMod(string path)
    {
        var pluginPath = Path.Combine(path, "plugin");
        var manifestPath = Path.Combine(pluginPath, "manifest.json");
        var metadataPath = Path.Combine(path, "meta.json");
        var containerNamePath = Path.GetFileName(Path.GetDirectoryName(path));

        if (!File.Exists(manifestPath))
        {
            return null;
        }

        var pluginManifest = ReadManifest(pluginPath, manifestPath);
        var metadata = JsonConvert.DeserializeObject<PluginMetadata>(File.ReadAllText(metadataPath));

        var owner = string.IsNullOrEmpty(metadata.owner) ? this.defaultOwner : metadata.owner;
        var fullName = $"{owner}-{pluginManifest.name}";

        var dateCreated = File.GetCreationTime(manifestPath).ToString("O");
        var dateUpdated = File.GetLastWriteTimeUtc(manifestPath).ToString("O");
        var zipPath = Path.Combine(path, "cache", pluginManifest.version_number + ".zip");
        Directory.CreateDirectory(Path.Combine(path, "cache"));
        var size = EnsureZipCreated(path, zipPath);
        
        return new ThunderstoreResponseModel
        {
            categories = metadata.categories,
            date_created = dateCreated,
            date_updated = dateUpdated,
            full_name = fullName,
            has_nsfw_content = metadata.has_nsfw_content,
            is_deprecated = metadata.is_deprecated,
            is_pinned = metadata.is_pinned,
            name = pluginManifest.name,
            package_url = $"{this.baseUrl}/package/{owner}/{pluginManifest.name}/",
            owner = owner,
            rating_score = metadata.rating_score,
            uuid4 = GuidFromString(pluginManifest.name).ToString(),
            versions = new()
            {
                new ThunderstoreVersionModel
                {
                    name = pluginManifest.name,
                    full_name = fullName,
                    description = pluginManifest.description,
                    icon = $"{this.baseUrl}/package/files/{containerNamePath}/{pluginManifest.name}/plugin/icon.png",
                    version_number = pluginManifest.version_number,
                    dependencies = pluginManifest.dependencies,
                    download_url = $"{this.baseUrl}/package/download/{containerNamePath}/{pluginManifest.name}/cache/{pluginManifest.version_number}.zip",
                    date_created = dateCreated,
                    file_size = size,
                    downloads = 1,
                    uuid4 = GuidFromString(pluginManifest.name + "_" + pluginManifest.version_number).ToString(),
                    is_active = true,
                    website_url = ""
                }
            }
        };
    }

    private ThunderstoreManifest ReadManifest(string pluginPath, string manifestPath)
    {
        var pluginManifest = JsonConvert.DeserializeObject<ThunderstoreManifest>(File.ReadAllText(manifestPath));
        if (string.IsNullOrEmpty(pluginManifest.name))
        {
            var p = this.pluginLocator.LocateAllPlugins(pluginPath).FirstOrDefaultAsync().Result;
            if (p == null)
            {
                return pluginManifest;
            }

            this.logger.LogInformation($"Updating manifest based on '{p.Path}'");
            pluginManifest.name = p.Name;
            pluginManifest.version_number = p.PluginVersion;
            File.WriteAllText(manifestPath, JsonConvert.SerializeObject(pluginManifest, Formatting.Indented));
        }

        return pluginManifest;
    }

    private long EnsureZipCreated(string containerPath, string zipPath)
    {
        if (File.Exists(zipPath))
        {
            return new FileInfo(zipPath).Length;
        }
        var pluginPath = Path.Combine(containerPath, "plugin");
        ZipFile.CreateFromDirectory(pluginPath, zipPath, CompressionLevel.Optimal, false);
        return new FileInfo(zipPath).Length;
    }
    
    private static Guid GuidFromString(string input)
    {
        using (MD5 md5 = MD5.Create())
        {
            byte[] hash = md5.ComputeHash(Encoding.Default.GetBytes(input));
            return new Guid(hash);
        }
    }
}