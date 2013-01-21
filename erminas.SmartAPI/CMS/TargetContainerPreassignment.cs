using System;
using erminas.SmartAPI.CMS.PageElements;
using erminas.SmartAPI.Utils;

namespace erminas.SmartAPI.CMS
{
    internal class TargetContainerPreassignment
    {
        private readonly IContentClassElement _element;
        private Container _cachedTargetContainer;

        internal TargetContainerPreassignment(IContentClassElement element)
        {
            _element = element;
        }

        internal Container TargetContainer
        {
            get
            {
                Guid guid;
                if (!_element.XmlNode.TryGetGuid("elttargetcontainerguid", out guid))
                {
                    return null;
                }

                if (_cachedTargetContainer != null && _cachedTargetContainer.Guid == guid)
                {
                    return _cachedTargetContainer;
                }

                return
                    _cachedTargetContainer =
                    (Container) PageElement.CreateElement(_element.ContentClass.Project, guid, _element.LanguageVariant);
            }
            set
            {
                _element.XmlNode.SetAttributeValue("elttargetcontainerguid",
                                                   value == null ? null : value.Guid.ToRQLString());
            }
        }

        internal bool IsDisplayingConnectedPagesInTargetContainerOfMainLinkIfAvailable
        {
            get { return _element.XmlNode.GetBoolAttributeValue("usepagemainlinktargetcontainer").GetValueOrDefault(); }
            set
            {
                _element.XmlNode.SetAttributeValue("usepagemainlinktargetcontainer", value.ToRQLString());
            }
        }
    }
}