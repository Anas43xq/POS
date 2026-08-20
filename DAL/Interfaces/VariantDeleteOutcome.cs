namespace DAL.Interfaces;

/// <summary>
/// Result of <see cref="IProductVariantRepository.TryDeleteAsync"/>.
/// </summary>
public enum VariantDeleteOutcome
{
    /// <summary>No variant existed with the given id.</summary>
    NotFound,

    /// <summary>The row was removed outright — nothing referenced it.</summary>
    Deleted,

    /// <summary>
    /// The row is referenced by historical TransactionItems (FK is
    /// NoAction, so history is never rewritten), so it was deactivated
    /// (IsActive = false) instead of removed.
    /// </summary>
    Deactivated
}
