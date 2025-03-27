#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UIElements;
using System.Reflection;
using System.Linq;
using System.IO;
using VRseBuilder.Core.Utility;
using UnityEngine.SceneManagement;
using System;

/// <summary>
/// Adds a dropdown menu to the Unity Editor toolbar to allow quick switching between
/// development scenes (experiences) as defined in the project configuration.
/// </summary>
[InitializeOnLoad]
public static class VrseExperienceSwitcher
{
    /// <summary>Array of scene names extracted from the project configuration.</summary>
    private static string[] sceneNames = Array.Empty<string>();

    /// <summary>Index of the currently selected scene in the dropdown.</summary>
    private static int selectedIndex = -1;

    /// <summary>Tracks the name of the last active scene for comparison.</summary>
    private static string lastActiveScene = string.Empty;

    /// <summary>Reference to the visual element added to the Unity toolbar.</summary>
    private static VisualElement toolbarUI;

    /// <summary>UI margin offset to position the dropdown closer to the Play button.</summary>
    private static readonly float positionOffset = 200f;

    /// <summary>Height of the dropdown selection box.</summary>
    private static readonly float dropdownBoxHeight = 20f;

    /// <summary>Reference to the loaded project configuration.</summary>
    private static ProjectConfig projectConfig;

    /// <summary>
    /// Static constructor, executed on load, schedules toolbar initialization.
    /// </summary>
    static VrseExperienceSwitcher()
    {
        try
        {
            EditorApplication.delayCall += Initialize;
        }
        catch (Exception e)
        {
            Debug.LogError($"VrseSceneSwitcher initialization failed: {e.Message}");
        }
    }

    /// <summary>
    /// Initializes the toolbar UI and scene data by fetching project config and setting up callbacks.
    /// </summary>
    private static void Initialize()
    {
        try
        {
            projectConfig = EditorProjectSettings.GetProjectConfig();
            if (projectConfig == null)
            {
                Debug.LogWarning("ProjectConfig is null. Scene switcher may not work correctly.");
                return;
            }

            RefreshSceneList();
            SelectCurrentScene();

            EditorSceneManager.activeSceneChangedInEditMode += OnActiveSceneChanged;
            EditorApplication.playModeStateChanged += OnPlayModeChanged;

            AddToolbarUI();
        }
        catch (Exception e)
        {
            Debug.LogError($"VrseSceneSwitcher initialization failed: {e.Message}");
        }
    }

    /// <summary>
    /// Adds the dropdown UI to the right-aligned area of Unity's editor toolbar.
    /// </summary>
    static void AddToolbarUI()
    {
        try
        {
            var toolbarType = typeof(Editor).Assembly.GetType("UnityEditor.Toolbar");
            if (toolbarType == null)
                return;

            var toolbars = Resources.FindObjectsOfTypeAll(toolbarType);
            if (toolbars.Length == 0)
                return;

            var rootField = toolbarType.GetField("m_Root", BindingFlags.NonPublic | BindingFlags.Instance);
            var root = rootField?.GetValue(toolbars[0]) as VisualElement;

            var rightContainer = root?.Q("ToolbarZoneRightAlign");
            if (rightContainer == null)
                return;

            if (toolbarUI != null)
                rightContainer.Remove(toolbarUI);

            toolbarUI = new IMGUIContainer(OnGUI)
            {
                style = { marginRight = positionOffset }
            };
            rightContainer.Add(toolbarUI);
        }
        catch (Exception e)
        {
            Debug.LogError($"Error adding toolbar UI: {e.Message}");
        }
    }

    /// <summary>
    /// Draws the dropdown menu in the Unity toolbar and handles selection logic.
    /// </summary>
    static void OnGUI()
    {
        try
        {
            CheckAndRefreshScenes();

            if (projectConfig?.roomManagerConfig?.experiences == null)
            {
                EditorGUILayout.LabelField("Project configuration not loaded.");
                return;
            }

            if (selectedIndex >= sceneNames.Length)
                selectedIndex = 0;

            bool isPlaying = EditorApplication.isPlaying;
            GUILayout.BeginHorizontal();

            EditorGUI.BeginDisabledGroup(isPlaying);
            RefreshSceneList();
            SelectCurrentScene();
            EditorGUI.EndDisabledGroup();

            EditorGUI.BeginDisabledGroup(isPlaying);
            GUIStyle popupStyle = new GUIStyle(EditorStyles.popup) { fixedHeight = dropdownBoxHeight };

            string[] displayedScenes = new string[sceneNames.Length + 1];
            displayedScenes[0] = "Select a Scene";
            Array.Copy(sceneNames, 0, displayedScenes, 1, sceneNames.Length);

            int displayIndex = selectedIndex == -1 ? 0 : selectedIndex + 1;
            int newDisplayIndex = EditorGUILayout.Popup(displayIndex, displayedScenes, popupStyle,
                GUILayout.Width(150), GUILayout.Height(dropdownBoxHeight));

            if (newDisplayIndex > 0 && newDisplayIndex != displayIndex)
            {
                selectedIndex = newDisplayIndex - 1;
                LoadScene(sceneNames[selectedIndex]);
            }

            EditorGUI.EndDisabledGroup();
            GUILayout.EndHorizontal();
        }
        catch (Exception e)
        {
            Debug.LogError($"Error in OnGUI: {e.Message}");
        }
    }

    /// <summary>
    /// Updates the list of available scenes from the current project configuration.
    /// </summary>
    static void RefreshSceneList()
    {
        try
        {
            sceneNames = projectConfig?.roomManagerConfig?.experiences?
                .Select(experience => experience.GetRoomName())
                .Where(name => !string.IsNullOrEmpty(name))
                .ToArray() ?? Array.Empty<string>();
        }
        catch (Exception e)
        {
            Debug.LogError($"Error refreshing scene list: {e.Message}");
            sceneNames = Array.Empty<string>();
        }
    }

    /// <summary>
    /// Re-checks the scene list and updates it if changes were detected in the configuration.
    /// </summary>
    static void CheckAndRefreshScenes()
    {
        try
        {
            string[] currentScenes = projectConfig?.roomManagerConfig?.experiences?
                .Select(experience => experience.GetRoomName())
                .Where(name => !string.IsNullOrEmpty(name))
                .ToArray() ?? Array.Empty<string>();

            if (!currentScenes.SequenceEqual(sceneNames))
            {
                sceneNames = currentScenes;
                SelectCurrentScene();
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error checking and refreshing scenes: {e.Message}");
        }
    }

    /// <summary>
    /// Determines which experience (scene) is currently open and updates the selection index.
    /// </summary>
    static void SelectCurrentScene()
    {
        try
        {
            string currentScene = EditorSceneManager.GetActiveScene().path;
            selectedIndex = -1;
            for (int i = 0; i < projectConfig.roomManagerConfig.experiences.Length; i++)
            {
                var experience = projectConfig.roomManagerConfig.experiences[i];
                if (experience?.GetDevScene() == currentScene)
                {
                    selectedIndex = i;
                    lastActiveScene = experience.GetRoomName() ?? string.Empty;
                    break;
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error selecting current scene: {e.Message}");
        }
    }

    /// <summary>
    /// Callback invoked when the active scene changes in the Editor.
    /// </summary>
    static void OnActiveSceneChanged(Scene prevScene, Scene currentScene)
    {
        try
        {
            UpdateSceneSelection();
        }
        catch (Exception e)
        {
            Debug.LogError($"Error on active scene changed: {e.Message}");
        }
    }

    /// <summary>
    /// Updates the internal state based on the newly active scene.
    /// </summary>
    static void UpdateSceneSelection()
    {
        try
        {
            string currentScenePath = EditorSceneManager.GetActiveScene().path;
            string currentSceneName = string.Empty;
            bool sceneFound = false;

            foreach (var experience in projectConfig.roomManagerConfig.experiences)
            {
                if (experience.GetDevScene() == currentScenePath)
                {
                    currentSceneName = experience.GetRoomName();
                    sceneFound = true;
                    break;
                }
            }

            if (sceneFound)
            {
                if (currentSceneName != lastActiveScene)
                {
                    lastActiveScene = currentSceneName;
                    SelectCurrentScene();
                }
            }
            else
            {
                selectedIndex = -1;
                lastActiveScene = string.Empty;
                Debug.Log($"Current scene {currentScenePath} not found in project experiences.");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error updating scene selection: {e.Message}");
        }
    }

    /// <summary>
    /// Loads the selected development and art scenes from the configuration.
    /// </summary>
    /// <param name="sceneName">The name of the scene to load.</param>
    static void LoadScene(string sceneName)
    {
        try
        {
            var experience = projectConfig?.roomManagerConfig?.experiences?
                .FirstOrDefault(exp => exp.GetRoomName() == sceneName);

            if (experience == null)
            {
                Debug.LogError($"Experience not found for scene: {sceneName}");
                return;
            }

            string devScenePath = experience.GetDevScene();
            string artScenePath = experience.GetArtScene();

            if (string.IsNullOrEmpty(devScenePath) || string.IsNullOrEmpty(artScenePath))
            {
                Debug.LogError($"Invalid scene paths for {sceneName}");
                return;
            }

            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                EditorSceneManager.OpenScene(devScenePath);
                EditorSceneManager.OpenScene(artScenePath, OpenSceneMode.Additive);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error loading scene {sceneName}: {e.Message}");
        }
    }

    /// <summary>
    /// Re-initializes the toolbar UI when entering or exiting Play mode.
    /// </summary>
    static void OnPlayModeChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredPlayMode || state == PlayModeStateChange.ExitingPlayMode)
        {
            EditorApplication.delayCall += AddToolbarUI;
        }
    }
}
#endif
