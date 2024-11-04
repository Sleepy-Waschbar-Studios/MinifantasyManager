# Package Formats

The author Krishna Palacio does an absolutely fantastic job at keeping the assets consistent and **typically** most packages have consistent naming schemes.  There are (sadly) a few cases where this isn't true so this document outlines all the possible formats.

To begin, we load packages in from the raw zip file which typically gives us pretty consistent folder paths but we have built it to be generic.

## Step 0. Package Name

We get the package name & version from the zip file name.  We support quite a few formats but the primary formats are;

- `{PackageName}_v{Version}_{Suffix}`
    - Suffix typically refers to something like `Commercial_Version`
- `All_Exclusives_{Date}`
    - This is supported but has a custom parser
    - The Date is used as the version

## Step 1. Find first folder

We might have 1 or 2 layers of folders until we find the actual top layer.  We ignore these first few empty folders.

## Step 2. Top layer

This will contain the following files:
- CommercialLicense.txt (**most**)
    - This is ignored, we build a license file from all packages loaded since they all contain the same license.
- Patreon_Minifantasy.url
    - Ignored
- {Package} this is the top layer

## Step 3. Separate into categories

**Most** packages will separate their assets up into our various categories such as Characters/Props/Tilesets.

The following files we always ignore;
- Mockup (both individual file & folder)
- Gifs (both folder & .gif)

## Step 4. Parsing Shadows & Foreground/Background

There are a few ways shadows are added with the two primary ways being:
- A `_Shadows` folder with matching names
- A `{File}_Shadows.png` file matching the file name
- A `Shadows.png` file with only one other png in that directory

The same applies for Foreground/Background.  We also support the following other suffixes:
- `_Light`
- `_Impact` & `_Projectile`
- `_Diagonal` & `_Orthogonal`

## Step 5. Parsing Animations

This is semi-tricky, but we generate animation information by parsing the `AnimationInfo.txt` file (there are a few acceptable filenames).

While the file is a txt file it's very consistent in format and typically we can very cleanly parse it.

## Unknown Files / Mismatched formats

This occurs sometimes, where we have cases where we just can't figure out the format.
