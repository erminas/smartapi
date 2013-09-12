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
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Xml;
using erminas.SmartAPI.CMS.Project;
using erminas.SmartAPI.Exceptions;

namespace erminas.SmartAPI.CMS
{
    public interface IAttributeAssignment
    {
        void AssignAllLanguageIndependentRedDotAttributes<T>(T source, T target);
        void AssignAllRedDotAttributesForLanguage<T>(T source, T target, string language) where T : IProjectObject;
    }

    public class AttributeAssignment : IAttributeAssignment
    {
        public void AssignAllLanguageIndependentRedDotAttributes<T>(T source, T target)
        {
            var properties =
                typeof (T).GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy);
            foreach (var curProperty in properties)
            {
                if (!curProperty.CanRead || !curProperty.CanWrite)
                {
                    continue;
                }

                var attribute = GetRedDotAttribute(curProperty);
                if (attribute == null || attribute.IsReadOnly)
                {
                    continue;
                }
                try
                {
                    CopyValue(source, target, curProperty);
                } catch (Exception e)
                {
                    throw new AttributeChangeException(curProperty.Name, e);
                }
            }
        }

        public void AssignAllRedDotAttributesForLanguage<T>(T source, T target, string language)
            where T : IProjectObject
        {
            var properties =
                typeof (T).GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy);
            foreach (var curProperty in properties)
            {
                if (!curProperty.CanRead)
                {
                    continue;
                }

                var attribute = GetRedDotAttribute(curProperty);
                if (attribute == null || attribute.IsReadOnly)
                {
                    continue;
                }
                try
                {
                    CopyValueForLanguage(source, target, curProperty, language);
                } catch (Exception e)
                {
                    throw new AttributeChangeException(curProperty.Name, e);
                }
            }
        }

        private static void CopyLanguageDependendValue<T>(T source, T target, PropertyInfo propertyInfo, string language)
            where T : IProjectObject
        {
            var sourceValue = propertyInfo.GetValue(source, null);
            var targetValue = propertyInfo.GetValue(target, null);

            var sourceLanguageIndexer = GetLanguageIndexer(sourceValue);
            var targetLanguageIndexer = GetLanguageIndexer(targetValue);

            var value = sourceLanguageIndexer.GetValue(sourceLanguageIndexer, new object[] {language});

            targetLanguageIndexer.SetValue(targetLanguageIndexer, value, new object[] {language});
        }

        private static void CopyValue<T>(T source, T target, PropertyInfo curProperty)
        {
            const bool DONT_SHOW_NON_PUBLIC_ACCESSORS = false;
            var getMethod = curProperty.GetGetMethod(DONT_SHOW_NON_PUBLIC_ACCESSORS);
            var setMethod = curProperty.GetSetMethod(DONT_SHOW_NON_PUBLIC_ACCESSORS);

            var value = getMethod.Invoke(source, new object[0]);

            setMethod.Invoke(target, new[] {value});
        }

        private static void CopyValueForLanguage<T>(T source, T target, PropertyInfo propertyInfo, string language)
            where T : IProjectObject
        {
            if (typeof (ILanguageDependentValueBase).IsAssignableFrom(propertyInfo.PropertyType))
            {
                CopyLanguageDependendValue(source, target, propertyInfo, language);
                return;
            }
            CopyValue(source, target, propertyInfo);
        }

        private static PropertyInfo GetLanguageIndexer(object @object)
        {
            return @object.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public).First(info =>
                {
                    var param = info.GetIndexParameters().FirstOrDefault();
                    return param != null && param.ParameterType == typeof (string);
                });
        }

        private static RedDotAttribute GetRedDotAttribute(PropertyInfo curProperty)
        {
            const bool USE_INHERITED = true;
            var attributes =
                curProperty.GetCustomAttributes(typeof (RedDotAttribute), USE_INHERITED).Cast<RedDotAttribute>();
            return attributes.FirstOrDefault();
        }
    }

    internal class AttributeChangeException : Exception
    {
        private readonly string _attributeName;

        public AttributeChangeException(string attributeName, Exception exception)
            : base(string.Format("Could not change attribute {0}", attributeName), exception)
        {
            _attributeName = attributeName;
        }

        public string AttributeName
        {
            get { return _attributeName; }
        }
    }

    internal abstract class AbstractAttributeContainer : ISessionObject, IXmlBasedObject
    {
        private readonly ISession _session;
        private XmlElement _xmlElement;

        internal AbstractAttributeContainer(ISession session)
        {
            _session = session;
        }

        internal AbstractAttributeContainer(ISession session, XmlElement xmlElement)
        {
            _session = session;
            if (xmlElement == null)
            {
                throw new SmartAPIInternalException("XmlElement is null");
            }
            _xmlElement = xmlElement;
        }

        public virtual ISession Session
        {
            get { return _session; }
        }

        public virtual XmlElement XmlElement
        {
            get { return _xmlElement; }
            protected internal set { _xmlElement = value; }
        }

        protected virtual T GetAttributeValue<T>([CallerMemberName] string callerName = "")
        {
            var project = this as IProjectObject;
            var property = GetProperty(callerName);
            var attribute = GetRedDotAttributeOfCallerMember(callerName);

            if (IsLanguageDependentProperty(property))
            {
                return GetLanguageDependentProperty<T>(attribute, property);
            }

            return attribute.ReadFrom<T>(project, XmlElement);
        }

        protected RedDotAttribute GetRedDotAttributeOfCallerMember(string callerName)
        {
            const bool USE_INHERITED_ATTRIBUTES = true;

            var property = GetProperty(callerName);
            return
                (RedDotAttribute)
                property.GetCustomAttributes(typeof (RedDotAttribute), USE_INHERITED_ATTRIBUTES).First();
        }

        protected virtual void SetAttributeValue<T>(T value, [CallerMemberName] string callerName = "")
        {
            var project = this as IProjectObject;
            var attribute = GetRedDotAttributeOfCallerMember(callerName);
            attribute.WriteTo(project, XmlElement, value);
        }

        private T GetLanguageDependentProperty<T>(RedDotAttribute attribute, PropertyInfo property)
        {
            try
            {
                var constructor =
                    typeof (T).GetConstructor(new[]
                        {typeof (ILanguageDependentPartialRedDotObject), typeof (RedDotAttribute)});

// ReSharper disable PossibleNullReferenceException
                return (T) constructor.Invoke(new object[] {this, attribute});
// ReSharper restore PossibleNullReferenceException
            } catch (Exception e)
            {
                throw new SmartAPIInternalException(
                    string.Format("Internal error in construction of language dependent property {0}", property.Name), e);
            }
        }

        private PropertyInfo GetProperty(string callerName)
        {
            return GetType()
                .GetProperty(callerName,
                             BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public |
                             BindingFlags.FlattenHierarchy);
        }

        private static bool IsLanguageDependentProperty(PropertyInfo property)
        {
            return
                property.PropertyType.GetInterfaces()
                        .Any(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof (ILanguageDependentValue<>));
        }
    }
}