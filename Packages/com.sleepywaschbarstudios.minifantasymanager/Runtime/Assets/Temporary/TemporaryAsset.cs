#nullable enable

using System;
using System.Collections.Generic;
using UnityEngine;

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
        public TemporaryCharacterDetails(string name)
        {
            Name = name;
        }

        public string Name { get; set; }
        public TemporaryAnimationDetails Details { get; set; } = new();
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

    public abstract class TemporaryAsset
    {
        public string Name { get; set; }

        protected TemporaryAsset(string name)
        {
            Name = name;
        }
    }

    public class TextAsset : TemporaryAsset
    {

        public string Contents { get; set; }

        public TextAsset(string name, string contents) : base(name)
        {
            Contents = contents;
        }
    }

    public class ImageAsset : TemporaryAsset
    {
        public ImageAsset(string name, Texture2D texture) : base(name)
        {
            Texture = texture;
        }

        public Texture2D Texture { get; set; }
    }
}
