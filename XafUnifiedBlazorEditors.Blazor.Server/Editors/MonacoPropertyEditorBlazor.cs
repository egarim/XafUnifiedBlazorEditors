using DevExpress.DataAccess.DataFederation;
using DevExpress.ExpressApp.Blazor.Components.Models;
using DevExpress.ExpressApp.Blazor.Editors;
using DevExpress.ExpressApp.Editors;
using DevExpress.ExpressApp.Model;
using DevExpress.ExpressApp;
using DevExpress.Persistent.BaseImpl;
using Microsoft.AspNetCore.Components;
using XafVsCodeEditor;

namespace XafUnifiedBlazorEditors.Blazor.Server.Editors
{
    [PropertyEditor(typeof(IMonacoEditorData),  XafMonacoEditorAlias.EditorAlias, true)]
    public class MonacoPropertyEditorBlazor : BlazorPropertyEditorBase, IComplexViewItem
    {
        public MonacoPropertyEditorBlazor(Type objectType, IModelMemberViewItem model) : base(objectType, model) { }

        IObjectSpace _objectSpace;
        XafApplication _application;
        public void Setup(IObjectSpace objectSpace, XafApplication application)
        {
            _objectSpace = objectSpace;
            _application = application;


        }


        public override MonacoEditorDataModel ComponentModel => (MonacoEditorDataModel)base.ComponentModel;
        protected override IComponentModel CreateComponentModel()
        {


            var model = new MonacoEditorDataModel();

            model.ValueChanged = EventCallback.Factory.Create<IMonacoEditorData>(this, value => {
                model.Value = value;
                OnControlValueChanged();
                WriteValue();
            });
            return model;
        }
        protected override void ReadValueCore()
        {
            base.ReadValueCore();
            ComponentModel.Value = (IMonacoEditorData)PropertyValue;
        }
        protected override object GetControlValueCore() => ComponentModel.Value;
        protected override void ApplyReadOnly()
        {
            base.ApplyReadOnly();
            ComponentModel?.SetAttribute("readonly", !AllowEdit);
        }


    }
}
