#nullable enable

using MinifantasyManager.Editor.Assets;
using SleepyWaschbarStudios.MinifantasyManager;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AssetLoader
{
    public IEnumerable<LoadedAsset> LoadAssetsFromTree(RootFileTree tree)
    {
        // 1. Find our actual nice root folder
        var root = tree.Tree ?? throw new NullReferenceException("Expected tree to not be null.");
        while (root != null && root.SubTrees.Count == 1 && root.Files.Count == 0)
        {
            root = root.SubTrees[0];
        }

        if (root == null)
        {
            throw new Exception("Couldn't find the root of the tree.");
        }

        DebugExtensions.VerboseLogFormat("The root of the tree is {0}", root.FullPath);

        // The root will contain a series of known folders like "Props" or "Tileset" or "Characters" ...
        // but the rest will be creatures or weapons or spell effects or something! So we have to identify them from that point.
        return root.SubTrees.SelectMany(folder =>
        {
            if (folder.Name.Equals("Props", StringComparison.OrdinalIgnoreCase))
            {
                return new PropsAssetType().Classify(folder, tree.AllLoadedFiles);
            }
            else if (folder.Name.Equals("Tileset", StringComparison.OrdinalIgnoreCase))
            {
                return new TilesetAssetType().Classify(folder, tree.AllLoadedFiles);
            }
            else if (folder.Name.Equals("Characters", StringComparison.OrdinalIgnoreCase))
            {
                return new CharactersAssetType().Classify(folder, tree.AllLoadedFiles);
            }
            else if (folder.Name.Equals("Animations", StringComparison.OrdinalIgnoreCase))
            {
                // TODO: This seems more like "Creatures"
                Debug.LogWarningFormat("Still designing out how Animations work {0}", folder.FullPath);
            }
            else
            {
                // Search through it's files to figure out what we think it is.
                Debug.LogWarningFormat("Couldn't find what {0} is", folder.Name);
            }

            return Enumerable.Empty<LoadedAsset>();
        }).ToList();
    }
}
