using System;
using System.Globalization;
using System.IO;
using System.Xml;

namespace erminas.SmartAPI.Utils
{
    public static class XmlUtil
    {
        /// <summary>
        ///   Creates an attribute via the owner document of <see cref="xmlElement" /> , sets its value and appends it to <see cref="xmlElement" /> .
        /// </summary>
        /// <param name="xmlElement"> The node, the attribute gets added to </param>
        /// <param name="attributeName"> Name of the attribute </param>
        /// <param name="value"> Value of the attribute </param>
        public static void AddAttribute(this XmlElement xmlElement, string attributeName, string value)
        {
            XmlAttribute attr = xmlElement.OwnerDocument.CreateAttribute(attributeName);
            attr.Value = value;
            xmlElement.Attributes.Append(attr);
        }

        /// <summary>
        ///   Creates an <see cref="XmlElement" /> and appends it as child to the XmlNode
        /// </summary>
        /// <param name="node"> The parent node </param>
        /// <param name="name"> Name of the newly created element </param>
        /// <returns> </returns>
        public static XmlElement AddElement(this XmlNode node, string name)
        {
            var doc = node as XmlDocument;
            doc = doc ?? node.OwnerDocument;
            var element = doc.CreateElement(name);
            node.AppendChild(element);
            return element;
        }

        /// <summary>
        ///   Sets an attribute to a value. If no fitting <see cref="XmlAttribute" /> exists, a new one is created/appended and its value set.
        /// </summary>
        /// <param name="xmlElement"> The node </param>
        /// <param name="attributeName"> Name of the attribute </param>
        /// <param name="value"> Value to set the attribute to </param>
        public static void SetAttributeValue(this XmlElement xmlElement, string attributeName, string value)
        {
            var attr = xmlElement.Attributes[attributeName];
            if (attr == null)
            {
                AddAttribute(xmlElement, attributeName, value);
            }
            else
            {
                attr.Value = value;
            }
        }

        /// <summary>
        ///   Creates a string representation of an <see cref="XmlNode" />
        /// </summary>
        /// <param name="xmlElement"> The node </param>
        /// <returns> string representation of <see cref="xmlElement" /> </returns>
        public static string NodeToString(this XmlElement xmlElement)
        {
            var sw = new StringWriter();
            var xw = new XmlTextWriter(sw);
            xmlElement.WriteTo(xw);

            return sw.ToString();
        }

        /// <summary>
        ///   Gets the value of an attribute. If the attribute does not exists, null is returned.
        /// </summary>
        /// <param name="xmlElement"> The node </param>
        /// <param name="attributeName"> Name of the attribute </param>
        /// <returns> Value of the attribute, null, if attribute doesn't exist </returns>
        public static string GetAttributeValue(this XmlElement xmlElement, string attributeName)
        {
            var attr = xmlElement.Attributes[attributeName];
            return attr == null ? null : attr.Value;
        }

        public static int? GetIntAttributeValue(this XmlElement xmlElement, string attributeName)
        {
            var attr = xmlElement.Attributes[attributeName];
            return attr == null ? (int?)null : int.Parse(attr.Value);
        }

        public static double? GetDoubleAttributeValue(this XmlElement xmlElement, string attributeName)
        {
            var attr = xmlElement.Attributes[attributeName];
            return attr == null ? (double?)null : Double.Parse(attr.Value, CultureInfo.InvariantCulture);
        }

        public static Guid GetGuid(this XmlElement xmlElement)
        {
            return xmlElement.GetGuid("guid");
        }

        public static Guid GetGuid(this XmlElement xmlElement, String attributeName)
        {
            return Guid.Parse(xmlElement.GetAttributeValue(attributeName));
        }

        public static bool TryGetGuid(this XmlElement xmlElement, out Guid guid)
        {
            return TryGetGuid(xmlElement, "guid", out guid);
        }

        public static bool TryGetGuid(this XmlElement xmlElement, string attributeName, out Guid guid)
        {
            var strValue = xmlElement.GetAttributeValue(attributeName);
            if (string.IsNullOrEmpty(strValue))
            {
                guid = Guid.Empty;
                return false;
            }

            guid = Guid.Parse(strValue);
            return true;
        }

        public static string GetName(this XmlElement xmlElement)
        {
            return xmlElement.GetAttributeValue("name");
        }

        public static DateTime? GetOADate(this XmlElement element, string attributeName = "date")
        {
            var strValue = element.GetAttributeValue(attributeName);
            if (String.IsNullOrEmpty(strValue))
            {
                return null;
            }

            return strValue.ToOADate();
        }

        public static DateTime ToOADate(this string value)
        {
            var valueNormalizedToInvariantCulture = value.Replace(",", ".");
            return DateTime.FromOADate(Double.Parse(valueNormalizedToInvariantCulture, CultureInfo.InvariantCulture));
        }

    }
}