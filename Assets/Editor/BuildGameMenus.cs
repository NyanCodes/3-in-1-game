using System;
using ThreeInOneGame;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class BuildGameMenus
{
    private static readonly Color Navy = new Color(0.035f, 0.065f, 0.12f);
    private static readonly Color Panel = new Color(0.075f, 0.13f, 0.21f, 0.98f);
    private static readonly Color ButtonColor = new Color(0.14f, 0.27f, 0.38f);
    private static readonly Color Cyan = new Color(0.27f, 0.82f, 0.88f);
    private static readonly Font Font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

    [MenuItem("Tools/Build 3 in 1 Menus")]
    public static void BuildAllMenus()
    {
        if (EditorApplication.isPlaying)
        {
            Debug.LogWarning("Stop Play mode before building the menu scenes.");
            return;
        }

        string[] names = { "MainMenu", "Driving", "Flying", "SumoBall" };
        foreach (string name in names)
        {
            Scene scene = EditorSceneManager.OpenScene("Assets/Scenes/" + name + ".unity", OpenSceneMode.Single);
            GameObject camera = GameObject.Find("Main Camera");
            GameMenus controller = camera != null ? camera.GetComponent<GameMenus>() : null;
            if (controller == null)
                throw new InvalidOperationException("GameMenus component is missing from " + name);

            GameObject oldCanvas = GameObject.Find("Game Menu Canvas");
            if (oldCanvas != null)
                UnityEngine.Object.DestroyImmediate(oldCanvas);
            GameObject oldEventSystem = GameObject.Find("Game Menu EventSystem");
            if (oldEventSystem != null)
                UnityEngine.Object.DestroyImmediate(oldEventSystem);

            Canvas canvas = CreateCanvas();
            if (name == "MainMenu")
                BuildMainMenu(canvas.transform, controller);
            else
                BuildPauseMenu(canvas.transform, controller);

            CreateEventSystem();
            canvas.transform.localScale = Vector3.one;
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity", OpenSceneMode.Single);
        Debug.Log("Built and saved the main menu and all three in-game menus.");
    }

    private static Canvas CreateCanvas()
    {
        GameObject go = new GameObject("Game Menu Canvas", typeof(RectTransform), typeof(Canvas),
            typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = go.GetComponent<Canvas>();
        go.transform.localScale = Vector3.one;
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        CanvasScaler scaler = go.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280, 720);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
        return canvas;
    }

    private static void CreateEventSystem()
    {
        InputActionAsset actions = AssetDatabase.LoadAssetAtPath<InputActionAsset>("Assets/InputSystem_Actions.inputactions");
        if (actions == null)
            throw new InvalidOperationException("The project's InputSystem_Actions asset is missing.");

        GameObject go = new GameObject("Game Menu EventSystem", typeof(EventSystem));
        InputSystemUIInputModule input = go.AddComponent<InputSystemUIInputModule>();
        input.actionsAsset = actions;
        input.point = ActionReference(actions, "Point");
        input.leftClick = ActionReference(actions, "Click");
        input.rightClick = ActionReference(actions, "RightClick");
        input.middleClick = ActionReference(actions, "MiddleClick");
        input.scrollWheel = ActionReference(actions, "ScrollWheel");
        input.move = ActionReference(actions, "Navigate");
        input.submit = ActionReference(actions, "Submit");
        input.cancel = ActionReference(actions, "Cancel");
        input.trackedDevicePosition = ActionReference(actions, "TrackedDevicePosition");
        input.trackedDeviceOrientation = ActionReference(actions, "TrackedDeviceOrientation");
        AssetDatabase.SaveAssets();
    }

    private static InputActionReference ActionReference(InputActionAsset actions, string name)
    {
        const string path = "Assets/Settings/MenuUIReferences.asset";
        InputAction action = actions.FindAction("UI/" + name, true);
        foreach (UnityEngine.Object item in AssetDatabase.LoadAllAssetsAtPath(path))
            if (item is InputActionReference existing && existing.action != null && existing.action.id == action.id)
                return existing;

        InputActionReference reference = InputActionReference.Create(action);
        reference.name = name;
        if (AssetDatabase.LoadMainAssetAtPath(path) == null)
            AssetDatabase.CreateAsset(reference, path);
        else
            AssetDatabase.AddObjectToAsset(reference, path);
        return reference;
    }

    private static void BuildMainMenu(Transform root, GameMenus controller)
    {
        StretchImage(root, "Background", Navy);
        Image rightWash = Image(root, "Right Accent", new Vector2(870, 0), new Vector2(410, 720), new Color(0.055f, 0.16f, 0.23f));
        rightWash.rectTransform.anchorMin = new Vector2(0, 0);
        rightWash.rectTransform.anchorMax = new Vector2(0, 0);
        rightWash.rectTransform.pivot = new Vector2(0, 0);
        rightWash.rectTransform.anchoredPosition = new Vector2(870, 0);
        Image(root, "Top Line", new Vector2(0, 355), new Vector2(1280, 10), Cyan);
        Image(root, "Title Accent", new Vector2(-270, 160), new Vector2(8, 125), Cyan);

        Text(root, "Title", "3 in 1 Game!", new Vector2(0, 215), new Vector2(920, 95), 68, Color.white, FontStyle.Bold);
        Text(root, "Subtitle", "THREE GAMES. ONE PLACE TO PLAY.", new Vector2(0, 145),
            new Vector2(700, 40), 20, Cyan, FontStyle.Bold);

        Image(root, "Menu Panel", new Vector2(0, -105), new Vector2(650, 385), Panel);
        AddButton(root, "Mad Driver", new Vector2(0, 35), new Vector2(540, 62), controller.StartDriving);
        AddButton(root, "Fly Like a Bird", new Vector2(0, -42), new Vector2(540, 62), controller.StartFlying);
        AddButton(root, "I'm a Sumo and a Ball", new Vector2(0, -119), new Vector2(540, 62), controller.StartSumoBall);
        AddButton(root, "Exit", new Vector2(0, -209), new Vector2(540, 58), controller.ExitGame);
    }

    private static void BuildPauseMenu(Transform root, GameMenus controller)
    {
        Button open = AddButton(root, "MENU  [ESC]", new Vector2(-115, -45), new Vector2(190, 58), controller.OpenPauseMenu);
        RectTransform openRect = open.GetComponent<RectTransform>();
        openRect.anchorMin = Vector2.one;
        openRect.anchorMax = Vector2.one;
        openRect.anchoredPosition = new Vector2(-115, -45);

        GameObject overlay = new GameObject("Pause Menu", typeof(RectTransform), typeof(Image));
        overlay.transform.SetParent(root, false);
        RectTransform overlayRect = overlay.GetComponent<RectTransform>();
        overlayRect.anchorMin = Vector2.zero;
        overlayRect.anchorMax = Vector2.one;
        overlayRect.offsetMin = Vector2.zero;
        overlayRect.offsetMax = Vector2.zero;
        overlay.GetComponent<Image>().color = new Color(0.025f, 0.045f, 0.09f, 0.88f);

        Image(overlay.transform, "Panel", Vector2.zero, new Vector2(570, 500), Panel);
        Image(overlay.transform, "Accent Line", new Vector2(0, 247), new Vector2(570, 6), Cyan);
        Text(overlay.transform, "Heading", "Game Paused", new Vector2(0, 165),
            new Vector2(500, 78), 50, Color.white, FontStyle.Bold);
        AddButton(overlay.transform, "Resume", new Vector2(0, 45), new Vector2(430, 65), controller.Resume);
        AddButton(overlay.transform, "Restart", new Vector2(0, -40), new Vector2(430, 65), controller.Restart);
        AddButton(overlay.transform, "Back to Main Menu", new Vector2(0, -125), new Vector2(430, 65), controller.BackToMainMenu);

        SerializedObject serialized = new SerializedObject(controller);
        serialized.FindProperty("pausePanel").objectReferenceValue = overlay;
        serialized.ApplyModifiedPropertiesWithoutUndo();
        overlay.SetActive(false);
    }

    private static Image StretchImage(Transform parent, string name, Color color)
    {
        Image image = Image(parent, name, Vector2.zero, Vector2.zero, color);
        RectTransform rect = image.rectTransform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        return image;
    }

    private static Image Image(Transform parent, string name, Vector2 position, Vector2 size, Color color)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.sizeDelta = size;
        rect.anchoredPosition = position;
        Image image = go.GetComponent<Image>();
        image.color = color;
        return image;
    }

    private static Text Text(Transform parent, string name, string value, Vector2 position,
        Vector2 size, int fontSize, Color color, FontStyle fontStyle = FontStyle.Normal)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(Text));
        go.transform.SetParent(parent, false);
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.sizeDelta = size;
        rect.anchoredPosition = position;
        Text text = go.GetComponent<Text>();
        text.text = value;
        text.font = Font;
        text.fontSize = fontSize;
        text.fontStyle = fontStyle;
        text.color = color;
        text.alignment = TextAnchor.MiddleCenter;
        text.raycastTarget = false;
        return text;
    }

    private static Button AddButton(Transform parent, string label, Vector2 position,
        Vector2 size, UnityAction action)
    {
        Image image = Image(parent, label + " Button", position, size, ButtonColor);
        Button button = image.gameObject.AddComponent<Button>();
        ColorBlock colors = button.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(0.75f, 1f, 1f);
        colors.pressedColor = new Color(0.45f, 0.85f, 0.9f);
        colors.selectedColor = Color.white;
        button.colors = colors;
        UnityEventTools.AddPersistentListener(button.onClick, action);
        Text(image.transform, "Label", label, Vector2.zero, size, 26, Color.white, FontStyle.Bold);
        return button;
    }
}
