using JellyDrop.Core;
using UnityEngine;

namespace JellyDrop.GameInput
{
    /// <summary>
    /// Turns touches into game actions:
    /// tap = rotate, drag sideways = move one cell per step, drag down = soft drop,
    /// quick flick down = hard drop, flick up = hold. Keyboard works too (editor / desktop testing).
    /// </summary>
    public sealed class GestureController
    {
        // Tunables (in cells or seconds). Exposed for a future settings screen.
        public float CellsPerMoveStep = 0.85f;
        public float TapMaxSeconds = 0.25f;
        public float TapMaxCells = 0.5f;
        public float SoftDropStartCells = 1.2f;
        public float FlickMinCellsPerSecond = 28f;
        public float FlickMinCells = 1.5f;
        public float HoldSwipeCells = 2.2f;
        public float KeyRepeatDelay = 0.17f;
        public float KeyRepeatRate = 0.05f;

        private bool _pressed;
        private bool _ignoreThisTouch;
        private Vector2 _start;
        private Vector2 _last;
        private float _anchorX;
        private float _startTime;
        private bool _movedSideways;
        private bool _softDropping;
        private bool _consumed;

        // Velocity estimate (cells per second, vertical), smoothed.
        private float _velocityY;

        private float _keyTimer;
        private int _keyDir;

        public System.Action OnRotate;
        public System.Action OnRotateCCW;
        public System.Action OnHardDrop;
        public System.Action OnHold;
        public System.Action OnMove;

        /// <param name="pixelsPerCell">How many screen pixels one board cell covers.</param>
        public void Update(GameSession session, float pixelsPerCell, float deltaTime)
        {
            if (session == null || session.IsOver) return;
            HandleKeyboard(session, deltaTime);
            HandleTouch(session, Mathf.Max(1f, pixelsPerCell), deltaTime);
        }

        public void Reset()
        {
            _pressed = false;
            _ignoreThisTouch = false;
            _softDropping = false;
        }

        private void HandleTouch(GameSession session, float cellPx, float dt)
        {
            bool down = PointerInput.TryGetPointer(out var pos);

            if (down && !_pressed)
            {
                _pressed = true;
                _ignoreThisTouch = PointerInput.IsOverUi(pos);
                _start = _last = pos;
                _anchorX = pos.x;
                _startTime = Time.unscaledTime;
                _movedSideways = false;
                _softDropping = false;
                _consumed = false;
                _velocityY = 0f;
                return;
            }

            if (!_pressed) return;
            if (_ignoreThisTouch)
            {
                if (!down) _pressed = false;
                return;
            }

            if (down)
            {
                float instantVy = dt > 0f ? (pos.y - _last.y) / cellPx / dt : 0f;
                _velocityY = Mathf.Lerp(_velocityY, instantVy, 0.5f);
                _last = pos;
                if (_consumed) return;

                // Sideways moves: one cell per step of finger travel.
                float step = cellPx * CellsPerMoveStep;
                while (pos.x - _anchorX >= step)
                {
                    _anchorX += step;
                    if (session.MoveRight()) OnMove?.Invoke();
                    _movedSideways = true;
                }
                while (_anchorX - pos.x >= step)
                {
                    _anchorX -= step;
                    if (session.MoveLeft()) OnMove?.Invoke();
                    _movedSideways = true;
                }

                // Soft drop while dragging down (mostly vertical drag).
                float dy = (pos.y - _start.y) / cellPx;
                float dx = Mathf.Abs(pos.x - _start.x) / cellPx;
                bool wantSoft = dy < -SoftDropStartCells && -dy > dx * 1.2f;
                if (wantSoft != _softDropping)
                {
                    _softDropping = wantSoft;
                    session.SetSoftDrop(wantSoft);
                }
                return;
            }

            // Released
            _pressed = false;
            if (_softDropping) session.SetSoftDrop(false);
            if (_consumed) return;

            float totalDy = (_last.y - _start.y) / cellPx;
            float totalDx = (_last.x - _start.x) / cellPx;
            float duration = Time.unscaledTime - _startTime;

            if (totalDy < -FlickMinCells && _velocityY < -FlickMinCellsPerSecond && -totalDy > Mathf.Abs(totalDx))
            {
                OnHardDrop?.Invoke();
            }
            else if (totalDy > HoldSwipeCells && totalDy > Mathf.Abs(totalDx) * 1.5f)
            {
                OnHold?.Invoke();
            }
            else if (!_movedSideways && !_softDropping && duration <= TapMaxSeconds &&
                     Mathf.Abs(totalDx) < TapMaxCells && Mathf.Abs(totalDy) < TapMaxCells)
            {
                OnRotate?.Invoke();
            }
            _softDropping = false;
        }

        private void HandleKeyboard(GameSession session, float dt)
        {
            if (PointerInput.KeyDown(GameKey.RotateCW)) OnRotate?.Invoke();
            if (PointerInput.KeyDown(GameKey.RotateCCW)) OnRotateCCW?.Invoke();
            if (PointerInput.KeyDown(GameKey.HardDrop)) OnHardDrop?.Invoke();
            if (PointerInput.KeyDown(GameKey.Hold)) OnHold?.Invoke();

            // Delayed auto shift for left/right.
            int dir = PointerInput.KeyHeld(GameKey.Left) ? -1 : PointerInput.KeyHeld(GameKey.Right) ? 1 : 0;
            if (dir != 0 && dir != _keyDir)
            {
                Shift(session, dir);
                _keyTimer = -KeyRepeatDelay;
            }
            else if (dir != 0)
            {
                _keyTimer += dt;
                while (_keyTimer >= KeyRepeatRate)
                {
                    _keyTimer -= KeyRepeatRate;
                    Shift(session, dir);
                }
            }
            _keyDir = dir;

            if (!_pressed) session.SetSoftDrop(PointerInput.KeyHeld(GameKey.Down));
        }

        private void Shift(GameSession session, int dir)
        {
            bool moved = dir < 0 ? session.MoveLeft() : session.MoveRight();
            if (moved) OnMove?.Invoke();
        }
    }
}
