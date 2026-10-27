using System.Collections.Generic;

namespace JellyDrop.Core
{
    /// <summary>
    /// Piece shapes and Super Rotation System (SRS) wall kicks.
    /// Coordinates are y-up, relative to the piece's bounding-box origin.
    /// Rotation states: 0 = spawn, 1 = R (clockwise), 2 = 180, 3 = L.
    /// </summary>
    public static class PieceData
    {
        public const int StandardPieceCount = 7;

        // Spawn-state cells (y-up) inside each piece's bounding box.
        private static readonly Int2[][] SpawnCells =
        {
            /* I */ new[] { new Int2(0, 2), new Int2(1, 2), new Int2(2, 2), new Int2(3, 2) },
            /* O */ new[] { new Int2(0, 0), new Int2(1, 0), new Int2(0, 1), new Int2(1, 1) },
            /* T */ new[] { new Int2(0, 1), new Int2(1, 1), new Int2(2, 1), new Int2(1, 2) },
            /* S */ new[] { new Int2(0, 1), new Int2(1, 1), new Int2(1, 2), new Int2(2, 2) },
            /* Z */ new[] { new Int2(0, 2), new Int2(1, 2), new Int2(1, 1), new Int2(2, 1) },
            /* J */ new[] { new Int2(0, 2), new Int2(0, 1), new Int2(1, 1), new Int2(2, 1) },
            /* L */ new[] { new Int2(2, 2), new Int2(0, 1), new Int2(1, 1), new Int2(2, 1) },
            /* Bomb */ new[] { new Int2(0, 0) },
        };

        // Bounding box size: rotation centre is (size - 1) / 2.
        private static readonly int[] BoxSize = { 4, 2, 3, 3, 3, 3, 3, 1 };

        // [type][rotation] -> cells
        private static readonly Int2[][][] Cells = BuildAllRotations();

        private static readonly Dictionary<int, Int2[]> KicksJLSTZ = new Dictionary<int, Int2[]>
        {
            { Key(0, 1), K(0, 0, -1, 0, -1, 1, 0, -2, -1, -2) },
            { Key(1, 0), K(0, 0, 1, 0, 1, -1, 0, 2, 1, 2) },
            { Key(1, 2), K(0, 0, 1, 0, 1, -1, 0, 2, 1, 2) },
            { Key(2, 1), K(0, 0, -1, 0, -1, 1, 0, -2, -1, -2) },
            { Key(2, 3), K(0, 0, 1, 0, 1, 1, 0, -2, 1, -2) },
            { Key(3, 2), K(0, 0, -1, 0, -1, -1, 0, 2, -1, 2) },
            { Key(3, 0), K(0, 0, -1, 0, -1, -1, 0, 2, -1, 2) },
            { Key(0, 3), K(0, 0, 1, 0, 1, 1, 0, -2, 1, -2) },
        };

        private static readonly Dictionary<int, Int2[]> KicksI = new Dictionary<int, Int2[]>
        {
            { Key(0, 1), K(0, 0, -2, 0, 1, 0, -2, -1, 1, 2) },
            { Key(1, 0), K(0, 0, 2, 0, -1, 0, 2, 1, -1, -2) },
            { Key(1, 2), K(0, 0, -1, 0, 2, 0, -1, 2, 2, -1) },
            { Key(2, 1), K(0, 0, 1, 0, -2, 0, 1, -2, -2, 1) },
            { Key(2, 3), K(0, 0, 2, 0, -1, 0, 2, 1, -1, -2) },
            { Key(3, 2), K(0, 0, -2, 0, 1, 0, -2, -1, 1, 2) },
            { Key(3, 0), K(0, 0, 1, 0, -2, 0, 1, -2, -2, 1) },
            { Key(0, 3), K(0, 0, -1, 0, 2, 0, -1, 2, 2, -1) },
        };

        private static readonly Int2[] NoKick = { new Int2(0, 0) };

        public static Int2[] GetCells(PieceType type, int rotation) => Cells[(int)type][Mod4(rotation)];

        public static bool CanRotate(PieceType type) => type != PieceType.O && type != PieceType.Bomb;

        public static Int2[] GetKicks(PieceType type, int fromRotation, int toRotation)
        {
            if (!CanRotate(type)) return NoKick;
            var table = type == PieceType.I ? KicksI : KicksJLSTZ;
            return table.TryGetValue(Key(Mod4(fromRotation), Mod4(toRotation)), out var kicks) ? kicks : NoKick;
        }

        /// <summary>Bounding-box origin where a piece spawns (lowest cell lands on <paramref name="spawnRow"/>).</summary>
        public static Int2 SpawnOrigin(PieceType type, int boardWidth, int spawnRow)
        {
            var cells = GetCells(type, 0);
            int minY = int.MaxValue;
            foreach (var c in cells) if (c.y < minY) minY = c.y;
            int size = BoxSize[(int)type];
            int x = (boardWidth - size) / 2;
            if (type == PieceType.O) x = boardWidth / 2 - 1;
            return new Int2(x, spawnRow - minY);
        }

        public static int Mod4(int r) => ((r % 4) + 4) % 4;

        private static int Key(int from, int to) => from * 4 + to;

        private static Int2[] K(params int[] xy)
        {
            var result = new Int2[xy.Length / 2];
            for (int i = 0; i < result.Length; i++) result[i] = new Int2(xy[i * 2], xy[i * 2 + 1]);
            return result;
        }

        private static Int2[][][] BuildAllRotations()
        {
            var all = new Int2[SpawnCells.Length][][];
            for (int t = 0; t < SpawnCells.Length; t++)
            {
                all[t] = new Int2[4][];
                all[t][0] = SpawnCells[t];
                for (int r = 1; r < 4; r++)
                {
                    all[t][r] = (t == (int)PieceType.O || t == (int)PieceType.Bomb)
                        ? SpawnCells[t]
                        : RotateCW(all[t][r - 1], BoxSize[t]);
                }
            }
            return all;
        }

        // Clockwise rotation about the box centre: (dx, dy) -> (dy, -dx).
        // Doubling coordinates keeps the half-unit centre of the I piece in integers.
        private static Int2[] RotateCW(Int2[] cells, int size)
        {
            int c2 = size - 1; // centre * 2
            var result = new Int2[cells.Length];
            for (int i = 0; i < cells.Length; i++)
            {
                int dx2 = cells[i].x * 2 - c2;
                int dy2 = cells[i].y * 2 - c2;
                result[i] = new Int2((c2 + dy2) / 2, (c2 - dx2) / 2);
            }
            return result;
        }
    }
}
