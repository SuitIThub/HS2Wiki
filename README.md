# HS2 Wiki Plugin

[![Latest Release](https://img.shields.io/github/v/release/SuitIThub/HS2Wiki)](https://github.com/SuitIThub/HS2Wiki/releases/latest)
[![License](https://img.shields.io/badge/License-MIT-blue.svg)](https://opensource.org/licenses/MIT)
[![BepInEx](https://img.shields.io/badge/BepInEx-5.4.22-green)](https://github.com/BepInEx/BepInEx)

A plugin for Honey Select 2 that provides an in-game wiki system. This plugin allows other plugins to register and display wiki pages to help users understand game features.

![HS2 Wiki Screenshot](example.png)

## Features

- In-game wiki accessible via a hotkey (default: F3)
- Category-based organization of wiki pages with unlimited subcategory depth
- Support for text, buttons, and images in wiki pages
- Scrollable content for detailed documentation
- Programmatic navigation between wiki pages
- Image viewer with automatic scaling
- Persistent category foldout state

## Installation

1. Ensure you have [BepInEx](https://github.com/BepInEx/BepInEx) installed
2. Place the `HS2Wiki.dll` in your `BepInEx/plugins` folder

## Usage

Press F3 (configurable) to open the wiki panel.

## For Plugin Developers: How to Register Wiki Pages

You can add your own wiki pages to the central wiki system by following these steps:

### 1. Register your wiki page

Use the public API to register your wiki pages. The simplest approach is:

```csharp
// Inside your plugin's Awake or Start method
RegisterWikiPage("Your Category", "Your Page Name", YourDrawPageMethod);
// For subcategories, use a forward slash separator
RegisterWikiPage("Your Category/Subcategory", "Your Page Name", YourDrawPageMethod);
// You can go as deep as you like
RegisterWikiPage("Your Category/Subcategory/SubSubcategory", "Your Page Name", YourDrawPageMethod);

// You can also programmatically open wiki pages
OpenWikiPage("Your Category", "Your Page Name");
OpenWikiPage("Your Category/Subcategory", "Your Page Name");

// And display images in a resizable, draggable viewer
OpenImagePage("path/to/your/image.png");


private static object apiInstance {
    get{
        Type wikiPluginType = Type.GetType("HS2Wiki.WikiPlugin, HS2Wiki");
        if (wikiPluginType == null)
        {
            Logger.LogWarning("Wiki plugin not found - registration skipped.");
            return null;
        }

        // Try to find the PublicAPI field
        FieldInfo apiField = wikiPluginType.GetField("PublicAPI", BindingFlags.Public | BindingFlags.Static);
        if (apiField == null)
        {
            Logger.LogWarning("Wiki API field not found - registration skipped.");
            return null;
        }

        object apiInstance = apiField.GetValue(null);
        if (apiInstance == null)
        {
            Logger.LogWarning("Wiki API is null - registration skipped.");
            return null;
        }
        return apiInstance;
    }
}

public static void RegisterWikiPage(string category, string pageName, Action drawPageAction)
{
    object apiInstance = WikiContent.apiInstance;
    if (apiInstance == null)
    {
        Logger.LogWarning("Wiki API is null - registration skipped.");
        return;
    }

    // Try to find the RegisterPage method
    MethodInfo registerPageMethod = apiInstance.GetType().GetMethod("RegisterPage", [
        typeof(string), typeof(string), typeof(Action)
    ]);

    if (registerPageMethod == null)
    {
        Logger.LogWarning("RegisterPage method not found.");
        return;
    }

    // Call up RegisterPage
    registerPageMethod.Invoke(apiInstance, [
        category,
        pageName,
        drawPageAction
    ]);

    Logger.LogInfo("Page successfully registered with the wiki.");
}

public static void OpenWikiPage(string category, string pageName)
{
    object apiInstance = WikiContent.apiInstance;
    if (apiInstance == null)
    {
        Logger.LogWarning("Wiki API is null - registration skipped.");
        return;
    }

    // Try to find the RegisterPage method
    MethodInfo registerPageMethod = apiInstance.GetType().GetMethod("OpenPage", [
        typeof(string), typeof(string)
    ]);

    if (registerPageMethod == null)
    {
        Logger.LogWarning("OpenPage method not found.");
        return;
    }

    // Call up OpenPage
    registerPageMethod.Invoke(apiInstance, [
        category,
        pageName
    ]);

    Logger.LogInfo("Page successfully registered with the wiki.");
}


public static void OpenImagePage(string imagePath)
{
    object apiInstance = WikiContent.apiInstance;
    if (apiInstance == null)
    {
        Logger.LogWarning("Wiki API is null - registration skipped.");
        return;
    }

    // Try to find the OpenImage method
    MethodInfo registerPageMethod = apiInstance.GetType().GetMethod("OpenImage", [typeof(string)]);

    if (registerPageMethod == null)
    {
        Logger.LogWarning("OpenImage method not found.");
        return;
    }

    // Call up OpenImage
    registerPageMethod.Invoke(apiInstance, [imagePath]);
}

// Define the method that will draw your wiki page content
private void YourDrawPageMethod()
{
    // Use Unity's IMGUI to create your page content
    GUILayout.Label("Your wiki content goes here");
    
    // You can use rich text
    GUILayout.Label("<b>Bold text</b> and <color=yellow>colored text</color>");
    
    // Add buttons
    if (GUILayout.Button("Click Me"))
    {
        // Handle button click
    }
    
    // Add images
    if (yourTexture != null)
    {
        GUILayout.Box(yourTexture, GUILayout.Width(200), GUILayout.Height(200));
    }
}
```

### 2. Content Guidelines

Your wiki page can include:

- Text (including rich text with HTML tags)
- Buttons with clickable actions
- Images (Texture2D objects)
- Scrollable content (the wiki system handles scrolling automatically)

But pretty much everything Unity's GUI System can handle, can be included into your page :D

It is good practice to use your plugin's title to prevent duplicate categories. You can also organize your content hierarchically using subcategories.

### 3. Example

Here's a complete example of how to register and implement a wiki page:

```csharp
using System;
using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using UnityEngine;

namespace YourNamespace
{
    [BepInPlugin("com.yourname.yourplugin", "Your Plugin Name", "1.0.0")]
    public class YourPlugin : BaseUnityPlugin
    {
        internal static ManualLogSource Logger;

        private void Awake()
        {
            Logger = base.Logger;
            Logger.LogInfo("Your plugin is loaded!");

            // Register your wiki pages
            RegisterWikiPage("YourPlugin/Features", "Basic Feature", DrawBasicFeaturePage);
            RegisterWikiPage("YourPlugin/Features/Advanced", "Advanced Feature", DrawAdvancedFeaturePage);
            
            // You can also open pages programmatically
            // Example: When user clicks a help button in your GUI
            // if (GUILayout.Button("Help")) 
            //     OpenWikiPage("YourPlugin/Features", "Basic Feature");
        }

        // Simple page with text
        private void DrawBasicFeaturePage()
        {
            GUILayout.Label("<size=20><b>Basic Feature</b></size>");
            GUILayout.Space(10);
            GUILayout.Label("This feature allows you to do basic things in the game.");
            GUILayout.Label("Follow these steps:");
            
            GUILayout.BeginVertical(GUI.skin.box);
            GUILayout.Label("1. Open the menu");
            GUILayout.Label("2. Select your options");
            GUILayout.Label("3. Click Apply");
            GUILayout.EndVertical();
            
            if (GUILayout.Button("Show Example Image"))
            {
                string imagePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "example.png");
                OpenImagePage(imagePath);
            }
        }
        
        // Advanced page with interactive elements
        private void DrawAdvancedFeaturePage()
        {
            GUILayout.Label("<size=20><b>Advanced Feature</b></size>");
            GUILayout.Space(10);
            
            GUILayout.BeginHorizontal();
            GUILayout.Label("This feature has advanced settings you can customize:", GUILayout.Width(300));
            GUILayout.BeginVertical(GUI.skin.box);
            GUILayout.Label("• Multiple profiles");
            GUILayout.Label("• Custom presets");
            GUILayout.Label("• Advanced configuration");
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
            
            GUILayout.Space(20);
            
            // Example of interactive elements
            GUILayout.Label("Try the interactive demo:");
            if (GUILayout.Button("Open Related Page"))
            {
                OpenWikiPage("YourPlugin/Features", "Basic Feature");
            }
        }

        #region Wiki API Integration
        private static object apiInstance {
            get{
                Type wikiPluginType = Type.GetType("HS2Wiki.WikiPlugin, HS2Wiki");
                if (wikiPluginType == null)
                {
                    Logger.LogWarning("Wiki plugin not found - registration skipped.");
                    return null;
                }

                // Try to find the PublicAPI field
                FieldInfo apiField = wikiPluginType.GetField("PublicAPI", BindingFlags.Public | BindingFlags.Static);
                if (apiField == null)
                {
                    Logger.LogWarning("Wiki API field not found - registration skipped.");
                    return null;
                }

                object apiInstance = apiField.GetValue(null);
                if (apiInstance == null)
                {
                    Logger.LogWarning("Wiki API is null - registration skipped.");
                    return null;
                }
                return apiInstance;
            }
        }

        public static void RegisterWikiPage(string category, string pageName, Action drawPageAction)
        {
            object api = apiInstance;
            if (apiInstance == null)
            {
                Logger.LogWarning("Wiki API is null - registration skipped.");
                return;
            }

            // Try to find the RegisterPage method
            MethodInfo registerPageMethod = apiInstance.GetType().GetMethod("RegisterPage", [
                typeof(string), typeof(string), typeof(Action)
            ]);

            if (registerPageMethod == null)
            {
                Logger.LogWarning("RegisterPage method not found.");
                return;
            }

            // Call the RegisterPage method
            registerPageMethod.Invoke(apiInstance, [
                category,
                pageName,
                drawPageAction
            ]);

            Logger.LogInfo($"Wiki page '{pageName}' registered in category '{category}'.");
        }

        public static void OpenWikiPage(string category, string pageName)
        {
            object api = apiInstance;
            if (apiInstance == null)
            {
                Logger.LogWarning("Wiki API is null - cannot open page.");
                return;
            }

            // Try to find the OpenPage method
            MethodInfo openPageMethod = apiInstance.GetType().GetMethod("OpenPage", [
                typeof(string), typeof(string)
            ]);

            if (openPageMethod == null)
            {
                Logger.LogWarning("OpenPage method not found.");
                return;
            }

            // Call the OpenPage method
            openPageMethod.Invoke(apiInstance, [
                category,
                pageName
            ]);

            Logger.LogInfo($"Opened wiki page '{pageName}' in category '{category}'.");
        }

        public static void OpenImagePage(string imagePath)
        {
            object api = apiInstance;
            if (apiInstance == null)
            {
                Logger.LogWarning("Wiki API is null - cannot open image.");
                return;
            }

            // Try to find the OpenImage method
            MethodInfo openImageMethod = apiInstance.GetType().GetMethod("OpenImage", [typeof(string)]);

            if (openImageMethod == null)
            {
                Logger.LogWarning("OpenImage method not found.");
                return;
            }

            // Call the OpenImage method
            openImageMethod.Invoke(apiInstance, [imagePath]);
            Logger.LogInfo($"Opened image: {imagePath}");
        }
        #endregion
    }
}
```

## Tips for Wiki Content

- Keep your wiki content organized by categories and subcategories
- Use clear, descriptive page names
- Use subcategories with the format "Category/Subcategory/SubSubcategory" for better organization
- Include visual examples when possible
- For complex features, consider adding step-by-step guides
- Test your wiki pages to ensure they display correctly
- Use OpenImagePage to show screenshots, diagrams, or other visual guides
- Add navigation buttons between related pages using OpenWikiPage
- Consider creating "See Also" sections that link to related wiki pages
- For tutorials, use a combination of text instructions and image examples

## Category Hierarchy and Navigation

The wiki now supports unlimited category nesting depth using the forward slash (/) as a separator:

- Simple category: `"YourPlugin"`
- Subcategory: `"YourPlugin/Features"`
- Nested subcategories: `"YourPlugin/Features/Advanced/Special"`

Each level in the hierarchy is automatically displayed with proper indentation and can be expanded or collapsed independently. This allows for much better organization of complex documentation.

### Persistent Foldout State

The wiki remembers which categories were expanded or collapsed between sessions, providing users with a consistent navigation experience. The "Open All" and "Close All" buttons allow for quick navigation through the category tree.

### Programmatic Navigation

You can programmatically navigate between wiki pages using the OpenWikiPage method:

```csharp
// Open a specific wiki page
OpenWikiPage("YourPlugin/Features", "YourFeaturePage");
```

When a page is opened programmatically, the category tree automatically expands to show the selected page.

### Image Viewer

Display images to users with the OpenImagePage method:

```csharp
// Open an image in the viewer
OpenImagePage("path/to/your/image.png");
```

The image viewer will automatically scale images to fit the window while preserving aspect ratio. Users can move the image viewer independently of the main wiki window.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Credits

Developed by Suit-Ji
