# Version history

* v5.1 - Updated to Visual Studio 2022, switched to V8 engine via ClearScript, updated Emmet engine to version 2.3.5.
* v4.2 - Updated ChakraCore to v1.11.15, Emmet engine to version 2.0.0-rc.10. Removed deprecated commands, improved JSX/TSX support.
* v4.1 - Added experimental 'Sort CSS properties' command.
* v4.0 - Upgrade to Visual Studio 2019.
* v3.3 - ChakraCore updated to version 1.8.4. Package converted to async and is loaded in background according to new Microsoft requirements.
* v3.2 - Upgraded to Emmet v1.6.3 without CanIUse database. Added nullable types support to C# mnemonics. Replaced V8 engine with ChakraCore for running JavaScript code.
* v3.1 - Upgraded to Emmet v1.6. Added experimental C# mnemonics implementation.
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