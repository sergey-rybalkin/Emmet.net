using System;
using System.Windows.Forms;

namespace Emmet
{
    public partial class AbbreviationPrompt : Form
    {
        public AbbreviationPrompt()
        {
            InitializeComponent();
        }

        public string Abbreviation { get; set; }

        private void btnWrap_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(txtAbbreviation.Text))
            {
                Abbreviation = txtAbbreviation.Text;
                DialogResult = DialogResult.OK;
            }
            else
                DialogResult = DialogResult.Cancel;

            Close();
        }
    }
}