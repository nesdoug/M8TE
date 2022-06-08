using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace M8TE
{
    public partial class frmOptions : Form
    {
        public frmOptions()
        {
            InitializeComponent();
        }


        public static bool skipTextChange = false;


        private void Form3_FormClosed(object sender, FormClosedEventArgs e)
        {
            frmMain.close_it3();
        }

        private void Form3_Load(object sender, EventArgs e)
        {
            // set the information
            textBox1.Text = frmMain.dither_factor.ToString();

            textBox2.Text = frmMain.max_import_color.ToString();

            if (frmMain.f3_cb2 == false)
            {
                checkBox2.Checked = false;
            }
            else
            {
                checkBox2.Checked = true;
            }
            skipTextChange = false;
        }

        /*private void checkBox1_Click(object sender, EventArgs e)
        {
            
        }*/

        private void checkBox2_Click(object sender, EventArgs e)
        {
            if (checkBox2.Checked == false)
            {
                frmMain.f3_cb2 = false;
            }
            else
            {
                frmMain.f3_cb2 = true;
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            if (skipTextChange == true) return;

            string str = textBox1.Text;
            if (str == "") return;


            skipTextChange = true;
            
            int value = 0;
            int.TryParse(str, out value);
            if (value > 12) value = 12; // max value
            if (value < 0) value = 0; // min value
            str = value.ToString();
            textBox1.Text = str;
            frmMain.dither_factor = value;
            skipTextChange = false;
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            if (skipTextChange == true) return;

            string str = textBox2.Text;
            if (str == "") return;
            if (str == "1") return;


            skipTextChange = true;

            int value = 0;
            int.TryParse(str, out value);
            if (value > 256) value = 256; // max value
            if (value < 2) value = 2; // min value
            str = value.ToString();
            textBox2.Text = str;
            frmMain.max_import_color = value;
            skipTextChange = false;
        }
    }
}
