using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SettingsData
{
    public Settings_Metadata    Metadata    = new Settings_Metadata();

    public Settings_Graphics    Graphics    = new Settings_Graphics();
    public Settings_Camera      Camera      = new Settings_Camera();
    public Settings_Haptics     Haptics     = new Settings_Haptics();

    public void Validate()
    {
        Metadata.Validate();

        Graphics.Validate();
        Camera.Validate();
        Haptics.Validate();
    }

    public void ApplySettings()
    {
        Graphics.ApplySettings();
        Camera.ApplySettings();
        Haptics.ApplySettings();        
    }
}

[System.Serializable]
public class Settings_Metadata
{
    public int Version = 1;

    public Settings_Metadata()
    {

    }

    public void Validate()
    {

    }
}

[System.Serializable]
public class GraphicsQualityAndResolution
{
    public int QualityLevel             = -1;

    public int Width                    = -1;
    public int Height                   = -1;
    public uint RefreshRate_Numerator   = 0;
    public uint RefreshRate_Denominator = 0;
    public bool FullScreen              = true;

    public GraphicsQualityAndResolution()
    {

    }

    public GraphicsQualityAndResolution(Resolution _resolution, int _qualityLevel, bool _fullscreen)
    {
        QualityLevel                = _qualityLevel;
        Width                       = _resolution.width;
        Height                      = _resolution.height;
        RefreshRate_Numerator       = _resolution.refreshRateRatio.numerator;
        RefreshRate_Denominator     = _resolution.refreshRateRatio.denominator;
        FullScreen                  = _fullscreen;
    }    

    public void Validate()
    {
        if (QualityLevel < 0)
            QualityLevel    = QualitySettings.names.Length - 1;

        if (Width < 0 || Height < 0 || 
            RefreshRate_Numerator == 0 || RefreshRate_Denominator == 0)
        {
            Width                       = Screen.currentResolution.width;
            Height                      = Screen.currentResolution.height;
            RefreshRate_Numerator       = Screen.currentResolution.refreshRateRatio.numerator;
            RefreshRate_Denominator     = Screen.currentResolution.refreshRateRatio.denominator;
            FullScreen                  = Screen.fullScreen;
        }
    }

    public void SetResolution(Resolution _resolution)
    {
        Width                   = _resolution.width;
        Height                  = _resolution.height;
        RefreshRate_Numerator   = _resolution.refreshRateRatio.numerator;
        RefreshRate_Denominator = _resolution.refreshRateRatio.denominator;
    }

    public void SetQualityLevel(int _qualityLevel)
    {
        QualityLevel    = _qualityLevel;
    }

    public void UpdateFrom(GraphicsQualityAndResolution other)
    {
        QualityLevel            = other.QualityLevel;
        Width                   = other.Width;
        Height                  = other.Height;
        RefreshRate_Numerator   = other.RefreshRate_Numerator;
        RefreshRate_Denominator = other.RefreshRate_Denominator;
        FullScreen              = other.FullScreen ;
    }
}

[System.Serializable]
public class Settings_Graphics
{
    [GraphicsQualityAndResolutionSetting("Graphics", "Quality and Resolution", "Controls the overall quality level and resolution for the graphics.", "ApplySettings")]
    public GraphicsQualityAndResolution QualityAndResolution;

    public Settings_Graphics()
    {
        QualityAndResolution = null;
    }

    public void Validate()
    {
        if (QualityAndResolution == null)
            QualityAndResolution = new GraphicsQualityAndResolution();

        QualityAndResolution.Validate();
    }

    public void ApplySettings()
    {
        RefreshRate refreshRate = new RefreshRate();
        refreshRate.numerator = QualityAndResolution.RefreshRate_Numerator;
        refreshRate.denominator = QualityAndResolution.RefreshRate_Denominator;

        Screen.SetResolution(QualityAndResolution.Width, QualityAndResolution.Height, FullScreenMode.ExclusiveFullScreen, refreshRate);

        QualitySettings.SetQualityLevel(QualityAndResolution.QualityLevel);
    }
}

[System.Serializable]
public class Settings_Camera
{
    [BooleanSetting(false, "Camera", "Invert Y Axis", "Controls if the y-axis (ie. vertical) input is inverted or not.")]
    public bool Invert_YAxis    = false;

    [FloatSetting(1, 20, 10, "Camera", "Horizontal Sensitivity", "Controls how responsive the camera is to horizontal movement (higher = more responsive).")]
    public float Sensitivity_X  = 10f;

    [FloatSetting(1, 20, 10, "Camera", "Vertical Sensitivity", "Controls how responsive the camera is to vertical movement (higher = more responsive).")]
    public float Sensitivity_Y  = 10f;

    public Settings_Camera()
    {
    }

    public void Validate()
    {
        
    }

    public void ApplySettings()
    {
        
    }
}

[System.Serializable]
public class Settings_Haptics
{
    [BooleanSetting(false, "Haptics", "Enabled", "Controls if haptic feedback (ie. rumbles) are enabled if your controller supports them.", "ApplySettings")]
    public bool Enabled         = true;

    public Settings_Haptics()
    {

    }

    public void Validate()
    {
        
    }

    public void ApplySettings()
    {

    }
}