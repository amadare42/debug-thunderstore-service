namespace PluginRepoService.Bepinex
{
    public class BepinexPluginLocator
    {
        private readonly ILogger logger;

        public BepinexPluginLocator(ILogger logger)
        {
            this.logger = logger;
        }

        public async IAsyncEnumerable<BepinexPlugin> LocateAllPlugins(string rootPath)
        {
            var count = 0;
            var enumeration = Directory.EnumerateFiles(rootPath, "*.*",
                    new EnumerationOptions
                    {
                        RecurseSubdirectories = true
                    })
                .Where(p => p.EndsWith(".dll"))
                .Select(LoadPlugin);
            
            foreach (var task in enumeration)
            {
                var r = await task;
                if (r != null)
                {
                    count++;
                    yield return r;
                }
            }
            
            this.logger.LogInformation($"Found {count} plugins!");
        }
        
        private Task<BepinexPlugin?> LoadPlugin(string path) => Task.Run(() => LoadPluginSync(path));
        private BepinexPlugin? LoadPluginSync(string path)
        {
            this.logger.LogInformation($"Checking dll {path}");
            try
            {
                return Find(path);
            }
            catch (Exception ex)
            {
                this.logger.LogError($"Error occurred when reading dll {path}: {ex.Message}");
            }
            return null;
        }
        private BepinexPlugin? Find(string fileName)
        {
            using var assembly = Mono.Cecil.AssemblyDefinition.ReadAssembly(fileName);
            var attribute = assembly.MainModule.Types
                .Select(t => t.CustomAttributes.FirstOrDefault(a => a.AttributeType.FullName == "BepInEx.BepInPlugin"))
                .FirstOrDefault(t => t != null);
            if (attribute == null)
            {
                return null;
            }

            return new BepinexPlugin
            {
                GUID = (string)attribute.ConstructorArguments[0].Value,
                Name = (string)attribute.ConstructorArguments[1].Value,
                PluginVersion = (string)attribute.ConstructorArguments[2].Value,
                Path = fileName
            };
        }
    }
}