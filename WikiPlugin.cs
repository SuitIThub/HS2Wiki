using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HS2Wiki.api;
using KKAPI.Utilities;
using UnityEngine;

namespace HS2Wiki;

[BepInPlugin("com.suit.hs2wiki", "HS2 Wiki", "1.2.0")]
[BepInProcess("StudioNEOV2")]
public class WikiPlugin : BaseUnityPlugin
{
    internal static new ManualLogSource Logger;
    private Rect _windowRect = new(200, 200, 1730, 1000);
    private Rect _imageRect = new(300, 300, 100, 100);
    private Vector2 _scrollPosition;
    private Vector2 _sidebarScrollPosition;
    private WikiAPI.PageInfo _selectedPage;
    private Dictionary<string, bool> _categoryFoldouts;
    private Dictionary<string, List<string>> _categoryTree;

    private const int _uniqueId = ('V' << 24) | ('I' << 16) | ('D' << 8) | 'E';

    public static WikiAPI PublicAPI;

    private bool _uiShow;
    private bool _isResizing = false;
    private Vector2 _resizeStartPosition;
    private Vector2 _originalSize;

    private Texture2D _image = null;

    

    public static ConfigEntry<KeyboardShortcut> KeyGui { get; private set; }
    private ConfigEntry<string> SavedFoldoutConfig { get; set; }

    private void Awake()
    {
        // Plugin startup logic
        Logger = base.Logger;
        Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");

        Init();

        // API initialisieren
        PublicAPI = new WikiAPI();
        PublicAPI.Initialize(this);
        Logger.LogInfo("Wiki API bereitgestellt!");
    }
    
    private void Init() {
        SavedFoldoutConfig = Config.Bind(
                "General", "Saved FoldoutConfig",
                string.Empty,
                new ConfigDescription("List of saved foldout configs in JSON format.", null, "Advanced", new BrowsableAttribute(false)));
        SavedFoldoutConfig.SettingChanged += (sender, args) => LoadFoldoutConfig();
        LoadFoldoutConfig();
    
        KeyGui = Config.Bind(
                "Keyboard shortcuts", "Open Wiki",
                new KeyboardShortcut(KeyCode.F3),
                new ConfigDescription("Open the wiki window."));
    }

    private void LoadFoldoutConfig() {
        // Initialize _categoryFoldouts if it's null
        _categoryFoldouts ??= [];
        
        if (!string.IsNullOrEmpty(SavedFoldoutConfig.Value))
        {
            string[] openFoldouts = SavedFoldoutConfig.Value.Split(',');
            foreach (string foldout in openFoldouts)
            {
                if (!_categoryFoldouts.ContainsKey(foldout))
                {
                    _categoryFoldouts.Add(foldout, true);
                }
                else
                {
                    _categoryFoldouts[foldout] = true;
                }
            }
        }
    }

    private void SaveFoldoutConfig() {
        // Ensure _categoryFoldouts is initialized
        if (_categoryFoldouts != null)
        {
            string[] openFoldouts = [.. _categoryFoldouts.Keys.Where(k => _categoryFoldouts[k])];
            SavedFoldoutConfig.Value = string.Join(",", openFoldouts);
        }
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
            string windowTitle = _selectedPage == null ? "HS2 Wiki" : $"HS2 Wiki - {_selectedPage.PageName}";
            _windowRect = GUILayout.Window(_uniqueId + 2, _windowRect, DrawWindow, windowTitle);
        }
        if (_image != null)
        {
            // Use GUI.Window but capture returned position for dragging
            Rect newRect = GUI.Window(_uniqueId + 3, _imageRect, DrawImageWindow, "Image");
            // Only update position, not size
            _imageRect.x = newRect.x;
            _imageRect.y = newRect.y;
        }
    }

    public void OpenImage(string path) 
    {
        Logger.LogInfo($"Trying to open image: {path}");
        if (File.Exists(path))
        {
            byte[] data = File.ReadAllBytes(path);
            _image = new Texture2D(2, 2);
            _image.LoadImage(data);
            _imageRect.width = _image.width * 3;
            _imageRect.height = _image.height * 3;

            // if the image is either too high or too wide based on the screen size, scale it down to make it fit
            if (_image.width > Screen.width || _image.height > Screen.height)
            {
                float aspectRatio = (float)_image.width / _image.height;
                if (_image.width > Screen.width)
                {
                    _imageRect.width = Screen.width;
                    _imageRect.height = _imageRect.width / aspectRatio;
                }
                else if (_image.height > Screen.height)
                {
                    _imageRect.height = Screen.height;
                    _imageRect.width = _imageRect.height * aspectRatio;
                }
            }
        }
    }

    public void OpenPage(WikiAPI.PageInfo page)
    {
        if (_selectedPage != page)
        {
            OpenFoldoutsToPage(page.Category);
            _selectedPage = page;
            _scrollPosition = Vector2.zero;
        }
    }

    private void OpenFoldoutsToPage(string category)
    {
        // Initialize _categoryFoldouts if it's null
        _categoryFoldouts ??= [];
        
        string currentCategory = category;
        _categoryFoldouts[currentCategory] = true;
        while (currentCategory != null)
        {
            int lastSlashIndex = currentCategory.LastIndexOf('/');
            if (lastSlashIndex >= 0)
            {
                currentCategory = currentCategory.Substring(0, lastSlashIndex);
                _categoryFoldouts[currentCategory] = true;
            }
            else
            {
                break;
            }
        }
        SaveFoldoutConfig();
    }

    private void OpenAllFoldouts()
    {
        foreach (var category in _categoryFoldouts.Keys)
        {
            _categoryFoldouts[category] = true;
        }
    }

    private void CloseAllFoldouts()
    {
        foreach (var category in _categoryFoldouts.Keys)
        {
            _categoryFoldouts[category] = false;
        }
    }

    private void DrawWindow(int id)
    {
        // GUIStyle resizeButtonStyle = new(GUI.skin.button)
        // {
        //     padding = new RectOffset(0, 0, 0, 0),
        //     margin = new RectOffset(0, 0, 0, 0)
        // };

        // Close button in top right
        if (GUI.Button(new Rect(_windowRect.width - 25, 2, 20, 16), "x"))
        {
            _uiShow = false;
        }

        GUILayout.BeginHorizontal();

        // Seitenleiste mit Kategorien
        // Fixed width vertical layout for the sidebar
        GUILayout.BeginVertical(GUILayout.Width(400), GUILayout.ExpandHeight(true));
        
        // Add a scrollview for the categories
        _sidebarScrollPosition = GUILayout.BeginScrollView(_sidebarScrollPosition, false, true, GUILayout.ExpandHeight(true));
        
        // Create a containing non-expanding vertical group for category content
        GUILayout.BeginVertical(GUILayout.ExpandHeight(false));
        
        // Gruppieren nach Kategorie
        var pagesByCategory = PublicAPI.GetPages()
            .GroupBy(p => p.Category)
            .ToDictionary(g => g.Key, g => g.ToList());
            
        // Die Faltungszustände werden jetzt in BuildCategoryTree() initialisiert
        
        // Style für Kategorie-Überschriften
        GUIStyle categoryStyle = new(GUI.skin.label)
        {
            normal = { textColor = Color.white },
            onNormal = { textColor = Color.white },
            fontStyle = FontStyle.Bold,
            margin = new RectOffset(0, 0, 0, 0),
            padding = new RectOffset(0, 0, 0, 0)
        };

        // Style for buttons to reduce padding
        GUIStyle compactButton = new(GUI.skin.button)
        {
            margin = new RectOffset(0, 0, 0, 0),
            padding = new RectOffset(5, 5, 2, 2)
        };

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Open All"))
        {
            OpenAllFoldouts();
        }
        if (GUILayout.Button("Close All"))
        {
            CloseAllFoldouts();
        }
        GUILayout.EndHorizontal();

        // Zeige alle Hauptkategorien an
        if (_categoryTree != null)
        {
            var rootCategories = _categoryTree.Keys
                .Where(k => !k.Contains('/'))
                .OrderBy(k => k)
                .ToList();
            
            foreach (var rootCategory in rootCategories)
            {
                DrawCategoryWithSubcategories(rootCategory, pagesByCategory, categoryStyle, compactButton, 0);
            }
        }
        else
        {
            // Falls der Kategoriebaum noch nicht initialisiert wurde
            GUILayout.Label("No categories found or tree has not yet been initialized.");
        }
        
        GUILayout.FlexibleSpace();
        // End the non-expanding content group
        GUILayout.EndVertical();
        
        GUILayout.EndScrollView();
        GUILayout.EndVertical();

        // Page Content
        GUILayout.BeginVertical();
        _scrollPosition = GUILayout.BeginScrollView(_scrollPosition);
        if (_selectedPage != null)
        {
            _selectedPage.PageContentCallback?.Invoke();
        }
        else
        {
            GUILayout.Label("No page selected.");
        }
        GUILayout.EndScrollView();
        GUILayout.EndVertical();

        GUILayout.EndHorizontal();
        
        // // Draw resize button in the bottom-right corner
        // Rect resizeBtnRect = new(_windowRect.width - 20, _windowRect.height - 20, 16, 16);
        // GUI.Box(resizeBtnRect, "↘", resizeButtonStyle);
        
        // // Handle resize button dragging
        // HandleResizeButton(resizeBtnRect, ref _windowRect);
        
        // Only use DragWindow for the title bar area
        GUI.DragWindow(new Rect(0, 0, _windowRect.width, 20));
        IMGUIUtils.EatInputInRect(_windowRect);
    }

    private void HandleResizeButton(Rect resizeBtnRect, ref Rect windowRect)
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
                    _originalSize = new Vector2(windowRect.width, windowRect.height);
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
                    windowRect.width = width;
                    windowRect.height = height;
                    
                    currentEvent.Use();
                }
                break;
        }
    }

    // Baut eine Baumstruktur aus den Kategorien auf
    private void BuildCategoryTree(Dictionary<string, List<WikiAPI.PageInfo>> pagesByCategory)
    {
        _categoryTree = [];
        
        // Initialisiere die Faltouts für alle Kategorien, wenn sie noch nicht existieren
        _categoryFoldouts ??= [];
        
        // Alle Kategorien durchlaufen und die Hierarchie aufbauen
        foreach (var fullCategory in pagesByCategory.Keys)
        {
            // Stellen Sie sicher, dass die Kategorie im Faltout-Dictionary existiert
            if (!_categoryFoldouts.ContainsKey(fullCategory))
            {
                _categoryFoldouts[fullCategory] = false; // Standardmäßig ausgeklappt
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
                    _categoryTree[currentPath] = [];
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
    private void DrawCategoryWithSubcategories(string category,
        Dictionary<string, List<WikiAPI.PageInfo>> pagesByCategory, 
        GUIStyle categoryStyle, GUIStyle buttonStyle, int indentLevel)
    {
        // Initialize _categoryFoldouts if it's null
        _categoryFoldouts ??= [];
        
        // Ensure this category exists in the dictionary
        if (!_categoryFoldouts.ContainsKey(category))
        {
            _categoryFoldouts[category] = false;
        }
        
        // Den letzten Teil des Kategoriepfads extrahieren, wenn es sich um einen verschachtelten Pfad handelt
        string[] parts = category.Split('/');
        string categoryName = parts[parts.Length - 1];
        
        // Einrückung basierend auf der Ebene
        GUILayout.BeginVertical(GUI.skin.box);
        GUILayout.BeginHorizontal();
        GUILayout.Space(indentLevel * 15); // 15 Pixel Einrückung pro Ebene
        
        // Faltbarer Header mit Pfeilsymbol für die Kategorie
        bool isFoldoutOpen = _categoryFoldouts[category];
        _categoryFoldouts[category] = GUILayout.Toggle(isFoldoutOpen, isFoldoutOpen ? "▼ " + categoryName : "► " + categoryName, categoryStyle, GUILayout.ExpandWidth(true));
        if (isFoldoutOpen != _categoryFoldouts[category])
        {
            SaveFoldoutConfig();
        }
            
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
                    var style = new GUIStyle(buttonStyle);
                    if (page == _selectedPage)
                    {
                        style.normal.textColor = Color.yellow;
                    }
                    if (GUILayout.Button(page.PageName, style, GUILayout.ExpandWidth(true)))
                    {
                        if (page != _selectedPage)
                        {
                            _selectedPage = page;
                            _scrollPosition = Vector2.zero;
                        }
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
                    DrawCategoryWithSubcategories(subCategory, pagesByCategory, categoryStyle, buttonStyle, indentLevel + 1);
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

    public void DrawImageWindow(int id) {
        // GUIStyle resizeButtonStyle = new(GUI.skin.button)
        // {
        //     padding = new RectOffset(0, 0, 0, 0),
        //     margin = new RectOffset(0, 0, 0, 0)
        // };

        // Close button in top right
        if (GUI.Button(new Rect(_imageRect.width - 25, 2, 20, 16), "x"))
        {
            _image = null;
        }

        // Calculate the available space for the image (accounting for window borders and padding)
        float availableWidth = _imageRect.width - 10;  // 5px padding on each side
        float availableHeight = _imageRect.height - 25; // Account for title bar and padding
        
        // Calculate aspect ratio to maintain proportions
        float aspectRatio = (float)_image.width / _image.height;
        float displayWidth, displayHeight;
        
        // Determine dimensions that fit within the available space while maintaining aspect ratio
        if (availableWidth / aspectRatio <= availableHeight) {
            // Width constrained
            displayWidth = availableWidth;
            displayHeight = availableWidth / aspectRatio;
        } else {
            // Height constrained
            displayHeight = availableHeight;
            displayWidth = availableHeight * aspectRatio;
        }
        
        // Calculate the centered position for the image
        float leftMargin = (_imageRect.width - displayWidth) * 0.5f;
        float topMargin = 20 + (availableHeight - displayHeight) * 0.5f;
        
        // Draw the texture directly with scaling
        Rect imageRect = new(leftMargin, topMargin, displayWidth, displayHeight);
        GUI.DrawTexture(imageRect, _image, ScaleMode.StretchToFill);
        
        // // Draw resize button in the bottom-right corner
        // Rect resizeBtnRect = new(_imageRect.width - 20, _imageRect.height - 20, 16, 16);
        // GUI.Box(resizeBtnRect, "↘", resizeButtonStyle);
        
        // // Handle resize button dragging
        // HandleResizeButton(resizeBtnRect, ref _imageRect);

        GUI.DragWindow(new Rect(0, 0, _imageRect.width, _imageRect.height));
        IMGUIUtils.EatInputInRect(_imageRect);
    }
}