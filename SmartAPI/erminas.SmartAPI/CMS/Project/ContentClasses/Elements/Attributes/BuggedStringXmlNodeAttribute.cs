namespace erminas.SmartAPI.CMS.Project.ContentClasses.Elements.Attributes
{
    internal class BuggedStringXmlNodeAttribute : StringXmlNodeAttribute
    {
        public BuggedStringXmlNodeAttribute(IAttributeContainer parent, string name) : base(parent, name)
        {
        }


        protected override void SetValue(string value)
        {
            if (value.Contains("EmptyBuffer"))
            {
                value = null;
            }
            base.SetValue(value);
        }
    }
}
