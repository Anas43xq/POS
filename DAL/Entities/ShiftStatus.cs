namespace DAL.Entities
{
    /// <summary>
    /// Represents the status of a shift in the Point of Sale system.
    /// </summary>
    public enum ShiftStatus : byte
    {
        /// <summary>
        /// Shift is currently open and cashier can perform sales.
        /// </summary>
        Open = 1,

        /// <summary>
        /// Shift is closed and no transactions are allowed.
        /// </summary>
        Closed = 0
    }
}
