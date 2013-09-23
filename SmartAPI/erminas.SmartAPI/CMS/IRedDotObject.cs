// SmartAPI - .Net programmatic access to RedDot servers
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
using System.Xml;
using erminas.SmartAPI.Exceptions;
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
    internal abstract class RedDotObject : AbstractAttributeContainer, IRedDotObject
    {
        protected static XmlDocument XMLDoc = new XmlDocument();

        private Guid _guid = Guid.Empty;
        protected string _name;

        protected RedDotObject(ISession session) : base(session)
        {
        }

        protected RedDotObject(ISession session, Guid guid) : base(session)
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
        protected RedDotObject(ISession session, XmlElement xmlElement) : base(session, xmlElement)
        {
            InitGuidAndName();
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

        public Guid Guid
        {
            get
            {
                if (_guid.Equals(Guid.Empty) && _xmlElement != null)
                {
                    InitIfPresent(ref _guid, "guid", GuidConvert);
                }
                return _guid;
            }
            internal set
            {
                if (_xmlElement != null)
                {
                    _xmlElement.Attributes["guid"].Value = value.ToRQLString();
                }
                _guid = value;
            }
        }

        /// <summary>
        ///     Convert a string to a guid, e.g. for <see cref="InitIfPresent{T}" />
        /// </summary>
        public static Guid GuidConvert(string str)
        {
            return new Guid(str);
        }

        public virtual string Name
        {
            get { return _name; }
            internal set { _name = value; }
        }

        public override string ToString()
        {
            return Name + " (" + Guid.ToRQLString() + ")";
        }

        /// <summary>
        ///     Get the string representation of the current object, which is needed in RQL to create/change a RedDotObject on the server. Adds an attribute "action" with value "save" to an XML node, replaces all attribute values which are null or empty with
        ///     <see
        ///         cref="RQL.SESSIONKEY_PLACEHOLDER" />
        ///     and returns the string representation of the resulting node. The replacement of empty attributes is necessary for RQL to actually set the attributes to an empty value instead of ignoring the attribute. Note that the node itself gets modified, so use a copy, if changes must not be made.
        /// </summary>
        /// <param name="xmlElement"> the XML node to be converted </param>
        /// <returns> </returns>
        protected internal static string GetSaveString(XmlElement xmlElement)
        {
            XmlAttributeCollection attributes = xmlElement.Attributes;
            foreach (XmlAttribute curAttr in attributes)
            {
                if (string.IsNullOrEmpty(curAttr.Value))
                {
                    curAttr.Value = RQL.SESSIONKEY_PLACEHOLDER;
                }
            }

            xmlElement.AddAttribute("action", "save");

            return xmlElement.NodeToString();
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
        ///     Init a variable with the value of an attribute of <see cref="XmlNode" /> .
        /// </summary>
        /// <typeparam name="T"> </typeparam>
        /// <param name="variable"> </param>
        /// <param name="attributeName"> </param>
        /// <param name="converter"> </param>
        protected void EnsuredInit<T>(ref T variable, string attributeName, Func<string, T> converter)
        {
            string value = _xmlElement.GetAttributeValue(attributeName);
            if (string.IsNullOrEmpty(value))
            {
                throw new SmartAPIException(Session.ServerLogin,
                                            string.Format("Missing value for attribute {0}", attributeName));
            }
            variable = converter(value);
        }

        protected void InitGuidAndName()
        {
            InitIfPresent(ref _guid, "guid", GuidConvert);
            InitIfPresent(ref _name, "name", x => x);
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
            string value = _xmlElement.GetAttributeValue(attributeName);
            if (!string.IsNullOrEmpty(value))
            {
                variable = converter(value);
            }
        }

        /// <summary>
        ///     Convert a string to a bool? value, e.g. for <see cref="InitIfPresent{T}" />
        /// </summary>
        protected static bool? NullableBoolConvert(string str)
        {
            return BoolConvert(str);
        }
    }

    public interface IRedDotObject
    {
        Guid Guid { get; }

        string Name { get; }
    }
}