using System.Windows;
using Microsoft.VisualStudio.PlatformUI;

namespace Emmet.Engine
{
    /// <summary>
    /// Dialog box that asks user to enter Emmet abbreviation to wrap selected content with.
    /// </summary>
    public partial class Prompt : DialogWindow
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Prompt"/> class.
        /// </summary>
        public Prompt()
        {
            InitializeComponent();
            Loaded += Prompt_Loaded;
        }

        /// <summary>
        /// Gets the abbreviation entered by user.
        /// </summary>
        public string Abbreviation { get; private set; }

        private void Prompt_Loaded(object sender, RoutedEventArgs e)
        {
            txtAbbreviation.Focus();
        }

        private void btnSubmit_Click(object sender, RoutedEventArgs e)
        {
            Abbreviation = txtAbbreviation.Text;
            DialogResult = true;
        }
    }
}
