﻿#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinifantasyManager.Runtime.Assets
{
    /// <summary>
    /// Imports the minifantasy files from external.
    /// </summary>
    public class Importer
    {

        public void PerformImport(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
            {
                throw new ArgumentException("Expected it to be a path to a directory that exists", nameof(directoryPath));
            }

            // We support only a few patterns but this should cover all files
            /* Typical structure of a mini fantasy file
               | Some levels of just nested folders (based on what they extract/how packaged it is)
               | Acknowledgement
               | CommercialLicense
               | Core folder (what matters i.e. Minifantasy_Magic_Weapons_And_Effects_v1.0)
               -> GIFs (completely ignored)
               -> _Mockups (completely ignored)
               -> _Premade Scene
               -> *.txt (GeneralInfo, AnimationInfo, ...)
               -> Sub Folders, recurses like as if it was a core folder
               -> Shadow*.png, we separate these out (sometimes they have a Shadows folder sometimes not)
               -> F/B*.png, foreground / background images
            
            We determine the following categories of assets based on common prefixes:
            - Creatures, they must have an Idle animation, or be in a folder called "Creatures"
            - Props, in folder "Props" or contain word "Prop"
            - Tilesets, in folder "Tileset" or contain word "Tileset"
            - Weapons, folder contains the word "Weapon"
            - Effects, 
            - Crafting/Professions (i.e. an addon character animation), 
            - Icons, folder contains the word "Icon"
            -> Else will typically be classified as just a "prop"

            You can pretty easily recategorize by just modifying the JSON

            We do the following:
            - Writes to `MinifantasyManager/Packages.json` with the following structure
                {
                    "Package": "Minifantasy_Magic_Weapons_And_Effects",
                    "Version": "1.0",
                    "LastImported": <Date>,
                    "Name": "Magic weapons and effects",
                    "Assets": {
                        "Creatures": [
                            ...
                        ],
                        "Weapons": [
                            ...
                        ],
                        ...
                    }
                }
            - Writes to `MinifantasyManager/...` but **won't** match original folder structure it
              instead will match the structure `MinifantasyManager/<Type>/<Package> i.e.
              MinifantasyManager/Creatures/Ancient_Danger/Attack_Range_Diagonal.png
              - Will typically cleanup names such as removing prefix `_` and cleanup repetititons for example above
                was originally _Attack_range_Diagonal_ancient_danger.png

            == Design ==
            I've tried a few designs and run into a few interesting issues.

            Originally, I wanted to avoid reproccessing the entire zip and so I processed items as I iterated through the zip
            mainly so that I avoid the "performance" issue of multiple iteration.

            But why?  This makes the code a lot more complex and I'm only seeking over the zip entries multiple times not
            the actual file data.  So this is just illogical.

            So instead we make a tree, we do tag the tree with information based on the path such as if it's a shadow.
             */
        }
    }
}
