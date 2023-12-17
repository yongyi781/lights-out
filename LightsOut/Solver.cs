using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics.X86;

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
            InitializeFlipActions();
        }

        public int Size { get; }
        public int[] FlipActions => flipActions;

        public (List<int> codes, int score) Solve(TileGrid grid)
        {
            int gridCode = 0;
            for (int i = 0; i < Size * Size; i++)
                if (grid.State[i % Size, i / Size])
                    gridCode ^= 1 << i;

            var solution = Solve(gridCode);
            for (int i = 0; i < Size * Size; i++)
                if ((solution.codes[0] & 1 << i) != 0)
                    grid.Imbue(i % Size, i / Size);
            return solution;
        }

        public (List<int> codes, int score) Solve(int gridCode)
        {
            var minScore = int.MaxValue;
            var solutionCodes = new List<int>();
            for (int i = 0; i < 1 << (Size * Size); i++)
            {
                var numBlack = (int)Popcnt.PopCount((uint)(gridCode ^ flipActions[i]));
                var score = (int)Popcnt.PopCount((uint)i) + Math.Min(numBlack, Size * Size - numBlack);
                if (minScore >= score)
                {
                    if (minScore > score)
                        solutionCodes.Clear();
                    solutionCodes.Add(i);
                    minScore = score;
                }
            }
            return (solutionCodes, minScore);
        }

        public string ToChessString(int code)
        {
            return "[" + string.Join(" ", from i in Enumerable.Range(0, Size * Size)
                                          where (code & 1 << i) != 0
                                          select IndexToChessString(i)) + "]";
        }

        /// <summary>
        /// Returns a byte array of size 2^(Size*Size) of scores for each starting position. The score of a position is the minimum
        /// number of flips (imbues and forces) needed to get it to all one color.
        /// </summary>
        /// <returns>A byte array of scores.</returns>
        public byte[] GetAllScores()
        {
            var n = 1 << (Size * Size);
            var scores = new byte[n];
            var agenda = new Queue<(int code, byte score)>();
            agenda.Enqueue((0, 0));
            agenda.Enqueue((n - 1, 0));
            while (agenda.Count > 0)
            {
                var (code, score) = agenda.Dequeue();
                for (int i = 0; i < Size * Size; i++)
                {
                    var code2 = code ^ (1 << i);
                    var code3 = code ^ flipActions[1 << i];
                    if (code2 != 0 && code2 != n - 1 && scores[code2] == 0)
                    {
                        scores[code2] = (byte)(score + 1);
                        agenda.Enqueue((code2, (byte)(score + 1)));
                    }
                    if (code3 != 0 && code3 != n - 1 && scores[code3] == 0)
                    {
                        scores[code3] = (byte)(score + 1);
                        agenda.Enqueue((code3, (byte)(score + 1)));
                    }
                }
            }
            return scores;
        }

        private void InitializeFlipActions()
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

        private string IndexToChessString(int index)
        {
            int y = index / Size, x = index % Size;
            return $"{(char)(x + 'a')}{(char)(y + '1')}";
        }
    }
}
