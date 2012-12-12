namespace erminas.SmartAPI.CMS.CCElements
{
    public interface ICanBeRequiredForEditing
    {
        bool IsEditingMandatory { get; set; }
        void Commit();
    }
}
