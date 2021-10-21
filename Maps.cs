
namespace M8TE
{
    public static class Maps
    {
        // 16 maps, because mode 7 is a big 128x128 tiles per map, and we
        // are only showing 32x32 tiles per map

        public static int[] tile = new int[32 * 32 * 16]; //x, y, 16 maps
        //tile can be value 0-1023, high 2 bits references the tileset

        public static int[] palette = new int[32 * 32 * 16]; //x, y, 16 maps - unused in 8bpp modes
        public static int[] h_flip = new int[32 * 32 * 16]; //x, y, 16 maps - unused in mode 7
        public static int[] v_flip = new int[32 * 32 * 16]; //x, y, 16 maps - unused in mode 7
        public static int[] priority = new int[32 * 32 * 16]; //x, y, 16 maps - unused in this app
        // priority affects how sprite layers show above or below
        // but, you can't see the difference here

        //if the height is not 32, it will save a shorter map
    }

    public static class MapsU // undo backup copy
    {
        // only backup 1 map, the current one
        public static int[] tile = new int[32 * 32]; //x, y
        //tile can be value 0-1023, high 2 bits references the tileset

        public static int[] palette = new int[32 * 32]; //x, y
        public static int[] h_flip = new int[32 * 32]; //x, y
        public static int[] v_flip = new int[32 * 32]; //x, y
        public static int[] priority = new int[32 * 32]; //x, y
        // priority affects how sprite layers show above or below
        // but, you can't see the difference here

    }
}