using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.XPath;

namespace InvoicingApp
{
    public class UserSettings
    {
        public static String SETTINGS_PATH = @"settings.xml";
        public static string ReadValueFromXML(string pstrValueToRead)
        {
            try
            {
                //settingsFilePath is a string variable storing the path of the settings file 
                XPathDocument doc = new XPathDocument(SETTINGS_PATH);
                XPathNavigator nav = doc.CreateNavigator();
                // Compile a standard XPath expression
                XPathExpression expr;
                expr = nav.Compile(@"/settings/" + pstrValueToRead);
                XPathNodeIterator iterator = nav.Select(expr);
                // Iterate on the node set
                while (iterator.MoveNext())
                {
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                    return iterator.Current.Value;
#pragma warning restore CS8602 // Dereference of a possibly null reference.
                }
                return string.Empty;
            }
            catch
            {
                //do some error logging here. Leaving for you to do 
                return string.Empty;
            }
        }

        public static bool WriteValueTOXML(string pstrValueToRead, string pstrValueToWrite)
        {
            try
            {
                //settingsFilePath is a string variable storing the path of the settings file 
                XmlTextReader reader = new XmlTextReader(SETTINGS_PATH);
                XmlDocument doc = new XmlDocument();
                doc.Load(reader);
                //we have loaded the XML, so it's time to close the reader.
                reader.Close();
                XmlNode oldNode;
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                XmlElement root = doc.DocumentElement;
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                oldNode = root.SelectSingleNode("/settings/" + pstrValueToRead);
#pragma warning restore CS8602 // Dereference of a possibly null reference.
                oldNode.InnerText = pstrValueToWrite;
                doc.Save(SETTINGS_PATH);
                return true;
            }
            catch
            {
                //properly you need to log the exception here. But as this is just an
                //example, I am not doing that. 
                return false;
            }
        }
    }
}
