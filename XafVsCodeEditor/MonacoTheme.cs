using Microsoft.AspNetCore.Razor.TagHelpers;
using System;
using System.ComponentModel;
using System.Linq;


namespace XafVsCodeEditor
{
    [DefaultProperty(nameof(DisplayName))]
    public class MonacoTheme
    {
        public string DisplayName { get; set; }
        public string PropertyValue { get; set; }
        public MonacoTheme(string displayName, string propertyValue)
        {
            DisplayName = displayName;
            PropertyValue = propertyValue;
        }
    }
}
