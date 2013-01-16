# Emmet.net - Emmet for Visual Studio

This project is a MS Visual Studio 2012 port of the emmet editor extensions by Sergey Chikuyonok. More information available on http://docs.emmet.io/

## Build

As emmet is originally created using JavaScript and Visual Studio cannot run it natively this project is using V8 JavaScript engine. In order to build this project you need to place V8 *.lib files (preparser_lib.lib v8_base.lib v8_nosnapshot.lib v8_snapshot.lib) to Emmet/lib/Debug|Release folder. See https://developers.google.com/v8/embed for details.

Visual Studio 2012 is required to build the project.

## Ported actions

Below is the list of actions that are currently implemented in this extension. The rest of them are either natively supported or exist in either ReSharper or WebEssentials extensions and therefore were not included.

1. Expand Abbreviation (TAB or Ctrl+Shift+Alt+X) - http://docs.emmet.io/actions/expand-abbreviation/
2. Wrap with Abbreviation (Ctrl+Alt+W) - http://docs.emmet.io/actions/wrap-with-abbreviation/
3. Remove Tag (Ctrl+Alt+Shift+R) - http://docs.emmet.io/actions/remove-tag/
4. Toggle Comment (Ctrl+Alt+Num /) - http://docs.emmet.io/actions/toggle-comment/
5. Merge Lines (Ctrl+Alt+Shift+M) - http://docs.emmet.io/actions/merge-lines/
6. Update Image Size (Ctrl+Alt+Shift+U) - http://docs.emmet.io/actions/update-image-size/

## Binaries

Precompiled version can be downloaded here - http://dl.dropbox.com/u/38120966/Emmet.vsix
