using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System;
using System.Linq;
using XafVsCodeEditor;


namespace XafVsCodeEditor
{
    [HtmlTargetElement("head")]
    public class MonacoEditorTagHelper : TagHelperComponent
    {
        public static string AddScriptTags = @"
		<script src=""_content/BlazorMonaco/lib/monaco-editor/min/vs/loader.js""></script>
        <script>require.config({ paths: { 'vs': '_content/BlazorMonaco/lib/monaco-editor/min/vs' } });</script>
        <script src=""_content/BlazorMonaco/lib/monaco-editor/min/vs/editor/editor.main.js""></script>
        <script src=""_content/BlazorMonaco/jsInterop.js""></script>
		<link href = ""_content/XafVsCodeEditor/monacoeditor.css"" rel=""stylesheet"" />";
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
        /// <summary>
		/// This extension works for blazor server and client
        /// This extension does NOT work for windows because the tag Helper is never called
		/// you can use the value of UltraCodeEditorTagHelper.AddScriptTags in your _Host.cshtml or index.html
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddMonacoEditorComponent(this IServiceCollection services)
        {
            services.AddTransient<ITagHelperComponent, MonacoEditorTagHelper>();
            return services;
        }
    }
}
