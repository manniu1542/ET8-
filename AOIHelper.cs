        public static long CreateCellId(int x, int y)
        {
            return ((long)(uint)x << 32) | (uint)y;
        }