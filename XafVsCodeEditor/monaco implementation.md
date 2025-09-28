# Monaco Editor integration (improved)

This document explains how the XafVsCodeEditor project integrates Monaco Editor and gives a clear, repeatable set of steps and code snippets you can apply to another project (both Blazor hosting and WinForms hosting via BlazorWebView).

Scope
- Shared component: XafVsCodeEditor exposes a reusable Blazor component (MonacoEditorComponent) that wraps BlazorMonaco.
- Hosts: XAF Blazor (server) and XAF WinForms (via BlazorWebView / WebView2).

Prerequisites
- .NET 8 target (project in this workspace uses net8.0-windows for WinForms and net8.0 for Blazor Server).
- Add the BlazorMonaco NuGet package to the component project (XafVsCodeEditor). Example csproj entry:

```xml
<PackageReference Include="BlazorMonaco" Version="1.*" />
```

(Use the latest stable BlazorMonaco release compatible with .NET 8.)

- For WinForms host: ensure Microsoft.AspNetCore.Components.WebView.WindowsForms package is referenced in the Win project (this repo already includes it).
- WebView2 runtime (Edge) must be installed on the machine running the WinForms host.

Key files in XafVsCodeEditor
- MonacoEditorComponent.razor — markup that instantiates the BlazorMonaco <MonacoEditor> and wires events.
- MonacoEditorComponent.razor.cs — the component code: options, setting/getting value, commands, decorations, and a [Parameter] Value of type IMonacoEditorData.
- MonacoEditorDataModel.cs — DevExpress ComponentModel mapper used by the Blazor property editor.
- IMonacoEditorData.cs / MonacoEditorData.cs — data contract used to pass Code and Language.
- MonacoEditorTagHelper.cs — helper to add the required <script>/<link> tags for Monaco in Blazor hosting; exposes AddScriptTags string that you can embed in static host pages.

What static assets are needed
- Monaco runtime files are provided by BlazorMonaco under _content/BlazorMonaco/...
- The TagHelper (MonacoEditorTagHelper.AddScriptTags) references these via _content paths:
  - _content/BlazorMonaco/lib/monaco-editor/min/vs/loader.js
  - _content/BlazorMonaco/lib/monaco-editor/min/vs/editor/editor.main.js
  - _content/BlazorMonaco/jsInterop.js
  - and a CSS asset from this component: _content/XafVsCodeEditor/monacoeditor.css

Blazor host: concrete steps
1. Add project reference to XafVsCodeEditor from your Blazor project.
2. Ensure BlazorMonaco is restored (either from component project or explicitly).
3. Register the Monaco tag helper so its script tags get injected in <head>. In Program.cs (Blazor Server example):

```csharp
var builder = WebApplication.CreateBuilder(args);
// ... existing builder setup
builder.Services.AddMonacoEditorComponent(); // registers the TagHelper
var app = builder.Build();
// ... existing app configuration
app.Run();
```

4. Confirm _Host.cshtml or Pages/_Layout.cshtml renders the head and allows tag helper components to append content. If not using the TagHelper, paste the contents of MonacoEditorTagHelper.AddScriptTags into your layout's <head>.
5. Use the component in your UI via the XAF Blazor editor (MonacoPropertyEditorBlazor is already wired to use the MonacoEditorDataModel). The ComponentModel.ComponentType points to XafVsCodeEditor.MonacoEditorComponent.

WinForms host (BlazorWebView): concrete steps
1. Add a project reference to XafVsCodeEditor in the WinForms project (already present in repo).
2. Ensure the WinForms csproj includes the WebView package:

```xml
<PackageReference Include="Microsoft.AspNetCore.Components.WebView.WindowsForms" Version="8.*" />
```

3. Create a HostPage file in the Win project (wwwroot/index.html) and copy MonacoEditorTagHelper.AddScriptTags into the <head>. Example minimal file:

```html
<!doctype html>
<html>
  <head>
    <!-- paste the contents of MonacoEditorTagHelper.AddScriptTags here -->
  </head>
  <body>
    <div id="app"></div>
  </body>
</html>
```

4. Mark wwwroot/index.html as Content and CopyToOutputDirectory=Always in the Win csproj so BlazorWebView can load it at runtime.
5. In the WinForms editor code (MonacoPropertyEditorWin.CreateControlCore shows the pattern):

- Create BlazorWebView
- Call services.AddWindowsFormsBlazorWebView()
- Set control.HostPage = "wwwroot/index.html" (path relative to output)
- control.RootComponents.Add<MonacoEditorComponent>("#app", parameters)

Example snippet (as used in repo):

```csharp
var services = new ServiceCollection();
services.AddWindowsFormsBlazorWebView();
control.Services = services.BuildServiceProvider();
control.HostPage = "wwwroot\\index.html";
control.RootComponents.Add<MonacoEditorComponent>("#app", parameters);
```

6. Ensure the parameter dictionary contains an IMonacoEditorData instance (Value) as expected by the component.

Wiring data both ways (component ? host)
- MonacoEditorComponent exposes a [Parameter] public IMonacoEditorData Value. The host must pass an instance for the component to read/set its content.
- For Blazor/XAF: MonacoPropertyEditorBlazor creates a MonacoEditorDataModel and wires ValueChanged using EventCallback.Factory.Create to propagate changes back to the XAF property editor.
- For WinForms/XAF: MonacoPropertyEditorWin creates a MonacoEditorData and passes it to RootComponents.Add as the "Value" parameter. ReadValueCore/WriteValueCore synchronize the domain property and the passed instance.

Troubleshooting tips
- If editor doesn't appear: open DevTools in WebView2 (right-click or use WebView2 diagnostics) and check console for 404s — missing _content assets indicate static web assets are not accessible.
- CSS missing: check _content/XafVsCodeEditor/monacoeditor.css path; ensure XafVsCodeEditor builds and exposes static web assets.
- TagHelper not executed: TagHelpers run only in server/client Razor pages; in WinForms you must embed AddScriptTags in index.html manually.
- WebView2 not found: install WebView2 runtime from Microsoft.
- Selector mismatch: confirm the selector you pass to RootComponents.Add matches the element id in HostPage (e.g. "#app").

Minimal checklist (rapid)
- [ ] Project reference to XafVsCodeEditor from host project.
- [ ] BlazorMonaco package present for component project.
- [ ] For Blazor Server: call builder.Services.AddMonacoEditorComponent() or paste script tags in layout head.
- [ ] For WinForms: create wwwroot/index.html with script tags (MonacoEditorTagHelper.AddScriptTags) and copy to output.
- [ ] WinForms: ensure Microsoft.AspNetCore.Components.WebView.WindowsForms is referenced and services.AddWindowsFormsBlazorWebView() is called.
- [ ] Confirm RootComponents.Add uses the correct selector and passes an IMonacoEditorData instance.
- [ ] WebView2 runtime installed for WinForms host.

If you want, I can do the following changes in this repository now:
- Add a ready-to-use XafUnifiedBlazorEditors.Win/wwwroot/index.html (with Monaco tags) and update the Win csproj to copy it to output.
- Add a Program.cs snippet to the Blazor Server project (XafUnifiedBlazorEditors.Blazor.Server) showing where to register AddMonacoEditorComponent().
- Add an explicit BlazorMonaco package reference to XafVsCodeEditor.csproj if missing.

Tell me which of the above you want me to apply and I will implement the edits and run a build.
