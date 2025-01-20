#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using MinifantasyManager.Runtime.Assets;
using MinifantasyManager.Runtime.Assets.Temporary;
using SleepyWaschbarStudios.MinifantasyManager;
using UnityEngine;

namespace MinifantasyManager.Editor.Assets.Loaders
{
    public class WeaponAssetLoader : AssetLoaderBase
    {
        public override bool TryLoad(TemporaryLoadedDetails details, ManagerMetadata currentMetadata, TemporaryAsset asset)
        {
            // Detect if it's a likely weapon
            if (asset.FullPath.Contains("Weapon", StringComparison.InvariantCultureIgnoreCase) || asset.FullPath.Contains("Attack", StringComparison.InvariantCultureIgnoreCase))
            {
                // This is semi-hard to find because weapons can be called anything, but this is reasonable enough
                // Weapons are hard to classify because it's quite inconsistent for example
                // Thrust_Attacks/Pitchfork and Charged_Attacks/Thrust/Pitchfork
                // So we "unify" as best as we can.  We do this by duplicating the animations across them all.
                // TODO: We need specialised cases for stuff like the plasma weapons in space derelict

                // Step 1. Find the classification of this animation
                var classification = asset.Segments[0];
                if (!details.Weapons.TryGetValue(classification, out var classificationDetails))
                {
                    details.Weapons[classification] = classificationDetails = new TemporaryWeaponClassificationDetails(classification);
                }

                if (asset.Segments.Length > 1)
                {
                    // We may either be a list of character animations for this "category" of weapons
                    // or we might be the actual weapon
                    if (asset.Segments[1].Equals("_Characters", StringComparison.InvariantCultureIgnoreCase))
                    {
                        // Figure out what characters this sprite belongs to
                        // They may be grouped i.e. _human_elf_orc...
                        // There are some relatively annoying inconsistencies here in particular sometimes they are surrounding in `(...)`
                        // and suffixes aren't super standardised.  So we have instead a list of 
                        var characters = asset.FilenameNoExt
                            .Split('_')
                            .Skip(1)
                            .Where(suffix =>
                                !suffix.Equals("_Characters", StringComparison.InvariantCultureIgnoreCase) &&
                                !suffix.Equals("_Shadows", StringComparison.InvariantCultureIgnoreCase));
                        foreach (var character in characters) {
                        }

                        // List of character animations
                        if (asset.Segments[2].Equals("_Shadows")) {
                            // We are a shadow sprite

                        }
                    }
                }

                // Inconsistency 1: AnimationInfo is sometimes Animation_Info.txt or _AnimationInfo.txt or AnimationInfo.txt
                if (asset.Filename.Replace("_", "").Equals("AnimationInfo.txt", StringComparison.InvariantCultureIgnoreCase)) {
                    // This animation needs to apply for the entire tree from this point
                    // so we have to figure out where we are
                    switch (asset.Segments.Length)
                    {
                        case 1:
                            {
                                // classification level
                                classificationDetails.AnimationFile = asset as Runtime.Assets.Temporary.TextAsset;
                                break;
                            }
                        case 2:
                            {
                                // weapon specific level
                                break;
                            }
                    }
                } else if (asset.Filename.EndsWith(".png", StringComparison.InvariantCultureIgnoreCase)) {
                    DebugExtensions.VerboseLog($"{asset.FullPath} classified as a weapon sprite with classification {classification} .");
                } else {
                    Debug.LogError($"Unexpected file expected either AnimationInfo or a png file but got {asset.FullPath} ignoring");
                    return false;
                }
            }

            return false;
        }
    }
}