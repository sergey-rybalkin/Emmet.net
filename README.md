# Emmet.net - Emmet for Visual Studio

This project is a MS Visual Studio port of the emmet editor extensions by Sergey Chikuyonok. More information available on [Emmet official website](http://docs.emmet.io/).

## Build

As emmet is originally created using JavaScript and Visual Studio cannot run it natively, so this project is using [V8.NET JavaScript engine](https://v8dotnet.codeplex.com/) to execute Emmet code. V8.NET libraries are included with source code and other dependencies are managed with NuGet.

Visual Studio 2015 and Visual Studio SDK are required to build the project.

## Ported actions

Below is the list of actions that are currently implemented in this extension. The rest of them are either natively supported by Visual Studio or exist in either ReSharper or WebEssentials extensions and therefore were not included.

1. Expand Abbreviation (Ctrl+Shift+Alt+X) - http://docs.emmet.io/actions/expand-abbreviation/
2. Wrap with Abbreviation (Shift+Alt+W) - http://docs.emmet.io/actions/wrap-with-abbreviation/
3. Toggle Comment (Ctrl+Alt+Num /) - http://docs.emmet.io/actions/toggle-comment/
4. Merge Lines (Ctrl+Alt+Shift+M) - http://docs.emmet.io/actions/merge-lines/

## Binaries

Precompiled version can be downloaded here - https://www.dropbox.com/s/03rl7qecwf5b7k7/Emmet.vsix?dl=0

### Version history

* v3.0 - Version for Visual Studio 2015 based on Emmet v1.3.1 and V8.NET v1.5.19.36. Almost complete rewrite in pure C#, added custom section to Visual Studio configuration options, support for custom preferences and extensions.
* v2.2.0 - Upgrade to Emmet engine v1.1.
* v2.1.3 - Added support for custom snippets.js file, should be located at the predefined path %APPDATA%\Emmet\snippets.js.
* v2.1.2 - Added CSS abbreviations for flex box model, removed XSL abbreviations.
* v2.1.1 - Added support for SCSS files
* v2.1 - Bugfixing release
* v2.0 - Visual Studio 2013 migration completed. Major changes:
    * In HTML documents default abbreviation expansion on TAB now relies on WebEssentials as they seem to have implemented all of the functionality, Emmet abbreviations expansion still available using shortcut Ctrl+Shift+Alt+X.
    * RemoveTag action removed as ReSharper now supports it.
    * UpdateImageSize action removed as Visual Studio now supports it.
    * Upgraded V8 to version 3.22.15
* v1.3 - New Emmet engine version, fixed compatibility issues for Visual Studio Web Tools update 2012.2
* v1.2 - Now using custom emmet build without actions that are irrelevant for visual studio, fixed tab stops bug when placeholders where not removed.
* v1.1 - Minor performance improvements and bug fixes.
* v1.0 - Initial release.