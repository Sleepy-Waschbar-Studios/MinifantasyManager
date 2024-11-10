#nullable enable

using Codice.Client.Common.TreeGrouper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

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
        public TextAsset? AnimationFile { get; set; }
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
        public string ShadowPrefix { get; }
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
        public TextAsset? AnimationFile { get; set; }
        public Dictionary<string, TemporaryAnimationDetails> CharacterAnimations { get; set; } = new(StringComparer.InvariantCultureIgnoreCase);
        public ImageAsset? ProjectileAnimation { get; set; }
        public TemporaryAnimationDetails WeaponAnimation { get; set; } = new();
    }

    public class TemporaryAnimationDetails
    {
        /// <summary>
        /// This should be shared between multiple classes if possible.
        /// </summary>
        public TextAsset? AnimationFile { get; set; }

        public ImageAsset? ForegroundAnimation { get; set; }
        public ImageAsset? BackgroundAnimation { get; set; }
        public ImageAsset? ShadowAnimation { get; set; }
    }

    [Flags]
    public enum AssetFlags
    {
        None = 0,

        /// <summary>
        /// _Characters
        /// </summary>
        CharacterAnimation = 1 << 0,

        /// <summary>
        /// _Shadows
        /// </summary>
        ShadowAnimation = 1 << 1,
    }

    public abstract class TemporaryAsset
    {
        public string FullPath { get; }
        public string Filename { get; }
        public string FilenameNoExt { get; }
        public string Ext { get; }
        public string[] Segments { get; }
        public AssetFlags Flags { get; private set; } = AssetFlags.None;

        protected TemporaryAsset(string path)
        {
            FullPath = path;

            // Note: might be a good idea to swap strings -> spans
            //       and simpify down this logic since we do a lot of similar ops
            Filename = Path.GetFileName(path);
            FilenameNoExt = Path.GetFileNameWithoutExtension(path);
            Ext = Path.GetExtension(path);

            Segments = path
                .Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                .Reverse()
                // Don't include the file name in segments
                .Skip(1)
                .SkipWhile(directory =>
                {
                    if (directory.Equals("_Shadows", StringComparison.InvariantCultureIgnoreCase))
                    {
                        Flags |= AssetFlags.ShadowAnimation;
                        return true;
                    }
                    else if (directory.Equals("_Characters", StringComparison.InvariantCultureIgnoreCase))
                    {
                        Flags |= AssetFlags.CharacterAnimation;
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

    public class TextAsset : TemporaryAsset
    {
        public string Contents { get; set; }

        public TextAsset(string path, string contents) : base(path)
        {
            Contents = contents;
        }
    }

    public class ImageAsset : TemporaryAsset
    {
        public ImageAsset(string path, Texture2D texture) : base(path)
        {
            Texture = texture;
        }

        public Texture2D Texture { get; set; }
    }
}
