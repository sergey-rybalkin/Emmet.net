﻿using System;
using System.ComponentModel;
using System.IO;
using Microsoft.VisualStudio.Shell;

namespace Emmet
{
    /// <summary>
    /// Contains configuration options for the whole extension that will be displayed in the Visual Studio
    /// tools / options window.
    /// </summary>
    internal class Options : DialogPage
    {
        public Options()
        {
            string documentsDir = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            ExtensionsDir = Path.Combine(documentsDir, "Visual Studio 2022\\Emmet");
            MnemonicsConfiguration = Path.Combine(ExtensionsDir, "mnemonics.ini");
        }

        /// <summary>
        /// Gets or sets a value indicating whether abbreviations expansion should occur on TAB key.
        /// </summary>
        [Category("General")]
        [DisplayName("Intercept TAB")]
        [Description("Intercept TAB key to expand abbreviations (restart required).")]
        public bool InterceptTabs { get; set; } = true;

        /// <summary>
        /// Gets or sets the full pathname of the extensions directory.
        /// </summary>
        [Category("General")]
        [DisplayName("Extensions directory")]
        [Description("Full path to the extensions directory, see http://docs.emmet.io/customization/.")]
        public string ExtensionsDir { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the full pathname of the mnemonics configuration file.
        /// </summary>
        [Category("General")]
        [DisplayName("Mnemonics configuration")]
        [Description("Full path to the mnemonics configuration file.")]
        public string MnemonicsConfiguration { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a value indicating whether the debug messages should be written to the output window.
        /// </summary>
        [Category("General")]
        [DisplayName("Write debug messages")]
        [Description("When enabled writes diagnostic messages to the Output Window (restart required).")]
        public bool WriteDebugMessages { get; set; } = true;

        protected override void OnApply(PageApplyEventArgs e)
        {
            base.OnApply(e);

            EmmetPackage.Instance?.ReloadOptions();
        }
    }
}