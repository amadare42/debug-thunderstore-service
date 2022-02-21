namespace PluginRepoService.Thunderstore.Model;

public class ThunderstoreManifest
{
    public string name { get; set; }
    public string description { get; set; }
    public string website_url { get; set; }
    public string version_number { get; set; }
    public List<string> dependencies { get; set; } = new();
}