using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using erminas.SmartAPI.CMS.Project.Pages;

namespace erminas.SmartAPI.CMS.Project.ContentClasses.Elements
{
    public interface IReferencePreassignable : IContentClassElement
    {
        IReferencePreassignTarget PreassignedReference { get; set; }
    }
}
