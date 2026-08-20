namespace DAL.Entities
{
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
