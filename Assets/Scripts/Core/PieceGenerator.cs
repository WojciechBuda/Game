using System.Collections.Generic;

namespace JellyDrop.Core
{
    /// <summary>A piece waiting in the queue, with its lucky decorations already rolled.</summary>
    public struct QueuedPiece
    {
        public PieceType Type;
        public bool Golden;
        /// <summary>Index of the cell carrying a coin, or -1.</summary>
        public int CoinCellIndex;

        public static QueuedPiece Plain(PieceType type) => new QueuedPiece { Type = type, CoinCellIndex = -1 };
    }

    /// <summary>
    /// "7-bag" randomizer: every 7 pieces contain each shape exactly once, which avoids long droughts.
    /// Also rolls golden pieces and coin cells.
    /// </summary>
    public sealed class PieceGenerator
    {
        private readonly IRandom _rng;
        private readonly LuckyConfig _lucky;
        private readonly List<PieceType> _bag = new List<PieceType>(7);
        private readonly List<QueuedPiece> _queue = new List<QueuedPiece>();

        public PieceGenerator(IRandom rng, LuckyConfig lucky, int previewCount)
        {
            _rng = rng;
            _lucky = lucky;
            PreviewCount = previewCount;
            Fill();
        }

        public int PreviewCount { get; }

        public IReadOnlyList<QueuedPiece> Queue => _queue;

        public QueuedPiece Next()
        {
            var piece = _queue[0];
            _queue.RemoveAt(0);
            Fill();
            return piece;
        }

        /// <summary>Puts a piece back at the front of the queue (used when a bomb replaces the current piece).</summary>
        public void PushFront(QueuedPiece piece) => _queue.Insert(0, piece);

        private void Fill()
        {
            while (_queue.Count < System.Math.Max(1, PreviewCount)) _queue.Add(Roll(DrawFromBag()));
        }

        private PieceType DrawFromBag()
        {
            if (_bag.Count == 0)
            {
                for (int i = 0; i < PieceData.StandardPieceCount; i++) _bag.Add((PieceType)i);
                // Fisher-Yates shuffle
                for (int i = _bag.Count - 1; i > 0; i--)
                {
                    int j = _rng.Range(0, i + 1);
                    (_bag[i], _bag[j]) = (_bag[j], _bag[i]);
                }
            }
            var type = _bag[_bag.Count - 1];
            _bag.RemoveAt(_bag.Count - 1);
            return type;
        }

        private QueuedPiece Roll(PieceType type)
        {
            var piece = QueuedPiece.Plain(type);
            if (_rng.NextDouble() < _lucky.GoldenPieceChance)
            {
                piece.Golden = true;
            }
            else if (_rng.NextDouble() < _lucky.CoinPieceChance)
            {
                piece.CoinCellIndex = _rng.Range(0, PieceData.GetCells(type, 0).Length);
            }
            return piece;
        }
    }
}
