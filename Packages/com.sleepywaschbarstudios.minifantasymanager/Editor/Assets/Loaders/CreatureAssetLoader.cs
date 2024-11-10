#nullable enable
using MinifantasyManager.Runtime.Assets;
using MinifantasyManager.Runtime.Assets.Temporary;
using UnityEngine;

namespace MinifantasyManager.Editor.Assets.Loaders
{
    /// <summary>
    /// The weapons pack (and friends) require tons of custom logic, so I'm putting it in here.
    /// </summary>
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

            // We don't want to grab shadow animation idle
            if (asset.Filename.EndsWith("Idle.png") && !asset.Flags.HasFlag(AssetFlags.ShadowAnimation))
            {
                // This will tell us the prefix that all future sprites will have
                var prefix = asset.Filename[..^"Idle.png".Length];
                var creature = new TemporaryCharacterDetails(possibleCreatureName, prefix);
                details.CreatureAsset.Add(possibleCreatureName, creature);
                creature.Details["Idle"] = new TemporaryAnimationDetails
                {
                    ForegroundAnimation = (ImageAsset)asset,
                };
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
            }
            else if (details.CreatureAsset.TryGetValue(possibleCreatureName, out var creature))
            {
                
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