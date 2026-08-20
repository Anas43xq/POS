namespace UI.Services
{
    /// <summary>
    /// Creates dialog/child ViewModels through the DI container instead of
    /// manual <c>new</c>. Handles the common case in this app where a
    /// ViewModel's constructor mixes injectable services (e.g.
    /// <c>IProductService</c>) with runtime-only data that can't come from
    /// the container (e.g. the record being edited, a running total, a
    /// parent ViewModel reference, a completion callback).
    /// <para>
    /// Pass the runtime-only values as <paramref name="parameters"/>, in
    /// any order — the factory resolves everything else from DI by
    /// matching constructor parameter types. This keeps every dialog
    /// ViewModel creation path consistent instead of some going through
    /// DI and others being hand-constructed.
    /// </para>
    /// </summary>
    public interface IViewModelFactory
    {
        /// <summary>
        /// Creates an instance of <typeparamref name="T"/>. Any constructor
        /// parameter whose type matches one of <paramref name="parameters"/>
        /// is filled from that argument; every other parameter is resolved
        /// from the DI container.
        /// </summary>
        T Create<T>(params object[] parameters) where T : class;
    }
}
