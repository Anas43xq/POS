namespace DAL.Entities
{
    /// <summary>
    /// Represents the status of a transaction in the Point of Sale system.
    /// </summary>
    public enum TransactionStatus : byte
    {
        Pending = 0,
        Completed = 1,
        Voided = 2
    }
}
