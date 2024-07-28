using BlazorMonaco;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XafVsCodeEditor
{
    public partial class MonacoEditorComponent : ComponentBase
    {
        static Random random = new Random();
        public const string EditorAlias = "CodePropertyEditor";

        public string EditorId { get; set; }
        public MonacoEditorComponent()
        {


            EditorId = Guid.NewGuid().ToString();
        }
        private MonacoEditor _editor { get; set; }
        public MonacoTheme SelectedTheme { get; set; }
        public List<MonacoTheme> Themes { get; set; }

        private StandaloneEditorConstructionOptions EditorConstructionOptions(MonacoEditor editor)
        {
            var options = new StandaloneEditorConstructionOptions();
            options.Language = Value?.Language;
            options.FontSize = 25;
            return options;
        }
        public void SetFontSize(int FontSize)
        {
            _editor.UpdateOptions(new StandaloneEditorConstructionOptions() { FontSize = FontSize });
        }

        private async Task EditorOnDidInit(MonacoEditorBase editor)
        {
            SetEditorValue();
            //this.SelectedTheme=Themes.FirstOrDefault(t=>t.PropertyValue==componentModel.SourceCode.Theme);
            SelectedTheme = Themes.FirstOrDefault(t => t.PropertyValue == "vs");
            SelectedThemeChanged.Invoke(SelectedTheme);

            await _editor.AddCommand((int)KeyMode.CtrlCmd | (int)KeyCode.KEY_H, async (editor, keyCode) =>
            {
               



            });

            var newDecorations = new ModelDeltaDecoration[]
            {
            new ModelDeltaDecoration
            {
                Range = new BlazorMonaco.Range(3,1,3,1),
                Options = new ModelDecorationOptions
                {
                    IsWholeLine = true,
                    ClassName = "decorationContentClass",
                    GlyphMarginClassName = "decorationGlyphMarginClass"
                }
            }
            };

        }

        private string[] decorationIds;

        public Action<MonacoTheme> SelectedThemeChanged { get; set; }
        IMonacoEditorData componentModel;
        [Parameter]
        public IMonacoEditorData Value
        {
            get
            {
                return componentModel;
            }
            set
            {
                componentModel = value;
                SetEditorValue();

            }
        }

        private void SetEditorValue()
        {
            if (_editor != null)
            {
                if (componentModel.Code == null)
                {
                    _editor?.SetValue("");
                }
                else
                {
                    _editor?.SetValue(componentModel.Code);
                }

            }
        }
        private async Task ModelContentChange(ModelContentChangedEvent e)
        {
            var CodeContent = await _editor.GetValue();
            if (Value != null && Value.Code != CodeContent)
            {
                Value.Code = CodeContent;


            }
        }
        private void OnContextMenu(EditorMouseEvent eventArg)
        {
            Console.WriteLine("OnContextMenu : " + System.Text.Json.JsonSerializer.Serialize(eventArg));
        }

        private async Task AddCommand()
        {
            await _editor.AddCommand((int)KeyMode.CtrlCmd | (int)KeyCode.Enter, (editor, keyCode) =>
            {
                Console.WriteLine("Ctrl+Enter : Editor command is triggered.");
            });
        }

        private async Task AddAction()
        {
            await _editor.AddAction("testAction", "Test Action", new int[] { (int)KeyMode.CtrlCmd | (int)KeyCode.KEY_D, (int)KeyMode.CtrlCmd | (int)KeyCode.KEY_B }, null, null, "navigation", 1.5, (editor, keyCodes) =>
            {
                Console.WriteLine("Ctrl+D : Editor action is triggered.");
            });
        }
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
                //this.ComponentModel.SourceCode.Theme = i.PropertyValue;
                Console.WriteLine(i);
            };

        }
    }
}
