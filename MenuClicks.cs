using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace M8TE
{
    public partial class frmMain
    {
        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        { // FILE / OPEN SESSION

            // note, we are ignoring the header, maybe change later
            // all sizes are fixed for now

            byte[] big_array = new byte[98832];
            int temp = 0;
            int[] bit1 = new int[8]; // 8 bit planes
            int[] bit2 = new int[8];
            int[] bit3 = new int[8];
            int[] bit4 = new int[8];
            int[] bit5 = new int[8];
            int[] bit6 = new int[8];
            int[] bit7 = new int[8];
            int[] bit8 = new int[8];

            int temp1 = 0;
            int temp2 = 0;
            int temp3 = 0;
            int temp4 = 0;
            int temp5 = 0;
            int temp6 = 0;
            int temp7 = 0;
            int temp8 = 0;

            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Title = "Open an M1 Session";
            openFileDialog1.Filter = "M8 File (*.M8)|*.M8|All files (*.*)|*.*";

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                undo_ready = false;
                
                System.IO.FileStream fs = (System.IO.FileStream)openFileDialog1.OpenFile();
                if (fs.Length == 98832)
                {
                    for (int i = 0; i < 98832; i++)
                    {
                        big_array[i] = (byte)fs.ReadByte();
                    }

                    if ((big_array[0] == (byte)'M') && (big_array[1] == (byte)'8'))
                    {
                        //copy the map height
                        map_height = big_array[6];
                        if((map_height < 1) || (map_height > 32))
                        {
                            map_height = 32;
                        }
                        textBox6.Text = map_height.ToString();

                        which_map = 0;
                        MapMenuSwitch();

                        set1ToolStripMenuItem.Checked = true;
                        set2ToolStripMenuItem.Checked = false;
                        set3ToolStripMenuItem.Checked = false;
                        set4ToolStripMenuItem.Checked = false;
                        tile_set = 0;
                        label10.Text = "1";

                        bg_mode = big_array[7];
                        if (bg_mode > 2) bg_mode = 0;
                        if (bg_mode == 0)
                        {
                            mode3TopToolStripMenuItem.Checked = true;
                            mode7ToolStripMenuItem.Checked = false;
                            mode7preToolStripMenuItem.Checked = false;
                            label11.Text = "Mode 3";
                            which_map = 0;
                            
                        }
                        else if(bg_mode == 1)
                        {
                            mode3TopToolStripMenuItem.Checked = false;
                            mode7ToolStripMenuItem.Checked = true;
                            mode7preToolStripMenuItem.Checked = false;
                            label11.Text = "Mode 7";
                        }
                        else
                        {
                            mode3TopToolStripMenuItem.Checked = true;
                            mode7ToolStripMenuItem.Checked = false;
                            mode7preToolStripMenuItem.Checked = false;
                            label11.Text = "Mode 7 Preview";
                        }

                        //copy the palette
                        int offset = 16;
                        for (int i = 0; i < 512; i += 2)
                        {
                            int j;
                            temp1 = big_array[offset++];
                            temp2 = big_array[offset++] << 8;
                            temp = temp1 + temp2;
                            //if ((i == 0x20) || (i == 0x40) || (i == 0x60) || (i == 0x80) ||
                            //    (i == 0xa0) || (i == 0xc0) || (i == 0xe0)) temp = 0;
                            // make the left most boxes black, but not the top most
                            j = i / 2;
                            Palettes.pal_r[j] = (byte)((temp & 0x001f) << 3);
                            Palettes.pal_g[j] = (byte)((temp & 0x03e0) >> 2);
                            Palettes.pal_b[j] = (byte)((temp & 0x7c00) >> 7);
                        }

                        // update the numbers in the boxes
                        rebuild_pal_boxes();

                        //copy the tile maps
                        for (int i = 0; i < 32 * 32 * 16; i++)
                        {
                            temp1 = big_array[offset++];
                            temp2 = big_array[offset++];
                            byte weird_byte = (byte)temp2;
                            int tile = temp1 + ((weird_byte & 3) << 8);
                            Maps.tile[i] = tile;
                            int pal = (weird_byte >> 2) & 7;
                            Maps.palette[i] = pal;
                            int pri = (weird_byte >> 5) & 1;
                            Maps.priority[i] = pri;
                            int h_flip = (weird_byte >> 6) & 1;
                            Maps.h_flip[i] = h_flip;
                            int v_flip = (weird_byte >> 7) & 1;
                            Maps.v_flip[i] = v_flip;
                        }

                        // copy the 8bpp tile sets
                        for (int temp_set = 0; temp_set < 4; temp_set++) // 4 sets
                        {
                            for (int i = 0; i < 256; i++) // 256 tiles
                            {
                                int index = offset + (temp_set * 256 * 64) + (64 * i); // start of current tile
                                for (int y = 0; y < 8; y++) // get 8 sets of bitplanes
                                {
                                    // get the 8 bitplanes for each tile row
                                    int y2 = y * 2; //0,2,4,6,8,10,12,14
                                    bit1[y] = big_array[index + y2];
                                    bit2[y] = big_array[index + y2 + 1];
                                    bit3[y] = big_array[index + y2 + 16];
                                    bit4[y] = big_array[index + y2 + 17];
                                    bit5[y] = big_array[index + y2 + 32];
                                    bit6[y] = big_array[index + y2 + 33];
                                    bit7[y] = big_array[index + y2 + 48];
                                    bit8[y] = big_array[index + y2 + 49];

                                    for (int x = 7; x >= 0; x--) // right to left
                                    {
                                        temp1 = bit1[y] & 1;    // get a bit from each bitplane
                                        bit1[y] = bit1[y] >> 1;
                                        temp2 = bit2[y] & 1;
                                        bit2[y] = bit2[y] >> 1;
                                        temp3 = bit3[y] & 1;
                                        bit3[y] = bit3[y] >> 1;
                                        temp4 = bit4[y] & 1;
                                        bit4[y] = bit4[y] >> 1;
                                        temp5 = bit5[y] & 1;
                                        bit5[y] = bit5[y] >> 1;
                                        temp6 = bit6[y] & 1;
                                        bit6[y] = bit6[y] >> 1;
                                        temp7 = bit7[y] & 1;
                                        bit7[y] = bit7[y] >> 1;
                                        temp8 = bit8[y] & 1;
                                        bit8[y] = bit8[y] >> 1;
                                        Tiles.Tile_Arrays[(temp_set * 256 * 8 * 8) + (i * 8 * 8) + (y * 8) + x] =
                                            (temp8 << 7) + (temp7 << 6) + (temp6 << 5) + (temp5 << 4) +
                                            (temp4 << 3) + (temp3 << 2) + (temp2 << 1) + temp1;
                                    }
                                }
                            }
                        }
                        //offset += 65536;


                    }
                    else
                    {
                        MessageBox.Show("Error. Not an M8 File.");
                    }


                }
                else
                {
                    MessageBox.Show("File size error. Expected 98832 bytes.",
                    "File size error", MessageBoxButtons.OK);
                }

                fs.Close();

                if(bg_mode != 0)
                {
                    map_height = 32; // the screen drawing code needs 1-32
                    textBox6.Text = "128";

                    // force tileset to 0
                    // it already is
                }

                if(bg_mode == 0)
                {
                    mode3TopToolStripMenuItem.Checked = true;
                    mode7ToolStripMenuItem.Checked = false;
                    mode7preToolStripMenuItem.Checked = false;
                    label11.Text = "Mode 3";
                    which_map = 0;
                    MapMenuSwitch(); // only use 1st map for mode 3
                }
                else if(bg_mode == 1)
                {
                    mode3TopToolStripMenuItem.Checked = false;
                    mode7ToolStripMenuItem.Checked = true;
                    mode7preToolStripMenuItem.Checked = false;
                    label11.Text = "Mode 7";
                }
                else
                {
                    mode3TopToolStripMenuItem.Checked = false;
                    mode7ToolStripMenuItem.Checked = false;
                    mode7preToolStripMenuItem.Checked = true;
                    label11.Text = "Mode 7 Preview";
                }

                update_palette();
                common_update2();

                disable_map_click = 1;  // fix bug, double click causing
                                        // mouse event on tilemap
            }
        } // end of OPEN SESSION



        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        { // FILE / SAVE SESSION

            byte[] big_array = new byte[98832];
            int temp, count;
            int[] bit1 = new int[8]; // bit planes
            int[] bit2 = new int[8];
            int[] bit3 = new int[8];
            int[] bit4 = new int[8];
            int[] bit5 = new int[8];
            int[] bit6 = new int[8];
            int[] bit7 = new int[8];
            int[] bit8 = new int[8];

            big_array[0] = (byte)'M';
            big_array[1] = (byte)'8';
            big_array[2] = 1; // M8 file version
            big_array[3] = 1; // # palettes (of 128 colors)
            big_array[4] = 16; // # maps
            big_array[5] = 4; // # 8bpp tilesets
            big_array[6] = (byte)map_height; // save map height
            big_array[7] = (byte)bg_mode; // save bg mode

            // I don't use these values currently, but maybe will later.
            for (int i = 8; i < 16; i++)
            {
                big_array[i] = 0;
            }

            count = 16;
            for (int i = 0; i < 256; i++) // palettes
            {
                temp = ((Palettes.pal_r[i] & 0xf8) >> 3) + ((Palettes.pal_g[i] & 0xf8) << 2) + ((Palettes.pal_b[i] & 0xf8) << 7);
                big_array[count++] = (byte)(temp & 0xff); // little end first
                big_array[count++] = (byte)((temp >> 8) & 0x7f); // 15 bit palette
            }

            for (int i = 0; i < 32 * 32 * 16; i++) // 16 background maps
            {
                big_array[count++] = (byte)(Maps.tile[i] & 0xff); // the low byte
                temp = ((Maps.tile[i] >> 8) & 3) + ((Maps.palette[i] & 7) << 2) +
                    ((Maps.priority[i] & 1) << 5) + ((Maps.h_flip[i] & 1) << 6) +
                    ((Maps.v_flip[i] & 1) << 7); // mishmash of weird bits
                // VHoP PPcc
                // VH flip, o priority, palette, upper 2 bits of tile number
                big_array[count++] = (byte)temp;
            }

            for (int temp_set = 0; temp_set < 4; temp_set++) // 4 tilesets 8bpp
            {
                for (int i = 0; i < 256; i++) // 256 tiles
                {
                    int z = (temp_set * 256 * 8 * 8) + (64 * i); // start of current tile
                    for (int y = 0; y < 8; y++)
                    {
                        for (int x = 0; x < 8; x++)
                        {
                            temp = Tiles.Tile_Arrays[z + (y * 8) + x];
                            bit1[y] = (bit1[y] << 1) + (temp & 1);
                            bit2[y] = (bit2[y] << 1) + ((temp >> 1) & 1); // NOTE, this was changed
                            bit3[y] = (bit3[y] << 1) + ((temp >> 2) & 1); 
                            bit4[y] = (bit4[y] << 1) + ((temp >> 3) & 1);
                            bit5[y] = (bit5[y] << 1) + ((temp >> 4) & 1);
                            bit6[y] = (bit6[y] << 1) + ((temp >> 5) & 1);
                            bit7[y] = (bit7[y] << 1) + ((temp >> 6) & 1);
                            bit8[y] = (bit8[y] << 1) + ((temp >> 7) & 1);
                        }
                    }
                    for (int j = 0; j < 8; j++)
                    {
                        big_array[count++] = (byte)bit1[j];
                        big_array[count++] = (byte)bit2[j];
                    }
                    for (int j = 0; j < 8; j++)
                    {
                        big_array[count++] = (byte)bit3[j];
                        big_array[count++] = (byte)bit4[j];
                    }
                    for (int j = 0; j < 8; j++)
                    {
                        big_array[count++] = (byte)bit5[j];
                        big_array[count++] = (byte)bit6[j];
                    }
                    for (int j = 0; j < 8; j++)
                    {
                        big_array[count++] = (byte)bit7[j];
                        big_array[count++] = (byte)bit8[j];
                    }
                }
            }


            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "M8 File (*.M8)|*.M8|All files (*.*)|*.*";
            saveFileDialog1.Title = "Save this Session";
            saveFileDialog1.ShowDialog();

            if (saveFileDialog1.FileName != "")
            {
                System.IO.FileStream fs = (System.IO.FileStream)saveFileDialog1.OpenFile();
                for (int i = 0; i < 98832; i++)
                {
                    fs.WriteByte(big_array[i]);
                }
                fs.Close();
            }
        } // END OF SAVE SESSION



        private void savePhotoToolStripMenuItem_Click(object sender, EventArgs e)
        { // FILE / EXPORT IMAGE
            
            // export image pic of the current view
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "PNG|*.png|BMP|*.bmp|JPG|*.jpg|GIF|*.gif";
            
            if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string ext = System.IO.Path.GetExtension(sfd.FileName);
                switch (ext)
                {
                    case ".jpg":
                    case ".jpeg":
                        pictureBox1.Image.Save(sfd.FileName, ImageFormat.Jpeg);
                        break;
                    case ".bmp":
                        pictureBox1.Image.Save(sfd.FileName, ImageFormat.Bmp);
                        break;
                    case ".gif":
                        pictureBox1.Image.Save(sfd.FileName, ImageFormat.Gif);
                        break;
                    default:
                        pictureBox1.Image.Save(sfd.FileName, ImageFormat.Png);
                        break;

                }
            }
        } // END EXPORT IMAGE



        private void endSessionToolStripMenuItem_Click(object sender, EventArgs e)
        { // FILE / CLOSE PROGRAM

            // close the program
            Application.Exit();
        }



        // MAPS **************************************************

        private void loadMapToolStripMenuItem_Click(object sender, EventArgs e)
        { // MAPS / LOAD A MAP / MODE 3 only
            if (bg_mode > 0)
            {
                MessageBox.Show("Select Mode 3 First.");
                return;
            }

            byte[] map_array = new byte[2 * 32 * 32]; // 128 entries * 2 bytes, little endian
            int map_size;

            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Title = "Select a Tile Map";
            openFileDialog1.Filter = "Tile Map (*.map)|*.map|All files (*.*)|*.*";

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                Checkpoint();
                
                System.IO.FileStream fs = (System.IO.FileStream)openFileDialog1.OpenFile();

                if (fs.Length < 2)
                {
                    MessageBox.Show("File size error. Expected 2 - 2048 bytes.",
                    "File size error", MessageBoxButtons.OK);
                }
                else
                {
                    map_size = (int)fs.Length; // how many bytes we need to copy
                    if (fs.Length > 0x800) map_size = 0x800;
                    map_size = map_size & 0xfffe; // should be even

                    {
                        for (int i = 0; i < map_size; i++)
                        {
                            map_array[i] = (byte)fs.ReadByte();
                        }

                        int offset = 0; // which_map * 32 * 32;
                        // always use map 0 for mode 3

                        //copy it here
                        
                        for (int i = 0; i < map_size; i += 2)
                        {
                            byte weird_byte = map_array[i + 1];
                            int tile = map_array[i] + ((weird_byte & 3) << 8);
                            Maps.tile[offset] = tile;
                            int pal = (weird_byte >> 2) & 7;
                            Maps.palette[offset] = pal;
                            int pri = (weird_byte >> 5) & 1;
                            Maps.priority[offset] = pri;
                            int h_flip = (weird_byte >> 6) & 1;
                            Maps.h_flip[offset] = h_flip;
                            int v_flip = (weird_byte >> 7) & 1;
                            Maps.v_flip[offset] = v_flip;
                            offset++;
                        }

                        
                    }
                }

                fs.Close();

                update_tilemap();

                disable_map_click = 1;  // fix bug, double click causing
                                        // mouse event on tilemap
            }
        } // END OF LOAD A MAP


        private void loadAMapToSelectedXYToolStripMenuItem_Click(object sender, EventArgs e)
        { // load map to specific map Y coordinates. MODE 3 only.
            if (bg_mode > 0)
            {
                MessageBox.Show("Select Mode 3 First.");
                return;
            }

            byte[] map_array = new byte[2 * 32 * 32]; // 128 entries * 2 bytes, little endian
            int map_size;

            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Title = "Select a Tile Map";
            openFileDialog1.Filter = "Tile Map (*.map)|*.map|All files (*.*)|*.*";

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                Checkpoint();

                System.IO.FileStream fs = (System.IO.FileStream)openFileDialog1.OpenFile();

                if (fs.Length < 2)
                {
                    MessageBox.Show("File size error. Expected 2 - 2048 bytes.",
                    "File size error", MessageBoxButtons.OK);
                }
                else
                {
                    map_size = (int)fs.Length; // how many bytes we need to copy
                    if (fs.Length > 0x800) map_size = 0x800;
                    map_size = map_size & 0xfffe; // should be even

                    {
                        for (int i = 0; i < map_size; i++)
                        {
                            map_array[i] = (byte)fs.ReadByte();
                        }

                        int offset = (32 * active_map_y);
                        int too_far = (32 * 32);
                        // always use map 0 for mode 3

                        //copy it here
                        
                        for (int i = 0; i < map_size; i += 2)
                        {
                            byte weird_byte = map_array[i + 1];
                            int tile = map_array[i] + ((weird_byte & 3) << 8);
                            Maps.tile[offset] = tile;
                            int pal = (weird_byte >> 2) & 7;
                            Maps.palette[offset] = pal;
                            int pri = (weird_byte >> 5) & 1;
                            Maps.priority[offset] = pri;
                            int h_flip = (weird_byte >> 6) & 1;
                            Maps.h_flip[offset] = h_flip;
                            int v_flip = (weird_byte >> 7) & 1;
                            Maps.v_flip[offset] = v_flip;
                            offset++;
                            if (offset >= too_far) break;
                        }

                    }
                }

                fs.Close();

                update_tilemap();

                disable_map_click = 1;  // fix bug, double click causing
                                        // mouse event on tilemap
            }
        }


        private void ldMapSelYAtSelTileOffsetToolStripMenuItem_Click(object sender, EventArgs e)
        { // MODE 3 only
            // load a map to a selected y position, offset the tiles to the selected tile
            if (bg_mode > 0)
            {
                MessageBox.Show("Select Mode 3 First.");
                return;
            }

            byte[] map_array = new byte[2 * 32 * 32]; // 2 bytes, little endian
            int map_size;

            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Title = "Select a Tile Map";
            openFileDialog1.Filter = "Tile Map (*.map)|*.map|All files (*.*)|*.*";

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                Checkpoint();

                System.IO.FileStream fs = (System.IO.FileStream)openFileDialog1.OpenFile();

                if (fs.Length < 2)
                {
                    MessageBox.Show("File size error. Expected 2 - 2048 bytes.",
                    "File size error", MessageBoxButtons.OK);
                }
                else
                {
                    tile_num = (tile_y * 16) + tile_x;
                    int tile_offset = tile_num + ((tile_set & 3) * 256);

                    map_size = (int)fs.Length; // how many bytes we need to copy
                    if (fs.Length > 0x800) map_size = 0x800;
                    map_size = map_size & 0xfffe; // should be even

                    {
                        for (int i = 0; i < map_size; i++)
                        {
                            map_array[i] = (byte)fs.ReadByte();
                        }

                        int offset = (32 * active_map_y);
                        int too_far = (32 * 32);
                        // always use map 0 for mode 3

                        //copy it here
                        
                        for (int i = 0; i < map_size; i += 2)
                        {
                            byte weird_byte = map_array[i + 1];
                            int tile = map_array[i] + ((weird_byte & 3) << 8);
                            Maps.tile[offset] = (tile + tile_offset) & 0x3ff; 
                              // add the tile offset, can't be higher than 1023 (3ff)
                            int pal = (weird_byte >> 2) & 7;
                            Maps.palette[offset] = pal;
                            int pri = (weird_byte >> 5) & 1;
                            Maps.priority[offset] = pri;
                            int h_flip = (weird_byte >> 6) & 1;
                            Maps.h_flip[offset] = h_flip;
                            int v_flip = (weird_byte >> 7) & 1;
                            Maps.v_flip[offset] = v_flip;
                            offset++;
                            if (offset >= too_far) break;
                        }

                        
                    }
                }

                fs.Close();

                update_tilemap();

                disable_map_click = 1;  // fix bug, double click causing
                                        // mouse event on tilemap
            }
        }


        private void try_RLE(byte[] out_array, byte[] in_array, int in_size)
        {
            // globals rle_index, rle_index2, rle_count;
            byte byte1, byte2, byte3;
            int old_index = rle_index;
            rle_count = 0;
            while(rle_index < in_size)
            {
                if (rle_count >= 4095) break; // max count
                if (in_array[rle_index - 1] == in_array[rle_index])
                {
                    rle_count++;
                    rle_index++;
                }
                else
                {
                    break;
                }
            }
            if(rle_count > 0) // zero is best here
            { 
                if(rle_count > 31) // 2 byte header
                {
                    byte1 = (byte)(((rle_count >> 8) & 0x0f) + 0xd0);
                    byte2 = (byte)(rle_count & 0xff);
                    byte3 = in_array[rle_index - 1];
                    out_array[rle_index2++] = byte1;
                    out_array[rle_index2++] = byte2;
                    out_array[rle_index2++] = byte3;
                }
                else // 1 byte header
                {
                    byte1 = (byte)((rle_count & 0x3f) + 0x40);
                    byte2 = in_array[rle_index - 1];
                    out_array[rle_index2++] = byte1;
                    out_array[rle_index2++] = byte2;
                }
                rle_index++;
            }
            else
            {
                rle_count = 0;
                rle_index = old_index;
            }
        }

        private void try_Plus(byte[] out_array, byte[] in_array, int in_size)
        {
            // globals rle_index, rle_index2, rle_count;
            byte byte1, byte2, byte3;
            int old_index = rle_index;
            int start_value = in_array[rle_index - 1];
            rle_count = 0;
            while (rle_index < in_size)
            {
                if (rle_count >= 255) break; // max count
                if (in_array[rle_index - 1] == in_array[rle_index] - 1)
                {
                    rle_count++;
                    rle_index++;
                }
                else
                {
                    break;
                }
            }
            if (rle_count > 0) // zero is best here.
            {
                if (rle_count > 31) // 2 byte header
                {
                    byte1 = (byte)(((rle_count >> 8) & 0x0f) + 0xe0);
                    byte2 = (byte)(rle_count & 0xff);
                    byte3 = (byte)start_value;
                    out_array[rle_index2++] = byte1;
                    out_array[rle_index2++] = byte2;
                    out_array[rle_index2++] = byte3;
                }
                else // 1 byte header
                {
                    byte1 = (byte)((rle_count & 0x3f) + 0x80);
                    byte2 = (byte)start_value;
                    out_array[rle_index2++] = byte1;
                    out_array[rle_index2++] = byte2;
                }
                rle_index++;
            }
            else
            {
                rle_count = 0;
                rle_index = old_index;
            }
        }

        private void do_Literal(byte[] out_array, byte[] in_array, int in_size)
        {
            // globals rle_index, rle_index2, rle_count;
            byte byte1, byte2, byte3;
            int start_index = rle_index - 1;
            rle_count = 0;
            rle_index++;
            while (rle_index < in_size)
            {
                if (rle_count >= 4094) break; // max count
                if ((in_array[rle_index - 2] == in_array[rle_index - 1]) &&
                    (in_array[rle_index - 1] == in_array[rle_index]))
                { // found a run > 1
                    break;
                }
                if (((in_array[rle_index - 2] == in_array[rle_index - 1] - 1)) &&
                    (in_array[rle_index - 1] == in_array[rle_index] - 1))
                { // found a run > 1
                    break;
                }
                rle_count++;
                rle_index++;
            }
            rle_count--;
            rle_index--;

            int nearend = in_size - rle_index;
            if(nearend < 2)
            { // near the end of the file, dump the rest
                if(nearend == 1)
                {
                    rle_count++;
                    rle_index++;
                }
                rle_count++;
                rle_index++;
            }

            if (rle_count >= 0) // always do
            {
                int count2 = rle_count + 1; 
                
                
                if (rle_count > 31) // 2 byte header
                {
                    byte1 = (byte)(((rle_count >> 8) & 0x0f) + 0xc0);
                    byte2 = (byte)(rle_count & 0xff);
                    out_array[rle_index2++] = byte1;
                    out_array[rle_index2++] = byte2;
                    for(int i = 0; i < count2; i++)
                    {
                        byte3 = in_array[start_index++];
                        out_array[rle_index2++] = byte3;
                    }
                    
                }
                else // 1 byte header
                {
                    byte1 = (byte)(rle_count & 0x3f);
                    out_array[rle_index2++] = byte1;
                    if (rle_count == 0)
                    {
                        byte2 = in_array[start_index];
                        out_array[rle_index2++] = byte2;
                    }
                    else
                    {
                        for (int i = 0; i < count2; i++)
                        {
                            byte2 = in_array[start_index++];
                            out_array[rle_index2++] = byte2;
                        }
                    }
                    
                }
                
            }

        }

        public int convert_RLE(byte[] in_array, int in_size)
        {
            byte[] in_array_P = new byte[65536];
            byte[] out_array_P = new byte[65536];
            byte[] out_array_notP = new byte[65536];
            byte[] split_array = new byte[32768];
            byte[] split_array2 = new byte[32768];
            int P_size, notP_size;
            // globals rle_index, rle_index2, rle_count;
            rle_index = 1; // // start at 1, we subtract 1
            rle_index2 = 0;
            rle_count = 0;

            if (in_size < 3) return 0; // minimum to avoid errors

            if (in_size > 32768) // for debugging
            {
                label6.Text = "ERROR, RLE in_size too big!";
            }


            // try not Planar first

            while (rle_index < in_size)
            {
                try_RLE(out_array_notP, in_array, in_size);
                if (rle_count == 0)
                {
                    try_Plus(out_array_notP, in_array, in_size);
                    if (rle_count == 0)
                    {
                        do_Literal(out_array_notP, in_array, in_size);
                    }
                }
            }

            // do a final literal, if needed
            if (rle_index == in_size)
            {
                out_array_notP[rle_index2++] = 0; // literal of 1
                out_array_notP[rle_index2++] = in_array[in_size - 1]; // the last byte
            }

            // put an end of file marker, non-planar
            out_array_notP[rle_index2++] = 0xf0;
            notP_size = rle_index2;


            // try again, Planar
            // split the array, low bytes in 1 array, high bytes in another
            // planar expects even. If odd, this will pad a zero at the end.
            int half_size = (in_size + 1) / 2;
            in_size = half_size * 2; // should round up even.
            for (int i = 0; i < half_size; i++)
            {
                int j = i * 2;
                int k = j + 1;
                split_array[i] = in_array[j];
                split_array2[i] = in_array[k];
            }
            // combine them into 1 array, so I don't have to modify the code
            for (int i = 0; i < half_size; i++)
            {
                in_array_P[i] = split_array[i];
                int j = i + half_size;
                in_array_P[j] = split_array2[i];
            }

            rle_index = 1;
            rle_index2 = 0;
            rle_count = 0;
            while (rle_index < in_size)
            {
                try_RLE(out_array_P, in_array_P, in_size);
                if (rle_count == 0)
                {
                    try_Plus(out_array_P, in_array_P, in_size);
                    if (rle_count == 0)
                    {
                        do_Literal(out_array_P, in_array_P, in_size);
                    }
                }
            }
            // do a final literal, if needed
            if (rle_index == in_size)
            {
                out_array_P[rle_index2++] = 0; // literal of 1
                out_array_P[rle_index2++] = in_array_P[in_size - 1]; // the last byte
            }

            // put an end of file marker, planar
            out_array_P[rle_index2++] = 0xff;
            P_size = rle_index2;

            // copy best array to global rle_array[]
            // and return the length
            if (notP_size <= P_size)
            { // not planar is best
                for (int i = 0; i < notP_size; i++)
                {
                    rle_array[i] = out_array_notP[i];
                }
                return notP_size;
            }
            else
            { // planar is best
                for (int i = 0; i < P_size; i++)
                {
                    rle_array[i] = out_array_P[i];
                }
                return P_size;
            }

        }

        private void saveMapToolStripMenuItem_Click(object sender, EventArgs e)
        { // MAPS / SAVE A 32 x 32 MAP / MODE 3 only
            
            // save a full size map, the current one
            if (bg_mode > 0)
            {
                MessageBox.Show("Select Mode 3 First.");
                return;
            }

            byte[] map_array = new byte[2048]; // 2 bytes * 32 * 32
            int temp, offset;

            offset = 0; // // use map 0 only for mode 3
            for (int i = 0; i < 1024; i++)
            {
                map_array[(i * 2)] = (byte)(Maps.tile[offset] & 0xff); // the low byte
                temp = ((Maps.tile[offset] >> 8) & 3) + ((Maps.palette[offset] & 7) << 2) +
                    ((Maps.priority[offset] & 1) << 5) + ((Maps.h_flip[offset] & 1) << 6) +
                    ((Maps.v_flip[offset] & 1) << 7); // mishmash of weird bits
                // VHoP PPcc
                // VH flip, o priority, palette, upper 2 bits of tile number
                map_array[(i * 2) + 1] = (byte)temp;
                offset++;
            }

            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "Tile Map (*.map)|*.map|RLE File (*.rle)|*.rle";
            saveFileDialog1.Title = "Save a Tile Map 32x32";
            saveFileDialog1.ShowDialog();

            if (saveFileDialog1.FileName != "")
            {
                System.IO.FileStream fs = (System.IO.FileStream)saveFileDialog1.OpenFile();

                string ext = System.IO.Path.GetExtension(saveFileDialog1.FileName);
                if(ext == ".map")
                {
                    for (int i = 0; i < 2048; i++)
                    {
                        fs.WriteByte(map_array[i]);
                    }
                    fs.Close();
                }
                else if(ext == ".rle")
                {
                    int rle_length = convert_RLE(map_array, 2048);
                    // global rle_array[] now has our compressed data
                    for (int i = 0; i < rle_length; i++)
                    {
                        fs.WriteByte(rle_array[i]);
                    }
                    float percent = (float)rle_length / 2048;
                    fs.Close();

                    MessageBox.Show(String.Format("RLE size is {0}, or {1:P2}", rle_length, percent));
                }
                else // something went wrong.
                {
                    fs.Close();
                }
                
            }
        } // END OF SAVE 32 x 32 MAP



        private void saveAMapXheightToolStripMenuItem_Click(object sender, EventArgs e)
        { // MAPS / SAVE 32 x HEIGHT MAP / MODE 3 only
            
            // save a map at a specific height
            if (bg_mode > 0)
            {
                MessageBox.Show("Select Mode 3 First.");
                return;
            }
            if ((map_height < 1) || (map_height > 32))
            {
                MessageBox.Show("Map Height needs to be 1-32");
                return;
            }

            int size_h = map_height * 32 * 2;
            byte[] map_array = new byte[size_h]; // 2 bytes * 32 * 32
            int temp, offset;
            int size_h2 = size_h / 2;

            offset = 0; 
            for (int i = 0; i < size_h2; i++)
            {
                map_array[(i * 2)] = (byte)(Maps.tile[offset] & 0xff); // the low byte
                temp = ((Maps.tile[offset] >> 8) & 3) + ((Maps.palette[offset] & 7) << 2) +
                    ((Maps.priority[offset] & 1) << 5) + ((Maps.h_flip[offset] & 1) << 6) +
                    ((Maps.v_flip[offset] & 1) << 7); // mishmash of weird bits
                // VHoP PPcc
                // VH flip, o priority, palette, upper 2 bits of tile number
                map_array[(i * 2) + 1] = (byte)temp;
                offset++;
            }

            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "Tile Map (*.map)|*.map|RLE File (*.rle)|*.rle";
            saveFileDialog1.Title = "Save a Tile Map 32xHeight";
            saveFileDialog1.ShowDialog();

            if (saveFileDialog1.FileName != "")
            {
                System.IO.FileStream fs = (System.IO.FileStream)saveFileDialog1.OpenFile();

                string ext = System.IO.Path.GetExtension(saveFileDialog1.FileName);
                if (ext == ".map")
                {
                    for (int i = 0; i < size_h; i++)
                    {
                        fs.WriteByte(map_array[i]);
                    }
                    fs.Close();
                }
                else if (ext == ".rle")
                {
                    int rle_length = convert_RLE(map_array, size_h);
                    // global rle_array[] now has our compressed data
                    for (int i = 0; i < rle_length; i++)
                    {
                        fs.WriteByte(rle_array[i]);
                    }
                    
                    float percent = (float)rle_length / size_h;
                    fs.Close();

                    MessageBox.Show(String.Format("RLE size is {0}, or {1:P2}", rle_length, percent));
                }
                else
                { // something went wrong.
                    fs.Close();
                }
                
            }
        } // END OF SAVE 32 x HEIGHT MAP



        private void clearAllMapsToolStripMenuItem_Click(object sender, EventArgs e)
        { // MAPS / CLEAR ALL MAPS
            //Checkpoint(); // undo code only saves 1 map
            undo_ready = false; // disallow undo, the code only saves 1 map

            for (int i = 0; i < 16 * 32 * 32; i++)
            {
                Maps.tile[i] = 0;
                Maps.palette[i] = 0;
                Maps.priority[i] = 0;
                Maps.h_flip[i] = 0;
                Maps.v_flip[i] = 0;
            }

            common_update2();
            
        }


        // MODE 7 MAPS **************************************


        private void loadAMode7MapToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // load a mode 7 map, assumed to be 32x32 or 128x128


            if (bg_mode == 0)
            {
                MessageBox.Show("Select Mode 7 First.");
                return;
            }

            byte[] map_array = new byte[16 * 32 * 32]; // 16 screens = 128x128 tiles
            int map_size;

            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Title = "Select a Tile Map";
            openFileDialog1.Filter = "Tile Map (*.map)|*.map|All files (*.*)|*.*";

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                Checkpoint();

                System.IO.FileStream fs = (System.IO.FileStream)openFileDialog1.OpenFile();

                
                if (fs.Length == 1024) // 32x32, current map
                {
                    map_size = (int)fs.Length; // how many bytes we need to copy
                    
                    for (int i = 0; i < map_size; i++)
                    {
                        map_array[i] = (byte)fs.ReadByte();
                    }

                    int offset = which_map * 32 * 32;

                    //copy it here
                    for (int i = 0; i < map_size; i++)
                    {
                        int tile = map_array[i];
                        Maps.tile[offset] = tile;
                        
                        offset++;
                    }
                }
                else if (fs.Length == 16384) // 128x128 all maps
                {
                    undo_ready = false; // disallow undo, the code only saves 1 map

                    map_size = (int)fs.Length; // how many bytes we need to copy
                    
                    {
                        for (int i = 0; i < map_size; i++)
                        {
                            map_array[i] = (byte)fs.ReadByte();
                        }

                        int offset = 0; // all maps
                        int offset2 = 0; // offset to map_array

                        //copy it here
                        for (int y = 0; y < 128; y++)
                        {
                            for (int x = 0; x < 128; x++)
                            {
                                int room_y = y / 32;
                                int room_x = x / 32;
                                int x_low = x & 0x1f;
                                int y_low = y & 0x1f;

                                offset = ((room_y * 4) + room_x) * 32 * 32;
                                offset += (y_low * 32) + x_low;
                                Maps.tile[offset] = map_array[offset2];
                                offset2++;
                            }
                        }

                    }
                }

                else
                {
                    MessageBox.Show("File size error. Expected 1024 or 16384 bytes.",
                    "File size error", MessageBoxButtons.OK);
                }

                fs.Close();

                update_tilemap();

                disable_map_click = 1;  // fix bug, double click causing
                                        // mouse event on tilemap
            }

        }


        private void saveA32x32M7MapToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // save a mode 7 map, 32x32 (the current screen)

            // save a full size map, the current one
            if (bg_mode == 0)
            {
                MessageBox.Show("Select Mode 7 First.");
                return;
            }

            byte[] map_array = new byte[1024]; // 1 bytes * 32 * 32
            int offset;

            offset = which_map * 32 * 32;
            for (int i = 0; i < 1024; i++)
            {
                map_array[i] = (byte)(Maps.tile[offset] & 0xff); // the low byte
                // cut the rest, mode 7 doesn't flip
                offset++;
            }

            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "Tile Map (*.map)|*.map|RLE File (*.rle)|*.rle";
            saveFileDialog1.Title = "Save a Tile Map 32x32 Mode 7";
            saveFileDialog1.ShowDialog();

            if (saveFileDialog1.FileName != "")
            {
                System.IO.FileStream fs = (System.IO.FileStream)saveFileDialog1.OpenFile();

                string ext = System.IO.Path.GetExtension(saveFileDialog1.FileName);
                if (ext == ".map")
                {
                    for (int i = 0; i < 1024; i++)
                    {
                        fs.WriteByte(map_array[i]);
                    }
                    fs.Close();
                }
                else if (ext == ".rle")
                {
                    int rle_length = convert_RLE(map_array, 1024);
                    // global rle_array[] now has our compressed data
                    for (int i = 0; i < rle_length; i++)
                    {
                        fs.WriteByte(rle_array[i]);
                    }
                    float percent = (float)rle_length / 1024;
                    fs.Close();

                    MessageBox.Show(String.Format("RLE size is {0}, or {1:P2}", rle_length, percent));
                }
                else // something went wrong.
                {
                    fs.Close();
                }

            }
        }


        private void saveA128x128M7MapToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // save a mode 7 map, 128x128 (the current screen)

            // save a full size map, all maps, with each row flowing accross 4 maps
            // before wrapping back to the first map
            if (bg_mode == 0)
            {
                MessageBox.Show("Select Mode 7 First.");
                return;
            }

            byte[] map_array = new byte[16384]; // 16 maps * 32 * 32
            int offset = 0;
            int offset2 = 0;

            
            for (int y = 0; y < 32; y++)
            {
                offset = y * 32;
                for(int x = 0; x < 32; x++) // room 1
                {
                    map_array[offset2] = (byte)Maps.tile[offset];
                    offset++;
                    offset2++;
                }
                offset = y * 32;
                offset += (32 * 32);
                for (int x = 0; x < 32; x++) // room 2
                {
                    map_array[offset2] = (byte)Maps.tile[offset];
                    offset++;
                    offset2++;
                }
                offset = y * 32;
                offset += (32 * 32 * 2);
                for (int x = 0; x < 32; x++) // room 3
                {
                    map_array[offset2] = (byte)Maps.tile[offset];
                    offset++;
                    offset2++;
                }
                offset = y * 32;
                offset += (32 * 32 * 3);
                for (int x = 0; x < 32; x++) // room 4
                {
                    map_array[offset2] = (byte)Maps.tile[offset];
                    offset++;
                    offset2++;
                }
            }
            for (int y = 0; y < 32; y++)
            {
                offset = y * 32;
                offset += (32 * 32 * 4);
                for (int x = 0; x < 32; x++) // room 5
                {
                    map_array[offset2] = (byte)Maps.tile[offset];
                    offset++;
                    offset2++;
                }
                offset = y * 32;
                offset += (32 * 32 * 5);
                for (int x = 0; x < 32; x++) // room 6
                {
                    map_array[offset2] = (byte)Maps.tile[offset];
                    offset++;
                    offset2++;
                }
                offset = y * 32;
                offset += (32 * 32 * 6);
                for (int x = 0; x < 32; x++) // room 7
                {
                    map_array[offset2] = (byte)Maps.tile[offset];
                    offset++;
                    offset2++;
                }
                offset = y * 32;
                offset += (32 * 32 * 7);
                for (int x = 0; x < 32; x++) // room 8
                {
                    map_array[offset2] = (byte)Maps.tile[offset];
                    offset++;
                    offset2++;
                }
            }
            for (int y = 0; y < 32; y++)
            {
                offset = y * 32;
                offset += (32 * 32 * 8);
                for (int x = 0; x < 32; x++) // room 9
                {
                    map_array[offset2] = (byte)Maps.tile[offset];
                    offset++;
                    offset2++;
                }
                offset = y * 32;
                offset += (32 * 32 * 9);
                for (int x = 0; x < 32; x++) // room 10
                {
                    map_array[offset2] = (byte)Maps.tile[offset];
                    offset++;
                    offset2++;
                }
                offset = y * 32;
                offset += (32 * 32 * 10);
                for (int x = 0; x < 32; x++) // room 11
                {
                    map_array[offset2] = (byte)Maps.tile[offset];
                    offset++;
                    offset2++;
                }
                offset = y * 32;
                offset += (32 * 32 * 11);
                for (int x = 0; x < 32; x++) // room 12
                {
                    map_array[offset2] = (byte)Maps.tile[offset];
                    offset++;
                    offset2++;
                }
            }
            for (int y = 0; y < 32; y++)
            {
                offset = y * 32;
                offset += (32 * 32 * 12);
                for (int x = 0; x < 32; x++) // room 13
                {
                    map_array[offset2] = (byte)Maps.tile[offset];
                    offset++;
                    offset2++;
                }
                offset = y * 32;
                offset += (32 * 32 * 13);
                for (int x = 0; x < 32; x++) // room 14
                {
                    map_array[offset2] = (byte)Maps.tile[offset];
                    offset++;
                    offset2++;
                }
                offset = y * 32;
                offset += (32 * 32 * 14);
                for (int x = 0; x < 32; x++) // room 15
                {
                    map_array[offset2] = (byte)Maps.tile[offset];
                    offset++;
                    offset2++;
                }
                offset = y * 32;
                offset += (32 * 32 * 15);
                for (int x = 0; x < 32; x++) // room 16
                {
                    map_array[offset2] = (byte)Maps.tile[offset];
                    offset++;
                    offset2++;
                }
            }


            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "Tile Map (*.map)|*.map|RLE File (*.rle)|*.rle";
            saveFileDialog1.Title = "Save a Tile Map 128x128 Mode 7";
            saveFileDialog1.ShowDialog();

            if (saveFileDialog1.FileName != "")
            {
                System.IO.FileStream fs = (System.IO.FileStream)saveFileDialog1.OpenFile();

                string ext = System.IO.Path.GetExtension(saveFileDialog1.FileName);
                if (ext == ".map")
                {
                    for (int i = 0; i < 16384; i++)
                    {
                        fs.WriteByte(map_array[i]);
                    }
                    fs.Close();
                }
                else if (ext == ".rle")
                {
                    int rle_length = convert_RLE(map_array, 16384);
                    // global rle_array[] now has our compressed data
                    for (int i = 0; i < rle_length; i++)
                    {
                        fs.WriteByte(rle_array[i]);
                    }
                    float percent = (float)rle_length / 16384;
                    fs.Close();

                    MessageBox.Show(String.Format("RLE size is {0}, or {1:P2}", rle_length, percent));
                }
                else // something went wrong.
                {
                    fs.Close();
                }

            }
        }



        // TILES **************************************************

        private void loadMode3TilesToolStripMenuItem_Click(object sender, EventArgs e)
        { // TILES / LOAD 8bpp Mode 3 style

            if (bg_mode > 0)
            {
                MessageBox.Show("Select Mode 3 first.");
                return;
            }

            int[] bit1 = new int[8]; // bit planes
            int[] bit2 = new int[8];
            int[] bit3 = new int[8];
            int[] bit4 = new int[8];
            int[] bit5 = new int[8];
            int[] bit6 = new int[8];
            int[] bit7 = new int[8];
            int[] bit8 = new int[8];
            int temp1 = 0;
            int temp2 = 0;
            int temp3 = 0;
            int temp4 = 0;
            int temp5 = 0;
            int temp6 = 0;
            int temp7 = 0;
            int temp8 = 0;
            int[] temp_tiles = new int[65536]; // max 4 tile sets
            int size_temp_tiles = 0;

            // tile_set assumed to be 0-3
            // so offset_tiles_ar = 0, 4000, 8000, or c000
            int offset_tiles_ar = 0x4000 * tile_set; // Tile_Arrays is 1 byte per pixel
            int start_tile = 256 * tile_set;

            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Title = "Select a 8bpp Mode 3 Tileset";
            openFileDialog1.Filter = "Tileset (*.chr)|*.chr|All files (*.*)|*.*";

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                Checkpoint();

                System.IO.FileStream fs = (System.IO.FileStream)openFileDialog1.OpenFile();
                if (fs.Length >= 64) // at least one tile.
                {
                    // refactored, loads to the start of the currently selected tile
                    
                    size_temp_tiles = (int)fs.Length;
                    if (size_temp_tiles > 65536) size_temp_tiles = 65536; // max
                    
                    int num_tiles = size_temp_tiles / 64;
                    if (num_tiles < 1) num_tiles = 1; // min
                    if (num_tiles > 1024) num_tiles = 1024; // max

                    // copy file to the temp array.
                    for (int i = 0; i < size_temp_tiles; i++)
                    {
                        temp_tiles[i] = (byte)fs.ReadByte();
                    }
                    
                    
                    for (int i = 0; i < num_tiles; i++) // 256 tiles
                    {
                        if (start_tile + i > 960) break; // skip the last 64
                        
                        int index = 64 * i; // start of current tile
                        for (int y = 0; y < 8; y++) // get 8 sets of bitplanes
                        {
                            // get the 8 bitplanes for each tile row
                            int y2 = y * 2; //0,2,4,6,8,10,12,14
                            bit1[y] = temp_tiles[index + y2];
                            bit2[y] = temp_tiles[index + y2 + 1];
                            bit3[y] = temp_tiles[index + y2 + 16];
                            bit4[y] = temp_tiles[index + y2 + 17];
                            bit5[y] = temp_tiles[index + y2 + 32];
                            bit6[y] = temp_tiles[index + y2 + 33];
                            bit7[y] = temp_tiles[index + y2 + 48];
                            bit8[y] = temp_tiles[index + y2 + 49];

                            int offset = offset_tiles_ar + (i * 64) + (y * 8);
                            for (int x = 7; x >= 0; x--) // right to left
                            {
                                temp1 = bit1[y] & 1;    // get a bit from each bitplane
                                bit1[y] = bit1[y] >> 1;
                                temp2 = bit2[y] & 1;
                                bit2[y] = bit2[y] >> 1;
                                temp3 = bit3[y] & 1;
                                bit3[y] = bit3[y] >> 1;
                                temp4 = bit4[y] & 1;
                                bit4[y] = bit4[y] >> 1;
                                temp5 = bit5[y] & 1;
                                bit5[y] = bit5[y] >> 1;
                                temp6 = bit6[y] & 1;
                                bit6[y] = bit6[y] >> 1;
                                temp7 = bit7[y] & 1;
                                bit7[y] = bit7[y] >> 1;
                                temp8 = bit8[y] & 1;
                                bit8[y] = bit8[y] >> 1;
                                    
                                Tiles.Tile_Arrays[offset + x] =
                                    (temp8 << 7) + (temp7 << 6) + (temp6 << 5) + (temp5 << 4) +
                                    (temp4 << 3) + (temp3 << 2) + (temp2 << 1) + temp1;
                            }
                        }

                    }
                }
                else
                {
                    MessageBox.Show("File size error. Too small.",
                    "File size error", MessageBoxButtons.OK);
                }

                fs.Close();

                common_update2();

                disable_map_click = 1;  // fix bug, double click causing
                                        // mouse event on tilemap
            }

        } // END OF LOAD mode 3 tiles



        private void loadM3ToSelectedTileToolStripMenuItem_Click(object sender, EventArgs e)
        { // TILES / LOAD MODE 3 to selected tile
            if (bg_mode > 0)
            {
                MessageBox.Show("Select Mode 3 first.");
                return;
            }

            int[] bit1 = new int[8]; // 8 bit planes
            int[] bit2 = new int[8];
            int[] bit3 = new int[8];
            int[] bit4 = new int[8];
            int[] bit5 = new int[8];
            int[] bit6 = new int[8];
            int[] bit7 = new int[8];
            int[] bit8 = new int[8];
            int temp1, temp2, temp3, temp4, temp5, temp6, temp7, temp8;
            int[] temp_tiles = new int[65536];
            int size_temp_tiles = 0;

            // tile_set 0-3
            int offset_tiles_ar = (tile_x * 64) + (tile_y * 1024) + (tile_set * 0x4000);

            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Title = "Load tiles to the selected tile";
            openFileDialog1.Filter = "Tileset (*.chr)|*.chr|All files (*.*)|*.*";

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                Checkpoint();

                System.IO.FileStream fs = (System.IO.FileStream)openFileDialog1.OpenFile();
                if (fs.Length >= 64) // at least one tile.
                {
                    size_temp_tiles = (int)fs.Length & 0x1ffc0; // round down
                    if (size_temp_tiles > 0x10000) size_temp_tiles = 0x10000; // max
                    // copy file to the temp array.
                    for (int i = 0; i < size_temp_tiles; i++)
                    {
                        temp_tiles[i] = (byte)fs.ReadByte();
                    }

                    int num_loops;
                    int chr_index = 0;


                    num_loops = size_temp_tiles / 64; // 64 bytes per tile
                    for (int i = 0; i < num_loops; i++)
                    {
                        // get 64 bytes per tile
                        for (int y = 0; y < 8; y++) // get 8 sets of bitplanes
                        {
                            // get the 8 bitplanes for each tile row
                            
                            int y2 = y * 2; 
                            bit1[y] = temp_tiles[chr_index + y2];
                            bit2[y] = temp_tiles[chr_index + y2 + 1];
                            bit3[y] = temp_tiles[chr_index + y2 + 16];
                            bit4[y] = temp_tiles[chr_index + y2 + 17];
                            bit5[y] = temp_tiles[chr_index + y2 + 32];
                            bit6[y] = temp_tiles[chr_index + y2 + 33];
                            bit7[y] = temp_tiles[chr_index + y2 + 48];
                            bit8[y] = temp_tiles[chr_index + y2 + 49];


                            for (int x = 7; x >= 0; x--) // right to left
                            {
                                temp1 = bit1[y] & 1;    // get a bit from each bitplane
                                bit1[y] = bit1[y] >> 1;
                                temp2 = bit2[y] & 1;
                                bit2[y] = bit2[y] >> 1;
                                temp3 = bit3[y] & 1;
                                bit3[y] = bit3[y] >> 1;
                                temp4 = bit4[y] & 1;
                                bit4[y] = bit4[y] >> 1;
                                temp5 = bit5[y] & 1;
                                bit5[y] = bit5[y] >> 1;
                                temp6 = bit6[y] & 1;
                                bit6[y] = bit6[y] >> 1;
                                temp7 = bit7[y] & 1;
                                bit7[y] = bit7[y] >> 1;
                                temp8 = bit8[y] & 1;
                                bit8[y] = bit8[y] >> 1;
                                Tiles.Tile_Arrays[offset_tiles_ar + x] =
                                    (temp8 << 7) + (temp7 << 6) + (temp6 << 5) + (temp5 << 4) +
                                    (temp4 << 3) + (temp3 << 2) + (temp2 << 1) + temp1;
                            }
                            offset_tiles_ar += 8;
                        }
                        chr_index += 64; // bytes per tile

                        //don't go too far, even if more tiles to read
                        if (offset_tiles_ar >= 65536) break; // end of the last set
                    }
                    

                }
                else
                {
                    MessageBox.Show("File size error. Too small.",
                    "File size error", MessageBoxButtons.OK);
                }

                fs.Close();

                common_update2();

                disable_map_click = 1;  // fix bug, double click causing
                                        // mouse event on tilemap
            }

        }



        private void loadM7tilesToolStripMenuItem_Click(object sender, EventArgs e)
        { // TILES / LOAD Mode 7 tiles (assume 1 set)

            if (bg_mode < 1)
            {
                MessageBox.Show("Select Mode 7 first.");
                return;
            }

            // no bitplanes
            int temp1 = 0;
            
            int[] temp_tiles = new int[16384]; // max 1 tile set
            int size_temp_tiles = 0;

            // tile_set assumed to be 0
            //int offset_tiles_ar = 0;
            //int start_tile = 0;

            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Title = "Select a 8bpp Mode 3 Tileset";
            openFileDialog1.Filter = "Tileset (*.chr)|*.chr|All files (*.*)|*.*";

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                Checkpoint();

                System.IO.FileStream fs = (System.IO.FileStream)openFileDialog1.OpenFile();
                if (fs.Length >= 64) // at least one tile.
                {
                    // refactored


                    size_temp_tiles = (int)fs.Length;
                    if (size_temp_tiles > 16384) size_temp_tiles = 16384; // max

                    int num_tiles = size_temp_tiles / 64;
                    if (num_tiles < 1) num_tiles = 1; // min
                    if (num_tiles > 256) num_tiles = 256; // max

                    // copy file to the temp array.
                    for (int i = 0; i < size_temp_tiles; i++)
                    {
                        temp_tiles[i] = (byte)fs.ReadByte();
                    }


                    int index = 0;
                    for (int i = 0; i < num_tiles; i++) // 256 tiles
                    {
                        
                        for (int y = 0; y < 8; y++) // get 8 rows
                        {
                            for (int x = 0; x < 8; x++) // 8 pixels per row
                            {

                                temp1 = temp_tiles[index];
                                Tiles.Tile_Arrays[index] = (byte)temp1;
                                index++;
                            }
                        }
                    }


                }
                else
                {
                    MessageBox.Show("File size error. Too small.",
                    "File size error", MessageBoxButtons.OK);
                }

                fs.Close();

                common_update2();

                disable_map_click = 1;  // fix bug, double click causing
                                        // mouse event on tilemap
            }

        } // END OF LOAD m7 tiles



        private void loadM7ToSelectedTileToolStripMenuItem_Click(object sender, EventArgs e)
        { // tiles / load mode 7 tiles to selected tile (assume 1 or less tileset)
            if (bg_mode < 1)
            {
                MessageBox.Show("Select Mode 7 first.");
                return;
            }

            // no bitplanes
            int temp1 = 0;

            int[] temp_tiles = new int[16384]; // max 1 tile set
            int size_temp_tiles = 0;


            // tile_set always 0
            
            int offset_tiles_ar = (tile_x * 64) + (tile_y * 1024);

            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Title = "Load tiles to the selected tile";
            openFileDialog1.Filter = "Tileset (*.chr)|*.chr|All files (*.*)|*.*";

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                Checkpoint();

                System.IO.FileStream fs = (System.IO.FileStream)openFileDialog1.OpenFile();
                if (fs.Length >= 64) // at least one tile.
                {
                    size_temp_tiles = (int)fs.Length & 0x1ffc0; // round down
                    if (size_temp_tiles > 16384) size_temp_tiles = 16384; // max
                    // copy file to the temp array.
                    for (int i = 0; i < size_temp_tiles; i++)
                    {
                        temp_tiles[i] = (byte)fs.ReadByte();
                    }

                    int num_loops;
                    int index = 0;
                    int chr_index = offset_tiles_ar;

                    num_loops = size_temp_tiles / 64; // 64 bytes per tile
                    for (int i = 0; i < num_loops; i++)
                    {
                        // get 64 bytes per tile
                        for (int y = 0; y < 8; y++) // get 8 rows
                        {
                            
                            for (int x = 7; x >= 0; x--) // 8 pixels per row
                            {
                                temp1 = temp_tiles[index];
                                Tiles.Tile_Arrays[chr_index] = (byte)temp1;
                                index++;
                                chr_index++;

                            }
                            
                        }
                        
                        //don't go too far, even if more tiles to read
                        if (chr_index >= 16384) break; // end of the last set
                    }
                    
                }
                else
                {
                    MessageBox.Show("File size error. Too small.",
                    "File size error", MessageBoxButtons.OK);
                }

                fs.Close();

                common_update2();

                disable_map_click = 1;  // fix bug, double click causing
                                        // mouse event on tilemap
            }

        }



        private void saveMode3SNESToolStripMenuItem_Click(object sender, EventArgs e)
        { // TILES / SAVE Mode 3 x 1 tileset

            // allow, even if in mode 7 view

            byte[] out_array = new byte[16384]; // 256 * 64
            int out_index = 0;
            int[] bit1 = new int[8]; // bit planes
            int[] bit2 = new int[8];
            int[] bit3 = new int[8];
            int[] bit4 = new int[8];
            int[] bit5 = new int[8];
            int[] bit6 = new int[8];
            int[] bit7 = new int[8];
            int[] bit8 = new int[8];
            int temp;

            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "Tileset (*.chr)|*.chr|RLE File (*.rle)|*.rle";
            saveFileDialog1.Title = "Save a 8bpp Mode 3 Tileset";
            saveFileDialog1.ShowDialog();

            if (saveFileDialog1.FileName != "")
            {
                System.IO.FileStream fs = (System.IO.FileStream)saveFileDialog1.OpenFile();
                for (int i = 0; i < 256; i++) // 256 tiles
                {
                    int z = (tile_set * 256 * 8 * 8) + (64 * i); // start of current tile
                    for (int y = 0; y < 8; y++)
                    {
                        for (int x = 0; x < 8; x++)
                        {
                            temp = Tiles.Tile_Arrays[z + (y * 8) + x];
                            bit1[y] = (bit1[y] << 1) + (temp & 1);
                            bit2[y] = (bit2[y] << 1) + ((temp >> 1) & 1); // changed
                            bit3[y] = (bit3[y] << 1) + ((temp >> 2) & 1); 
                            bit4[y] = (bit4[y] << 1) + ((temp >> 3) & 1);
                            bit5[y] = (bit5[y] << 1) + ((temp >> 4) & 1);
                            bit6[y] = (bit6[y] << 1) + ((temp >> 5) & 1);
                            bit7[y] = (bit7[y] << 1) + ((temp >> 6) & 1);
                            bit8[y] = (bit8[y] << 1) + ((temp >> 7) & 1);
                        }
                    }
                    for (int j = 0; j < 8; j++)
                    {
                        out_array[out_index++] = (byte)bit1[j];
                        out_array[out_index++] = (byte)bit2[j];
                    }
                    for (int j = 0; j < 8; j++)
                    {
                        out_array[out_index++] = (byte)bit3[j];
                        out_array[out_index++] = (byte)bit4[j];
                    }
                    for (int j = 0; j < 8; j++)
                    {
                        out_array[out_index++] = (byte)bit5[j];
                        out_array[out_index++] = (byte)bit6[j];
                    }
                    for (int j = 0; j < 8; j++)
                    {
                        out_array[out_index++] = (byte)bit7[j];
                        out_array[out_index++] = (byte)bit8[j];
                    }
                }
                string ext = System.IO.Path.GetExtension(saveFileDialog1.FileName);

                int final_size = 16384;
                if(tile_set == 3)
                {
                    final_size = 12288; // skip the last 64 tiles
                }

                if (ext == ".chr")
                {
                    for (int j = 0; j < final_size; j++)
                    {
                        fs.WriteByte(out_array[j]);
                    }

                    fs.Close();
                }
                else if (ext == ".rle")
                {
                    int rle_length = convert_RLE(out_array, final_size);
                    // global rle_array[] now has our compressed data
                    for (int i = 0; i < rle_length; i++)
                    {
                        fs.WriteByte(rle_array[i]);
                    }

                    float percent = (float)rle_length / final_size;
                    fs.Close();

                    MessageBox.Show(String.Format("RLE size is {0}, or {1:P2}", rle_length, percent));
                }
                else
                { // something went wrong.
                    fs.Close();
                }

            }

        } // END OF SAVE mode 3 tiles x 1



        private void saveMode3X2ToolStripMenuItem_Click(object sender, EventArgs e)
        { // TILES / SAVE Mode 3 x 2 tilesets
          // we can't do x 4 tilesets because of a size limitation in the RLE code

            // allow, even if in mode 7 view

            byte[] out_array = new byte[32768]; // 256 * 64 * 2
            int out_index = 0;
            int[] bit1 = new int[8]; // bit planes
            int[] bit2 = new int[8];
            int[] bit3 = new int[8];
            int[] bit4 = new int[8];
            int[] bit5 = new int[8];
            int[] bit6 = new int[8];
            int[] bit7 = new int[8];
            int[] bit8 = new int[8];
            int temp;
            int temp_tile_set = tile_set;
            if (temp_tile_set > 2) temp_tile_set = 2; // don't overflow

            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "Tileset (*.chr)|*.chr|RLE File (*.rle)|*.rle";
            saveFileDialog1.Title = "Save 8bpp Mode 3 Tilesets x2";
            saveFileDialog1.ShowDialog();

            if (saveFileDialog1.FileName != "")
            {
                System.IO.FileStream fs = (System.IO.FileStream)saveFileDialog1.OpenFile();
                for (int i = 0; i < 512; i++) // 512 tiles
                {
                    int z = (temp_tile_set * 256 * 8 * 8) + (64 * i); // start of current tile
                    for (int y = 0; y < 8; y++)
                    {
                        for (int x = 0; x < 8; x++)
                        {
                            temp = Tiles.Tile_Arrays[z + (y * 8) + x];
                            bit1[y] = (bit1[y] << 1) + (temp & 1);
                            bit2[y] = (bit2[y] << 1) + ((temp >> 1) & 1); // changed
                            bit3[y] = (bit3[y] << 1) + ((temp >> 2) & 1); 
                            bit4[y] = (bit4[y] << 1) + ((temp >> 3) & 1);
                            bit5[y] = (bit5[y] << 1) + ((temp >> 4) & 1);
                            bit6[y] = (bit6[y] << 1) + ((temp >> 5) & 1);
                            bit7[y] = (bit7[y] << 1) + ((temp >> 6) & 1);
                            bit8[y] = (bit8[y] << 1) + ((temp >> 7) & 1);
                        }
                    }
                    for (int j = 0; j < 8; j++)
                    {
                        out_array[out_index++] = (byte)bit1[j];
                        out_array[out_index++] = (byte)bit2[j];
                    }
                    for (int j = 0; j < 8; j++)
                    {
                        out_array[out_index++] = (byte)bit3[j];
                        out_array[out_index++] = (byte)bit4[j];
                    }
                    for (int j = 0; j < 8; j++)
                    {
                        out_array[out_index++] = (byte)bit5[j];
                        out_array[out_index++] = (byte)bit6[j];
                    }
                    for (int j = 0; j < 8; j++)
                    {
                        out_array[out_index++] = (byte)bit7[j];
                        out_array[out_index++] = (byte)bit8[j];
                    }
                }
                string ext = System.IO.Path.GetExtension(saveFileDialog1.FileName);

                int final_size = 32768;
                if(temp_tile_set == 2)
                {
                    final_size = 28672; // skip the last 64 tiles
                }

                if (ext == ".chr")
                {
                    for (int j = 0; j < final_size; j++)
                    {
                        fs.WriteByte(out_array[j]);
                    }

                    fs.Close();
                }
                else if (ext == ".rle")
                {
                    int rle_length = convert_RLE(out_array, final_size);
                    // global rle_array[] now has our compressed data
                    for (int i = 0; i < rle_length; i++)
                    {
                        fs.WriteByte(rle_array[i]);
                    }

                    float percent = (float)rle_length / final_size;
                    fs.Close();

                    MessageBox.Show(String.Format("RLE size is {0}, or {1:P2}", rle_length, percent));
                }
                else
                { // something went wrong.
                    fs.Close();
                }

            }


        } // END OF SAVE mode 3 tiles x 2



        private void saveM3TilesInARangeToolStripMenuItem_Click(object sender, EventArgs e)
        { // mode 3, save tiles in a range, opens an input box
          // see form4.cs for the function 

            // allow, even if in mode 7

            // open Form4, Options for saving a specific range of tiles
            if (newChild4 != null)
            {
                newChild4.BringToFront();
            }
            else
            {
                newChild4 = new frmSave3();
                newChild4.Owner = this;

                newChild4.Top = this.Top + 100;
                newChild4.Left = this.Left + 100;

                newChild4.Show();

            }
        }



        private void saveM7tilesToolStripMenuItem_Click(object sender, EventArgs e)
        { // TILES / SAVE Mode 7 x 1

            if (bg_mode < 1)
            {
                MessageBox.Show("Select Mode 7 first.");
                return;
            }
            if (tile_set > 0)
            {
                MessageBox.Show("Select Tileset 1 first.");
                return;
            }

            byte[] out_array = new byte[16384]; // 256 * 64
            int out_index = 0;
            // no bitplanes in mode 7
            int temp;

            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "Tileset (*.chr)|*.chr|RLE File (*.rle)|*.rle";
            saveFileDialog1.Title = "Save a 8bpp Mode 7 Tileset";
            saveFileDialog1.ShowDialog();

            if (saveFileDialog1.FileName != "")
            {
                System.IO.FileStream fs = (System.IO.FileStream)saveFileDialog1.OpenFile();
                for (int i = 0; i < 256; i++) // 256 tiles
                {
                    int z = (tile_set * 256 * 8 * 8) + (64 * i); // start of current tile
                    for (int y = 0; y < 8; y++)
                    {
                        for (int x = 0; x < 8; x++)
                        {
                            temp = Tiles.Tile_Arrays[z + (y * 8) + x];
                            out_array[out_index++] = (byte)temp;
                        }
                    }
                    
                }
                string ext = System.IO.Path.GetExtension(saveFileDialog1.FileName);
                if (ext == ".chr")
                {
                    for (int j = 0; j < 16384; j++)
                    {
                        fs.WriteByte(out_array[j]);
                    }

                    fs.Close();
                }
                else if (ext == ".rle")
                {
                    int rle_length = convert_RLE(out_array, 16384);
                    // global rle_array[] now has our compressed data
                    for (int i = 0; i < rle_length; i++)
                    {
                        fs.WriteByte(rle_array[i]);
                    }

                    float percent = (float)rle_length / 16384;
                    fs.Close();

                    MessageBox.Show(String.Format("RLE size is {0}, or {1:P2}", rle_length, percent));
                }
                else
                { // something went wrong.
                    fs.Close();
                }

            }

        } // END OF SAVE mode 7 tiles x 1



        private void saveM7inRangeToolStripMenuItem_Click(object sender, EventArgs e)
        { // TILES / SAVE M7 tiles in a range
          // see form5.cs for the function

            if (bg_mode < 1)
            {
                MessageBox.Show("Select Mode 7 first.");
                return;
            }
            if(tile_set > 0)
            {
                MessageBox.Show("Select Tileset 1 first.");
                return;
            }

            // open Form5, Options for saving a specific range of tiles
            if (newChild5 != null)
            {
                newChild5.BringToFront();
            }
            else
            {
                newChild5 = new frmSave7();
                newChild5.Owner = this;

                newChild5.Top = this.Top + 100;
                newChild5.Left = this.Left + 100;

                newChild5.Show();

            }


        } // END OF SAVE m7 tiles in a range



        private void clearAllTilesToolStripMenuItem1_Click(object sender, EventArgs e)
        { // TILES / CLEAR ALL TILES
            Checkpoint();

            for (int i = 0; i < 65536; i++)
            {
                Tiles.Tile_Arrays[i] = 0;
            }
            common_update2();
        }



        // PALETTES **************************************************

        private void loadPaletteToolStripMenuItem_Click(object sender, EventArgs e)
        { // PALETTE / LOAD FULL PALETTE
            byte[] pal_array = new byte[512]; // 256 entries * 2 bytes, little endian
            int temp, max_size;

            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Title = "Select a Palette file";
            openFileDialog1.Filter = "Palette files (*.pal)|*.pal|All files (*.*)|*.*";

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                System.IO.FileStream fs = (System.IO.FileStream)openFileDialog1.OpenFile();
                max_size = (int)fs.Length & 0x3fe; // should be even
                if(fs.Length > 512)
                {
                    max_size = 512; // handle unusually large
                }
                if (max_size >= 2)
                {
                    for (int i = 0; i < 512; i++)
                    {
                        if (i >= max_size) break;
                        pal_array[i] = (byte)fs.ReadByte();
                    }

                    for (int i = 0; i < 512; i += 2)
                    {
                        if(i >= max_size) break;
                        int j;
                        temp = pal_array[i] + (pal_array[i + 1] << 8);
                        //if ((i == 0x20) || (i == 0x40) || (i == 0x60) || (i == 0x80) ||
                        //    (i == 0xa0) || (i == 0xc0) || (i == 0xe0)) temp = 0;
                        // make the left most boxes black, but not the top most
                        j = i / 2;
                        Palettes.pal_r[j] = (byte)((temp & 0x001f) << 3);
                        Palettes.pal_g[j] = (byte)((temp & 0x03e0) >> 2);
                        Palettes.pal_b[j] = (byte)((temp & 0x7c00) >> 7);
                    }

                    // update the numbers in the boxes
                    rebuild_pal_boxes();
                    update_palette();
                    common_update2();
                }
                else
                {
                    MessageBox.Show("File size error. Expected 512 bytes.",
                    "File size error", MessageBoxButtons.OK);
                }

                fs.Close();

                disable_map_click = 1;  // fix bug, double click causing
                                        // mouse event on tilemap
            }
        } // END OF LOAD FULL PALETTE



        private void load32BytesToolStripMenuItem_Click(object sender, EventArgs e)
        { // PALETTE / LOAD 32 bytes
            // load just 1 palette (16 colors = 32 bytes)
            byte[] pal_array = new byte[32]; // 16 entries * 2 bytes, little endian
            int temp, max_size;

            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Title = "Select a Palette file";
            openFileDialog1.Filter = "Palette files (*.pal)|*.pal|All files (*.*)|*.*";

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                System.IO.FileStream fs = (System.IO.FileStream)openFileDialog1.OpenFile();
                max_size = (int)fs.Length & 0x00fe; // should be even
                if (fs.Length > 0xfe)
                {
                    max_size = 0xfe; // handle unusually large
                }
                if (max_size >= 2)
                {
                    for (int i = 0; i < 32; i++)
                    {
                        if (i >= max_size) break;
                        pal_array[i] = (byte)fs.ReadByte();
                    }

                    for (int i = 0; i < 32; i += 2)
                    {
                        if (i >= max_size) break;
                        int j;
                        temp = pal_array[i] + (pal_array[i + 1] << 8);
                        //if ((i == 0) && (pal_y != 0)) continue;
                        // skip the left most boxes, but not the top most

                        j = (i / 2) + (pal_y * 16);
                        Palettes.pal_r[j] = (byte)((temp & 0x001f) << 3);
                        Palettes.pal_g[j] = (byte)((temp & 0x03e0) >> 2);
                        Palettes.pal_b[j] = (byte)((temp & 0x7c00) >> 7);
                    }

                    // update the numbers in the boxes
                    rebuild_pal_boxes();
                    update_palette();
                    common_update2();
                }
                else
                {
                    MessageBox.Show("File size error. Expected 32 bytes.",
                    "File size error", MessageBoxButtons.OK);
                }

                fs.Close();

                disable_map_click = 1;  // fix bug, double click causing
                                        // mouse event on tilemap
            }
        } // END LOAD 32 byte palette



        private void loadPaletteFromRGBToolStripMenuItem_Click(object sender, EventArgs e)
        { // PALETTE / LOAD FROM RGB
            byte[] pal_array = new byte[768]; // 256 entries * 3 colors
            int max_size;

            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Title = "Select a Palette file";
            openFileDialog1.Filter = "Palette files (*.pal)|*.pal|All files (*.*)|*.*";

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                System.IO.FileStream fs = (System.IO.FileStream)openFileDialog1.OpenFile();
                max_size = (int)fs.Length;
                max_size = (max_size / 3) * 3; // should be multiple of 3
                if (fs.Length > 768) max_size = 768; // handle unusually large

                if (max_size >= 3)
                {
                    for (int i = 0; i < 768; i++)
                    {
                        if (i >= max_size) break;
                        pal_array[i] = (byte)fs.ReadByte();
                    }

                    int offset = 0;

                    for (int i = 0; i < 768; i += 3) //256 * 3 color
                    {
                        if (i >= max_size) break;
                        Palettes.pal_r[offset] = (byte)(pal_array[i] & 0xf8);
                        Palettes.pal_g[offset] = (byte)(pal_array[i + 1] & 0xf8);
                        Palettes.pal_b[offset] = (byte)(pal_array[i + 2] & 0xf8);
                        offset++;
                    }

                    // update the numbers in the boxes
                    rebuild_pal_boxes();
                    update_palette();
                    common_update2();
                }
                else
                {
                    MessageBox.Show("File size error. Expected 3 - 768 bytes.",
                    "File size error", MessageBoxButtons.OK);
                }

                fs.Close();

                disable_map_click = 1;  // fix bug, double click causing
                                        // mouse event on tilemap
            }
        } // END PALETTE LOAD FROM RGB



        private void savePaletteToolStripMenuItem_Click(object sender, EventArgs e)
        { // PALETTE / SAVE PALETTE
            byte[] pal_array = new byte[512]; // 256 entries * 2 bytes, little endian
            int temp;

            for (int i = 0; i < 256; i++)
            {
                temp = ((Palettes.pal_r[i] & 0xf8) >> 3) + ((Palettes.pal_g[i] & 0xf8) << 2) + ((Palettes.pal_b[i] & 0xf8) << 7);
                pal_array[(i * 2)] = (byte)(temp & 0xff); // little end first
                pal_array[(i * 2) + 1] = (byte)((temp >> 8) & 0x7f); // 15 bit palette
            }

            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "Palette files (*.pal)|*.pal|All files (*.*)|*.*";
            saveFileDialog1.Title = "Save a Palette";
            saveFileDialog1.ShowDialog();

            if (saveFileDialog1.FileName != "")
            {
                System.IO.FileStream fs = (System.IO.FileStream)saveFileDialog1.OpenFile();
                for (int i = 0; i < 512; i++)
                {
                    fs.WriteByte(pal_array[i]);
                }
                fs.Close();
            }
        }



        private void save32BytesToolStripMenuItem_Click(object sender, EventArgs e)
        { // PALETTE / SAVE 32 bytes
            // save just 1 palette (16 colors = 32 bytes)
            byte[] pal_array = new byte[32]; // 16 entries * 2 bytes, little endian
            int temp;

            for (int i = 0; i < 16; i++)
            {
                int j = i + (pal_y * 16);
                temp = ((Palettes.pal_r[j] & 0xf8) >> 3) + ((Palettes.pal_g[j] & 0xf8) << 2) + ((Palettes.pal_b[j] & 0xf8) << 7);
                pal_array[(i * 2)] = (byte)(temp & 0xff); // little end first
                pal_array[(i * 2) + 1] = (byte)((temp >> 8) & 0x7f); // 15 bit palette
            }

            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "Palette files (*.pal)|*.pal|All files (*.*)|*.*";
            saveFileDialog1.Title = "Save a Palette";
            saveFileDialog1.ShowDialog();

            if (saveFileDialog1.FileName != "")
            {
                System.IO.FileStream fs = (System.IO.FileStream)saveFileDialog1.OpenFile();
                for (int i = 0; i < 32; i++)
                {
                    fs.WriteByte(pal_array[i]);
                }
                fs.Close();
            }
        }



        private void savePaletteAsASMToolStripMenuItem_Click(object sender, EventArgs e)
        { // PALETTE / SAVE PALETTE AS ASM
            byte[] pal_array = new byte[512]; // 256 entries * 2 bytes, little endian
            int temp;

            for (int i = 0; i < 256; i++)
            {
                temp = ((Palettes.pal_r[i] & 0xf8) >> 3) + ((Palettes.pal_g[i] & 0xf8) << 2) + ((Palettes.pal_b[i] & 0xf8) << 7);
                pal_array[(i * 2)] = (byte)(temp & 0xff); // little end first
                pal_array[(i * 2) + 1] = (byte)((temp >> 8) & 0x7f); // 15 bit palette
            }

            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "ASM File (*.asm)|*.asm|All files (*.*)|*.*";
            saveFileDialog1.Title = "Save a Palette as ASM";
            saveFileDialog1.ShowDialog();

            if (saveFileDialog1.FileName != "")
            {
                using (StreamWriter sw = new StreamWriter(saveFileDialog1.OpenFile()))
                {
                    int count = 0;
                    string str = "";
                    sw.Write("Palette:\r\n");
                    for (int i = 0; i < 16; i++)
                    {
                        sw.Write("\r\n.byte ");
                        for (int j = 0; j < 16; j++)
                        {
                            str = pal_array[count].ToString("X2"); // convert int to hex string
                            sw.Write("$" + str + ", ");
                            count++;
                            str = pal_array[count].ToString("X2");
                            sw.Write("$" + str);
                            if (j < 15)
                            {
                                sw.Write(", ");
                            }
                            count++;
                        }
                    }
                    sw.Write("\r\n\r\n");
                    sw.Close();
                }
            }
        } // END SAVE PALETTE AS ASM



        private void savePalAsRGBToolStripMenuItem_Click(object sender, EventArgs e)
        { // PALETTE / SAVE PAL AS RBG (for YY-CHR)
            byte[] pal_array = new byte[768]; // 256 entries * 3 = r,g,b
            
            int offset = 0;
            for (int i = 0; i < 256; i++)
            {
                pal_array[offset++] = (byte)(Palettes.pal_r[i] & 0xf8);
                pal_array[offset++] = (byte)(Palettes.pal_g[i] & 0xf8);
                pal_array[offset++] = (byte)(Palettes.pal_b[i] & 0xf8);
            }

            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "Palette (*.pal)|*.pal|All files (*.*)|*.*";
            saveFileDialog1.Title = "Save a Palette";
            saveFileDialog1.ShowDialog();

            if (saveFileDialog1.FileName != "")
            {
                System.IO.FileStream fs = (System.IO.FileStream)saveFileDialog1.OpenFile();
                for (int i = 0; i < 768; i++)
                {
                    fs.WriteByte(pal_array[i]);
                }
                fs.Close();
            }
        } // END SAVE PAL AS RGB



        // CHANGE BG MODE **************************************************

        private void bGmode3TopToolStripMenuItem_Click(object sender, EventArgs e)
        { // BG MODE / MODE 3

            mode3TopToolStripMenuItem.Checked = true;
            mode7ToolStripMenuItem.Checked = false;
            mode7preToolStripMenuItem.Checked = false;
            label11.Text = "Mode 3";
            bg_mode = BG_MODE_3; //mode 3
            which_map = 0;
            MapMenuSwitch(); // only use 1st map for mode 3

            textBox6.Text = map_height.ToString();

            update_tilemap();

        }



        private void bGmode7ToolStripMenuItem_Click(object sender, EventArgs e)
        { // BG MODE / MODE 7

            mode3TopToolStripMenuItem.Checked = false;
            mode7ToolStripMenuItem.Checked = true;
            mode7preToolStripMenuItem.Checked = false;
            label11.Text = "Mode 7";
            bg_mode = BG_MODE_7; //mode 7
            
            if (tile_set > 0)
            {
                set1ToolStripMenuItem.Checked = true;
                set2ToolStripMenuItem.Checked = false;
                set3ToolStripMenuItem.Checked = false;
                set4ToolStripMenuItem.Checked = false;
                
                label10.Text = "1";
                tile_set = 0; // force mode 7 to use the first tileset
                tile_show_num();
            }

            map_height = 32;
            textBox6.Text = "128";


            common_update2();
        }



        private void bGmode7preToolStripMenuItem_Click(object sender, EventArgs e)
        { // mode 7 preview (zoomed out)

            mode3TopToolStripMenuItem.Checked = false;
            mode7ToolStripMenuItem.Checked = false;
            mode7preToolStripMenuItem.Checked = true;
            label11.Text = "Mode 7 Preview";
            bg_mode = BG_MODE_7P; //mode 7 preview
            
            if (tile_set > 0)
            {
                set1ToolStripMenuItem.Checked = true;
                set2ToolStripMenuItem.Checked = false;
                set3ToolStripMenuItem.Checked = false;
                set4ToolStripMenuItem.Checked = false;
                
                label10.Text = "1";
                tile_set = 0; // force mode 7 to use first tile set
                tile_show_num();
            }

            map_height = 32;
            textBox6.Text = "128";

            common_update2();
        }



        



        // CHANGE TILESET VIEW **************************************************

        public void set1_change()
        {
            set1ToolStripMenuItem.Checked = true;
            set2ToolStripMenuItem.Checked = false;
            set3ToolStripMenuItem.Checked = false;
            set4ToolStripMenuItem.Checked = false;
            
            tile_set = 0;
            label10.Text = "1";

            if (newChild != null)
            {
                newChild.update_tile_box();
            }
            
            common_update2(); // includes map
            update_palette();
            tile_show_num();
        }
        private void set1ToolStripMenuItem_Click(object sender, EventArgs e)
        { // TILESET / SET 1

            set1_change();
        }


        public void set2_change()
        {
            if(bg_mode != 0)
            {
                MessageBox.Show("Mode 7 can only use tileset 1");
                return;
            }
            
            set1ToolStripMenuItem.Checked = false;
            set2ToolStripMenuItem.Checked = true;
            set3ToolStripMenuItem.Checked = false;
            set4ToolStripMenuItem.Checked = false;
            
            tile_set = 1;
            label10.Text = "2";

            if (newChild != null)
            {
                newChild.update_tile_box();
            }
            
            common_update2(); // includes map
            update_palette();
            tile_show_num();
        }

        private void set2ToolStripMenuItem_Click(object sender, EventArgs e)
        { // TILESET / SET 2

            set2_change();
        }


        public void set3_change()
        {
            if (bg_mode != 0)
            {
                MessageBox.Show("Mode 7 can only use tileset 1");
                return;
            }

            set1ToolStripMenuItem.Checked = false;
            set2ToolStripMenuItem.Checked = false;
            set3ToolStripMenuItem.Checked = true;
            set4ToolStripMenuItem.Checked = false;
            
            tile_set = 2;
            label10.Text = "3";

            if (newChild != null)
            {
                newChild.update_tile_box();
            }
            
            common_update2(); // includes map
            update_palette();
            tile_show_num();
        }

        private void set3ToolStripMenuItem_Click(object sender, EventArgs e)
        { // TILESET / SET 3

            set3_change();
        }


        public void set4_change()
        {
            if (bg_mode != 0)
            {
                MessageBox.Show("Mode 7 can only use tileset 1");
                return;
            }

            if(tile_y >= 12)
            {
                tile_y = 0;
                tile_x = 0;
                tile_num = (tile_y * 16) + tile_x;
                tile_show_num();
            }

            set1ToolStripMenuItem.Checked = false;
            set2ToolStripMenuItem.Checked = false;
            set3ToolStripMenuItem.Checked = false;
            set4ToolStripMenuItem.Checked = true;
            
            tile_set = 3;
            label10.Text = "4";

            if (newChild != null)
            {
                newChild.update_tile_box();
            }
            
            common_update2(); // includes map
            update_palette();
            tile_show_num();
        }

        private void set4ToolStripMenuItem_Click(object sender, EventArgs e)
        { // TILESET / SET 4

            set4_change();
        }



        // SELECT A MAP (MODE 7 only) ***************************************



        private void map1ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            which_map = 0;
            MapMenuSwitch();
            common_update2();
        }

        private void map2ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            which_map = 1;
            MapMenuSwitch();
            common_update2();
        }

        private void map3ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            which_map = 2;
            MapMenuSwitch();
            common_update2();
        }

        private void map4ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            which_map = 3;
            MapMenuSwitch();
            common_update2();
        }

        private void map5ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            which_map = 4;
            MapMenuSwitch();
            common_update2();
        }

        private void map6ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            which_map = 5;
            MapMenuSwitch();
            common_update2();
        }

        private void map7ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            which_map = 6;
            MapMenuSwitch();
            common_update2();
        }

        private void map8ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            which_map = 7;
            MapMenuSwitch();
            common_update2();
        }

        private void map9ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            which_map = 8;
            MapMenuSwitch();
            common_update2();
        }

        private void map10ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            which_map = 9;
            MapMenuSwitch();
            common_update2();
        }

        private void map11ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            which_map = 10;
            MapMenuSwitch();
            common_update2();
        }

        private void map12ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            which_map = 11;
            MapMenuSwitch();
            common_update2();
        }

        private void map13ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            which_map = 12;
            MapMenuSwitch();
            common_update2();
        }

        private void map14ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            which_map = 13;
            MapMenuSwitch();
            common_update2();
        }



        private void map15ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            which_map = 14;
            MapMenuSwitch();
            common_update2();
        }



        private void map16ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            which_map = 15;
            MapMenuSwitch();
            common_update2();
        }



        // INFO ******************************


        private void aboutM8TEToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("M8TE = 8bpp Tile Editor for SNES, by Doug Fraker, 2021.\n\nnesdoug.com");
        }
        


    }
}
