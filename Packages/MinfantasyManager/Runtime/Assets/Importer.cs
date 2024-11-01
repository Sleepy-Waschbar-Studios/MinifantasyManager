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
            - Copy CommercialLicense to `Licenses/Minifantasy/<File>_CommercialLicense.txt
              - You may want to just have one license in here
            - Writes to `MinifantasyManager/Packages.json` with the following structure
                {
                    "Package": "Minifantasy_Magic_Weapons_And_Effects",
                    "Version": "v1.0",
                    "LastImported": <Date>,
                    "Name": "Magic weapons and effects",
                    "Creatures": [
                        ... List of creatures
                    ],
                    "Weapons": [
                        ... List of weapons
                    ],
                    ... and so on for each category
                }
            - Writes to `MinifantasyManager/...` but **won't** match original folder structure it
              instead will match the structure `MinifantasyManager/<Type>/<Package> i.e.
              MinifantasyManager/Creatures/Ancient_Danger/Attack_Range_Diagonal.png
              - Will typically cleanup names such as removing prefix `_` and cleanup repetititons for example above
                was originally _Attack_range_Diagonal_ancient_danger.png

             */
        }
    }
}
