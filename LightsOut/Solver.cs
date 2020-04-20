using System;
using System.Linq;

namespace LightsOut
{
    public class Solver
    {
        private int[] flipActionBasis;
        private int[] flipActions;
        private int[] bits;

        public Solver(int size = 5)
        {
            if (size > 5)
                size = 5;
            Size = size;

            bits = new int[size * size];
            for (int i = 0; i < bits.Length; i++)
                bits[i] = 1 << i;

            flipActionBasis = (from i in Enumerable.Range(0, size * size)
                               select GetFlipActionBasis(i)).ToArray();

            flipActions = new int[1 << (size * size)];
            for (int i = 0; i < flipActions.Length; i++)
            {
                flipActions[i] = GetFlipAction(i);
            }
        }

        public int Size { get; }

        public (int Code, int Score) Solve(TileGrid grid)
        {
            int gridCode = 0;
            for (int i = 0; i < Size * Size; i++)
            {
                if (grid.State[i % Size, i / Size])
                    gridCode ^= bits[i];
            }

            var solution = Solve(gridCode);
            for (int i = 0; i < Size * Size; i++)
            {
                if ((solution.Code & bits[i]) != 0)
                    grid.Imbue(i % Size, i / Size);
            }
            return solution;
        }

        public (int Code, int Score) Solve(int gridCode)
        {
            var minScore = int.MaxValue;
            var solutionCode = 0;
            for (int i = 0; i < flipActions.Length; i++)
            {
                var numBlack = CountSetBits(gridCode ^ flipActions[i]);
                var score = CountSetBits(i) + Math.Min(numBlack, Size * Size - numBlack);
                if (minScore > score)
                {
                    minScore = score;
                    solutionCode = i;
                }
            }
            return (solutionCode, minScore);
        }

        private int GetFlipActionBasis(int index)
        {
            var result = bits[index];
            int y = index / Size, x = index % Size;
            if (x > 0)
                result ^= bits[y * Size + (x - 1)];
            if (x < Size - 1)
                result ^= bits[y * Size + (x + 1)];
            if (y > 0)
                result ^= bits[(y - 1) * Size + x];
            if (y < Size - 1)
                result ^= bits[(y + 1) * Size + x];
            return result;
        }

        private int GetFlipAction(int code)
        {
            int result = 0;
            for (int i = 0; i < bits.Length; i++)
                if ((code & bits[i]) != 0)
                    result ^= flipActionBasis[i];
            return result;
        }

        public string ToChessString(int code)
        {
            return string.Join(" ", from i in Enumerable.Range(0, bits.Length)
                                    where (code & bits[i]) != 0
                                    select IndexToChessString(i));
        }

        private string IndexToChessString(int index)
        {
            int y = index / Size, x = index % Size;
            return $"{(char)(x + 'a')}{(char)((Size - 1 - y) + '1')}";
        }

        static int CountSetBits(int i)
        {
            // Java: use >>> instead of >>
            // C or C++: use uint32_t
            i -= ((i >> 1) & 0x55555555);
            i = (i & 0x33333333) + ((i >> 2) & 0x33333333);
            return (((i + (i >> 4)) & 0x0F0F0F0F) * 0x01010101) >> 24;
        }
    }
}
