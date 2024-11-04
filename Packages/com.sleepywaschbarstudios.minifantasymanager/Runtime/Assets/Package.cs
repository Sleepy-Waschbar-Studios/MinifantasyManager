using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace MinifantasyManager.Runtime.Assets
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum PackageCategory
    {
        Creatures,
        Weapons,
        Props,
        Tilesets,
        Effects,
        /// <summary>An animation for a creature this covers animations like crafting/professions.</summary>
        Actions,
        Icons,
        UI,
    }

    public class Asset
    {

    }

    public class Package
    {
        public string PackageId { get; set; }
        public string Name { get; set; }
        /// <summary>This is not <see cref="ManagerMetadata.Version"/> this is instead
        /// the version of the package loaded.</summary>
        public string Version { get; set; }
        public DateTimeOffset LastImportedAt { get; set; } = DateTimeOffset.UtcNow;
        public Dictionary<PackageCategory, List<Asset>> Assets { get; set; } = new();

        public static readonly Regex PackageNameAndVersionRegex = new(@"(?:minifantasy)?(?<name>.*?)_v(?<version>.*?)(?:_(?<suffix>.*?))?.zip", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    }

    public class ManagerMetadata
    {
        /// <summary>For now just a simple version so that we can do migrations.</summary>
        public string Version { get; set; } = "1.0";

        /// <summary>Used purely just for auditability purposes.</summary>
        public DateTimeOffset LastModifiedAt { get; set; } = DateTimeOffset.UtcNow;

        /// <summary>Indexed by the package name (not the id).</summary>
        public Dictionary<string, Package> Packages { get; set; } = new();
    }
}
