﻿using System;
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
    public partial class frmSave7 : Form
    {
        public frmSave7()
        {
            InitializeComponent();
        }

        public static int start_val, end_val;
        public static bool skipTextChange = false;

        private void Form5_FormClosed(object sender, FormClosedEventArgs e)
        {
            frmMain.close_it5();
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            if (skipTextChange == true) return;

            string str = textBox2.Text;
            if (str == "") return;

            skipTextChange = true;

            int value = 0;
            int.TryParse(str, out value);
            if (value > 255) value = 255; // max value
            if (value < 0) value = 0; // min value
            str = value.ToString();
            textBox2.Text = str;
            end_val = value;
            skipTextChange = false;
        }

        private void button1_Click(object sender, EventArgs e)
        { // save mode 7 tiles in a range

            // allow, even if in mode 7

            if ((start_val < 0) || (start_val > 255))
            {
                MessageBox.Show("Error. Start value is out of range.");
                return;
            }
            if ((end_val < 0) || (end_val > 255))
            {
                MessageBox.Show("Error. End value is out of range.");
                return;
            }
            if (start_val > end_val)
            {
                MessageBox.Show("Error. Start value > end value.");
                return;
            }


            // 64 bytes per tile x 256 possible tiles = 16384
            byte[] out_array = new byte[16384];
            int out_size = 0;
            int end_val2 = end_val + 1;
            int out_index = 0;
            
            int temp;

            out_size = (end_val2 - start_val) * 64; // 64 bytes per tile

            for (int i = start_val; i < end_val2; i++)
            {
                int z = i * 64; // 64 bytes per tile // start of current tile
                for (int y = 0; y < 8; y++)
                {
                    for (int x = 0; x < 8; x++)
                    {
                        temp = Tiles.Tile_Arrays[z + (y * 8) + x];
                        out_array[out_index++] = (byte)temp;
                    }
                }
                
            }

            // now save it
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "Tileset (*.chr)|*.chr|RLE File (*.rle)|*.rle";
            saveFileDialog1.Title = "Save Tiles in Range, Mode 7";
            //saveFileDialog1.ShowDialog();

            frmMain f = (this.Owner as frmMain);

            if ((saveFileDialog1.ShowDialog() == DialogResult.OK) && (saveFileDialog1.FileName != ""))
            {
                System.IO.FileStream fs = (System.IO.FileStream)saveFileDialog1.OpenFile();
                string ext = System.IO.Path.GetExtension(saveFileDialog1.FileName);
                if (ext == ".chr")
                {
                    for (int j = 0; j < out_size; j++)
                    {
                        fs.WriteByte(out_array[j]);
                    }

                    fs.Close();
                }
                else if (ext == ".rle")
                {
                    int rle_length = f.convert_RLE(out_array, out_size);
                    // global rle_array[] now has our compressed data
                    for (int i = 0; i < rle_length; i++)
                    {
                        fs.WriteByte(frmMain.rle_array[i]);
                    }

                    float percent = (float)rle_length / out_size;
                    fs.Close();

                    MessageBox.Show(String.Format("RLE size is {0}, or {1:P2}", rle_length, percent));
                }
                else
                { // something went wrong.
                    fs.Close();
                }

                // close this form, only if result = ok
                this.Close();
            }

        }

        private void Form5_Load(object sender, EventArgs e)
        {
            start_val = 0;
            end_val = 255;

            textBox1.Text = start_val.ToString();
            textBox2.Text = end_val.ToString();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            if (skipTextChange == true) return;

            string str = textBox1.Text;
            if (str == "") return;


            skipTextChange = true;

            int value = 0;
            int.TryParse(str, out value);
            if (value > 255) value = 255; // max value
            if (value < 0) value = 0; // min value
            str = value.ToString();
            textBox1.Text = str;
            start_val = value;
            skipTextChange = false;
        }

        


    }
}
