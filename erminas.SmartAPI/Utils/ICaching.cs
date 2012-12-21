namespace erminas.SmartAPI.Utils
{
    public interface ICaching
    {
        /// <summary>
        ///   Clear the cache and refresh it on the next access.
        /// </summary>
        void InvalidateCache();

        /// <summary>
        ///   Updates the cache immediatly.
        /// </summary>
        void Refresh();
    }
}