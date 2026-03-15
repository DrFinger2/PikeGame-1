using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

public static class ParallaxEditorUtility
{
    public static void DrawViewportPlane(Camera camera, float depth, Color planeColor)
    {
        if (camera == null)
        {
            return;
        }

        // Calculate the four corners of the viewport at the specific parallax depth
        Vector3 bottomLeft = camera.ViewportToWorldPoint(new Vector3(0, 0, depth));
        Vector3 bottomRight = camera.ViewportToWorldPoint(new Vector3(1, 0, depth));
        Vector3 topRight = camera.ViewportToWorldPoint(new Vector3(1, 1, depth));
        Vector3 topLeft = camera.ViewportToWorldPoint(new Vector3(0, 1, depth));

        Gizmos.color = planeColor;
        Gizmos.DrawLine(bottomLeft, bottomRight);
        Gizmos.DrawLine(bottomRight, topRight);
        Gizmos.DrawLine(topRight, topLeft);
        Gizmos.DrawLine(topLeft, bottomLeft);
    }

    public static void DrawLayerBounds(Bounds bounds, Color boundsColor, bool isVisible)
    {
        Gizmos.color = isVisible ? boundsColor : Color.red;
        Gizmos.DrawWireCube(bounds.center, bounds.size);
    }
}
#endif