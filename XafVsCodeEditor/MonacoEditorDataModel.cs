using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

using DevExpress.ExpressApp.Blazor.Components.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using XafVsCodeEditor;



namespace XafVsCodeEditor
{

    public class MonacoEditorDataModel : ComponentModelBase
    {
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

        public string Height
        {
            get => GetPropertyValue<string>();
            set => SetPropertyValue(value);
        }

        public string Width
        {
            get => GetPropertyValue<string>();
            set => SetPropertyValue(value);
        }

        public override Type ComponentType => typeof(MonacoEditorComponent);
    }
}
