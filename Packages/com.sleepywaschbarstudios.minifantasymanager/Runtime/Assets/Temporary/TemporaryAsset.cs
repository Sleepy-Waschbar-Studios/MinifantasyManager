#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace MinifantasyManager.Runtime.Assets.Temporary
{
    /* Temporary Assets
     * 
     * These are used to build the asset tree with partial information.  This is because we don't have full information about the tree
     * depending on the zip entry iteration order.
     * 
     * In particular; "Minifantasy_Weapons_v3.0\Minifantasy_Weapons_Assets\Charged_Attacks\Thrust\AnimationInfo.txt"
     * we know that for all thrust weapons with a charged attack (spear & pitchfork) that they have these animation frames
     * but we may not have visited all the weapons yet so we need to store this partial information.
     */

    public class TemporaryWeaponClassificationDetails
    {
        /// <summary>
        /// i.e. "Thrust Attack"
        /// </summary>
        public string WeaponClassification { get; set; }

        public TemporaryWeaponClassificationDetails(string weaponClassification)
        {
            WeaponClassification = weaponClassification;
        }

        public Dictionary<string, TemporaryAnimationDetails> Animations { get; set; } = new();
        public TemporaryTextAsset? AnimationFile { get; set; }
    }

    public class TemporaryCharacterDetails
    {
        public TemporaryCharacterDetails(string name, string prefix)
        {
            Name = name;
            Prefix = prefix;
        }

        public string Name { get; }
        public string Prefix { get; }
        public TemporaryTextAsset? AnimationFile { get; set; }
        public Dictionary<string, TemporaryAnimationDetails> Details { get; set; } = new();
    }

    public class TemporaryWeaponDetails
    {
        /// <summary>
        /// i.e. "Spear"
        /// </summary>
        public string WeaponName { get; set; }

        public TemporaryWeaponDetails(string weaponName)
        {
            WeaponName = weaponName;
        }
    }

    public class TemporaryWeaponAnimationDetails
    {
        public TemporaryTextAsset? AnimationFile { get; set; }
        public Dictionary<string, TemporaryAnimationDetails> CharacterAnimations { get; set; } = new(StringComparer.OrdinalIgnoreCase);
        public TemporaryImageAsset? ProjectileAnimation { get; set; }
        public TemporaryAnimationDetails? WeaponAnimation { get; set; }
    }

    public class TemporaryAnimationDetails
    {
        public string Name { get; set; }

        public TemporaryAnimationDetails(string name)
        {
            Name = name;
        }

        /// <summary>
        /// This should be shared between multiple classes if possible.
        /// </summary>
        public TemporaryTextAsset? AnimationFile { get; set; }

        public TemporaryImageAsset? ForegroundAnimation { get; set; }
        public TemporaryImageAsset? BackgroundAnimation { get; set; }
        public TemporaryImageAsset? ShadowAnimation { get; set; }
    }

    [Flags]
    public enum AssetFlags
    {
        None = 0,

        /// <summary>
        /// _Characters
        /// </summary>
        Character = 1 << 0,

        /// <summary>
        /// _Shadows
        /// </summary>
        Shadow = 1 << 1,

        ForegroundAnimation = 1 << 2,

        BackgroundAnimation = 1 << 3,

        AnimationInfo = 1 << 4,
    }

    public abstract class TemporaryAsset
    {
        public string FullPath { get; }
        public string Filename { get; }
        public string FilenameNoExt { get; }
        public string Ext { get; }
        public string FullDirPath { get; }
        public string[] Segments { get; }
        public AssetFlags Flags { get; private set; } = AssetFlags.None;

        public override string ToString()
        {
            return FullPath;
        }

        protected TemporaryAsset(string path)
        {
            FullPath = path;

            // Note: might be a good idea to swap strings -> spans
            //       and simpify down this logic since we do a lot of similar ops
            Filename = Path.GetFileName(path);
            FilenameNoExt = Path.GetFileNameWithoutExtension(path);
            Ext = Path.GetExtension(path);
            FullDirPath = Path.GetDirectoryName(path);

            if (Filename.Contains("_b.", StringComparison.OrdinalIgnoreCase))
            {
                Flags |= AssetFlags.BackgroundAnimation;
            }
            else
            {
                Flags |= AssetFlags.ForegroundAnimation;
            }

            if (Ext.Equals(".txt", StringComparison.OrdinalIgnoreCase) && Filename.Contains("Animation", StringComparison.OrdinalIgnoreCase))
            {
                Flags |= AssetFlags.AnimationInfo;
            }

            if (FullPath.Contains("Shadow", StringComparison.OrdinalIgnoreCase))
            {
                Flags |= AssetFlags.Shadow;
            }

            Segments = path
                .Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                .Reverse()
                // Don't include the file name in segments
                .Skip(1)
                .SkipWhile(directory =>
                {
                    if (directory.Replace("_", "").Equals("Shadows", StringComparison.OrdinalIgnoreCase))
                    {
                        Flags |= AssetFlags.Shadow;
                        return true;
                    }
                    else if (directory.Replace("_", "").Equals("Characters", StringComparison.OrdinalIgnoreCase))
                    {
                        Flags |= AssetFlags.Character;
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                })
                .ToArray();
        }
    }

    public class TemporaryTextAsset : TemporaryAsset
    {
        public string Contents { get; set; }

        public TemporaryTextAsset(string path, string contents) : base(path)
        {
            Contents = contents;
        }
    }

    public class TemporaryImageAsset : TemporaryAsset
    {
        public TemporaryImageAsset(string path, Func<Stream> textureStream) : base(path)
        {
            TextureStream = textureStream;
        }

        public Func<Stream> TextureStream { get; set; }
    }
}
