# KSPNext GameData Disabler

A mod for Kerbal Space Program that allows you to prevent files in GameData from loading.
It works by renaming the files in the internal game database when the game launches and won't change
any of the actual names on your filesystem.

This was heavily inspired by Restocks
[asset black lister](https://github.com/PorktoberRevolution/ReStocked/blob/master/Source/Restock/ResourceBlacklist.cs)
and the disabling mechanism is actually the same.

## Usage

Create a file with the file extension *.kspn_gdd* for example *something.kspn_gdd*.

Every line in this file can either be
 - empty
 - start with a `#` sign to signify a comment (comments can only be on their own line)
 - `[disable]` to signify that the following lines contain patterns that disable files
 - `[enable]` to signify that the following lines contain patterns that explicitly enable files
 - a pattern that matches a file or multiple files (globs are supported)

### Example

```
# by default patterns are being disabled even with
# no [disable] at the start of the file
Squad/Parts/Aero/basicFin/

# these files are explicitly enabled which means
# that they will not be disabled even if they are
# in a [disable] section somewhere else
[enable]
Squad/Parts/Aero/basicFin/

[disable]
Squad/Parts/Aero/airbrake/
# globs are supported
Squad/Parts/Engine/**/*
```

## Building

Copy *KSP.props.example* into *KSP.props* and input the root directory of your KSP install.
Then build as usual.
