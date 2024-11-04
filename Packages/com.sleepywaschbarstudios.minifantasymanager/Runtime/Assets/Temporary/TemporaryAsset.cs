#nullable enable
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using UnityEngine;

namespace MinifantasyManager.Runtime.Assets.Temporary
{
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

    public class TemporaryAnimationDetails
    {
        /// <summary>
        /// This should be shared between multiple classes if possible.
        /// </summary>
        public TextAsset? AnimationFile { get; set; }

        public Dictionary<string, TemporaryAnimationDetails> CharacterAnimations { get; set; } = new(StringComparer.InvariantCultureIgnoreCase);
        public ImageAsset? ForegroundAnimation { get; set; }
        public ImageAsset? BackgroundAnimation { get; set; }
        public ImageAsset? ProjectileAnimation { get; set; }
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
