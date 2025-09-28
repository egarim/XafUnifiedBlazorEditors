# Monaco Editor Sizing Issues - Fixes and Solutions

## Problem Description

When implementing the unified Monaco Editor property editor in XAF projects, the editor appears very small (tiny) in both Blazor Server and WinForms applications. This document explains the root causes and provides solutions.

## Root Causes

### 1. **Missing Container Styling**
- Monaco Editor requires explicit height/width specification
- Without proper CSS container, the editor collapses to minimal size
- BlazorMonaco component doesn't inherit parent container dimensions automatically

### 2. **Fixed Small Size in WinForms**
```csharp
// PROBLEMATIC CODE:
control.Size = new System.Drawing.Size(300, 300); // Too small!
```

### 3. **No Height Management in Blazor Component**
- Original component lacked Height/Width parameters
- No CSS wrapper around Monaco Editor
- No responsive sizing capabilities

### 4. **Missing AutomaticLayout Configuration**
- Monaco Editor needs `AutomaticLayout = true` for responsive behavior
- Without it, the editor doesn't resize properly

## Fixed Implementation

### 1. **Updated Razor Component**

```razor
@using Microsoft.AspNetCore.Components.Web
@using BlazorMonaco

@inject Microsoft.JSInterop.IJSRuntime JS

<div class="monaco-editor-container" style="@ContainerStyle">
    <MonacoEditor @ref="_editor" 
                  Id="EditorId" 
                  ConstructionOptions="EditorConstructionOptions" 
                  OnDidInit="EditorOnDidInit" 
                  OnContextMenu="OnContextMenu" 
                  OnDidChangeModelContent="ModelContentChange" />
</div>
```

**Key Changes:**
- Added `monaco-editor-container` wrapper div
- Added dynamic `style="@ContainerStyle"` for flexible sizing
- Container provides proper boundaries for Monaco Editor

### 2. **Enhanced CSS Styling**

```css
.monaco-editor-container {
    height: 400px;
    min-height: 200px;
    width: 100%;
    border: 1px solid #d3d3d3;
    border-radius: 4px;
    overflow: hidden;
    position: relative;
}

.monaco-editor-container .monaco-editor {
    height: 100% !important;
    width: 100% !important;
}

/* XAF specific styling */
.monaco-editor-container.xaf-property-editor {
    height: 300px;
    min-height: 150px;
}

/* Blazor WebView specific styling */
.monaco-editor-container.blazor-webview {
    height: 100%;
    min-height: 250px;
    border: none;
}

/* Ensure the editor fills available space in XAF */
.dx-blazor-property-editor .monaco-editor-container {
    height: 250px;
    width: 100%;
}
```

**Key Features:**
- Responsive design with min-height
- XAF-specific styling classes
- Force Monaco Editor to fill container
- Proper overflow handling

### 3. **Component Code-Behind Updates**

```csharp
public partial class MonacoEditorComponent : ComponentBase
{
    // Size parameters for flexible sizing
    [Parameter] public string Height { get; set; } = "400px";
    [Parameter] public string Width { get; set; } = "100%";

    private string ContainerStyle => $"height: {Height}; width: {Width}; min-height: 200px;";

    private StandaloneEditorConstructionOptions EditorConstructionOptions(MonacoEditor editor)
    {
        var options = new StandaloneEditorConstructionOptions();
        options.Language = Value?.Language ?? "markdown";
        options.FontSize = 14; // More reasonable default
        options.AutomaticLayout = true; // CRITICAL for responsive sizing
        return options;
    }

    private async Task EditorOnDidInit(MonacoEditorBase editor)
    {
        SetEditorValue();
        SelectedTheme = Themes.FirstOrDefault(t => t.PropertyValue == "vs");
        SelectedThemeChanged?.Invoke(SelectedTheme);

        // Trigger layout update after initialization
        await Task.Delay(100);
        await _editor.Layout(); // Force layout recalculation
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender && _editor != null)
        {
            // Ensure proper layout after first render
            await Task.Delay(50);
            await _editor.Layout();
        }
        await base.OnAfterRenderAsync(firstRender);
    }
}
```

**Key Features:**
- `Height` and `Width` parameters for external control
- `AutomaticLayout = true` for responsive behavior
- Explicit layout calls for proper rendering
- Better default font size (14 instead of 25)

### 4. **Fixed WinForms Property Editor**

```csharp
protected override object CreateControlCore()
{
    control = new BlazorWebView();
    control.Dock = DockStyle.Fill;
    
    var services = new ServiceCollection();
    services.AddWindowsFormsBlazorWebView();
    control.Services = services.BuildServiceProvider();
    control.HostPage = "wwwroot\\index.html";
    
    parameters = new Dictionary<string, object>();
    if (PropertyValue == null)
    {
        PropertyValue = new MonacoEditorData() { Language = "markdown" };
    }

    // FIXED: Add height parameter to ensure proper sizing
    parameters.Add("Value", PropertyValue);
    parameters.Add("Height", "100%");      // NEW: Pass height to component
    parameters.Add("Width", "100%");       // NEW: Pass width to component
    
    control.RootComponents.Add<MonacoEditorComponent>("#app", parameters);

    // FIXED: Better default size
    control.MinimumSize = new System.Drawing.Size(300, 200);
    control.Size = new System.Drawing.Size(600, 300); // Larger default
    
    return control;
}
```

**Key Changes:**
- Pass `Height="100%"` and `Width="100%"` parameters
- Increase default size from 300x300 to 600x300
- Add `MinimumSize` constraint
- Use `DockStyle.Fill` for container filling

### 5. **Enhanced Blazor Property Editor**

```csharp
protected override IComponentModel CreateComponentModel()
{
    var model = new MonacoEditorDataModel();

    model.ValueChanged = EventCallback.Factory.Create<IMonacoEditorData>(this, value => {
        model.Value = value;
        OnControlValueChanged();
        WriteValue();
    });
    
    // FIXED: Set height for better display in XAF Blazor
    model.SetAttribute("Height", "250px");
    model.SetAttribute("Width", "100%");
    model.SetAttribute("class", "xaf-property-editor");
    
    return model;
}
```

**Key Changes:**
- Explicitly set Height="250px" for XAF Blazor
- Add CSS class for specific styling
- Ensure proper width allocation

### 6. **Updated Data Model**

```csharp
public class MonacoEditorDataModel : ComponentModelBase
{
    public IMonacoEditorData Value { get; set; }
    public EventCallback<IMonacoEditorData> ValueChanged { get; set; }
    
    // NEW: Height and Width properties
    public string Height { get; set; }
    public string Width { get; set; }

    public override Type ComponentType => typeof(MonacoEditorComponent);
}
```

## Implementation Checklist for New Projects

### ? **Component Library Setup**
- [ ] Add `monaco-editor-container` wrapper div
- [ ] Include `Height` and `Width` parameters
- [ ] Set `AutomaticLayout = true` in options
- [ ] Add explicit layout calls in `OnDidInit` and `OnAfterRenderAsync`
- [ ] Update CSS with proper container styling

### ? **WinForms Property Editor**
- [ ] Pass `Height` and `Width` parameters to component
- [ ] Set reasonable default size (600x300 minimum)
- [ ] Use `DockStyle.Fill` for container
- [ ] Add `MinimumSize` constraint

### ? **Blazor Property Editor**
- [ ] Set `Height` attribute on component model
- [ ] Add CSS class for XAF-specific styling
- [ ] Ensure proper width allocation

### ? **CSS Styling**
- [ ] Define `.monaco-editor-container` with explicit height
- [ ] Add `.xaf-property-editor` class for XAF integration
- [ ] Include responsive breakpoints
- [ ] Force Monaco Editor to fill container with `!important`

## Common Sizing Patterns

### **Fixed Height**
```csharp
// Component usage
<MonacoEditorComponent Height="300px" Width="100%" />

// In property editor
model.SetAttribute("Height", "300px");
```

### **Responsive Height**
```css
.monaco-editor-container {
    height: 40vh; /* 40% of viewport height */
    min-height: 200px;
    max-height: 600px;
}
```

### **Container-Based Sizing**
```csharp
// WinForms - fill available space
parameters.Add("Height", "100%");
parameters.Add("Width", "100%");
```

## Troubleshooting

### **Editor Still Too Small**
1. Check CSS is loaded: `_content/XafVsCodeEditor/monacoeditor.css`
2. Verify container div has explicit height in browser DevTools
3. Ensure `AutomaticLayout = true` in editor options
4. Add explicit layout call: `await _editor.Layout()`

### **Editor Not Responsive**
1. Add `AutomaticLayout = true` to construction options
2. Call `_editor.Layout()` after container size changes
3. Use percentage-based heights for flexibility

### **XAF Integration Issues**
1. Add `.dx-blazor-property-editor` CSS rules
2. Set explicit height on component model
3. Use `SetAttribute("Height", "250px")` in property editor

This comprehensive fix ensures Monaco Editor (and any Blazor component) displays properly across all XAF platforms with appropriate sizing.