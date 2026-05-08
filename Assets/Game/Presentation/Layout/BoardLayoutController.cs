using Game.UI;
using UnityEngine;

namespace Game.Presentation.Layout
{
    public static class BoardLayoutController
    {
        public static void Apply(Camera cam, Transform boardTransform, int width, int height, float cellSize)
        {
            if (cam == null || boardTransform == null || Screen.width <= 0 || Screen.height <= 0)
            {
                return;
            }

            cam.orthographic = true;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.51f, 0.72f, 0.78f, 1f);

            float screenW = Mathf.Max(1f, Screen.width);
            float screenH = Mathf.Max(1f, Screen.height);
            float aspect = screenW / screenH;

            float topReserved = SimpleHud.ReservedTopPixels + 18f;
            float bottomReserved = Mathf.Max(28f, Screen.safeArea.yMin + 22f);
            float availablePx = Mathf.Max(screenH * 0.45f, screenH - topReserved - bottomReserved);

            float boardW = Mathf.Max(1f, (width - 1) * cellSize + 1.18f);
            float boardH = Mathf.Max(1f, (height - 1) * cellSize + 1.18f);
            float worldPerPixel = boardH / availablePx;

            float sizeForHeight = worldPerPixel * screenH * 0.5f;
            float sizeForWidth = (boardW * 0.5f) / aspect;
            float orthoSize = Mathf.Max(sizeForHeight, sizeForWidth) * 1.08f;

            var boardCenter = boardTransform.position + new Vector3((width - 1) * cellSize * 0.5f, (height - 1) * cellSize * 0.5f, 0f);
            float desiredCenterScreenY = bottomReserved + availablePx * 0.5f;
            float normalizedY = Mathf.Clamp01(desiredCenterScreenY / screenH);
            float cameraY = boardCenter.y + orthoSize - normalizedY * (orthoSize * 2f);

            cam.orthographicSize = orthoSize;
            cam.transform.position = new Vector3(boardCenter.x, cameraY, -10f);
            cam.transform.rotation = Quaternion.identity;
        }
    }
}
