using System;
using System.Globalization;
using System.Xml;
using erminas.SmartAPI.CMS.Project;
using erminas.SmartAPI.CMS.Project.ContentClasses.Elements.Attributes;
using erminas.SmartAPI.Exceptions;
using erminas.SmartAPI.Utils;

namespace erminas.SmartAPI.CMS
{
    public interface ILanguageDependentValue<T>
    {
        T ForCurrentLanguage { get; set; }
        T ForMainLanguage { get; set; }
        T this[ILanguageVariant languageVariant] { get; set; }
        T this[string languageAbbreviation] { get; set; }
    }

    internal class LanguageDependentValue<T> : ILanguageDependentValue<T>
    {
        private readonly ILanguageDependentPartialRedDotObject _parent;
        private readonly RedDotAttribute _attribute;

        public LanguageDependentValue(ILanguageDependentPartialRedDotObject parent, RedDotAttribute attribute)
        {
            _parent = parent;
            _attribute = attribute;
        }

        public T ForCurrentLanguage
        {
            get { return this[_parent.Project.LanguageVariants.Current]; }
            set { this[_parent.Project.LanguageVariants.Current] = value; }
        }

        public T ForMainLanguage
        {
            get { return this[_parent.Project.LanguageVariants.Main]; }
            set { this[_parent.Project.LanguageVariants.Main] = value; }
        }

        public T this[ILanguageVariant languageVariant]
        {
            get { return this[languageVariant.Abbreviation]; }
            set { this[languageVariant.Abbreviation] = value; }
        }

        public T this[string languageAbbreviation]
        {
            get { 
                var xmlElement = _parent.GetXmlElementForLanguage(languageAbbreviation);
                return _attribute.ReadFrom<T>(xmlElement);
            }
            set
            {
                var xmlElement = _parent.GetXmlElementForLanguage(languageAbbreviation);
                _attribute.WriteTo(xmlElement, value);
            }
        }
    }

    public interface IConverter<T>
    {
        T ConvertFrom(XmlElement element, RedDotAttribute attribute);
        void WriteTo(XmlElement element, RedDotAttribute attribute, T value);
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class RedDotAttribute : Attribute
    {
        public readonly string ElementName;
        private object _converterInstance;
        private Type _converterType;
        private string _description;
        private Type _targetType;

        public RedDotAttribute(string elementName)
        {
            ElementName = elementName;
        }

        public T ReadFrom<T>(XmlElement element)
        {
            Type type = typeof (T);
            return _converterInstance != null
                       ? GetCustomConversion<T>(element, type)
                       : GetDefaultConversion<T>(element, type);
        }

        public Type ConverterType
        {
            get { return _converterType; }
            set
            {
                if (!IsConverterType(value))
                {
                    throw new SmartAPIInternalException(string.Format("Invalid converter type '{0}' for element {1}",
                                                                      value.Name, ElementName));
                }
                try
                {
                    _targetType = _converterType.GetInterface(typeof (IConverter<object>).Name).GetGenericArguments()[0];
                    _converterInstance = value.GetConstructor(new Type[0]).Invoke(new object[0]);
                    _converterType = value;
                } catch (Exception e)
                {
                    throw new SmartAPIInternalException(
                        string.Format("Invalid converter type '{0}' for element {1}", value.Name, ElementName), e);
                }
            }
        }

        public string Description
        {
            get { return _description ?? RDXmlNodeAttribute.ELEMENT_DESCRIPTION[ElementName]; }
            set { _description = value; }
        }

        public void WriteTo<T>(XmlElement element, T value)
        {
            if (_converterInstance != null)
            {
                SetWithCustomConversion(element, value);
            }
            else
            {
                SetWithDefaultConversion(element, value);
            }
        }

        private T GetCustomConversion<T>(XmlElement element, Type type)
        {
            if (_targetType != type)
            {
                throw new SmartAPIInternalException(
                    string.Format("Converter type does not match Convert<T> call for element {0}", ElementName));
            }

            return ((IConverter<T>) _converterInstance).ConvertFrom(element, this);
        }

        private T GetDefaultConversion<T>(XmlElement element, Type type)
        {
            if (type == typeof (string))
            {
                return (T) (object) element.GetAttributeValue(ElementName);
            }
            if (type == typeof (bool))
            {
                return (T) (object) element.GetBoolAttributeValue(ElementName).GetValueOrDefault();
            }
            if (type == typeof (int?))
            {
                return (T) (object) element.GetIntAttributeValue(ElementName);
            }
            throw new SmartAPIInternalException(string.Format("No matching conversion for element {0} to type {1}",
                                                              ElementName, type.Name));
        }

        private bool IsConverterType(Type value)
        {
            return value.GetInterface(typeof (IConverter<object>).Name) == null;
        }

        private void SetWithCustomConversion<T>(XmlElement element, T value)
        {
            if (typeof (T) != _targetType)
            {
                throw new SmartAPIInternalException(
                    string.Format("Converter type {0} does not match Set<T> call for element {1} with type {2}",
                                  _targetType.Name, ElementName, typeof (T).Name));
            }

            ((IConverter<T>) _converterInstance).WriteTo(element, this, value);
        }

        private void SetWithDefaultConversion<T>(XmlElement element, T value)
        {
            Type type = typeof (T);
            if (type == typeof (string))
            {
                element.SetAttributeValue(ElementName, (string) (object) value);
                return;
            }

            if (type == typeof (bool))
            {
                element.SetAttributeValue(ElementName, (bool) (object) value ? "1" : "0");
                return;
            }
            if (type == typeof (int?))
            {
                var i = ((int?) (object) value);
                element.SetAttributeValue(ElementName, i != null ? i.Value.ToString(CultureInfo.InvariantCulture) : null);
                return;
            }
            throw new SmartAPIInternalException(string.Format("No matching conversion for element {0} to type {1}",
                                                              ElementName, type.Name));
        }
    }
}