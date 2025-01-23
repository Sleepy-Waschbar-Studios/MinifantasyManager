#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using MinifantasyManager.Editor.Assets.Handler;
using MinifantasyManager.Runtime.Assets;
using MinifantasyManager.Runtime.Assets.Temporary;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;
using Unity.SharpZipLib.Zip;

namespace MinifantasyManager.Editor.Assets.Loaders
{
    public class TemporaryLoadedDetails
    {
        public Dictionary<string, TemporaryWeaponClassificationDetails> Weapons = new(StringComparer.OrdinalIgnoreCase);
        /// <summary>
        /// This is free for your loader to use (though ideally try to add a unique suffix for your loader).
        /// 
        /// is useful if you want to make some processing dependent on finding certain assets later.
        /// </summary>
        public Dictionary<string, List<TemporaryAsset>> UnprocessedAssets = new(StringComparer.OrdinalIgnoreCase);
        public Dictionary<string, TemporaryCharacterDetails> CreatureAsset = new(StringComparer.OrdinalIgnoreCase);

        public HashSet<string> ProcessedAssets = new(StringComparer.OrdinalIgnoreCase);
    }

    public class LoaderWindow : EditorWindow
    {
        [MenuItem("Assets/Minifantasy/Load Package", false, 1)]
        private static void LoadPackage()
        {
            var path = EditorUtility.OpenFilePanelWithFilters("Choose a package!", "", new string[] { "Zip Files", "zip" });
            var window = GetWindow<LoaderWindow>();
            try
            {
                PackageLoader.LoadPackage(path, true);
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }
    }

    public static class PackageLoader
    {
        private const string PackagePath = "MinifantasyManager/Packages.json";
        private static readonly IHandler IgnoreHandler = new IgnoreHandler();
        public static readonly IReadOnlyDictionary<string, IHandler> ExtensionHandlers = new Dictionary<string, IHandler>(StringComparer.OrdinalIgnoreCase) {
            [".png"] = new PngHandler(),
            [".txt"] = new TextHandler(),
            [".url"] = IgnoreHandler,
            [".gif"] = IgnoreHandler,
            [".asperite"] = IgnoreHandler,
            [".aseprite"] = IgnoreHandler,
            [""] = IgnoreHandler,
        };

        public static readonly HashSet<string> FilesToSkip = new(StringComparer.OrdinalIgnoreCase)
        {
            "Acknowledgment.txt",
            "CommercialLicense.txt"
        };

        public static readonly List<AssetLoaderBase> Loaders = typeof(AssetLoaderBase).Assembly.GetTypes()
            .Where(t => t.IsSubclassOf(typeof(AssetLoaderBase))).Select(t => (AssetLoaderBase)Activator.CreateInstance(t)).ToList();

        private static T EnsureJsonAssetExists<T>(string path) where T : new()
        {
            if (!AssetDatabase.AssetPathExists(path)) {
                var metadata = new T();
                Directory.CreateDirectory(Path.Combine(Application.dataPath, Directory.GetParent(path).FullName));
                File.WriteAllText(Path.Combine(Application.dataPath, path), JsonConvert.SerializeObject(metadata));
                AssetDatabase.ImportAsset(Path.Combine("Assets", path));
                return metadata;
            }

            var asset = AssetDatabase.LoadAssetAtPath<UnityEngine.TextAsset>(path);
            return JsonConvert.DeserializeObject<T>(asset.text)!;
        }

        private static ManagerMetadata LoadMetadata()
        {
            return EnsureJsonAssetExists<ManagerMetadata>(PackagePath);
        }

        public static readonly Regex PatreonAllExclusivesRegex = new(@"All_Exclusives_(?<date>.*?).zip", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static void LoadPackage(string path, bool showProgress)
        {
            if (!File.Exists(path) && Path.GetExtension(path).ToLowerInvariant() == ".zip") {
                throw new ArgumentException("Path should exist and should be a zip file.", nameof(path));
            }

            var currentMetadata = LoadMetadata();

            var filename = Path.GetFileName(path);
            using var tree = RootFileTree.OpenFromFile(path, showProgress);
            if (tree == null) return;

            var details = new TemporaryLoadedDetails();

            var classifier = new AssetLoader();
            var assets = classifier.LoadAssetsFromTree(tree);

            foreach (var file in tree.AllLoadedFiles)
            {
                Debug.LogErrorFormat("File {0} has not been processed", file);
            }

            var match = PatreonAllExclusivesRegex.Match(filename);
            if (match.Success) {
                // Specialised parsing for patreon archives
                var date = match.Groups["date"].Value;
                // TODO:
                Debug.LogError("TODO: Implement patreon loading");
            } else {
                // Normal parsing
                match = Package.PackageNameAndVersionRegex.Match(filename);
                if (!match.Success) {
                    throw new Exception("Filename didn't match expected formats.  You might need to rename it.");
                }
                var name = match.Groups["name"].Value;
                var version = match.Groups["version"].Value;
                // For now the suffix is unused
                _ = match.Groups["suffix"].Value;
                
                // Let's try to find a matching package first
                if (currentMetadata.Packages.TryGetValue(name, out var pkg)) {
                    if (pkg.Version == version) {
                        bool force_load = EditorUtility.DisplayDialog("Force load package?", $"We already have loaded {filename} with version {version}, are you sure you want to force load it?", "Force load", "Cancel");
                        if (!force_load) {
                            return;
                        }
                        // Else we are just going to pretend it's a "new version"
                    }
                    pkg.Version = version;
                } else {
                    pkg = new Package {
                        Name = name,
                        Version = version,
                        PackageId = Path.GetFileNameWithoutExtension(path),
                    };
                }

                /*
                    if (!Loaders.Any(loader => loader.TryLoad(details, currentMetadata, asset)))
                    {
                        unprocessedAssets.Add(asset.FullPath);
                    }
                 */

                //// Now we can verify that all unprocessed assets have been processed.
                //foreach (var asset in unprocessedAssets.Except(details.ProcessedAssets, StringComparer.OrdinalIgnoreCase))
                //{
                //    Debug.LogWarning($"Failed to process asset {asset} no handler was registered that could handle it.");
                //}
            }
        }
    }
}
