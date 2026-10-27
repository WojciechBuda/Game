using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine.InputSystem;
#endif

namespace JellyDrop.GameInput
{
    public enum GameKey { Left, Right, Down, RotateCW, RotateCCW, HardDrop, Hold, Pause }

    /// <summary>
    /// Reads the primary touch / mouse and the keyboard through whichever input backend the
    /// project uses (new Input System or the legacy Input Manager), so the game works in both.
    /// </summary>
    public static class PointerInput
    {
        private static readonly List<RaycastResult> RaycastResults = new List<RaycastResult>();

        public static bool TryGetPointer(out Vector2 position)
        {
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
            var touch = Touchscreen.current;
            if (touch != null && touch.primaryTouch.press.isPressed)
            {
                position = touch.primaryTouch.position.ReadValue();
                return true;
            }
            var mouse = Mouse.current;
            if (mouse != null && mouse.leftButton.isPressed)
            {
                position = mouse.position.ReadValue();
                return true;
            }
#else
            if (Input.touchCount > 0)
            {
                position = Input.GetTouch(0).position;
                return true;
            }
            if (Input.GetMouseButton(0))
            {
                position = Input.mousePosition;
                return true;
            }
#endif
            position = default;
            return false;
        }

        public static bool KeyDown(GameKey key)
        {
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
            var k = Keyboard.current;
            if (k == null) return false;
            switch (key)
            {
                case GameKey.Left: return k.leftArrowKey.wasPressedThisFrame || k.aKey.wasPressedThisFrame;
                case GameKey.Right: return k.rightArrowKey.wasPressedThisFrame || k.dKey.wasPressedThisFrame;
                case GameKey.Down: return k.downArrowKey.wasPressedThisFrame || k.sKey.wasPressedThisFrame;
                case GameKey.RotateCW: return k.upArrowKey.wasPressedThisFrame || k.xKey.wasPressedThisFrame || k.wKey.wasPressedThisFrame;
                case GameKey.RotateCCW: return k.zKey.wasPressedThisFrame;
                case GameKey.HardDrop: return k.spaceKey.wasPressedThisFrame;
                case GameKey.Hold: return k.cKey.wasPressedThisFrame || k.leftShiftKey.wasPressedThisFrame;
                case GameKey.Pause: return k.escapeKey.wasPressedThisFrame || k.pKey.wasPressedThisFrame;
            }
            return false;
#else
            switch (key)
            {
                case GameKey.Left: return Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A);
                case GameKey.Right: return Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D);
                case GameKey.Down: return Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S);
                case GameKey.RotateCW: return Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.X) || Input.GetKeyDown(KeyCode.W);
                case GameKey.RotateCCW: return Input.GetKeyDown(KeyCode.Z);
                case GameKey.HardDrop: return Input.GetKeyDown(KeyCode.Space);
                case GameKey.Hold: return Input.GetKeyDown(KeyCode.C) || Input.GetKeyDown(KeyCode.LeftShift);
                case GameKey.Pause: return Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P);
            }
            return false;
#endif
        }

        public static bool KeyHeld(GameKey key)
        {
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
            var k = Keyboard.current;
            if (k == null) return false;
            switch (key)
            {
                case GameKey.Left: return k.leftArrowKey.isPressed || k.aKey.isPressed;
                case GameKey.Right: return k.rightArrowKey.isPressed || k.dKey.isPressed;
                case GameKey.Down: return k.downArrowKey.isPressed || k.sKey.isPressed;
            }
            return false;
#else
            switch (key)
            {
                case GameKey.Left: return Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A);
                case GameKey.Right: return Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D);
                case GameKey.Down: return Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S);
            }
            return false;
#endif
        }

        /// <summary>Android back button / Escape.</summary>
        public static bool BackPressed() => KeyDown(GameKey.Pause);

        /// <summary>True if a UI element that blocks raycasts is under the screen position.</summary>
        public static bool IsOverUi(Vector2 screenPosition)
        {
            var es = EventSystem.current;
            if (es == null) return false;
            var data = new PointerEventData(es) { position = screenPosition };
            RaycastResults.Clear();
            es.RaycastAll(data, RaycastResults);
            return RaycastResults.Count > 0;
        }
    }
}
