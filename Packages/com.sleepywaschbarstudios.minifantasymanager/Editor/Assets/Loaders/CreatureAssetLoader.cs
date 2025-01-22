#nullable enable
using MinifantasyManager.Runtime.Assets;
using MinifantasyManager.Runtime.Assets.Temporary;
using MinifantasyManager.Runtime.Extensions;
using System;
using System.Linq;
using UnityEngine;

namespace MinifantasyManager.Editor.Assets.Loaders
{
    public class CreatureAssetLoader : AssetLoaderBase
    {
        public const string UNPROCESSED_ASSET_PREFIX = "$Creature-";

        public override bool TryLoad(TemporaryLoadedDetails details, ManagerMetadata currentMetadata, TemporaryAsset asset)
        {
            // Creatures we can only process once we see a certain animation, for now Idle.png is the most reliable.
            // We have to reprocess basically all sprites that were previously processed for this creature, so the first step is to identify it's name
            // We should try to find the "name" of this creature prior to idle since we might already have matched idle.

            // This is not as complicated as Weapon in terms of edge cases but is much more annoying to find a nice creature name.
            // Some files use `_` to separate sprite names which make finding names easy but some don't and more annoying some only use **some** of the words
            // of the parent folders for example the path "Minifantasy_Creatures_Assets\Base_Humanoids\Dwarf\Dwarf_Yellow_Beard\YellowBeardIdle.png"
            // (not to speak of the misspellings such as YellowBearDmg.png).  We don't handle misspellings, we instead just handle that in an "exception" class that
            // will run over each sprite and update names prior to this.

            // We want to grab the first folder name from the segment
            // this only holds as long as either we have found Idle *or* there is details for this creature
            var possibleCreatureName = asset.Segments[0];
            Debug.LogFormat("Creature {0} has a possible name of {1}", string.Join("..", asset.Segments), possibleCreatureName);

            // We don't want to grab shadow animation idle
            if (asset.Filename.EndsWith("Idle.png", StringComparison.OrdinalIgnoreCase) && !asset.Flags.HasFlag(AssetFlags.ShadowAnimation))
            {
                // This will tell us the prefix that all future sprites will have
                var prefix = asset.Filename[..^"Idle.png".Length].Replace("_", "");
                var creature = new TemporaryCharacterDetails(possibleCreatureName, prefix);
                details.CreatureAsset.Add(possibleCreatureName, creature);
                creature.Details["Idle"] = new TemporaryAnimationDetails("Idle")
                {
                    ForegroundAnimation = (TemporaryImageAsset)asset,
                };
                details.ProcessedAssets.Add(asset.FullPath);
                if (details.UnprocessedAssets.TryGetValue(UNPROCESSED_ASSET_PREFIX + possibleCreatureName, out var unprocessedAssets))
                {
                    // Reprocess subassets
                    unprocessedAssets.RemoveAll((asset) =>
                    {
                        if (!TryLoad(details, currentMetadata, asset))
                        {
                            // We should be successfully loading these assets!!
                            Debug.LogWarning($"Expected for asset {asset.FullPath} to be handled by {nameof(CreatureAssetLoader)} but it wasn't.");
                            return false;
                        }

                        return true;
                    });
                }
                return true;
            }
            else if (details.CreatureAsset.TryGetValue(possibleCreatureName, out var creature))
            {
                if (asset is TemporaryImageAsset imageAsset)
                {
                    // Canonicalisation
                    string animationName = asset.Filename;
                    
                    // 1. Remove _Shadow suffix and Shadow prefix
                    if (asset.Flags.HasFlag(AssetFlags.ShadowAnimation))
                    {
                        // TODO: Maybe use trim?
                        animationName = animationName.ReplaceCaseInsensitive("Shadows", "").ReplaceCaseInsensitive("Shadow", "").Trim('_');
                    }

                    // 2. Foreground/background
                    animationName = animationName.ReplaceCaseInsensitive("_f.", ".").ReplaceCaseInsensitive("_b.", ".");
                    animationName = animationName.Replace("_", "");

                    // Take a prefix
                    if (!animationName.StartsWith(creature.Prefix))
                    {
                        Debug.LogWarning($"Animation {animationName} doesn't start with {creature.Prefix} so ignoring.");
                        return false;
                    }
                    else
                    {
                        animationName = animationName[creature.Prefix.Length..];
                    }
                    animationName = animationName.ReplaceCaseInsensitive(".png", "");

                    if (!creature.Details.TryGetValue(animationName, out var animationDetails))
                    {
                        animationDetails = creature.Details[animationName] = new(animationName);
                    }
                    if (asset.Flags.HasFlag(AssetFlags.ShadowAnimation))
                    {
                        animationDetails.ShadowAnimation = imageAsset;
                    }
                    else if (asset.Flags.HasFlag(AssetFlags.ForegroundAnimation))
                    {
                        animationDetails.ForegroundAnimation = imageAsset;
                    }
                }
                else if (asset.Flags.HasFlag(AssetFlags.AnimationInfo))
                {
                    if (creature.AnimationFile != null)
                    {
                        Debug.LogWarning($"We have two animation info assets, overriding {creature.AnimationFile.FullPath} with {asset.FullPath}");
                    }
                    creature.AnimationFile = (Runtime.Assets.Temporary.TemporaryTextAsset)asset;
                }
                else
                {
                    Debug.LogWarning($"We don't know how to process file {asset.FullPath}");
                    return false;
                }

                details.ProcessedAssets.Add(asset.FullPath);
                return true;
            }
            else
            {
                if (!details.UnprocessedAssets.TryGetValue(UNPROCESSED_ASSET_PREFIX + possibleCreatureName, out var unprocessedAssets))
                {
                    unprocessedAssets = details.UnprocessedAssets[UNPROCESSED_ASSET_PREFIX + possibleCreatureName] = new();
                }
                unprocessedAssets.Add(asset);
            }

            return false;
        }
    }
}