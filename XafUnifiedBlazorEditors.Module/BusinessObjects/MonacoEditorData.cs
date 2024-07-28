using DevExpress.ExpressApp;
using DevExpress.ExpressApp.DC;
using System;
using System.Linq;


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
                OnPropertyChanged(nameof(Code));
            }
        }

        public string Language { get; set; }
    }
}

