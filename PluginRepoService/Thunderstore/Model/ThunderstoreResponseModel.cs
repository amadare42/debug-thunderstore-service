namespace PluginRepoService.Thunderstore.Model;

public class ThunderstoreResponseModel
{
    public string name { get; set; }
    public string full_name { get; set; }
    public string owner { get; set; }
    public string package_url { get; set; }
    public string date_created { get; set; }
    public string date_updated { get; set; }
    public string uuid4 { get; set; }
    public int rating_score { get; set; }
    public bool is_pinned { get; set; }
    public bool is_deprecated { get; set; }
    public bool has_nsfw_content { get; set; }
    public List<string> categories { get; set; } = new();
    public List<ThunderstoreVersionModel> versions { get; set; } = new();
}