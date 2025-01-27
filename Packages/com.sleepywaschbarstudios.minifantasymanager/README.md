# Minifantasy Manager

An asset aiming to manage to importing of minifantasy packages.

It does the following:
- Maintain a convenient package.json file that is easy to search through programatically
- Allow instantiation of creatures and importantly layering sprites easily ontop of each other
  - Does support a simplistic pixel matching system to do both palettes and outfits
- Supports a ton of other useful unity assets such as:
  - Tilemaps (with animations)
  - Character animations
  - UI sprites

It does **not** aim to do the following:
- Match all future packages, in particular it presumes that the current format for packages is standardised, and if new "types" of assets are added it is likely it will fail to parse those.
- It also relies heavily on convention of naming so if this changes it will require changes to adjust.

While I will likely keep this relatively up to date with new packages for the meantime, it's open source for a reason, so feel free to open PRs with fixes and improvements.

## Outputs

> Note: if you are loading in patreon assets please just use the exclusive all-in-one archive rather than the separate patreon files, we do special loading for it and it'll also include any fixes made to the other assets since you last imported it.

This project loads in any minifantasy asset pack (.zip only) and produces the following outputs.
1. `MinifantasyManager/Packages.json`
   - This (which can be referenced at runtime) will list all the assets loaded as well as a flattened list of all the creatures/weapons/tilesets/...
2. `MinifantasyManager/Assets/<Type>/<Name>/...` will list all the flattened assets of that type & name.
   - Highly recommended that you **don't** directly reference these in your codebase, instead please use the `Ref` types below.  These assets *could* have name change updates in the future.
3. `MinifantasyManager/Ref/<Type>/<Name>.prefab` will contain a singular asset for each creature/weapon/... this is your "best" tool for this manager and is what your code should ideally use/reference.
   - We normalise names so **ideally** names won't change when you re-import files, but there could be edgecases where this occurs (such as typos) but this won't break any references you have to these assets because of below.
   - `Packages.json` has a `remappings` section that will allow you to reference assets by their old names, however this will **only** work for `Refs` and won't work for flattened assets so is why you should use these.  If you load in an asset where this has occurred it'll prompt you for the remapping (if you close this window or make a mistake you can just edit the package.json).

The assets inside of `MinifantasyManager/Assets/...` aren't meant to be touched by you after loading, this is because they will be replaced/updated on next load.  If you wish to make a modification to an asset you can write an override script to handle this.

The ref assets contain a bunch of useful scripts that you can use (and you can very easily extend them by adding new scripts) but the key parts are:
- `<Type>Ref` i.e. `CreatureRef.cs` contains all the information about that asset, this includes all it's animation info, it's name and category, and so on.
- `<Type>Animator` i.e. `CreatureAnimator.cs` will animate a sprite attached

You can do `CreatureRef.LoadAsync("MyCreature")` to load in a creature (there are also sync variants).

> **Please** don't reference assets directly, use the prefabs or the code path!!  A really good example is that we don't have any sprite atlases packing being built for now, but we will 100% be doing this at some point in the future for performance reasons.  This will result in assets being changed.

### Palette Swapping

We also support palette swapping, the tricky part of this is two fold;
1. Modifying the sprites to be palette swappable
2. Easy palettes to work with

Conceptually, this isn't *too* difficult, we could do something like this;
- Supply a large palette (64/256 colors) and generate a color map between the "original" style by choosing the closest color
  - This obviously relies on a good algorithm to choose a close color, but ideally you have enough colors that there isn't a "missing color"
- This color map would be implemented in shaders (for performance) and would work by having all sprites be changed to be an indexed sprite instead, for example we could store the color in the R component (if there are 256 colors this works!) then it just becomes a lookup on the map using the R value.
  - While this does require modifying every sprite it's not as crazy as it looks since we already have this process in-place.

Since this is trying to **match** colors it works pretty well but it gets more tricky if we want to have color variations of units!  For example maybe wanting a blue or green dragon!
- Some colors like in the "dmg" animation can't be changed since they are red for a reason
- Requires a custom material for each color and we have to generate a color map of red -> blue or green for example, this gets especially tricky when we have shading you need darker variants of the green/blue and need to select those correctly.

My current thinking is as follows (for above):
1. Generate the "master" palette, the idea of this palette is that it contains every single color ("sorted")
   1. This lets me understand how big of a palette I should have, I likely will require quite a large palette but with normal mapping the shading might not be as necessary?
2. Colors will make up the following structure `R/G = X/Y` and `B` is flags (such as "is outline" for stuff like "dmg effects"?), the alpha is not replaced and is kept when applying palette
3. Support a 256x256 color palette, this ideally will be large enough to support the "master" palette but we can also generate a color map that supports a smaller set of colors.
4. Support a "color" mask that can be applied to specific items, the color mask would apply prior to the palette swap and would swap specific indexed cells
   1. We could implement this through having a texture the size of a palette that just maps each color X -> X, with Y -> Z.  This is quite memory intensive though
   2. But we could also implement it like this `lerp(To, In, saturate(distance(From, In) / e-f))`, any value > 0 should become extremely large and a value < 0 becomes extremely small, and 0 -> 0, meaning saturation becomes either 0 or 1.

### Image Outputs (& Shadows)

All assets come with sprite shadows as a separate asset but we also support generating normals for the use in dynamic shadowing.

When importing assets we automatically generate a normal texture of the asset and place it as a secondary texture.  This is done through unity's greyscale -> normal map functionality.  This adds some nice detail bumps and helps the textures standout.



### CustomCreature.prefab (and it's scripts)

This is an (experimental) object that is intended to allow you to create completely custom creatures based upon layering sprites, the idea is that it'll offset the sprite layers so everything lines up even during animations.

This is done procedurally and will never be perfect and will always be experimental.  My worry is changing the way the algorithm works that ends up breaking some already created custom creatures... for this reason when I make changes that result in any behavioural changes I'll try to put them behind a version flag.

This means that if you are programatically instantiating the custom creatures you might want to pin the version and only update it after doing your own testing (if you are manually creating them from the prefab in scenes then they will automatically be pinned to the version from when you created it).  We have a pretty large set of snapshot tests that do pixel %s, but there are lots of frames of animations and I'm not going to manually verify every single one.

> If you want to disable this (might be useful during development just to say "stay up to latest") then you can just set `CustomCreatureVersion.DisableVersioning()` though keep in mind this is purely a runtime override so if you wanted to remove this later (and not have to manually update every pinned version) you would need to set `CustomCreatureVersion.SetOverrideVersion(X)`.

### Loading multiple assets efficiently

Assets by default will load lazily, but this may result in pop-in on first use.  What you can do is await on `RefLoader.WaitForAllToLoadAsync()` (recommend you use `UniTask` or something similar).  This will continually wait while we are still loading in refs, there is an optional timeout to set (though it doesn't throw an exception just return `false`).

Keep in mind if there haven't been any assets added to the scene yet this won't work, so it's recommended you wait for the scene to load first (using LoadSceneAsync).

The other option is called "Asset Groups", and you can create one through (menubar) "Asset -> Minifantasy -> Manage Asset Groups", these are stored inside of the `Packages.json` and each group just stores a list of assets.  The value of this is that you can ask the system to pre-load an entire asset group.

## Overriding a loaded asset

If you wish to override some property of a loaded asset this can be done very easily.

TODO:

## Installation

### Requirements

* TODO:

```
$ git clone https://github.com/Zigurous/Template.git
```

### Usage

Find the `manifest.json` file in the `Packages` directory in your project and edit it as follows:
```
{
  "dependencies": {
    "com.SleepyWaschbarStudios.MinifantasyManager": "https://github.com/Zigurous/Template.git",
    ...
  },
}
```
To select a particular version of the package, add the suffix `#{git-tag}`.

* e.g. `"com.SleepyWaschbarStudios.MinifantasyManager": "https://github.com/Zigurous/Template.git#1.2.0"`


## Acknowledgements

This package was heavily inspired by Vincent Douchin's great [MiniCharacterCreator package](https://github.com/VincentDouchin/MiniCharacterCreator?tab=readme-ov-file)

This package's layout was copied from [Shane Celis](https://twitter.com/shanecelis)'s [unity-package-template](https://github.com/shanecelis/unity-package-template) since sadly the auto generator didn't work.

## Dependencies

- We use Newtonsoft.Json just as a personal preference over the System.Json
- We use SharpLibZip because it supports parallel zip entry access, this is significantly faster than using the default one and enables us to load the patreon exclusive file in just 1s on my computer.

## Overall Design of Codebase

Just putting some thoughts down here for documentation, but might be useful for someone trying to hack on this to extend it in some way.

The rough idea is that we want to iterate through the directory tree using it as context for what item is what.  For example `Mimic/_Shadows/Attack_Shadow.png` is relatively obvious.  There are some parts here that make this significantly more tricky which I'm going to try to fully cover below.
- Finding the "creature" / "weapon" name.  This sounds relatively obvious (oh just the parent name) but in cases with nested folders like `_Shadows` it's hard.
- Categories... in some cases weapons are categorised for example `Two-Handed/LongSword/Attack.png`, this is tricky because some of the sections of the frames might (and often are) shared between weapons so only exist in the `Two-Handed` path.  This means that it's conceptually quite hard to figure out if a given folder is actually our "leaf" or not.
- Inconsistencies in structure.  Sometimes it is `Mimic/Attack_Shadow.png` sometimes it's `_Shadow`, this is not including the filenames themselves which are wildly inconsistent sometimes.
  - This is completely expected, this is a project done by one person and typos will always happen without validation systems in-place.  Since this project identifies inconsistencies, this **might** make it simpler long term.

This means that we do the following process.
1. Iterate through the zip and build a tree, we skip commonly files like Acknowledgement and load files into "TemporaryAssets" which include some basic flags like:
   - Shadow file?  Or Background/Foreground?
   - Animation?
2. Then we iterate through the tree and try to figure out what assets exist based on existing conventions.
3. We have specific loaders that then load the above assets for example "character" or "weapon" or "tileset"

> We *could* unzip when we make the tree but this isn't actually faster, for one we have to unzip files that we don't want to read (mockups/gifs/...), for two some of the zips have very long filenames (patreon exclusive) that actually break a default windows system (ouch), and I don't want to have end users to have to handle that.  The benefit would be that we could skip iteration of entire folders at once but since to extract them we have to read their contents it sort of defeats the point.

Imperfections exist in this entire process but the goal is to have the loader be 95% then to handle the specific edgecases manually either in the code (or by the user in rare cases).  For example, we handle typos by having a process that renames assets as they get loaded to the right name (and we report the typos so they can get fixed up).

Because of this it may mean that "new" assets will fail to load but I'll ideally load them and fix up any issues in the codebase pretty swiftly (since I'll probably want to use them), I however don't promise this and this project may eventually become "archived" or at-least not maintained, but the code should be pretty approachable (at-least the "fixup" code).
