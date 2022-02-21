# Debug Thunderstore service

This is simple application that will replicate Thunderstore API for [r2modman](https://github.com/ebkr/r2modmanPlus) (or Thunderstore Mod Manager).
Designed specifically to be self-contained to easily host and share your locally build mods during development before publishing. There are some features to make this process easier.

> Please note: this service isn't fully-featured Thunderstore API and isn't optimized for a lot of requests. It's sole purpose is to help with debug. 

## Setup

1. Setup `data` directory that will contain mods you want to distribute (refer to [Data directory](#data-directory) section)
2. In `appsettings.json` setup following variables:
   - `BaseUrl` - base URL path to your service (e.g. `https://localhost:44362`) _(this step can be skipped if service will be run in IIS Express and no port change is required)_
   - `RootPath` - absolute or relative path for your data directory (e.g. "C:\\thunderstore-shared-mods")
   - `DefaultOwner` - this name will be used if missing in `meta.json`
3. Make sure host process have access to necessary directories
   - if you run application in IIS Express, it should be fine
   - if you run application in IIS Application Pool, you'll have to [allow it](https://docs.microsoft.com/en-us/iis/manage/configuring-security/application-pool-identities) to access built binaries location (e.g. bin/Debug/net6.0) as well as your data directory (e.g. "C:\\thunderstore-shared-mods")

To run service automatically in IIS, refer to [Publish an ASP.NET Core app to IIS](https://docs.microsoft.com/en-us/aspnet/core/tutorials/publish-to-iis?view=aspnetcore-6.0&tabs=netcore-cli) article.

## Data directory

This service will list and download endpoints for your local packages. Those are stored in filesystem with following structure:

- /data
    - \[gamename]
      - \[mod name] _(only short name - e.g. Bepinex, not bbepis-Bepinex)_
        - plugin
          - [mod files]
        - cache _(created automatically)_
          - [zipped version of your mods] _(created automatically)_
        - meta.json _(thunderstore specific information)_
          
Whenever `GET /package` endpoint is called, multiple operations are performed:

   - service will search for all game mods
   - service will populate `plugin/manifest.json` with mod version and name from first found bepinex plugin binary (presumably you should only have one there)
   - if there is no `cache/<version>.zip` file, service will create one based on files in `plugin` directory
   - service will respond with Thunderstore-compatible model listing all available mods

## meta.json file

This file will describe all information that is returned from Thunderstore API, but isn't deducible from plugin files. Here is example of this file:
```json
{
  "categories": ["categories", "for", "the", "mod"],
  "rating_score": 10,
  "is_pinned": true,
  "is_deprecated": false,
  "has_nsfw_content": false,
  "owner": "ModOwnerName"
}
```

# Licence
MIT