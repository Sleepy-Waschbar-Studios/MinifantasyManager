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
