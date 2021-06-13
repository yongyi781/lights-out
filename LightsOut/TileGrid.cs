namespace LightsOut
{
    /// <summary>
    /// Represents a grid of tiles, each of which has two states.
    /// </summary>
    public class TileGrid
    {
        /// <summary>
        /// Initializes a new tile grid with the specified size.
        /// </summary>
        /// <param name="size">The size of the grid.</param>
        public TileGrid(int size)
        {
            Size = size;
            State = new bool[size, size];
            ImbueState = new bool[size, size];
        }

        public bool this[int x, int y]
        {
            get { return State[x, y]; }
            set { State[x, y] = value; }
        }

        public int Size { get; }
        public bool[,] State { get; private set; }
        /// <summary>
        /// Gets a table of flip parities, where 1 is imbued and 0 is not.
        /// </summary>
        public bool[,] ImbueState { get; private set; }

        public void Reset()
        {
            for (int y = 0; y < Size; y++)
            {
                for (int x = 0; x < Size; x++)
                {
                    State[x, y] = false;
                    ImbueState[x, y] = false;
                }
            }
        }

        /// <summary>
        /// Flips a tile and the four tiles surrounding it.
        /// </summary>
        /// <param name="x">The x-coordinate of the center tile.</param>
        /// <param name="y">The y-coordinate of the center tile.</param>
        public void Imbue(int x, int y, bool flipParity = true)
        {
            Force(x, y);
            Force(x - 1, y);
            Force(x, y - 1);
            Force(x + 1, y);
            Force(x, y + 1);

            if (flipParity)
                ImbueState[x, y] = !ImbueState[x, y];
        }

        /// <summary>
        /// Flips a single tile.
        /// </summary>
        /// <param name="x">The x-coordinate.</param>
        /// <param name="y">The y-coordinate.</param>
        public void Force(int x, int y)
        {
            if (x >= 0 && y >= 0 && x < Size && y < Size)
                State[x, y] = !State[x, y];
        }

        /// <summary>
        /// Returns the integer representation of this grid.
        /// </summary>
        /// <returns>The integer representation of this grid.</returns>
        public int ToInt32()
        {
            int gridCode = 0;
            for (int i = 0; i < Size * Size; i++)
                if (State[i % Size, i / Size])
                    gridCode ^= 1 << i;
            return gridCode;
        }

        public void LoadInt32(int code)
        {
            for (int i = 0; i < Size * Size; i++)
                State[i % Size, i / Size] = (code & (1 << i)) != 0;
        }
    }
}
