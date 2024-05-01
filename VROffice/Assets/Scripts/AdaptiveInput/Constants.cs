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

    public static Dictionary<string, EventModifiers> m_inputMappingModifiers = new Dictionary<string, EventModifiers>()
    {
        { "CUT", EventModifiers.Control },
        { "COPY", EventModifiers.Control },
        { "PASTE", EventModifiers.Control }
    };

    // public static Dictionary<AdaptiveLayout.LayoutAdaptationMethod, string> m_environmentLabels = new Dictionary<AdaptiveLayout.LayoutAdaptationMethod, string>() 
    // {
    //     { AdaptiveLayout.LayoutAdaptationMethod.FIXED, "User-centered" },
    //     { AdaptiveLayout.LayoutAdaptationMethod.CONSISTENCY, "Environment-driven" },
    //     { AdaptiveLayout.LayoutAdaptationMethod.INTERACTION, "Interaction-driven" }
    // };
}