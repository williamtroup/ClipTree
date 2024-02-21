using System.IO;
using System.Collections.Generic;
using System.Xml;
using ClipTree.Engine.Settings.Interfaces;

namespace ClipTree.Engine.Settings;

public class XMLSettings(string filename = "settings.xml", string root = "Configuration", string entryName = "Setting")
    : IXMLSettings
{
    private const string EntryName = "Name";
    private const string EntryValue = "Value";
    private const string EntryFormat = "/{0}/{1}/{2}[@{3}=\"{4}\"]";

    public XmlDocument GetDocument()
    {
        Create();

        XmlDocument xmlDocument = new XmlDocument();
        xmlDocument.Load(filename);

        return xmlDocument;
    }

    public void SaveDocument(XmlDocument xmlDocument)
    {
        xmlDocument.Save(filename);
    }

    public string Read(string section, string setting, string defaultValue, XmlDocument xmlOverrideDocument = null)
    {
        string value = defaultValue;

        XmlDocument xmlDocument = LoadDocument(xmlOverrideDocument);

        if (xmlDocument.DocumentElement != null)
        {
            XmlNode xmlNode = xmlDocument.DocumentElement.SelectSingleNode(string.Format(EntryFormat, root, section, entryName, EntryName, setting));
			
            if (xmlNode != null && xmlNode.Attributes != null)
            {
                string newValue = xmlNode.Attributes.GetNamedItem(EntryValue).Value;

                if (!string.IsNullOrEmpty(newValue))
                {
                    value = newValue;
                }
            }
        }

        return value;
    }
		
    public void Write(string section, string setting, string value, XmlDocument xmlOverrideDocument = null)
    {
        XmlDocument xmlDocument = LoadDocument(xmlOverrideDocument);

        if (xmlDocument.DocumentElement != null)
        {
            XmlElement xmlNode = (XmlElement)(xmlDocument.DocumentElement.SelectSingleNode(string.Format(EntryFormat, root, section, entryName, EntryName, setting)));

            if (xmlNode != null)
            {
                xmlNode.Attributes.GetNamedItem(EntryValue).Value = value;
            }
            else
            {
                xmlNode = xmlDocument.CreateElement(entryName);
                xmlNode.SetAttribute(EntryValue, value);
                xmlNode.SetAttribute(EntryName, setting);

                if (xmlDocument.DocumentElement != null)
                {
                    XmlNode xmlNodeRoot = xmlDocument.DocumentElement.SelectSingleNode(string.Format("/{0}/{1}", root, section));

                    if (xmlNodeRoot != null)
                    {
                        xmlNodeRoot.AppendChild(xmlNode);
                    }
                    else
                    {
                        xmlNodeRoot = xmlDocument.DocumentElement.SelectSingleNode(string.Format("/{0}", root));
                        if (xmlNodeRoot != null)
                        {
                            xmlNodeRoot.AppendChild(xmlDocument.CreateElement(section));
                        }

                        if (xmlDocument.DocumentElement != null)
                        {
                            xmlNodeRoot = xmlDocument.DocumentElement.SelectSingleNode(string.Format("/{0}/{1}", root, section));
                            if (xmlNodeRoot != null)
                            {
                                xmlNodeRoot.AppendChild(xmlNode);
                            }
                        }
                    }
                }
            }
        }

        if (xmlOverrideDocument == null)
        {
            xmlDocument.Save(filename);
        }
    }

    public bool RemoveSection(string section, XmlDocument xmlOverrideDocument = null)
    {
        bool done = false;

        XmlDocument xmlDocument = LoadDocument(xmlOverrideDocument);

        if (xmlDocument.DocumentElement != null)
        {
            XmlNode xmlNode = xmlDocument.DocumentElement.SelectSingleNode(string.Format("/{0}/{1}", root, section));

            if (xmlNode != null)
            {
                xmlNode.RemoveAll();

                xmlDocument.DocumentElement.RemoveChild(xmlNode);

                if (xmlOverrideDocument == null)
                {
                    xmlDocument.Save(filename);
                }

                done = true;
            }
        }

        return done;
    }

    public Dictionary<string, string> ReadAll(string section, XmlDocument xmlOverrideDocument = null)
    {
        Dictionary<string, string> items = new Dictionary<string, string>();

        XmlDocument xmlDocument = LoadDocument(xmlOverrideDocument);

        if (xmlDocument.DocumentElement != null)
        {
            XmlNodeList xmlNodeList = xmlDocument.DocumentElement.SelectNodes(string.Format("/{0}/{1}/{2}", root, section, entryName));
            if (xmlNodeList != null)
            {
                foreach (XmlNode xmlNode in xmlNodeList)
                {
                    if (xmlNode != null && xmlNode.Attributes != null)
                    {
                        string name = xmlNode.Attributes.GetNamedItem(EntryName).Value;
                        string value = xmlNode.Attributes.GetNamedItem(EntryValue).Value;

                        items[name] = value;
                    }
                }
            }
        }

        return items;
    }

    private XmlDocument LoadDocument(XmlDocument xmlOverrideDocument)
    {
        XmlDocument xmlDocument = xmlOverrideDocument ?? new XmlDocument();

        if (xmlOverrideDocument == null)
        {
            Create();

            xmlDocument.Load(filename);
        }

        return xmlDocument;
    }

    private void Create()
    {
        if (!File.Exists(filename))
        {
            using (StreamWriter fileStreamWriter = new StreamWriter(File.Open(filename, FileMode.Create)))
            {
                fileStreamWriter.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
                fileStreamWriter.WriteLine("<{0}>", root);
                fileStreamWriter.WriteLine("</{0}>", root);
                fileStreamWriter.Close();
            }
        }
    }
}