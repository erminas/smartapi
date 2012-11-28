using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Xml;

namespace erminas.SmartAPI.Utils
{
    public static class XmlUtil
    {
        /// <summary>
        /// Creates an attribute via the owner document of <see cref="node"/>, sets its value and appends
        /// it to <see cref="node"/>.
        /// </summary>
        /// <param name="node">The node, the attribute gets added to</param>
        /// <param name="attributeName">Name of the attribute</param>
        /// <param name="value">Value of the attribute</param>
        public static void AddAttribute(this XmlNode node, string attributeName, string value)
        {
            if (node.Attributes == null)
            {
                return;
            }
            XmlAttribute attr = node.OwnerDocument.CreateAttribute(attributeName);
            attr.Value = value;
            node.Attributes.Append(attr);
        }

        /// <summary>
        /// Creates an <see cref="XmlElement"/> and appends it as child to the XmlNode
        /// </summary>
        /// <param name="node">The parent node</param>
        /// <param name="name">Name of the newly created element</param>
        /// <returns></returns>
        public static XmlElement AddElement(this XmlNode node, string name)
        {
            var doc = node as XmlDocument;
            doc = doc ?? node.OwnerDocument;
            var element = doc.CreateElement(name);
            node.AppendChild(element);
            return element;
        }

        /// <summary>
        /// Sets an attribute to a value.
        /// If no fitting <see cref="XmlAttribute"/> exists, a new one is created/appended and
        /// its value set.
        /// </summary>
        /// <param name="node">The node</param>
        /// <param name="attributeName">Name of the attribute</param>
        /// <param name="value">Value to set the attribute to</param>
        public static void SetAttributeValue(this XmlNode node, string attributeName, string value)
        {
            if (node.Attributes == null)
            {
                return;
            }
            var attr = node.Attributes[attributeName];
            if (attr == null)
            {
                AddAttribute(node, attributeName, value);
            }
            else
            {
                attr.Value = value;
            }
        }

        /// <summary>
        /// Creates a string representation of an <see cref="XmlNode"/>
        /// </summary>
        /// <param name="node">The node</param>
        /// <returns>string representation of <see cref="node"/></returns>
        public static string NodeToString(this XmlNode node)
        {
            var sw = new StringWriter();
            var xw = new XmlTextWriter(sw);
            node.WriteTo(xw);

            return sw.ToString();
        }

        /// <summary>
        /// Gets the value of an attribute.
        /// If the attribute does not exists, null is returned.
        /// </summary>
        /// <param name="node">The node</param>
        /// <param name="attributeName">Name of the attribute</param>
        /// <returns>Value of the attribute, null, if attribute doesn't exist</returns>
        public static string GetAttributeValue(this XmlNode node, string attributeName)
        {
            if (node.Attributes == null)
            {
                return null;
            }
            var attr = node.Attributes[attributeName];
            return attr == null ? null : attr.Value;
        }

        public static int? GetIntAttributeValue(this XmlNode node, string attributeName)
        {
            if (node.Attributes == null)
            {
                return null;
            }
            var attr = node.Attributes[attributeName];
            return attr == null ? (int?)null : int.Parse(attr.Value);
        }

        public static double? GetDoubleAttributeValue(this XmlNode node, string attributeName)
        {
            if (node.Attributes == null)
            {
                return null;
            }
            var attr = node.Attributes[attributeName];
            return attr == null ? (double?)null : Double.Parse(attr.Value, CultureInfo.InvariantCulture);
        }

        public static Guid GetGuid(this XmlNode node)
        {
            return node.GetGuid("guid");
        }

        public static Guid GetGuid(this XmlNode node, String attributeName)
        {
            return Guid.Parse(node.GetAttributeValue(attributeName));
        }

        public static bool TryGetGuid(this XmlNode node, out Guid guid)
        {
            return TryGetGuid(node, "guid", out guid);
        }

        public static bool TryGetGuid(this XmlNode node, string attributeName, out Guid guid)
        {
            var strValue = node.GetAttributeValue(attributeName);
            if (string.IsNullOrEmpty(strValue))
            {
                guid = Guid.Empty;
                return false;
            }

            guid = Guid.Parse(strValue);
            return true;
        }

        public static string GetName(this XmlNode node)
        {
            return node.GetAttributeValue("name");
        }
    }
}
