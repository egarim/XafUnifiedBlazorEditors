using DevExpress.DataAccess.DataFederation;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp;
using System;
using System.Linq;


namespace XafVsCodeEditor
{
    public class XafMonacoEditorAlias
    {
        public const string EditorAlias = "CodePropertyEditor";
        public XafMonacoEditorAlias()
        {
            
        }
    }
    public interface IMonacoEditorData
    {



        string Code { get; set; }
        string Language { get; set; }
    }


    [DomainComponent()]
    public class MonacoEditorData : NonPersistentBaseObject, IMonacoEditorData
    {

        public MonacoEditorData(string Language, string Code)
        {
            this.Language = Language;
            this.Code = Code;
        }
        public MonacoEditorData(Guid oid) : base(oid)
        {

        }
        public MonacoEditorData()
        {
        }

        string code;
        public string Code
        {
            get => code; set
            {
                if (code == value)
                {
                    return;
                }

                code = value;
                SetPropertyValue(ref code, value);
                this.OnPropertyChanged(nameof(Code));
            }
        }

        public string Language { get; set; }
    }
}

