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

[BepInPlugin("com.suit.hs2wiki", "HS2 Wiki", "1.1.0")]
[BepInProcess("StudioNEOV2")]
public class WikiPlugin : BaseUnityPlugin
{
    internal static new ManualLogSource Logger;
    private Rect _windowRect = new Rect(100, 100, 1630, 720);
    private Vector2 _scrollPosition;
    private Vector2 _sidebarScrollPosition;
    private WikiAPI.PageInfo _selectedPage;
    private Dictionary<string, bool> _categoryFoldouts;
    private Dictionary<string, List<string>> _categoryTree;

    private const int _uniqueId = ('V' << 24) | ('I' << 16) | ('D' << 8) | 'E';

    public static WikiAPI PublicAPI;

    private Texture2D exampleImage;
    private string imagePath = Path.Combine(Paths.PluginPath, "HS2Wiki", "example.png");

    private bool _uiShow;
    private bool _isResizing = false;
    private Vector2 _resizeStartPosition;
    private Vector2 _originalSize;

    public static ConfigEntry<KeyboardShortcut> KeyGui { get; private set; }

    private void Awake()
    {
        // Plugin startup logic
        Logger = base.Logger;
        Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");

        // API initialisieren
        PublicAPI = new WikiAPI();
        PublicAPI.Initialize(this);
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
    
    // Kategoriebaum bei Bedarf neu erstellen
    public void RebuildCategoryTree()
    {
        var pages = PublicAPI.GetPages();
        var pagesByCategory = pages
            .GroupBy(p => p.Category)
            .ToDictionary(g => g.Key, g => g.ToList());
            
        BuildCategoryTree(pagesByCategory);
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
        // Define resize button style
        GUIStyle resizeButtonStyle = new GUIStyle(GUI.skin.button);
        resizeButtonStyle.padding = new RectOffset(0, 0, 0, 0);
        resizeButtonStyle.margin = new RectOffset(0, 0, 0, 0);
        
        GUILayout.BeginHorizontal();

        // Seitenleiste mit Kategorien
        // Fixed width vertical layout for the sidebar
        GUILayout.BeginVertical(GUILayout.Width(300), GUILayout.ExpandHeight(true));
        
        // Add a scrollview for the categories
        _sidebarScrollPosition = GUILayout.BeginScrollView(_sidebarScrollPosition, false, true, GUILayout.ExpandHeight(true));
        
        // Create a containing non-expanding vertical group for category content
        GUILayout.BeginVertical(GUILayout.ExpandHeight(false));
        
        // Gruppieren nach Kategorie
        var pagesByCategory = WikiPlugin.PublicAPI.GetPages()
            .GroupBy(p => p.Category)
            .ToDictionary(g => g.Key, g => g.ToList());
            
        // Die Faltungszustände werden jetzt in BuildCategoryTree() initialisiert
        
        // Style für Kategorie-Überschriften
        GUIStyle categoryStyle = new GUIStyle(GUI.skin.GetStyle("foldout"));
        categoryStyle.normal.textColor = Color.white;
        categoryStyle.onNormal.textColor = Color.white;
        categoryStyle.fontStyle = FontStyle.Bold;
        categoryStyle.margin = new RectOffset(0, 0, 0, 0);
        categoryStyle.padding = new RectOffset(0, 0, 0, 0);
        
        // Style for buttons to reduce padding
        GUIStyle compactButton = new GUIStyle(GUI.skin.button);
        compactButton.margin = new RectOffset(0, 0, 0, 0);
        compactButton.padding = new RectOffset(5, 5, 2, 2);
        
        // Zeige alle Hauptkategorien an
        if (_categoryTree != null)
        {
            var rootCategories = _categoryTree.Keys
                .Where(k => !k.Contains('/'))
                .OrderBy(k => k)
                .ToList();
            
            foreach (var rootCategory in rootCategories)
            {
                DrawCategoryWithSubcategories(rootCategory, "", pagesByCategory, categoryStyle, compactButton, 0);
            }
        }
        else
        {
            // Falls der Kategoriebaum noch nicht initialisiert wurde
            GUILayout.Label("Keine Kategorien gefunden oder Baum wurde noch nicht initialisiert.");
        }
        
        GUILayout.FlexibleSpace();
        // End the non-expanding content group
        GUILayout.EndVertical();
        
        GUILayout.EndScrollView();
        GUILayout.EndVertical();

        // Page Content
        GUILayout.BeginVertical();
        _scrollPosition = GUILayout.BeginScrollView(_scrollPosition);
        _selectedPage?.PageContentCallback?.Invoke();
        GUILayout.EndScrollView();
        GUILayout.EndVertical();

        GUILayout.EndHorizontal();
        
        // Draw resize button in the bottom-right corner
        Rect resizeBtnRect = new Rect(_windowRect.width - 20, _windowRect.height - 20, 16, 16);
        GUI.Box(resizeBtnRect, "↘", resizeButtonStyle);
        
        // Handle resize button dragging
        HandleResizeButton(resizeBtnRect);
        
        // Only use DragWindow for the title bar area
        GUI.DragWindow(new Rect(0, 0, _windowRect.width, 20));
        IMGUIUtils.EatInputInRect(_windowRect);
    }

    private void HandleResizeButton(Rect resizeBtnRect)
    {
        Event currentEvent = Event.current;
        Vector2 mousePos = currentEvent.mousePosition;
        
        switch (currentEvent.type)
        {
            case EventType.MouseDown:
                if (resizeBtnRect.Contains(mousePos))
                {
                    _isResizing = true;
                    _resizeStartPosition = mousePos;
                    _originalSize = new Vector2(_windowRect.width, _windowRect.height);
                    currentEvent.Use();
                }
                break;
                
            case EventType.MouseUp:
                if (_isResizing)
                {
                    _isResizing = false;
                    currentEvent.Use();
                }
                break;
                
            case EventType.MouseDrag:
                if (_isResizing)
                {
                    // Calculate the new size based on mouse movement
                    float width = Mathf.Max(300, _originalSize.x + (mousePos.x - _resizeStartPosition.x));
                    float height = Mathf.Max(200, _originalSize.y + (mousePos.y - _resizeStartPosition.y));
                    
                    // Apply the new size
                    _windowRect.width = width;
                    _windowRect.height = height;
                    
                    currentEvent.Use();
                }
                break;
        }
    }

    // Baut eine Baumstruktur aus den Kategorien auf
    private void BuildCategoryTree(Dictionary<string, List<WikiAPI.PageInfo>> pagesByCategory)
    {
        _categoryTree = new Dictionary<string, List<string>>();
        
        // Initialisiere die Faltouts für alle Kategorien, wenn sie noch nicht existieren
        if (_categoryFoldouts == null)
        {
            _categoryFoldouts = new Dictionary<string, bool>();
        }
        
        // Alle Kategorien durchlaufen und die Hierarchie aufbauen
        foreach (var fullCategory in pagesByCategory.Keys)
        {
            // Stellen Sie sicher, dass die Kategorie im Faltout-Dictionary existiert
            if (!_categoryFoldouts.ContainsKey(fullCategory))
            {
                _categoryFoldouts[fullCategory] = true; // Standardmäßig ausgeklappt
            }
            
            // Alle Teile der Kategoriehierarchie verarbeiten
            string currentPath = "";
            string[] parts = fullCategory.Split('/');
            
            for (int i = 0; i < parts.Length; i++)
            {
                string parentPath = currentPath;
                
                // Aktuellen Pfad aufbauen
                if (i == 0)
                    currentPath = parts[i];
                else
                    currentPath = $"{currentPath}/{parts[i]}";
                
                // Stellen Sie sicher, dass der aktuelle Pfad im Baum existiert
                if (!_categoryTree.ContainsKey(currentPath))
                {
                    _categoryTree[currentPath] = new List<string>();
                }
                
                // Stellen Sie sicher, dass der aktuelle Pfad im Faltout-Dictionary existiert
                if (!_categoryFoldouts.ContainsKey(currentPath))
                {
                    _categoryFoldouts[currentPath] = true;
                }
                
                // Wenn es sich nicht um die Wurzelkategorie handelt, fügen Sie sie als Unterkategorie zur übergeordneten Kategorie hinzu
                if (i > 0 && !string.IsNullOrEmpty(parentPath) && !_categoryTree[parentPath].Contains(currentPath))
                {
                    _categoryTree[parentPath].Add(currentPath);
                }
            }
        }
    }
    
    // Zeichnet eine Kategorie mit all ihren Unterkategorien rekursiv
    private void DrawCategoryWithSubcategories(string category, string displayName, 
        Dictionary<string, List<WikiAPI.PageInfo>> pagesByCategory, 
        GUIStyle categoryStyle, GUIStyle buttonStyle, int indentLevel)
    {
        // Wenn ein Anzeigename angegeben wurde, verwenden Sie diesen, andernfalls den Kategorienamen
        string categoryName = string.IsNullOrEmpty(displayName) ? category : displayName;
        
        // Den letzten Teil des Kategoriepfads extrahieren, wenn es sich um einen verschachtelten Pfad handelt
        if (string.IsNullOrEmpty(displayName))
        {
            string[] parts = category.Split('/');
            categoryName = parts[parts.Length - 1];
        }
        
        // Einrückung basierend auf der Ebene
        GUILayout.BeginVertical(GUI.skin.box);
        GUILayout.BeginHorizontal();
        GUILayout.Space(indentLevel * 15); // 15 Pixel Einrückung pro Ebene
        
        // Faltbarer Header mit Pfeilsymbol für die Kategorie
        _categoryFoldouts[category] = GUILayout.Toggle(
            _categoryFoldouts[category], 
            (_categoryFoldouts[category] ? "▼ " : "► ") + categoryName,
            categoryStyle, GUILayout.ExpandWidth(true));
            
        GUILayout.EndHorizontal();
            
        // Wenn ausgeklappt, zeige alle Seiten und Unterkategorien
        if (_categoryFoldouts[category])
        {
            // Zeige zunächst direkte Seiten dieser Kategorie an (wenn vorhanden)
            if (pagesByCategory.ContainsKey(category) && pagesByCategory[category].Count > 0)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space((indentLevel + 1) * 15);
                GUILayout.BeginVertical(GUI.skin.box, GUILayout.ExpandWidth(true));
                
                foreach (var page in pagesByCategory[category])
                {
                    if (GUILayout.Button(page.PageName, buttonStyle, GUILayout.ExpandWidth(true)))
                    {
                        _selectedPage = page;
                    }
                }
                
                GUILayout.EndVertical();
                GUILayout.EndHorizontal();
            }
            
            // Dann alle Unterkategorien anzeigen (wenn vorhanden)
            if (_categoryTree.ContainsKey(category) && _categoryTree[category].Count > 0)
            {
                foreach (var subCategory in _categoryTree[category].OrderBy(c => c))
                {
                    DrawCategoryWithSubcategories(subCategory, null, pagesByCategory, categoryStyle, buttonStyle, indentLevel + 1);
                }
            }
        }
        
        // Füge etwas Abstand zwischen den Kategorien hinzu, aber nur für die äußerste Ebene
        if (indentLevel == 0)
        {
            GUILayout.Space(2);
        }
        GUILayout.EndVertical();
    }

    private void DrawDemoPage()
    {
        GUILayout.Label("<b>This is an example page for the wiki.</b>");
        GUILayout.Space(10);

        GUILayout.Label("You can:");
        GUILayout.Label("✅ Show text");
        GUILayout.Label("✅ Use buttons");
        GUILayout.Label("✅ Use scrollable content");
        GUILayout.Label("✅ Show images");

        GUILayout.Space(10);

        if (GUILayout.Button("Click me!"))
        {
            Logger.LogInfo("Button was clicked in the example page!");
        }

        GUILayout.Space(20);
        GUILayout.Label("Image example:");

        if (exampleImage != null)
        {
            GUILayout.Box(exampleImage, GUILayout.Width(200), GUILayout.Height(200));
        }
        else
        {
            GUILayout.Label("No image found. Place a PNG file under:");
            GUILayout.Label(imagePath);
        }

        GUILayout.Space(10);

        GUILayout.Label("🎞️ Video or GIFs in Unity GUI are more difficult...");
        GUILayout.Label("Use an external plugin or HTML/asset browser window if necessary.");
    }

}