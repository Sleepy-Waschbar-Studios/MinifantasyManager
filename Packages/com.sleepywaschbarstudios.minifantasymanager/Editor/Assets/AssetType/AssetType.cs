#nullable enable

using MinifantasyManager.Editor.Assets;
using MinifantasyManager.Runtime.Assets.Temporary;
using MinifantasyManager.Runtime.Extensions;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class AssetType
{
    public abstract IEnumerable<LoadedAsset> Classify(FileTree assetGroup, HashSet<string> file_paths_to_handle);

    protected static T? GetSingleAssetOrError<T>(IEnumerable<T> files, string type)
    {
        if (!files.TrySingleOrDefault(out var tileset))
        {
            if (tileset != null)
            {
                Debug.LogErrorFormat("Multiple files could be {0} i.e. [{1}]", type, string.Join(", ", files.Select(f => f?.ToString())));
            }
            else
            {
                Debug.LogErrorFormat("Failed to find {0}", type);
            }
            return default;
        }

        return tileset;
    }
}

public class TilesetAssetType : AssetType
{
    public override IEnumerable<LoadedAsset> Classify(FileTree assetGroup, HashSet<string> file_paths_to_handle)
    {
        // We want to look for our "tileset"
        var files = assetGroup.Files
            .Where(f => !f.Flags.HasFlag(AssetFlags.Shadow) && f is TemporaryImageAsset)
            .Cast<TemporaryImageAsset>();
        if (GetSingleAssetOrError(files, "tileset") is not TemporaryImageAsset tileset) yield break;
        file_paths_to_handle.Remove(tileset.FullPath);

        // Find shadow
        var shadow = assetGroup.Files
            .Where(f => f.Flags.HasFlag(AssetFlags.Shadow) && f is TemporaryImageAsset)
            .Cast<TemporaryImageAsset>();
        if (GetSingleAssetOrError(shadow, "shadowTileset") is not TemporaryImageAsset shadowTileset) yield break;
        file_paths_to_handle.Remove(shadowTileset.FullPath);

        var tilesetAsset = new TilesetLoadedAsset(tileset, shadowTileset);
        yield return tilesetAsset;
    }
}

public class PropsAssetType : AssetType
{
    public override IEnumerable<LoadedAsset> Classify(FileTree assetGroup, HashSet<string> file_paths_to_handle)
    {
        yield break;
    }
}

public class CharactersAssetType : AssetType
{
    public override IEnumerable<LoadedAsset> Classify(FileTree assetGroup, HashSet<string> file_paths_to_handle)
    {
        yield break;
    }
}
