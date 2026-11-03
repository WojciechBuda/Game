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
        /// <summary>The marked cell (CoinCellIndex) carries a mushroom instead of a coin.</summary>
        public bool Mushroom;

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
            while (_queue.Count < System.Math.Max(1, PreviewCount))
            {
                // Now and then a surprise piece slips in between the bag pieces (the bag itself is not touched).
                var surprise = RollSurprise();
                _queue.Add(surprise.HasValue ? QueuedPiece.Plain(surprise.Value) : Roll(DrawFromBag()));
            }
        }

        private PieceType? RollSurprise()
        {
            double total = _lucky.SmallPieceChance + _lucky.BigPieceChance + _lucky.BombPieceChance + 2 * _lucky.LineBlastChance;
            if (total <= 0) return null;
            double r = _rng.NextDouble();
            if ((r -= _lucky.SmallPieceChance) < 0) return _rng.NextDouble() < 0.5 ? PieceType.Mono : PieceType.Domino;
            if ((r -= _lucky.BigPieceChance) < 0) return PieceType.Big;
            if ((r -= _lucky.BombPieceChance) < 0) return PieceType.Bomb;
            if ((r -= _lucky.LineBlastChance) < 0) return PieceType.RowBlast;
            if ((r -= _lucky.LineBlastChance) < 0) return PieceType.ColumnBlast;
            return null;
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
            else if (_lucky.MushroomPieceChance > 0 && _rng.NextDouble() < _lucky.MushroomPieceChance)
            {
                piece.CoinCellIndex = _rng.Range(0, PieceData.GetCells(type, 0).Length);
                piece.Mushroom = true;
            }
            return piece;
        }
    }
}
