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

Imperfections exist in this entire process but the goal is to have the loader be 95% then to handle the specific edgecases manually either in the code (or by the user in rare cases).  For example, we handle typos by having a process that renames assets as they get loaded to the right name (and we report the typos so they can get fixed up).

Because of this it may mean that "new" assets will fail to load but I'll ideally load them and fix up any issues in the codebase pretty swiftly (since I'll probably want to use them), I however don't promise this and this project may eventually become "archived" or at-least not maintained, but the code should be pretty approachable (at-least the "fixup" code).
