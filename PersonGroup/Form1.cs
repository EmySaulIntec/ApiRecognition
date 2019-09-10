using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PersonGroup
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Button1_Click(object sender, EventArgs e)
        {

        }

        public void DO()
        {
            var faceClient = new FaceServiceClient(new ApiKeyServiceClientCredentials("<subscription key>"), new System.Net.Http.DelegatingHandler[] { });

        }
    }
}
