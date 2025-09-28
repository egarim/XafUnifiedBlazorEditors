namespace XafSemanticKernelStudio.Win
{
    using System.Windows.Forms;
    using DevExpress.ExpressApp.Model;
    using DevExpress.ExpressApp.Editors;
    using DevExpress.ExpressApp.Win.Editors;
    using DevExpress.XtraEditors.Repository;
    using Microsoft.AspNetCore.Components.WebView.WindowsForms;
    using Microsoft.Extensions.DependencyInjection;
    using DevExpress.XtraRichEdit.Model;
    using XafVsCodeEditor;

    [PropertyEditor(typeof(IMonacoEditorData), XafMonacoEditorAlias.EditorAlias, true)]
    public class MonacoPropertyEditorWin : PropertyEditor
    {

        BlazorWebView control;
        Dictionary<string, object> parameters;
        
        protected override void ReadValueCore()
        {
            if (control != null)
            {
                if (CurrentObject != null)
                {
                    var CurrentSourceCode = ((IMonacoEditorData)parameters["Value"]);
                    var CurrentProperty = (IMonacoEditorData)PropertyValue;
                    if (CurrentProperty != null)
                    {
                        CurrentSourceCode.Code = CurrentProperty.Code;
                        CurrentSourceCode.Language = CurrentProperty.Language;
                    }
                }
            }
        }
        
        private void control_ValueChanged(object sender, EventArgs e)
        {
            if (!IsValueReading)
            {
                OnControlValueChanged();
                WriteValueCore();
            }
        }

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

            // Add height parameter to ensure proper sizing
            parameters.Add("Value", PropertyValue);
            parameters.Add("Height", "100%");
            parameters.Add("Width", "100%");
            
            control.RootComponents.Add<MonacoEditorComponent>("#app", parameters);

            // Set minimum size but allow it to grow
            control.MinimumSize = new System.Drawing.Size(300, 200);
            control.Size = new System.Drawing.Size(600, 300); // Better default size
            
            return control;
        }
        
        protected override void OnControlCreated()
        {
            base.OnControlCreated();
            ReadValue();
        }
        
        public MonacoPropertyEditorWin(Type objectType, IModelMemberViewItem info)
            : base(objectType, info)
        {
        }
        
        protected override void Dispose(bool disposing)
        {
            if (control != null)
            {
                control = null;
            }
            base.Dispose(disposing);
        }

        protected override object GetControlValueCore()
        {
            if (control != null)
            {
                return parameters["Value"];
            }
            return null;
        }
    }
}
