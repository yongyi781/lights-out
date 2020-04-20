using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace LightsOut
{
    public class Solver
    {
        private readonly int[] flipActions;

        public Solver(int size = 5)
        {
            if (size > 5)
                size = 5;
            Size = size;

            flipActions = new int[1 << (Size * Size)];
            InitiailizeFlipActions();
        }

        public int Size { get; }

        public (int[] Codes, int Score) Solve(TileGrid grid)
        {
            int gridCode = 0;
            for (int i = 0; i < Size * Size; i++)
                if (grid.State[i % Size, i / Size])
                    gridCode ^= 1 << i;

            var solution = Solve(gridCode);
            for (int i = 0; i < Size * Size; i++)
                if ((solution.Codes[0] & 1 << i) != 0)
                    grid.Imbue(i % Size, i / Size);
            return solution;
        }

        public (int[] Codes, int Score) Solve(int gridCode)
        {
            var minScore = int.MaxValue;
            var solutionCodes = new List<int>();
            for (int i = 0; i < flipActions.Length; i++)
            {
                var numBlack = CountSetBits(gridCode ^ flipActions[i]);
                var score = CountSetBits(i) + Math.Min(numBlack, Size * Size - numBlack);
                if (minScore >= score)
                {
                    if (minScore > score)
                        solutionCodes.Clear();
                    solutionCodes.Add(i);
                    minScore = score;
                }
            }
            return (solutionCodes.ToArray(), minScore);
        }

        private void InitiailizeFlipActions()
        {
            var fileName = $"flip-actions-{Size}.bin";
            if (File.Exists(fileName))
            {
                using var file = File.OpenRead(fileName);
                file.Read(MemoryMarshal.AsBytes<int>(flipActions));
            }
            else
            {
                var flipActionBasis = (from i in Enumerable.Range(0, Size * Size)
                                       select GetFlipActionBasis(i)).ToArray();
                for (int i = 0; i < flipActions.Length; i++)
                    flipActions[i] = GetFlipAction(i, flipActionBasis);
                using var file = File.OpenWrite(fileName);
                file.Write(MemoryMarshal.AsBytes<int>(flipActions));
            }
        }

        private int GetFlipActionBasis(int index)
        {
            var result = 1 << index;
            int y = index / Size, x = index % Size;
            if (x > 0)
                result ^= 1 << (y * Size + (x - 1));
            if (x < Size - 1)
                result ^= 1 << (y * Size + (x + 1));
            if (y > 0)
                result ^= 1 << ((y - 1) * Size + x);
            if (y < Size - 1)
                result ^= 1 << ((y + 1) * Size + x);
            return result;
        }

        private int GetFlipAction(int code, int[] flipActionBasis)
        {
            int result = 0;
            for (int i = 0; i < Size * Size; i++)
                if ((code & 1 << i) != 0)
                    result ^= flipActionBasis[i];
            return result;
        }

        public string ToChessString(int code)
        {
            return string.Join(" ", from i in Enumerable.Range(0, Size * Size)
                                    where (code & 1 << i) != 0
                                    select IndexToChessString(i));
        }

        private string IndexToChessString(int index)
        {
            int y = index / Size, x = index % Size;
            return $"{(char)(x + 'a')}{(char)((Size - 1 - y) + '1')}";
        }

        static int CountSetBits(int i)
        {
            i -= ((i >> 1) & 0x55555555);
            i = (i & 0x33333333) + ((i >> 2) & 0x33333333);
            return (((i + (i >> 4)) & 0x0F0F0F0F) * 0x01010101) >> 24;
        }
    }
}
