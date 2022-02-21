namespace PluginRepoService.Bepinex;

public class BepinexPlugin
{
    public string Name { get; set; }
    public string GUID { get; set; }
    public string Path { get; set; }
    public string PluginVersion { get; set; }

    public Version Version => Version.Parse(this.PluginVersion);

    public override string ToString()
    {
        return $"{this.Name}:{this.PluginVersion}";
    }
}