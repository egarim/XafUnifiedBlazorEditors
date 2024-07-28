using DevExpress.DataAccess.DataFederation;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp;
using System;
using System.Linq;


namespace XafVsCodeEditor
{
    public interface IMonacoEditorData
    {



        string Code { get; set; }
        string Language { get; set; }
    }
}

