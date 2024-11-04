#nullable enable
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
using SleepyWaschbarStudios.MinifantasyManager;
using UnityEditor;
using UnityEngine;

namespace MinifantasyManager.Editor.Assets
{
    /// <summary>
    /// The weapons pack (and friends) require tons of custom logic, so I'm putting it in here.
    /// </summary>
    public static partial class Loader
    {
        public static bool HandleCreature(TemporaryLoadedDetails details, ManagerMetadata currentMetadata, string entryPath, string entryFilename, string filenameNoExt, string extension, string[] segments, TemporaryAsset? asset)
        {
            // Creatures we can only process once we see a certain animation, for now Idle.png is the most reliable.
            // We have to reprocess basically all sprites that were previously processed for this creature, so the first step is to identify it's name
            // We should try to find the "name" of this creature prior to idle since we might already have matched idle.

            // This is not as complicated as Weapon in terms of edge cases but is much more annoying to find a nice creature name.
            // Some files use `_` to separate sprite names which make finding names easy but some don't and more annoying some only use **some** of the words
            // of the parent folders for example the path "Minifantasy_Creatures_Assets\Base_Humanoids\Dwarf\Dwarf_Yellow_Beard\YellowBeardIdle.png"
            // (not to speak of the misspellings such as YellowBearDmg.png).  We don't handle misspellings, we instead just handle that in an "exception" class that
            // will run over each sprite and update names prior to this.

            if (entryFilename.EndsWith("Idle.png"))
            {

            }

            return false;
        }
    }
}