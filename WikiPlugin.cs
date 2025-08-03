using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HS2Wiki.api;
using KKAPI;
using KKAPI.Utilities;
using UnityEngine;

namespace HS2Wiki;

[BepInPlugin("com.suit.hs2wiki", "HS2 Wiki", "1.0.0")]
public class WikiPlugin : BaseUnityPlugin
{
    internal static new ManualLogSource Logger;
    private Rect _windowRect = new Rect(100, 100, 600, 400);
    private Vector2 _scrollPosition;
    private WikiAPI.PageInfo _selectedPage;
    private Dictionary<string, bool> _categoryFoldouts;

    private const int _uniqueId = ('V' << 24) | ('I' << 16) | ('D' << 8) | 'E';

    public static WikiAPI PublicAPI;

    private Texture2D exampleImage;
    private string imagePath = Path.Combine(Paths.PluginPath, "HS2Wiki", "example.png");

    private bool _uiShow;

    public static ConfigEntry<KeyboardShortcut> KeyGui { get; private set; }

    private void Awake()
    {
        // Plugin startup logic
        Logger = base.Logger;
        Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");

        PublicAPI = new WikiAPI();
        Logger.LogInfo("Wiki API bereitgestellt!");

        KeyGui = Config.Bind(
                "Keyboard shortcuts", "Open Wiki",
                new KeyboardShortcut(KeyCode.F3),
                new ConfigDescription("Open the wiki window."));

        if (File.Exists(imagePath))
        {
            byte[] data = File.ReadAllBytes(imagePath);
            exampleImage = new Texture2D(2, 2);
            exampleImage.LoadImage(data);
        }

        WikiPlugin.PublicAPI?.RegisterPage("Beispiele", "GUI-Demo", DrawDemoPage);
    }
    private void Update()
    {
        // Only allow a single type of screenshot in one frame
        if (KeyGui.Value.IsDown())
        {
            _uiShow = !_uiShow;
        }
    }
    private void OnGUI()
    {
        if (_uiShow)
        {
            _windowRect = GUILayout.Window(_uniqueId + 2, _windowRect, DrawWindow, "Wiki Panel");
        }
    }

    private void DrawWindow(int id)
    {
       
        GUILayout.BeginHorizontal();

        // Seitenleiste mit Kategorien
        GUILayout.BeginVertical(GUILayout.Width(150));
        
        // Gruppieren nach Kategorie
        var pagesByCategory = WikiPlugin.PublicAPI.GetPages()
            .GroupBy(p => p.Category)
            .ToDictionary(g => g.Key, g => g.ToList());
            
        // Faltbare Zustandspeicherung initialisieren, falls noch nicht existiert
        if (_categoryFoldouts == null)
        {
            _categoryFoldouts = new Dictionary<string, bool>();
            foreach (var category in pagesByCategory.Keys)
            {
                _categoryFoldouts[category] = true; // Standardmäßig ausgeklappt
            }
        }
        
        // Für jede Kategorie eine faltbare Gruppe anzeigen
        foreach (var category in pagesByCategory.Keys.OrderBy(k => k))
        {
            // Faltbarer Header für die Kategorie
            _categoryFoldouts[category] = GUILayout.Toggle(
                _categoryFoldouts[category], 
                category, 
                GUI.skin.GetStyle("foldout"));
                
            // Wenn ausgeklappt, zeige alle Seiten dieser Kategorie
            if (_categoryFoldouts[category])
            {
                GUILayout.BeginVertical(GUI.skin.box);
                foreach (var page in pagesByCategory[category])
                {
                    if (GUILayout.Button(page.PageName))
                    {
                        _selectedPage = page;
                    }
                }
                GUILayout.EndVertical();
            }
        }
        
        GUILayout.EndVertical();

        // Page Content
        GUILayout.BeginVertical();
        _scrollPosition = GUILayout.BeginScrollView(_scrollPosition);
        _selectedPage?.PageContentCallback?.Invoke();
        GUILayout.EndScrollView();
        GUILayout.EndVertical();

        GUILayout.EndHorizontal();
        
        // Only use DragWindow for the title bar area
        GUI.DragWindow(new Rect(0, 0, _windowRect.width, 20));
        IMGUIUtils.EatInputInRect(_windowRect);
    }

    private void DrawDemoPage()
    {
        GUILayout.Label("<b>Das ist eine Beispielseite für das Wiki.</b>");
        GUILayout.Space(10);

        GUILayout.Label("Du kannst:");
        GUILayout.Label("✅ Text anzeigen");
        GUILayout.Label("✅ Buttons verwenden");
        GUILayout.Label("✅ Scrollbare Inhalte verwenden");
        GUILayout.Label("✅ Bilder anzeigen");

        GUILayout.Space(10);

        if (GUILayout.Button("Klick mich!"))
        {
            Logger.LogInfo("Button wurde in der Beispielseite geklickt!");
        }

        GUILayout.Space(20);
        GUILayout.Label("Bild-Beispiel:");

        if (exampleImage != null)
        {
            GUILayout.Box(exampleImage, GUILayout.Width(200), GUILayout.Height(200));
        }
        else
        {
            GUILayout.Label("Kein Bild gefunden. Lege eine PNG-Datei unter:");
            GUILayout.Label(imagePath);
        }

        GUILayout.Space(10);

        GUILayout.Label("🎞️ Video oder GIFs in Unity GUI sind schwieriger...");
        GUILayout.Label("Nutze ggf. ein externes Plugin oder HTML/Asset-Browser-Fenster.");
    }

}