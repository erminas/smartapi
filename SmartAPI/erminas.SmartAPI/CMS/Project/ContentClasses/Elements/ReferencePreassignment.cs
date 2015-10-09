using System;
using erminas.SmartAPI.CMS.Project.Pages;
using erminas.SmartAPI.Exceptions;
using erminas.SmartAPI.Utils;

namespace erminas.SmartAPI.CMS.Project.ContentClasses.Elements
{
    internal class ReferencePreassignment
    {
        private readonly IReferencePreassignable _parent;
        private bool _isInitialized;
        private IReferencePreassignTarget _preassignTarget;

        public ReferencePreassignment(IReferencePreassignable parent)
        {
            _parent = parent;
        }

        public IReferencePreassignTarget Target
        {
            get
            {
                if (_isInitialized)
                {
                    return _preassignTarget;
                }

                var parent = (ContentClassElement) _parent;
                parent.EnsureInitialization();
                _isInitialized = true;

                var referenceGuidStr = parent.XmlReadWriteWrapper.GetAttributeValue("eltrefelementguid");
                Guid referenceGuid;
                if (Guid.TryParse(referenceGuidStr, out referenceGuid))
                {
                    var type = parent.XmlReadWriteWrapper.GetAttributeValue("eltrefelementtype");
                    if (type == "PAG")
                    {
                        IPage targetPage;
                        _preassignTarget = parent.Project.Pages.TryGetByGuid(
                                                                             referenceGuid,
                                                                             parent.Project.LanguageVariants.Current,
                                                                             out targetPage)
                                               ? targetPage
                                               : null;
                        return _preassignTarget;
                    }

                    IContentClassElement ccOut;
                    if (parent.ContentClass.Elements.TryGetByGuid(referenceGuid, out ccOut))
                    {
                        _preassignTarget = (IReferencePreassignTarget) ccOut;
                        return _preassignTarget;
                    }
                }
                _preassignTarget = null;

                return _preassignTarget;
            }
            set
            {
                RemovePreassignedTarget();
                if (value != null)
                {
                    SetPreassignedTarget(value);
                    _preassignTarget = value;
                }
            }
        }

        private void SetPreassignedTarget(IReferencePreassignTarget value)
        {
            const string SET_TARGET =
                @"<CLIPBOARD action=""ReferenceToPage"" guid=""{0}"" type=""project.4145"" descent=""unknown"" addition="""">
<ENTRY guid=""{1}"" type=""page"" descent=""unknown"" />
</CLIPBOARD>";
            var result = _parent.Project.ExecuteRQL(SET_TARGET.RQLFormat(_parent, value));
            if (!result.InnerText.Contains("ok"))
            {
                throw new SmartAPIException(
                    _parent.Project.Session.ServerLogin,
                    string.Format("Could not set reference on {0} to {1}", _parent, value));
            }
        }

        private void RemovePreassignedTarget()
        {
            if (Target == null)
            {
                return;
            }

            const string REMOVE_TARGET =
                @"<TEMPLATE><ELEMENT action=""unlink"" guid=""{0}""><ELEMENT guid=""{1}""/></ELEMENT></TEMPLATE>";
            _parent.Project.ExecuteRQL(REMOVE_TARGET.RQLFormat(_parent, Target));
            _isInitialized = false;
            ((ContentClassElement) _parent).IsInitialized = false;
        }

        public void InvalidateCache()
        {
            _isInitialized = false;
        }
    }
}
