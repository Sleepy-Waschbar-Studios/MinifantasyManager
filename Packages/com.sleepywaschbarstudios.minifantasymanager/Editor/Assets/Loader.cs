﻿#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;
using MinifantasyManager.Editor.Assets.Handler;
using MinifantasyManager.Runtime.Assets;
using MinifantasyManager.Runtime.Assets.Temporary;
using MinifantasyManager.Runtime.Extensions;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

namespace MinifantasyManager.Editor.Assets
{
    public class TemporaryLoadedDetails
    {
        public Dictionary<string, TemporaryWeaponClassificationDetails> Weapons = new(StringComparer.InvariantCultureIgnoreCase);
    }

    public static class Loader
    {
        [MenuItem("Assets/Minifantasy/Load Package", false, 1)]
        private static void LoadPackage()
        {
            var path = EditorUtility.OpenFilePanelWithFilters("Choose a package!", "", new string[] { "Zip Files", "zip" });
            LoadPackage(path);
        }

        private const string PackagePath = "MinifantasyManager/Packages.json";
        private static readonly IHandler IgnoreHandler = new IgnoreHandler();
        private static readonly IReadOnlyDictionary<string, IHandler> ExtensionHandlers = new Dictionary<string, IHandler>(StringComparer.InvariantCultureIgnoreCase) {
            [".png"] = new PngHandler(),
            [".txt"] = new TextHandler(),
            [".url"] = IgnoreHandler,
            [".gif"] = IgnoreHandler,
            [".asperite"] = IgnoreHandler,
            [""] = IgnoreHandler,
        };

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

        public static void LoadPackage(string path)
        {
            if (!File.Exists(path) && Path.GetExtension(path).ToLowerInvariant() == ".zip") {
                throw new ArgumentException("Path should exist and should be a zip file.", nameof(path));
            }

            var currentMetadata = LoadMetadata();

            var filename = Path.GetFileName(path);
            using var unzip = ZipFile.OpenRead(path);

            var match = PatreonAllExclusivesRegex.Match(filename);
            if (match.Success) {
                // Specialised parsing for patreon archives
                var date = match.Groups["date"].Value;
                // TODO:
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

                // From this point we can treat them the safe, we'll just treat an empty package as having a previous version
                // of null & no assets previously (so no merge issues).

                // Because we often want to query to see what other files exist (such as to match shadows) we will do a pre-parse step here
                // We also perform canonicalisation here and some early construction
                var details = new TemporaryLoadedDetails();
                // Scan for CommercialLicense.txt so we know the path that we can skip
                var skipCounter = Path.GetDirectoryName(unzip.Entries.First(e => e.Name.Equals("CommercialLicense.txt")).FullName)
                    .Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                    .Length;

                foreach (var entry in unzip.Entries)
                {
                    // Skip dirs
                    if (entry.IsDirectory()) continue;

                    var entryPath = entry.FullName;
                    var entryFilename = entry.Name;
                    var filenameNoExt = Path.GetFileNameWithoutExtension(entryPath);
                    var segments = entry.FullName.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)[skipCounter..];

                    // Safe files to skip
                    if (entryFilename.Equals("CommercialLicense.txt", StringComparison.InvariantCultureIgnoreCase)
                    || entryFilename.Equals("Acknowledgment.txt", StringComparison.InvariantCultureIgnoreCase))
                    {
                        continue;
                    }

                    using var stream = entry.Open();
                    // Use extension to determine handler
                    var extension = Path.GetExtension(entryPath);
                    if (!ExtensionHandlers.TryGetValue(extension, out var handler))
                    {
                        Debug.LogError($"Found an extension we couldn't handle {extension}, ignoring.");
                        continue;
                    }

                    var asset = handler.HandleFile(entryPath, stream);
                    if (asset == null) continue;
                    WeaponLoader.HandleWeapon(details, entryPath, entryFilename, filenameNoExt, extension, segments, asset);

                    // // Shadow mapping
                    // if (filenameNoExt.Equals("Shadows", StringComparison.InvariantCultureIgnoreCase)) {

                    // } else if (filenameNoExt.EndsWith("Shadows", StringComparison.InvariantCultureIgnoreCase)) {

                    // } else if (Path.GetDirectoryName(entryPath).EndsWith("_Shadows", StringComparison.InvariantCultureIgnoreCase)) {

                    // }
                }

                // Let's begin 
            }
        }

        
    }
}