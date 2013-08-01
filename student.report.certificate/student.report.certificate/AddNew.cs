using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace plugins.student.report.certificate
{
    public partial class AddNew : FISCA.Presentation.Controls.BaseForm
    {
        public string name = "";
        public AddNew()
        {
            InitializeComponent();
        }
        private void buttonX1_Click(object sender, EventArgs e)
        {
            this.name = textBoxX1.Text;
            this.Close();
        }
    }
}
