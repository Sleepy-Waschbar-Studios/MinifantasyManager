#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using UnityEngine;

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

    public class CreatureAsset : Asset
    {
        public string Name { get; set; }
        public Dictionary<string, AnimationAsset> CommonAnimations { get; set; } = new();
    }

    public class WeaponAsset : Asset
    {
        public Dictionary<string, AttackAnimationAsset> Attack { get; set; } = new();
    }

    public class AttackAnimationAsset : Asset
    {
        public Dictionary<string, AnimationAsset> CharacterAnimations { get; set; } = new();
        public AnimationAsset WeaponAnimation { get; set; }
        public AnimationAsset ProjectileAnimation { get; set; }
    }

    public class AnimationAsset : Asset
    {
        public AnimationInfo AnimationInfo { get; set; }
        public Texture2D Foreground { get; set; }
        public Texture2D? Background { get; set; }
        public Texture2D? Shadow { get; set; }
    }

    public class AnimationInfo : Asset
    {
        public Vector2Int FrameSize { get; set; }
        public int FrameDurationMs { get; set; }
    }

    public class Asset
    {

    }

    public class Package
    {
        public static readonly Regex PackageNameAndVersionRegex = new(@"(?:minifantasy)?(?<name>.*?)_v(?<version>.*?)(?:_(?<suffix>.*?))?.zip", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public string PackageId { get; set; }
        public string Name { get; set; }
        /// <summary>This is not <see cref="ManagerMetadata.Version"/> this is instead
        /// the version of the package loaded.</summary>
        public string Version { get; set; }
        public DateTimeOffset LastImportedAt { get; set; } = DateTimeOffset.UtcNow;
        public Dictionary<string, WeaponAsset> Weapons { get; set; } = new();
        public Dictionary<string, CreatureAsset> Creatures { get; set; } = new();
    }

    public enum AssetSuffixType
    {
        Creature,
        Weapon,
        Foreground,
        Background,
    }

    public class ManagerMetadata
    {
        /// <summary>For now just a simple version so that we can do migrations.</summary>
        public string Version { get; set; } = "1.0";

        /// <summary>Used purely just for auditability purposes.</summary>
        public DateTimeOffset LastModifiedAt { get; set; } = DateTimeOffset.UtcNow;

        /// <summary>Indexed by the package name (not the id).</summary>
        public Dictionary<string, Package> Packages { get; set; } = new();

        /// <summary>These are a list of valid suffixes, for example you might have `_b` for background or `_orc`</summary>
        [JsonIgnore]
        public Dictionary<string, AssetSuffixType> ValidSuffixes { get; set; }

        [OnDeserialized]
        internal void OnDeserialized(StreamingContext context)
        {
            // TODO: It might be a better idea to use a Lazy?  Primarily, because we don't need to pay this cost in play mode.

            // Build the full set of suffixes, I don't want to persist this since ideally we should be regenerating it frequently
            // and I prefer for indexes to not be maintained by an application (and instead rebuilt) until they become a significant performance impact.
            // In this case, this could easily build 10s of thousands of creatures/weapons before becoming an issue.
            ValidSuffixes = new Dictionary<string, AssetSuffixType>(Packages.Values.SelectMany(p =>
                    p.Creatures.Select(c => new KeyValuePair<string, AssetSuffixType>(c.Key, AssetSuffixType.Creature))
                    .Concat(p.Weapons.Select(w => new KeyValuePair<string, AssetSuffixType>(w.Key, AssetSuffixType.Weapon)))
                .Append(new KeyValuePair<string, AssetSuffixType>("f", AssetSuffixType.Foreground))
                .Append(new KeyValuePair<string, AssetSuffixType>("b", AssetSuffixType.Background))
            ), StringComparer.InvariantCultureIgnoreCase);
        }
    }
}
