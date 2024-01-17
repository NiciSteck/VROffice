using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Constants
{
    public const int InteractableLayer = 6;
    public const int SurfaceLayer = 7;
    public const int WidgetLayer = 8;
    public const int ObstacleLayer = 12;
    public const int SurfaceMask = (1 << SurfaceLayer);
    public const int WidgetMask = (1 << WidgetLayer);
    public const int ObstacleMask = 1 << ObstacleLayer;
    public const int InteractableMask = (1 << InteractableLayer) | WidgetMask;
    public const int VoxelLayer = 11;
    public const int VolumeLayer = 13;
    
    public static Dictionary<string, KeyCode> m_inputMappingKeys = new Dictionary<string, KeyCode>()
    {
        { "LEFT", KeyCode.LeftArrow },
        { "RIGHT", KeyCode.RightArrow },
        { "UP", KeyCode.UpArrow },
        { "DOWN", KeyCode.DownArrow },
        { "CUT", KeyCode.X },
        { "COPY", KeyCode.C },
        { "PASTE", KeyCode.Z },
        { "DELETE", KeyCode.Delete },
        { "BACK", KeyCode.Backspace },
        { "ENTER", KeyCode.Return },
        { "SPACE", KeyCode.Space }
    };

    // public static Dictionary<string, EventModifiers> m_inputMappingModifiers = new Dictionary<string, EventModifiers>()
    // {
    //     { "CUT", EventModifiers.Control },
    //     { "COPY", EventModifiers.Control },
    //     { "PASTE", EventModifiers.Control }
    // };

    // public static Dictionary<string, Vector3> m_taskElementLayout = new Dictionary<string, Vector3>()
    // {
    //     {"seq-notification", new Vector3(-0.36f,0.45f,0.50f)},
    //     {"seq-map", new Vector3(-0.38f,0.22f,0.52f) },
    //     {"seq-presentation", new Vector3(-0.19f,-0.16f,0.62f) },
    //     {"par-1-spreadsheet-1", new Vector3(0.51f,-0.14f,0.53f) },
    //     {"par-1-spreadsheet-2", new Vector3(0.20f,-0.14F,0.63f) },
    //     {"par-2-video", new Vector3(0.30f,0.29f,0.62f) },
    //     {"par-2-chat", new Vector3(-0.08F,0.28f,0.59f) },
    //     {"par-3-keyboard", new Vector3(-0.11f,-0.38f,0.40f) },
    //     {"par-3-document", new Vector3(-0.53f,-0.21f, 0.50f) }
    // };

    public enum Environment { EMPTY, WORKSTATION, WALL, NONE };
    // public static Dictionary<AdaptiveLayout.LayoutAdaptationMethod, string> m_environmentLabels = new Dictionary<AdaptiveLayout.LayoutAdaptationMethod, string>() 
    // {
    //     { AdaptiveLayout.LayoutAdaptationMethod.FIXED, "User-centered" },
    //     { AdaptiveLayout.LayoutAdaptationMethod.CONSISTENCY, "Environment-driven" },
    //     { AdaptiveLayout.LayoutAdaptationMethod.INTERACTION, "Interaction-driven" }
    // };
}