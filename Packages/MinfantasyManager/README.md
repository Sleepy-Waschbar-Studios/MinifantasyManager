# Template README

Lorem ipsum description.

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
    "com.SleepyWaschbarStudios.MiniCharacterGen": "https://github.com/Zigurous/Template.git",
    ...
  },
}
```
To select a particular version of the package, add the suffix `#{git-tag}`.

* e.g. `"com.SleepyWaschbarStudios.MiniCharacterGen": "https://github.com/Zigurous/Template.git#1.2.0"`


## Acknowledgements

This package was heavily inspired by Vincent Douchin's great [MiniCharacterCreator package](https://github.com/VincentDouchin/MiniCharacterCreator?tab=readme-ov-file)

This package's layout was copied from [Shane Celis](https://twitter.com/shanecelis)'s [unity-package-template](https://github.com/shanecelis/unity-package-template) since sadly the auto generator didn't work.
