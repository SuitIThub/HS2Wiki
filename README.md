# HS2 Wiki Plugin

[![Latest Release](https://img.shields.io/github/v/release/Suit-Ji/HS2Wiki?label=Latest%20Release&style=flat-square)](https://github.com/Suit-Ji/HS2Wiki/releases/latest)
[![License](https://img.shields.io/badge/License-MIT-blue.svg)](https://opensource.org/licenses/MIT)
[![BepInEx](https://img.shields.io/badge/BepInEx-5.4.22-green)](https://github.com/BepInEx/BepInEx)

A plugin for Honey Select 2 that provides an in-game wiki system. This plugin allows other plugins to register and display wiki pages to help users understand game features.

![HS2 Wiki Screenshot](example.png)

## Features

- In-game wiki accessible via a hotkey (default: F3)
- Category-based organization of wiki pages
- Support for text, buttons, and images in wiki pages
- Scrollable content for detailed documentation

## Installation

1. Ensure you have [BepInEx](https://github.com/BepInEx/BepInEx) installed
2. Place the `HS2Wiki.dll` in your `BepInEx/plugins` folder

## Usage

Press F3 (configurable) to open the wiki panel.

## For Plugin Developers: How to Register Wiki Pages

You can add your own wiki pages to the central wiki system by following these steps:

### 1. Add a reference to HS2Wiki

In your plugin project, add a reference to `HS2Wiki.dll`.

### 2. Register your wiki page

Use the public API to register your wiki pages. The simplest approach is:

```csharp
using HS2Wiki;

// Inside your plugin's Awake or Start method
WikiPlugin.PublicAPI.RegisterPage("Your Category", "Your Page Name", YourDrawPageMethod);

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

### 3. Content Guidelines

Your wiki page can include:

- Text (including rich text with HTML tags)
- Buttons with clickable actions
- Images (Texture2D objects)
- Scrollable content (the wiki system handles scrolling automatically)

### 4. Example

Here's a complete example of how to register and implement a wiki page:

```csharp
using BepInEx;
using HS2Wiki;
using UnityEngine;

[BepInPlugin("com.yourname.yourplugin", "Your Plugin", "1.0.0")]
public class YourPlugin : BaseUnityPlugin
{
    private Texture2D _exampleImage;

    private void Awake()
    {
        // Check if the Wiki plugin is available
        if (WikiPlugin.PublicAPI != null)
        {
            // Load your image
            _exampleImage = LoadYourImage();
            
            // Register your wiki page
            WikiPlugin.PublicAPI.RegisterPage("Your Features", "Feature Guide", DrawWikiPage);
        }
    }
    
    private void DrawWikiPage()
    {
        GUILayout.Label("<b>Your Feature Guide</b>");
        GUILayout.Space(10);
        
        GUILayout.Label("This is how to use your feature:");
        GUILayout.Label("1. First step");
        GUILayout.Label("2. Second step");
        
        if (GUILayout.Button("Show More Details"))
        {
            // Handle button click
        }
        
        if (_exampleImage != null)
        {
            GUILayout.Label("Screenshot:");
            GUILayout.Box(_exampleImage, GUILayout.Width(300), GUILayout.Height(200));
        }
    }
    
    private Texture2D LoadYourImage()
    {
        // Your image loading code
        // ...
    }
}
```

## Tips for Wiki Content

- Keep your wiki content organized by categories
- Use clear, descriptive page names
- Include visual examples when possible
- For complex features, consider adding step-by-step guides
- Test your wiki pages to ensure they display correctly

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Credits

Developed by Suit-Ji