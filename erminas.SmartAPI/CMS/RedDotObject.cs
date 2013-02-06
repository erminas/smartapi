// Smart API - .Net programatical access to RedDot servers
//  
// Copyright (C) 2013 erminas GbR
// 
// This program is free software: you can redistribute it and/or modify it 
// under the terms of the GNU General Public License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
// See the GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License along with this program.
// If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Web.Script.Serialization;
using System.Xml;
using erminas.SmartAPI.Utils;

namespace erminas.SmartAPI.CMS
{
    /// <summary>
    ///     Base class for all red dot objects. It contains a Guid and can be initialized with a XML node containing the guid and other attributes.
    /// </summary>
    /// <remarks>
    ///     If you create objects that need to be initialized with a guid only and other values can be retrieved lazy (e.g. you have a folder guid, the folder name can optionally be retrieved, if needed), please use
    ///     <see
    ///         cref="PartialRedDotObject" />
    ///     as base class instead.
    /// </remarks>
    public abstract class RedDotObject : AbstractAttributeContainer, IRedDotObject
    {
        [ScriptIgnore] protected static XmlDocument XMLDoc = new XmlDocument();

        private Guid _guid = Guid.Empty;
        protected string _name;

        protected RedDotObject()
        {
        }

        protected RedDotObject(Guid guid)
        {
            Guid = guid;
        }

        /// <summary>
        ///     Create a new RedDotObject out of an XML element.
        /// </summary>
        /// <remarks>
        ///     A copy of the XML element is created and the <see cref="Guid" /> gets initialized with the "guid" attribute value of the XML node.
        /// </remarks>
        /// <exception cref="ArgumentNullException">thrown, if xmlElement is null</exception>
        protected RedDotObject(XmlElement xmlElement) : base(xmlElement)
        {
            if (xmlElement == null)
            {
                throw new ArgumentNullException("xmlElement");
            }

            InitGuidAndName();
        }

        #region IRedDotObject Members

        public Guid Guid
        {
            get
            {
                if (XmlElement != null && _guid.Equals(Guid.Empty))
                {
                    InitIfPresent(ref _guid, "guid", GuidConvert);
                }
                return _guid;
            }
            set
            {
                if (XmlElement != null)
                {
                    XmlElement.Attributes["guid"].Value = value.ToRQLString();
                }
                _guid = value;
            }
        }

        public virtual string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        #endregion

        protected void InitGuidAndName()
        {
            InitIfPresent(ref _guid, "guid", GuidConvert);
            InitIfPresent(ref _name, "name", x => x);
        }

        /// <summary>
        ///     Convert a string to a guid, e.g. for <see cref="InitIfPresent{T}" />
        /// </summary>
        public static Guid GuidConvert(string str)
        {
            return new Guid(str);
        }

        /// <summary>
        ///     Convert a string to a boolean value, e.g. for <see cref="InitIfPresent{T}" />
        /// </summary>
        /// <remarks>
        ///     <see cref="bool.Parse" /> can't be used, because RQL uses "1" and "0" to represent boolean values.
        /// </remarks>
        protected static bool BoolConvert(string str)
        {
            switch (str.Trim())
            {
                case "1":
                    return true;
                case "0":
                    return false;
                default:
                    throw new Exception("Illegal value for bool '" + str + "', '1' or '0' expected");
            }
        }

        /// <summary>
        ///     Convert a string to a bool? value, e.g. for <see cref="InitIfPresent{T}" />
        /// </summary>
        protected static bool? NullableBoolConvert(string str)
        {
            return BoolConvert(str);
        }

        /// <summary>
        ///     Get the string representation of the current object, which is needed in RQL to create/change a RedDotObject on the server. Adds an attribute "action" with value "save" to an XML node, replaces all attribute values which are null or empty with
        ///     <see
        ///         cref="Session.SESSIONKEY_PLACEHOLDER" />
        ///     and returns the string representation of the resulting node. The replacement of empty attributes is necessary for RQL to actually set the attributes to an empty value instead of ignoring the attribute. Note that the node itself gets modified, so use a copy, if changes must not be made.
        /// </summary>
        /// <param name="xmlElement"> the XML node to be converted </param>
        /// <returns> </returns>
        protected static string GetSaveString(XmlElement xmlElement)
        {
            XmlAttributeCollection attributes = xmlElement.Attributes;
            foreach (XmlAttribute curAttr in attributes)
            {
                if (string.IsNullOrEmpty(curAttr.Value))
                {
                    curAttr.Value = Session.SESSIONKEY_PLACEHOLDER;
                }
            }

            xmlElement.AddAttribute("action", "save");

            return xmlElement.NodeToString();
        }

        /// <summary>
        ///     Init a variable with the value of an attribute of <see cref="XmlNode" /> . The variable only gets set to a value, if the
        ///     <see
        ///         cref="XmlNode" />
        ///     contains the attribute and the attribute value is neither null nor empty.
        /// </summary>
        /// <typeparam name="T"> TypeId of the variable </typeparam>
        /// <param name="variable"> reference to the variable </param>
        /// <param name="attributeName"> name of the XML attribute </param>
        /// <param name="converter"> a function that converts the string value of the XML attribute to the actual type of the variable </param>
        protected void InitIfPresent<T>(ref T variable, string attributeName, Func<string, T> converter)
        {
            string value = XmlElement.GetAttributeValue(attributeName);
            if (!string.IsNullOrEmpty(value))
            {
                variable = converter(value);
            }
        }

        /// <summary>
        ///     Init a variable with the value of an attribute of <see cref="XmlNode" /> .
        /// </summary>
        /// <typeparam name="T"> </typeparam>
        /// <param name="variable"> </param>
        /// <param name="attributeName"> </param>
        /// <param name="converter"> </param>
        protected void EnsuredInit<T>(ref T variable, string attributeName, Func<string, T> converter)
        {
            string value = XmlElement.GetAttributeValue(attributeName);
            if (string.IsNullOrEmpty(value))
            {
                //TODO eigene exception
                throw new Exception("Missing value for attribute " + attributeName);
            }
            variable = converter(value);
        }

        /// <summary>
        ///     Two RedDotObjects are considered equal, if their guids are equal.
        /// </summary>
        /// <param name="other"> </param>
        /// <returns> </returns>
        public override bool Equals(object other)
        {
            var o = other as IRedDotObject;
            if (o == null)
            {
                return false;
            }
            return ReferenceEquals(this, other) || o.Guid.Equals(_guid);
        }

        public override int GetHashCode()
        {
            return _guid.GetHashCode();
        }

        public override string ToString()
        {
            return Name + " (" + Guid.ToRQLString() + ")";
        }
    }
}