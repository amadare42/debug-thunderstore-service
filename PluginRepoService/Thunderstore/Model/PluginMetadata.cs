namespace PluginRepoService.Thunderstore.Model;

public class PluginMetadata
{
    public List<string> categories { get; set; } = new();
    public int rating_score { get; set; }
    public bool is_pinned { get; set; }
    public bool is_deprecated { get; set; }
    public bool has_nsfw_content { get; set; }
    public string owner { get; set; }
}