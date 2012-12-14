using System;
using System.Collections.Generic;
using erminas.SmartAPI.Utils;

namespace erminas.SmartAPI.CMS.CCElements.Attributes
{
    public class StringEnumXmlNodeAttribute<T> : EnumXmlNodeAttribute<T> where T : struct, IConvertible
    {
        private readonly Func<string, T> _parseEnum;
        private readonly Func<T, string> _toStringValue;

        public StringEnumXmlNodeAttribute(CCElement parent, string name, Func<T, string> toStringValueRepresentation,
                                          Func<string, T> parseEnum) : base(parent, name, false)
        {
            _toStringValue = toStringValueRepresentation;
            _parseEnum = parseEnum;
            UpdateValue(Parent.XmlNode.GetAttributeValue(Name));
        }

        public StringEnumXmlNodeAttribute(IAttributeContainer parent, string name, Dictionary<T, string> displayStrings,
                                          Func<T, string> toStringValueRepresentation, Func<string, T> parseEnum)
            : base(parent, name, displayStrings, false)
        {
            _toStringValue = toStringValueRepresentation;
            _parseEnum = parseEnum;
            UpdateValue(Parent.XmlNode.GetAttributeValue(Name));
        }

        public override T Value
        {
            get { return base.Value; }
            set { SetValue(_toStringValue(value)); }
        }

        protected override sealed void UpdateValue(string value)
        {
            try
            {
                if (string.IsNullOrEmpty(value) || value == Session.SESSIONKEY_PLACEHOLDER)
                {
                    _value = default(T);
                    return;
                }
                _value = _parseEnum(value);
            } catch (Exception e)
            {
                throw new Exception(string.Format("Could not convert value {0} to enum {1}", value, typeof (T).Name), e);
            }
        }
    }
}