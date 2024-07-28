using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using XafVsCodeEditor;

namespace XafUnifiedBlazorEditors.Module.BusinessObjects
{
    [DefaultClassOptions]
    //[ImageName("BO_Contact")]
    //[DefaultProperty("DisplayMemberNameForLookupEditorsOfThisType")]
    //[DefaultListViewOptions(MasterDetailMode.ListViewOnly, false, NewItemRowPosition.None)]
    //[Persistent("DatabaseTableName")]
    // Specify more UI options using a declarative approach (https://documentation.devexpress.com/#eXpressAppFramework/CustomDocument112701).
    public class DomainObject1 : BaseObject, IXafEntityObject
    { // Inherit from a different class to provide a custom primary key, concurrency and deletion behavior, etc. (https://documentation.devexpress.com/eXpressAppFramework/CustomDocument113146.aspx).
        // Use CodeRush to create XPO classes and properties with a few keystrokes.
        // https://docs.devexpress.com/CodeRushForRoslyn/118557
        public DomainObject1(Session session)
            : base(session)
        {
        }
        public override void AfterConstruction()
        {
            base.AfterConstruction();
            // Place your initialization code here (https://documentation.devexpress.com/eXpressAppFramework/CustomDocument112834.aspx).
        }

        MonacoEditorData monacoEditorData;
        string text;
        
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
            this.MonacoEditorData = new MonacoEditorData("markdown", "");
            MonacoEditorData.PropertyChanged += SourceEditor_PropertyChanged;
        }

        void IXafEntityObject.OnSaving()
        {
            this.Text = this.MonacoEditorData.Code;

        }

        void IXafEntityObject.OnLoaded()
        {
            this.MonacoEditorData = new MonacoEditorData("markdown", this.Text);
            MonacoEditorData.PropertyChanged += SourceEditor_PropertyChanged;
        }

        private void SourceEditor_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            this.Text = this.MonacoEditorData.Code;
        }
    }
}