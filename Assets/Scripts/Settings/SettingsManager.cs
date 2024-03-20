using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class SettingsManager : MonoBehaviour
{
    [SerializeField]
    private GraphicsSettingsSection settingsSection;

    public static SettingsData GraphicsSettings { get; private set; }

    private static string SavedSettingsFilePath => Application.persistentDataPath + "/SavedSettings.dat";

    private GameSettingsSection[] settingsSections;

    public void CreateGameSettingsSections()
    {
        settingsSections = new[]
        {
            new GameSettingsSection("����", null),
            new GameSettingsSection("�������", new GameSetting[]
            {
                new GameSetting<Resolution>("���������� ������", settingsSection.CreateSettingView(),
                    GetScreenResolutionSettingValues(),
                    (resolution) =>
                    {
                        Screen.SetResolution(resolution.width, resolution.height, false);
                    }),
                new GameSetting<FullScreenMode>("�������� �����", settingsSection.CreateSettingView(),
                    GetScreenModeSettingValues(),
                    (screenMode) =>
                    {
                        Screen.fullScreenMode = screenMode;
                    }),

                new GameSetting<int>("�������� �������", settingsSection.CreateSettingView(),
                    GetGraphicsQualitySettingValues(),
                    (qualityLevel) =>
                    {
                        QualitySettings.SetQualityLevel(qualityLevel);
                    }),

                new GameSetting<int>("������������ �������������", settingsSection.CreateSettingView(),
                    GetVSyncSettingValues(),
                    (vSyncValue) =>
                    {
                        QualitySettings.vSyncCount = vSyncValue;
                    }),

                new GameSetting<float>("�������", settingsSection.CreateSettingView(),
                    GetBrightnessSettingValues(),
                    (brightness) =>
                    {
                        RenderSettings.ambientLight = new Color(brightness, brightness, brightness, 1);
                    })
            }),
            new GameSettingsSection("����", null),
            new GameSettingsSection("����������", null),
        };
    }

    private (float, string)[] GetBrightnessSettingValues()
    {
        var brightnessValues = Enumerable.Range(1, 10);
        return brightnessValues.Select(value => ((float)value / brightnessValues.Count(), value.ToString())).ToArray();
    }

    private (Resolution, string)[] GetScreenResolutionSettingValues()
    {
        return Screen.resolutions
            .Where(resolution => Math.Abs((float)resolution.width / resolution.height - (16f / 9)) < 1e-5)
            .Select(resolution => (resolution, resolution.width + " x " + resolution.height))
            .ToArray();
    }

    private (int, string)[] GetGraphicsQualitySettingValues()
    {
        return new[]
        {
            (0, "������"),
            (1, "�������"),
            (2, "�������")
        };
    }

    private (FullScreenMode, string)[] GetScreenModeSettingValues()
    {
        return new[]
        {
            (FullScreenMode.ExclusiveFullScreen, "�������������"),
            (FullScreenMode.FullScreenWindow, "������� (��� �����)"),
            (FullScreenMode.Windowed, "�������")
        };
    }

    private (int, string)[] GetVSyncSettingValues()
    {
        return new[]
        {
            (0, "����."),
            (1, "���.")
        };
    }

    public static void SaveSettings()
    {
        using (var writer = new StreamWriter(SavedSettingsFilePath))
        {
            var serializedSettings = JsonUtility.ToJson(GraphicsSettings);
            writer.Write(serializedSettings);
            writer.Close();
            Debug.Log("��������� ��������� � ����: " + SavedSettingsFilePath);
            ApplySettings();
        }        
    }

    private void LoadSettings() 
    {
        if (File.Exists(SavedSettingsFilePath))
        {
            using (StreamReader reader = new StreamReader(SavedSettingsFilePath))
            {
                var savedSettings = reader.ReadToEnd();
                GraphicsSettings = JsonUtility.FromJson<SettingsData>(savedSettings);
                reader.Close();
                Debug.Log("��������� ��������� �� �����: " + SavedSettingsFilePath);
                ApplySettings();
            }          
        }       
        else
        {
            GraphicsSettings = new SettingsData()
            {
                ScreenResolutionWidth = Screen.currentResolution.width,
                ScreenResolutionHeight = Screen.currentResolution.height,
                ScreenMode = FullScreenMode.ExclusiveFullScreen,
                QualityLevel = QualitySettings.GetQualityLevel(),
                VSyncState = 0,
                Brightness = RenderSettings.ambientLight.r
            };
            SaveSettings();
        }       
    }

    private static void ApplySettings()
    {
        Screen.SetResolution(GraphicsSettings.ScreenResolutionWidth, GraphicsSettings.ScreenResolutionHeight, GraphicsSettings.ScreenMode);
        QualitySettings.SetQualityLevel(GraphicsSettings.QualityLevel);
        QualitySettings.vSyncCount = GraphicsSettings.VSyncState;
        RenderSettings.ambientLight = new Color(GraphicsSettings.Brightness, GraphicsSettings.Brightness, GraphicsSettings.Brightness, 1);
    }

    private void Awake()
    {
        LoadSettings();
    }
}
