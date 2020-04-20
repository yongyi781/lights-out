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
            ImbueState = new int[size, size];
        }

        public bool this[int x, int y]
        {
            get { return State[x, y]; }
            set { State[x, y] = value; }
        }

        public int Size { get; }
        public bool[,] State { get; private set; }
        /// <summary>
        /// Gets a table of flip parities, where 1 is odd and 0 is even.
        /// </summary>
        public int[,] ImbueState { get; private set; }

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
                ImbueState[x, y] = 1 - ImbueState[x, y];
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
        /// Counts the number of tiles that are of a specified state.
        /// </summary>
        /// <param name="on">Either true or false.</param>
        /// <returns>The number of tiles with state equal to <paramref name="b"/>.</returns>
        public int CountTiles(bool b)
        {
            int count = 0;
            for (int y = 0; y < Size; y++)
                for (int x = 0; x < Size; x++)
                    if (State[x, y] == b)
                        count++;
            return count;
        }

        public void Reset()
        {
            for (int y = 0; y < Size; y++)
            {
                for (int x = 0; x < Size; x++)
                {
                    State[x, y] = false;
                    ImbueState[x, y] = 0;
                }
            }
        }
    }
}
