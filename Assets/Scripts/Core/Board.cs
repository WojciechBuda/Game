using System;
using System.Collections.Generic;

namespace JellyDrop.Core
{
    [Flags]
    public enum CellFlags : byte
    {
        None = 0,
        Coin = 1,
        Golden = 2
    }

    /// <summary>One board cell. <see cref="Color"/> 0 means empty, otherwise (int)PieceType + 1.</summary>
    public struct BoardCell
    {
        public byte Color;
        public CellFlags Flags;

        public bool IsEmpty => Color == 0;

        public static BoardCell Empty => default;

        public static BoardCell Of(PieceType type, CellFlags flags = CellFlags.None) =>
            new BoardCell { Color = (byte)((int)type + 1), Flags = flags };

        public PieceType PieceType => (PieceType)(Color - 1);
    }

    /// <summary>Snapshot of a cleared row, handed to the view so it can spawn effects.</summary>
    public sealed class ClearedRow
    {
        public int Row;
        public BoardCell[] Cells;
    }

    /// <summary>The playfield grid. Row 0 is the bottom. Rows at or above <see cref="VisibleHeight"/> are hidden buffer.</summary>
    public sealed class Board
    {
        public int Width { get; }
        public int Height { get; }
        public int VisibleHeight { get; }

        private readonly BoardCell[,] _cells;

        public Board(int width = 10, int visibleHeight = 20, int bufferRows = 4)
        {
            Width = width;
            VisibleHeight = visibleHeight;
            Height = visibleHeight + bufferRows;
            _cells = new BoardCell[width, Height];
        }

        public BoardCell this[int x, int y]
        {
            get => _cells[x, y];
            set => _cells[x, y] = value;
        }

        public bool IsInside(int x, int y) => x >= 0 && x < Width && y >= 0 && y < Height;

        public bool IsFree(int x, int y) => IsInside(x, y) && _cells[x, y].IsEmpty;

        public bool Fits(PieceType type, int rotation, Int2 origin)
        {
            foreach (var c in PieceData.GetCells(type, rotation))
            {
                if (!IsFree(origin.x + c.x, origin.y + c.y)) return false;
            }
            return true;
        }

        public bool IsRowFull(int y)
        {
            for (int x = 0; x < Width; x++) if (_cells[x, y].IsEmpty) return false;
            return true;
        }

        public bool IsRowEmpty(int y)
        {
            for (int x = 0; x < Width; x++) if (!_cells[x, y].IsEmpty) return false;
            return true;
        }

        /// <summary>Height of the stack (index of highest non-empty row + 1).</summary>
        public int StackHeight()
        {
            for (int y = Height - 1; y >= 0; y--) if (!IsRowEmpty(y)) return y + 1;
            return 0;
        }

        /// <summary>Removes all full rows, collapses the stack and returns snapshots of the removed rows.</summary>
        public List<ClearedRow> ClearFullRows()
        {
            var cleared = new List<ClearedRow>();
            for (int y = 0; y < Height; y++)
            {
                if (IsRowFull(y)) cleared.Add(new ClearedRow { Row = y, Cells = SnapshotRow(y) });
            }
            if (cleared.Count == 0) return cleared;

            // Collapse from the top of the cleared list down so indices stay valid.
            for (int i = cleared.Count - 1; i >= 0; i--) RemoveRow(cleared[i].Row);
            return cleared;
        }

        /// <summary>Removes a row and shifts everything above it down by one.</summary>
        public void RemoveRow(int row)
        {
            for (int y = row; y < Height - 1; y++)
            {
                for (int x = 0; x < Width; x++) _cells[x, y] = _cells[x, y + 1];
            }
            for (int x = 0; x < Width; x++) _cells[x, Height - 1] = BoardCell.Empty;
        }

        /// <summary>Shifts everything up by one row and empties row 0. The top row is discarded.</summary>
        public void InsertRowAtBottom()
        {
            for (int y = Height - 1; y > 0; y--)
                for (int x = 0; x < Width; x++)
                    _cells[x, y] = _cells[x, y - 1];
            for (int x = 0; x < Width; x++) _cells[x, 0] = BoardCell.Empty;
        }

        /// <summary>Clears a square area (used by the bomb). Returns how many cells were destroyed.</summary>
        public int ClearArea(int centerX, int centerY, int radius, List<Int2> destroyed = null)
        {
            int count = 0;
            for (int x = centerX - radius; x <= centerX + radius; x++)
            {
                for (int y = centerY - radius; y <= centerY + radius; y++)
                {
                    if (!IsInside(x, y) || _cells[x, y].IsEmpty) continue;
                    _cells[x, y] = BoardCell.Empty;
                    destroyed?.Add(new Int2(x, y));
                    count++;
                }
            }
            return count;
        }

        /// <summary>Lets floating cells fall straight down after an explosion so no holes hang in the air above gaps.</summary>
        public void ApplyColumnGravity()
        {
            for (int x = 0; x < Width; x++)
            {
                int write = 0;
                for (int y = 0; y < Height; y++)
                {
                    if (_cells[x, y].IsEmpty) continue;
                    if (write != y)
                    {
                        _cells[x, write] = _cells[x, y];
                        _cells[x, y] = BoardCell.Empty;
                    }
                    write++;
                }
            }
        }

        public BoardCell[] SnapshotRow(int y)
        {
            var row = new BoardCell[Width];
            for (int x = 0; x < Width; x++) row[x] = _cells[x, y];
            return row;
        }

        public void Clear()
        {
            for (int x = 0; x < Width; x++)
                for (int y = 0; y < Height; y++)
                    _cells[x, y] = BoardCell.Empty;
        }
    }
}
