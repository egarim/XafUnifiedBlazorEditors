# XAF Unified Property Editor Implementation Guide

This guide demonstrates how to create a unified property editor that works seamlessly across both Blazor Server and WinForms platforms in DevExpress XAF (eXpress Application Framework).

**Note**: While this guide uses Monaco Editor as a practical example, the same architectural principles and implementation patterns can be applied to **any Blazor-compatible component** (e.g., charts, rich text editors, data grids, custom input controls, etc.).

## Architecture Overview

The unified property editor architecture consists of several key components:

1. **Shared Razor Component** (`XafVsCodeEditor` project) - *Contains the reusable Blazor component*
2. **Platform-Specific Property Editors** (Blazor and WinForms implementations) - *Handle XAF integration*
3. **Common Interface and Data Models** (Shared module) - *Define the data contract*
4. **Business Object Integration** (Domain objects using the editor) - *Business layer integration*

This pattern works for any component that can be implemented as a Blazor component, including:
- **Text/Code Editors**: Monaco, CodeMirror, custom text editors
- **Rich Content**: WYSIWYG editors, markdown editors, HTML editors  
- **Data Visualization**: Charts (Chart.js, D3.js), graphs, dashboards
- **Input Controls**: Date pickers, color pickers, file uploaders
- **Custom Widgets**: Drawing canvas, signature pads, interactive forms

## Project Structure

```
Solution
??? XafVsCodeEditor/                     # Shared Blazor component library
??? XafUnifiedBlazorEditors.Module/      # Common business objects and interfaces
??? XafUnifiedBlazorEditors.Blazor.Server/  # Blazor Server application
??? XafUnifiedBlazorEditors.Win/         # WinForms application
```

## Implementation Steps

### Step 1: Create the Shared Component Library

Create a `Microsoft.NET.Sdk.Razor` project that contains the reusable Blazor component. **This works for any Blazor component**, not just Monaco Editor.

**Project File Configuration:**
```xml
<Project Sdk="Microsoft.NET.Sdk.Razor">
    <PropertyGroup>
        <TargetFramework>net8.0</TargetFramework>
        <Nullable>enable</Nullable>
        <ImplicitUsings>enable</ImplicitUsings>
        <Platforms>AnyCPU;x64</Platforms>
    </PropertyGroup>
    
    <ItemGroup>
        <SupportedPlatform Include="browser" />
    </ItemGroup>
    
    <ItemGroup>
        <PackageReference Include="Microsoft.AspNetCore.Components.Web" Version="8.0.5" />
        <!-- Monaco Editor specific package - replace with your component's package -->
        <PackageReference Include="BlazorMonaco" Version="2.1.0" />
        <PackageReference Include="Microsoft.AspNetCore.Razor" Version="2.2.0" />
        <PackageReference Include="DevExpress.ExpressApp.Blazor" Version="24.1.3" />
        
        <!-- Example alternatives for different component types:
        <PackageReference Include="Blazor.Charts" Version="1.0.0" />
        <PackageReference Include="BlazorDateRangePicker" Version="1.0.0" />  
        <PackageReference Include="Radzen.Blazor" Version="4.0.0" />
        -->
    </ItemGroup>
</Project>
```

### Step 2: Define Common Interface and Data Model

**The key principle**: Define a common interface that represents your component's data, regardless of the underlying component technology.

**Interface Definition (`IMonacoEditorData.cs`):**
```csharp
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp;

namespace XafVsCodeEditor
{
    // Example for Monaco Editor - adapt this interface for your component
    public interface IMonacoEditorData
    {
        string Code { get; set; }
        string Language { get; set; }
    }
    
    /* Examples for other component types:
    
    // Chart component interface
    public interface IChartData
    {
        string ChartType { get; set; }
        string DataJson { get; set; }
        string Configuration { get; set; }
    }
    
    // Rich text editor interface  
    public interface IRichTextData
    {
        string HtmlContent { get; set; }
        string PlainText { get; set; }
        string Format { get; set; }
    }
    
    // Date range picker interface
    public interface IDateRangeData
    {
        DateTime? StartDate { get; set; }
        DateTime? EndDate { get; set; }
        string DisplayFormat { get; set; }
    }
    */
}
```

**Data Model Implementation:**
```csharp
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.DC;

namespace XafVsCodeEditor
{
    [DomainComponent()]
    public class MonacoEditorData : NonPersistentBaseObject, IMonacoEditorData
    {
        public MonacoEditorData(string Language, string Code)
        {
            this.Language = Language;
            this.Code = Code;
        }
        
        public MonacoEditorData(Guid oid) : base(oid) { }
        
        public MonacoEditorData() { }

        private string code;
        public string Code
        {
            get => code;
            set
            {
                if (code == value) return;
                code = value;
                SetPropertyValue(ref code, value);
                OnPropertyChanged(nameof(Code));
            }
        }

        public string Language { get; set; }
    }
}
```

**Editor Alias Constants:**
```csharp
namespace XafVsCodeEditor
{
    public class XafMonacoEditorAlias
    {
        public const string EditorAlias = "CodePropertyEditor";
        // For other components, use descriptive names:
        // public const string EditorAlias = "ChartPropertyEditor";
        // public const string EditorAlias = "RichTextPropertyEditor";
        // public const string EditorAlias = "DateRangePropertyEditor";
    }
}
```

### Step 3: Create the Blazor Component

**Component Markup (`MonacoEditorComponent.razor`):**
```razor
@using Microsoft.AspNetCore.Components.Web
@using BlazorMonaco

@inject Microsoft.JSInterop.IJSRuntime JS

<!-- Monaco Editor specific markup - adapt for your component -->
<MonacoEditor @ref="_editor" 
              Id="EditorId" 
              ConstructionOptions="EditorConstructionOptions" 
              OnDidInit="EditorOnDidInit" 
              OnContextMenu="OnContextMenu"
              OnDidChangeModelContent="ModelContentChange" />

@* Examples for other component types:

Chart Component:
<div id="@ChartId" style="width: 100%; height: 400px;"></div>

Rich Text Editor:
<div class="rich-text-editor">
    <TinyMCE @ref="_editor" Id="@EditorId" @bind-Value="HtmlContent" />
</div>

Date Range Picker:
<RadzenDateRangePicker @bind-Start="StartDate" @bind-End="EndDate" 
                       DateFormat="dd/MM/yyyy" />
*@
```

**Component Code-Behind - Universal Pattern:**
```csharp
using BlazorMonaco;  // Replace with your component's using statements
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace XafVsCodeEditor
{
    public partial class MonacoEditorComponent : ComponentBase
    {
        // This pattern works for ANY Blazor component
        public const string EditorAlias = "CodePropertyEditor";

        private MonacoEditor _editor { get; set; }  // Replace with your component reference
        
        public string EditorId { get; set; }
        public MonacoTheme SelectedTheme { get; set; }  // Component-specific properties
        public List<MonacoTheme> Themes { get; set; }
        public Action<MonacoTheme> SelectedThemeChanged { get; set; }

        public MonacoEditorComponent()
        {
            EditorId = Guid.NewGuid().ToString();
        }

        private IMonacoEditorData componentModel;  // Replace with your interface type
        
        // UNIVERSAL PATTERN: Parameter for data binding
        [Parameter]
        public IMonacoEditorData Value  // Replace with your interface type
        {
            get => componentModel;
            set
            {
                componentModel = value;
                SetEditorValue();
            }
        }

        // Component-specific configuration - adapt for your component
        private StandaloneEditorConstructionOptions EditorConstructionOptions(MonacoEditor editor)
        {
            var options = new StandaloneEditorConstructionOptions();
            options.Language = Value?.Language;
            options.FontSize = 25;
            return options;
        }

        // UNIVERSAL PATTERN: Initialize component and set initial value
        private async Task EditorOnDidInit(MonacoEditorBase editor)
        {
            SetEditorValue();
            SelectedTheme = Themes.FirstOrDefault(t => t.PropertyValue == "vs");
            SelectedThemeChanged?.Invoke(SelectedTheme);

            // Add component-specific initialization
            await _editor.AddCommand((int)KeyMode.CtrlCmd | (int)KeyCode.KEY_H, async (editor, keyCode) =>
            {
                // Add custom commands here
            });
        }

        // UNIVERSAL PATTERN: Set component value from data model
        private void SetEditorValue()
        {
            if (_editor != null)
            {
                if (componentModel?.Code == null)
                {
                    _editor?.SetValue("");
                }
                else
                {
                    _editor?.SetValue(componentModel.Code);
                }
            }
        }

        // UNIVERSAL PATTERN: Handle value changes from component
        private async Task ModelContentChange(ModelContentChangedEvent e)
        {
            var codeContent = await _editor.GetValue();
            if (Value != null && Value.Code != codeContent)
            {
                Value.Code = codeContent;
            }
        }

        private void OnContextMenu(EditorMouseEvent eventArg)
        {
            Console.WriteLine("OnContextMenu : " + System.Text.Json.JsonSerializer.Serialize(eventArg));
        }

        // UNIVERSAL PATTERN: Component initialization
        protected override void OnInitialized()
        {
            base.OnInitialized();
            Themes = new List<MonacoTheme>();
            Themes.Add(new MonacoTheme("Visual Studio", "vs"));
            Themes.Add(new MonacoTheme("Visual Studio Dark", "vs-dark"));
            Themes.Add(new MonacoTheme("High Contrast Black", "hc-black"));
            
            SelectedThemeChanged = async (i) =>
            {
                await MonacoEditorBase.SetTheme(i.PropertyValue);
                Console.WriteLine(i);
            };
        }
    }
}
```

**Component Data Model - Universal Pattern:**
```csharp
using DevExpress.ExpressApp.Blazor.Components.Models;
using Microsoft.AspNetCore.Components;

namespace XafVsCodeEditor
{
    // This pattern works for ANY component data model
    public class MonacoEditorDataModel : ComponentModelBase
    {
        // Replace IMonacoEditorData with your interface type
        public IMonacoEditorData Value
        {
            get => GetPropertyValue<IMonacoEditorData>();
            set => SetPropertyValue(value);
        }

        public EventCallback<IMonacoEditorData> ValueChanged
        {
            get => GetPropertyValue<EventCallback<IMonacoEditorData>>();
            set => SetPropertyValue(value);
        }

        // Replace MonacoEditorComponent with your component type
        public override Type ComponentType => typeof(MonacoEditorComponent);
    }
    
    /* Examples for other component types:
    
    public class ChartDataModel : ComponentModelBase
    {
        public IChartData Value
        {
            get => GetPropertyValue<IChartData>();
            set => SetPropertyValue(value);
        }
        
        public EventCallback<IChartData> ValueChanged
        {
            get => GetPropertyValue<EventCallback<IChartData>>();
            set => SetPropertyValue(value);
        }
        
        public override Type ComponentType => typeof(ChartComponent);
    }
    */
}
```

**MonacoTheme Helper Class (Component-Specific):**
```csharp
namespace XafVsCodeEditor
{
    // This is Monaco-specific - adapt for your component's configuration needs
    public class MonacoTheme
    {
        public string DisplayName { get; set; }
        public string PropertyValue { get; set; }
        
        public MonacoTheme(string displayName, string propertyValue)
        {
            DisplayName = displayName;
            PropertyValue = propertyValue;
        }
    }
}
```

### Step 4: Implement Blazor Property Editor

**Universal Pattern - Works for Any Component:**

```csharp
using DevExpress.ExpressApp.Blazor.Components.Models;
using DevExpress.ExpressApp.Blazor.Editors;
using DevExpress.ExpressApp.Editors;
using DevExpress.ExpressApp.Model;
using DevExpress.ExpressApp;
using Microsoft.AspNetCore.Components;
using XafVsCodeEditor;  // Replace with your component namespace

namespace XafUnifiedBlazorEditors.Blazor.Server.Editors
{
    // Replace IMonacoEditorData and XafMonacoEditorAlias with your types
    [PropertyEditor(typeof(IMonacoEditorData), XafMonacoEditorAlias.EditorAlias, true)]
    public class MonacoPropertyEditorBlazor : BlazorPropertyEditorBase, IComplexViewItem
    {
        private IObjectSpace _objectSpace;
        private XafApplication _application;

        public MonacoPropertyEditorBlazor(Type objectType, IModelMemberViewItem model) 
            : base(objectType, model) { }

        public void Setup(IObjectSpace objectSpace, XafApplication application)
        {
            _objectSpace = objectSpace;
            _application = application;
        }

        // Replace MonacoEditorDataModel with your data model type
        public override MonacoEditorDataModel ComponentModel => 
            (MonacoEditorDataModel)base.ComponentModel;

        // UNIVERSAL PATTERN: Create component model with value change handling
        protected override IComponentModel CreateComponentModel()
        {
            var model = new MonacoEditorDataModel();  // Replace with your data model
            model.ValueChanged = EventCallback.Factory.Create<IMonacoEditorData>(this, value => {
                model.Value = value;
                OnControlValueChanged();
                WriteValue();
            });
            return model;
        }

        // UNIVERSAL PATTERN: Read value from business object
        protected override void ReadValueCore()
        {
            base.ReadValueCore();
            ComponentModel.Value = (IMonacoEditorData)PropertyValue;  // Replace with your interface
        }

        // UNIVERSAL PATTERN: Get current editor value
        protected override object GetControlValueCore() => ComponentModel.Value;

        // UNIVERSAL PATTERN: Handle read-only state
        protected override void ApplyReadOnly()
        {
            base.ApplyReadOnly();
            ComponentModel?.SetAttribute("readonly", !AllowEdit);
        }
    }
}
```

### Step 5: Implement WinForms Property Editor

**Universal Pattern - Works for Any Blazor Component:**

```csharp
using System.Windows.Forms;
using DevExpress.ExpressApp.Model;
using DevExpress.ExpressApp.Editors;
using DevExpress.ExpressApp.Win.Editors;
using Microsoft.AspNetCore.Components.WebView.WindowsForms;
using Microsoft.Extensions.DependencyInjection;
using XafVsCodeEditor;  // Replace with your component namespace

namespace XafUnifiedBlazorEditors.Win.Editors
{
    // Replace IMonacoEditorData and XafMonacoEditorAlias with your types
    [PropertyEditor(typeof(IMonacoEditorData), XafMonacoEditorAlias.EditorAlias, true)]
    public class MonacoPropertyEditorWin : PropertyEditor
    {
        private BlazorWebView control;
        private Dictionary<string, object> parameters;

        public MonacoPropertyEditorWin(Type objectType, IModelMemberViewItem info)
            : base(objectType, info) { }

        // UNIVERSAL PATTERN: Create BlazorWebView control
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
                // Replace MonacoEditorData with your data model constructor
                PropertyValue = new MonacoEditorData() { Language = "markdown" };
            }
            
            parameters.Add("Value", PropertyValue);
            // Replace MonacoEditorComponent with your component type
            control.RootComponents.Add<MonacoEditorComponent>("#app", parameters);
            control.Size = new System.Drawing.Size(300, 300);
            
            return control;
        }

        protected override void OnControlCreated()
        {
            base.OnControlCreated();
            ReadValue();
        }

        // UNIVERSAL PATTERN: Synchronize data between XAF and component
        protected override void ReadValueCore()
        {
            if (control != null && CurrentObject != null)
            {
                var currentSourceCode = ((IMonacoEditorData)parameters["Value"]);  // Replace with your interface
                var currentProperty = (IMonacoEditorData)PropertyValue;
                if (currentProperty != null)
                {
                    // Adapt these property assignments for your component
                    currentSourceCode.Code = currentProperty.Code;
                    currentSourceCode.Language = currentProperty.Language;
                }
            }
        }

        // UNIVERSAL PATTERN: Return current control value
        protected override object GetControlValueCore()
        {
            return control != null ? parameters["Value"] : null;
        }

        protected override void Dispose(bool disposing)
        {
            if (control != null)
            {
                control = null;
            }
            base.Dispose(disposing);
        }
    }
}
```

### Step 6: Configure Project Dependencies

**The dependency pattern is universal - only the specific packages change:**

**Blazor Server Project:**
```xml
<PackageReference Include="DevExpress.ExpressApp.Blazor" Version="24.1.3" />
<PackageReference Include="DevExpress.ExpressApp.Validation.Blazor" Version="24.1.3" />
<ProjectReference Include="..\XafUnifiedBlazorEditors.Module\XafUnifiedBlazorEditors.Module.csproj" />
<ProjectReference Include="..\XafVsCodeEditor\XafVsCodeEditor.csproj" />
<!-- Replace XafVsCodeEditor with your component project name -->
```

**WinForms Project:**
```xml
<Project Sdk="Microsoft.NET.Sdk.Razor">
    <PropertyGroup>
        <OutputType>WinExe</OutputType>
        <TargetFramework>net8.0-windows</TargetFramework>
        <UseWindowsForms>true</UseWindowsForms>
    </PropertyGroup>
    
    <ItemGroup>
        <PackageReference Include="DevExpress.ExpressApp.Win" Version="24.1.3" />
        <!-- This package is ALWAYS required for BlazorWebView -->
        <PackageReference Include="Microsoft.AspNetCore.Components.WebView.WindowsForms" Version="8.0.70" />
    </ItemGroup>
    
    <ItemGroup>
        <ProjectReference Include="..\XafUnifiedBlazorEditors.Module\XafUnifiedBlazorEditors.Module.csproj" />
        <ProjectReference Include="..\XafVsCodeEditor\XafVsCodeEditor.csproj" />
        <!-- Replace with your component project -->
    </ItemGroup>
</Project>
```

### Step 7: Configure Static Resources and TagHelper

**Universal Pattern - Adapt script/CSS references for your component:**

**TagHelper for Script Injection:**
```csharp
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace XafVsCodeEditor  // Use your component namespace
{
    [HtmlTargetElement("head")]
    public class MonacoEditorTagHelper : TagHelperComponent  // Rename for your component
    {
        // Adapt these script/CSS references for your component
        public static string AddScriptTags = @"
        <script src=""_content/BlazorMonaco/lib/monaco-editor/min/vs/loader.js""></script>
        <script>require.config({ paths: { 'vs': '_content/BlazorMonaco/lib/monaco-editor/min/vs' } });</script>
        <script src=""_content/BlazorMonaco/lib/monaco-editor/min/vs/editor/editor.main.js""></script>
        <script src=""_content/BlazorMonaco/jsInterop.js""></script>
        <link href=""_content/XafVsCodeEditor/monacoeditor.css"" rel=""stylesheet"" />";
        
        /* Examples for other components:
        
        // Chart.js component
        public static string AddScriptTags = @"
        <script src=""_content/Blazor.Charts/chart.min.js""></script>
        <script src=""_content/Blazor.Charts/blazor-charts.js""></script>
        <link href=""_content/YourChartProject/chart-styles.css"" rel=""stylesheet"" />;
        
        // Rich text editor
        public static string AddScriptTags = @"
        <script src=""_content/TinyMCE.Blazor/tinymce.min.js""></script>
        <script src=""_content/TinyMCE.Blazor/tinymce-blazor.js""></script>
        <link href=""_content/YourEditorProject/editor-styles.css"" rel=""stylesheet"" />;
        */

        public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            if (string.Equals(context.TagName, "head", StringComparison.OrdinalIgnoreCase))
            {
                output.PostContent.AppendHtml(AddScriptTags).AppendLine();
            }
            return Task.CompletedTask;
        }
    }
}

namespace Microsoft.Extensions.DependencyInjection
{
    public static class StartupExtensions
    {
        // Rename this method for your component
        public static IServiceCollection AddMonacoEditorComponent(this IServiceCollection services)
        {
            services.AddTransient<ITagHelperComponent, MonacoEditorTagHelper>();
            return services;
        }
    }
}
```

**For WinForms (`wwwroot/index.html`) - Universal Pattern:**
```html
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>WinFormsBlazor</title>
    <base href="/" />
    <link href="css/bootstrap/bootstrap.min.css" rel="stylesheet" />
    <link href="css/app.css" rel="stylesheet" />

    <!-- Component-specific scripts and styles - adapt for your component -->
    <script src="_content/BlazorMonaco/lib/monaco-editor/min/vs/loader.js"></script>
    <script>require.config({ paths: { 'vs': '_content/BlazorMonaco/lib/monaco-editor/min/vs' } });</script>
    <script src="_content/BlazorMonaco/lib/monaco-editor/min/vs/editor/editor.main.js"></script>
    <script src="_content/BlazorMonaco/jsInterop.js"></script>
    <link href="_content/XafVsCodeEditor/monacoeditor.css" rel="stylesheet" />
    
    <!-- Examples for other components:
    
    Chart Component:
    <script src="_content/Blazor.Charts/chart.min.js"></script>
    <script src="_content/Blazor.Charts/blazor-charts.js"></script>
    
    Rich Text Editor:
    <script src="_content/TinyMCE.Blazor/tinymce.min.js"></script>
    
    Date Picker:
    <script src="_content/Radzen.Blazor/Radzen.Blazor.js"></script>
    <link href="_content/Radzen.Blazor/css/material-base.css" rel="stylesheet" />
    -->
</head>
<body>
    <div id="app">Loading...</div>
    <div id="blazor-error-ui" data-nosnippet>
        An unhandled error has occurred.
        <a href="" class="reload">Reload</a>
        <a class="dismiss">??</a>
    </div>
    <script src="_framework/blazor.webview.js"></script>
</body>
</html>
```

**Mark index.html to copy to output in WinForms project (Universal):**
```xml
<ItemGroup>
    <Content Include="wwwroot\index.html">
        <CopyToOutputDirectory>Always</CopyToOutputDirectory>
    </Content>
</ItemGroup>
```

### Step 8: Configure Application Startup

**Universal Pattern - The XAF configuration is the same regardless of component:**

**Blazor Server Startup Configuration:**
```csharp
using DevExpress.ExpressApp.ApplicationBuilder;
using DevExpress.ExpressApp.Blazor.ApplicationBuilder;
using XafVsCodeEditor; // Replace with your component namespace

namespace XafUnifiedBlazorEditors.Blazor.Server
{
    public class Startup 
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRazorPages();
            services.AddServerSideBlazor();
            services.AddHttpContextAccessor();
            
            services.AddXaf(Configuration, builder => {
                builder.UseApplication<XafUnifiedBlazorEditorsBlazorApplication>();
                builder.Modules
                    .AddCloningXpo()
                    .AddConditionalAppearance()
                    .AddValidation()
                    .Add<XafUnifiedBlazorEditors.Module.XafUnifiedBlazorEditorsModule>()
                    .Add<XafUnifiedBlazorEditorsBlazorModule>();
                // Configure ObjectSpaceProviders...
            });
            
            // Register your component's TagHelper (rename method as needed)
            services.AddMonacoEditorComponent();
        }
    }
}
```

**WinForms Application Builder (Universal Pattern):**
```csharp
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.ApplicationBuilder;
using DevExpress.ExpressApp.Win.ApplicationBuilder;

namespace XafUnifiedBlazorEditors.Win
{
    public class ApplicationBuilder : IDesignTimeApplicationFactory
    {
        public static WinApplication BuildApplication(string connectionString)
        {
            var builder = WinApplication.CreateBuilder();
            
            builder.UseApplication<XafUnifiedBlazorEditorsWindowsFormsApplication>();
            builder.Modules
                .AddCloningXpo()
                .AddConditionalAppearance()
                .AddValidation()
                .Add<XafUnifiedBlazorEditors.Module.XafUnifiedBlazorEditorsModule>()
                .Add<XafUnifiedBlazorEditorsWinModule>();
            builder.ObjectSpaceProviders
                .AddXpo((application, options) => {
                    options.ConnectionString = connectionString;
                })
                .AddNonPersistent();
                
            builder.AddBuildStep(application => {
                application.ConnectionString = connectionString;
#if DEBUG
                if(System.Diagnostics.Debugger.IsAttached && 
                   application.CheckCompatibilityType == CheckCompatibilityType.DatabaseSchema) {
                    application.DatabaseUpdateMode = DatabaseUpdateMode.UpdateDatabaseAlways;
                }
#endif
            });
            
            return builder.Build();
        }

        XafApplication IDesignTimeApplicationFactory.Create()
            => BuildApplication(XafApplication.DesignTimeConnectionString);
    }
}
```

### Step 9: Business Object Integration

**Universal Pattern - Adapt the interface and property types:**

```csharp
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Xpo;
using System.ComponentModel;
using XafVsCodeEditor;  // Replace with your component namespace

namespace XafUnifiedBlazorEditors.Module.BusinessObjects
{
    [DefaultClassOptions]
    public class DomainObject1 : BaseObject, IXafEntityObject
    {
        public DomainObject1(Session session) : base(session) { }

        // Replace MonacoEditorData with your data model type
        private MonacoEditorData monacoEditorData;
        private string text;

        public MonacoEditorData MonacoEditorData
        {
            get => monacoEditorData;
            set => SetPropertyValue(nameof(MonacoEditorData), ref monacoEditorData, value);
        }

        [Size(SizeAttribute.DefaultStringMappingFieldSize)]
        public string Text
        {
            get => text;
            set => SetPropertyValue(nameof(Text), ref text, value);
        }

        public void OnCreated()
        {
            // Adapt constructor parameters for your component
            this.MonacoEditorData = new MonacoEditorData("markdown", "");
            MonacoEditorData.PropertyChanged += SourceEditor_PropertyChanged;
        }

        void IXafEntityObject.OnSaving()
        {
            // Adapt property mapping for your component's data
            this.Text = this.MonacoEditorData?.Code;
        }

        void IXafEntityObject.OnLoaded()
        {
            // Adapt constructor and property mapping for your component
            this.MonacoEditorData = new MonacoEditorData("markdown", this.Text ?? "");
            MonacoEditorData.PropertyChanged += SourceEditor_PropertyChanged;
        }

        private void SourceEditor_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // Adapt property synchronization for your component
            this.Text = this.MonacoEditorData?.Code;
        }
    }
}
```

### Step 10: XAF Module Registration

**Universal Pattern - Module registration is the same regardless of component:**

**Shared Module:**
```csharp
using DevExpress.ExpressApp;

namespace XafUnifiedBlazorEditors.Module
{
    public sealed class XafUnifiedBlazorEditorsModule : ModuleBase
    {
        public XafUnifiedBlazorEditorsModule()
        {
            RequiredModuleTypes.Add(typeof(DevExpress.ExpressApp.SystemModule.SystemModule));
            RequiredModuleTypes.Add(typeof(DevExpress.ExpressApp.Objects.BusinessClassLibraryCustomizationModule));
            RequiredModuleTypes.Add(typeof(DevExpress.ExpressApp.CloneObject.CloneObjectModule));
            RequiredModuleTypes.Add(typeof(DevExpress.ExpressApp.ConditionalAppearance.ConditionalAppearanceModule));
            RequiredModuleTypes.Add(typeof(DevExpress.ExpressApp.Validation.ValidationModule));
        }
    }
}
```

**Platform-Specific Modules (Universal Pattern):**
```csharp
// Blazor Module
[ToolboxItemFilter("Xaf.Platform.Blazor")]
public sealed class XafUnifiedBlazorEditorsBlazorModule : ModuleBase { }

// WinForms Module  
[ToolboxItemFilter("Xaf.Platform.Win")]
public sealed class XafUnifiedBlazorEditorsWinModule : ModuleBase { }
```

## Key Architecture Principles

### 1. **Universal Component Pattern**
- **Any Blazor Component**: This architecture works with charts, editors, pickers, widgets, or any Blazor-compatible component
- **Shared Component Library**: Contains the reusable Blazor component and common interfaces
- **Platform-Specific Editors**: Handle platform-specific XAF integration and property editor implementation
- **Common Module**: Defines interfaces, business objects, and shared business logic

### 2. **Interface-Based Design**
- Use a common interface that represents your component's data contract
- Both property editors work with the same interface ensuring consistent behavior
- Business objects implement the interface for seamless integration
- **Flexible**: Interface can represent any data structure (text, JSON, binary, configuration objects)

### 3. **Component Reuse Strategy**
- The same Blazor component works in both environments regardless of complexity
- WinForms uses `BlazorWebView` to host any Blazor component natively
- Blazor Server uses the component directly through the XAF Blazor property editor
- **Universal**: Works for simple inputs or complex interactive components

### 4. **Resource Management**
- Static resources (JS, CSS, images) are packaged in the component library using static web assets
- TagHelpers automate resource injection for Blazor Server applications  
- Manual script inclusion required for WinForms `wwwroot/index.html`
- **Scalable**: Handles multiple component dependencies automatically

### 5. **XAF Integration Pattern**
- Property editors are registered using `[PropertyEditor]` attribute with shared alias
- `IComplexViewItem` interface enables advanced XAF features
- Proper `ReadValueCore`/`WriteValueCore` implementation ensures data synchronization
- **Consistent**: Same XAF integration pattern regardless of underlying component

## Benefits of This Universal Approach

1. **Maximum Code Reuse**: Single component implementation for multiple platforms
2. **Consistent UX**: Same behavior and appearance across platforms regardless of component type
3. **Maintainability**: Changes to any component only need to be made once
4. **Extensibility**: Easy to add new features to both platforms simultaneously
5. **Modern UI**: Leverage any modern web component in traditional desktop apps
6. **XAF Integration**: Full integration with XAF's model-driven architecture
7. **Technology Flexibility**: Use any JavaScript library or Blazor component ecosystem
8. **Future-Proof**: New Blazor components can be integrated using the same pattern

## Component Examples That Work With This Pattern

### Text and Code Editors
- **Monaco Editor** (VS Code editor) - *Current example*
- **CodeMirror** - Advanced code editor
- **TinyMCE** - Rich text editor
- **Quill** - Modern WYSIWYG editor
- **Custom markdown editors**

### Data Visualization  
- **Chart.js** - Charts and graphs
- **D3.js** - Complex data visualization
- **Plotly** - Scientific plotting
- **Google Charts** - Interactive charts
- **Custom dashboards**

### Input Controls
- **Date/Time pickers** with complex ranges
- **Color pickers** with advanced features  
- **File uploaders** with drag-and-drop
- **Multi-select components**
- **Custom form builders**

### Interactive Widgets
- **Drawing/Canvas components**
- **Signature pads**
- **Image editors**
- **Map components** (Google Maps, Leaflet)
- **Custom interactive forms**

## Prerequisites and Dependencies

### Required NuGet Packages (Universal):
- **Component Library**: Your component's packages + `DevExpress.ExpressApp.Blazor`
- **Blazor Server**: `DevExpress.ExpressApp.Blazor`, `DevExpress.ExpressApp.Validation.Blazor`
- **WinForms**: `DevExpress.ExpressApp.Win`, `Microsoft.AspNetCore.Components.WebView.WindowsForms`

### Runtime Requirements (Universal):
- .NET 8.0 or later
- WebView2 Runtime (for WinForms applications) - **Always required**
- DevExpress XAF 24.1.3 or compatible version
- Component-specific JavaScript libraries (handled automatically via static web assets)

## Considerations and Limitations

### Universal Considerations:
1. **Performance**: BlazorWebView adds overhead in WinForms applications
2. **Dependencies**: Additional NuGet packages required for WebView support
3. **Debugging**: Blazor component debugging in WinForms can be more complex
4. **Resource Size**: Modern web components can add significant size to applications
5. **WebView2 Dependency**: WinForms applications always require WebView2 runtime installation
6. **Platform Differences**: Some advanced component features may behave differently across platforms

### Component-Specific Considerations:
- **Network Dependencies**: Some components require internet connectivity
- **Browser Compatibility**: Ensure components work in WebView2's Chromium engine
- **Performance**: Complex components (3D graphics, large datasets) may need optimization
- **Security**: Client-side components may have different security implications

## Troubleshooting

### Universal Issues:
1. **Component not appearing**: Check DevTools for missing `_content` assets
2. **Scripts not loading**: Verify TagHelper registration or manual script inclusion in index.html
3. **WebView2 errors**: Ensure WebView2 runtime is installed on target machines
4. **Component not found**: Check namespace imports and component registration
5. **Data not syncing**: Verify PropertyEditor implementation and event wiring

### Component-Specific Issues:
- **Component-specific errors**: Check component documentation for common issues
- **JavaScript errors**: Use browser DevTools to debug component-specific JavaScript
- **Styling issues**: Ensure component CSS is properly loaded
- **Performance problems**: Monitor component resource usage and optimize as needed

## Conclusion

This architecture provides a **universal, robust, and maintainable pattern** for creating modern property editors in XAF applications. While Monaco Editor serves as our example, **the exact same principles, code patterns, and architecture can be applied to any Blazor-compatible component**.

Whether you're integrating charts, rich text editors, date pickers, drawing canvas, or any other modern web component, this unified approach ensures:

- **Single implementation** that works across Blazor Server and WinForms
- **Consistent user experience** regardless of platform
- **Easy maintenance** with changes made in one place
- **Full XAF integration** with model-driven development
- **Future flexibility** to adopt new component technologies

The pattern scales from simple input controls to complex interactive applications, making it a powerful foundation for modern XAF development.