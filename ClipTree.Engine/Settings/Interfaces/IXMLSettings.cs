using System.Collections.Generic;
using System.Xml;

namespace ClipTree.Engine.Settings.Interfaces
{
    public interface IXMLSettings
    {
        XmlDocument GetDocument();
        void SaveDocument(XmlDocument xmlDocument);
        string Read(string section, string setting, string defaultValue, XmlDocument xmlOverrideDocument = null);
        void Write(string section, string setting, string value, XmlDocument xmlOverrideDocument = null);
        bool RemoveSection(string section, XmlDocument xmlOverrideDocument = null);
        Dictionary<string, string> ReadAll(string section, XmlDocument xmlOverrideDocument = null);
    }
}
