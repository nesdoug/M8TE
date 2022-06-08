namespace M8TE
{
    public static class TilesU // undo backup copy
    {
        public static int[] Tile_Arrays = new int[4 * 256 * 8 * 8]; // 65536 
    }

        public static class Tiles
    {
        public static int[] Tile_Arrays = new int[4 * 256 * 8 * 8]; // 65536  
                                                                    // 4 sets, 256 tiles, 8 high, 8 wide
                                                                    // values 0-255
        public static int[] Tile_Copier = new int[8 * 8]; // one tile
        public static bool Has_Copied = false;

        public static void shift_left()
        {
            int z = (frmMain.tile_set * 256 * 8 * 8) + (frmMain.tile_num * 8 * 8); // base index
            for (int y = 0; y < 8; y++)
            {
                int temp = Tile_Arrays[z + (y * 8)]; // save the left most
                for (int x = 0; x < 7; x++)
                {
                    Tile_Arrays[z + (y * 8) + x] = Tile_Arrays[z + (y * 8) + x + 1];
                }
                Tile_Arrays[z + (y * 8) + 7] = temp; // put it on the right
            }
        }

        public static void shift_right()
        {
            int z = (frmMain.tile_set * 256 * 8 * 8) + (frmMain.tile_num * 8 * 8); // base index
            for (int y = 0; y < 8; y++)
            {
                int temp = Tile_Arrays[z + (y * 8) + 7]; // save the right most
                for (int x = 6; x >= 0; x--)
                {
                    Tile_Arrays[z + (y * 8) + x + 1] = Tile_Arrays[z + (y * 8) + x];
                }
                Tile_Arrays[z + (y * 8)] = temp; // put it on the left
            }
        }

        public static void shift_up()
        {
            int z = (frmMain.tile_set * 256 * 8 * 8) + (frmMain.tile_num * 8 * 8); // base index
            for (int x = 0; x < 8; x++)
            {
                int temp = Tile_Arrays[z + x]; // save the top most
                for (int y = 0; y < 7; y++)
                {
                    Tile_Arrays[z + (y * 8) + x] = Tile_Arrays[z + ((y + 1) * 8) + x];
                }
                Tile_Arrays[z + 56 + x] = temp; // put it on the bottom
            }
        }

        public static void shift_down()
        {
            int z = (frmMain.tile_set * 256 * 8 * 8) + (frmMain.tile_num * 8 * 8); // base index
            for (int x = 0; x < 8; x++)
            {
                int temp = Tile_Arrays[z + 56 + x]; // save the bottom most
                for (int y = 6; y >= 0; y--)
                {
                    Tile_Arrays[z + ((y + 1) * 8) + x] = Tile_Arrays[z + (y * 8) + x];
                }
                Tile_Arrays[z + x] = temp; // put it on the top
            }
        }

        public static void tile_copy()
        {
            int z = (frmMain.tile_set * 256 * 8 * 8) + (frmMain.tile_num * 8 * 8); // base index
            for (int x = 0; x < 64; x++)
            {
                Tile_Copier[x] = Tile_Arrays[z + x];
            }
            Has_Copied = true;
        }

        public static void tile_paste()
        {
            if (Has_Copied == true)
            {
                int z = (frmMain.tile_set * 256 * 8 * 8) + (frmMain.tile_num * 8 * 8); // base index
                if(frmMain.bg_mode == 2) //2bpp
                {
                    for (int x = 0; x < 64; x++)
                    {
                        Tile_Arrays[z + x] = Tile_Copier[x] & 3; //values 0-3
                    }
                }
                else
                {
                    for (int x = 0; x < 64; x++)
                    {
                        Tile_Arrays[z + x] = Tile_Copier[x];
                    }
                }
                
            }
        }

        public static void tile_delete()
        {
            int z = (frmMain.tile_set * 256 * 8 * 8) + (frmMain.tile_num * 8 * 8); // base index
            for (int x = 0; x < 64; x++)
            {
                Tile_Arrays[z + x] = 0;
            }
        }

        public static void tile_h_flip()
        {
            int z = (frmMain.tile_set * 256 * 8 * 8) + (frmMain.tile_num * 8 * 8); // base index
            for (int y = 0; y < 8; y++)
            {
                for (int x = 0; x < 4; x++)
                {
                    int temp = Tile_Arrays[z + (y * 8) + x];
                    Tile_Arrays[z + (y * 8) + x] = Tile_Arrays[z + (y * 8) + (7 - x)];
                    Tile_Arrays[z + (y * 8) + (7 - x)] = temp;
                }
            }

        }

        public static void tile_v_flip()
        {
            int z = (frmMain.tile_set * 256 * 8 * 8) + (frmMain.tile_num * 8 * 8); // base index
            for (int y = 0; y < 4; y++)
            {
                for (int x = 0; x < 8; x++)
                {
                    int temp = Tile_Arrays[z + (y * 8) + x];
                    Tile_Arrays[z + (y * 8) + x] = Tile_Arrays[z + ((7 - y) * 8) + x];
                    Tile_Arrays[z + ((7 - y) * 8) + x] = temp;
                }
            }
        }

        public static void tile_rot_cw() // R, rotate clockwise
        {
            int z = (frmMain.tile_set * 256 * 8 * 8) + (frmMain.tile_num * 8 * 8); // base index
            int[] temp_arr = new int[64];
            int count = 0;
            for(int x = 0; x < 8; x++)
            {
                for (int y = 7; y >= 0; y--)
                {
                    temp_arr[count++] = Tile_Arrays[z + (y*8) + x];
                }
            }
            for(int i = 0; i < 64; i++)
            {
                Tile_Arrays[z + i] = temp_arr[i];
            }
        }

        public static void tile_rot_ccw() // L, rotate counter clockwise
        {
            int z = (frmMain.tile_set * 256 * 8 * 8) + (frmMain.tile_num * 8 * 8); // base index
            int[] temp_arr = new int[64];
            int count = 0;
            for (int x = 7; x >= 0; x--)
            {
                for (int y = 0; y < 8; y++)
                {
                    temp_arr[count++] = Tile_Arrays[z + (y * 8) + x];
                }
            }
            for (int i = 0; i < 64; i++)
            {
                Tile_Arrays[z + i] = temp_arr[i];
            }
        }

        public static void tile_fill()
        { // fill with currently selected color.
            int z = (frmMain.tile_set * 256 * 8 * 8) + (frmMain.tile_num * 8 * 8); // base index
            int color = frmMain.pal_x + (frmMain.pal_y * 16);
            
            for (int x = 0; x < 64; x++)
            {
                Tile_Arrays[z + x] = color;
            }
        }


    }
}
