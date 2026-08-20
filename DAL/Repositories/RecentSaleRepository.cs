using Contracts.Sales;
using DAL.Infrastructure;
using DAL.Interfaces;
using Microsoft.Data.SqlClient;
using System.Data;

namespace DAL.Repositories
{
    public class RecentSaleRepository : IRecentSaleRepository
    {
        private readonly ISqlConnectionStringProvider _connectionStringProvider;

        public RecentSaleRepository(ISqlConnectionStringProvider connectionStringProvider)
        {
            _connectionStringProvider = connectionStringProvider;
        }

        public async Task<List<RecentTransactionDto>> GetRecentTransactionsByCashierId(int cashierId, int shiftId, int take = 10,CancellationToken ct= default)
        {
            await using var connection = new SqlConnection(_connectionStringProvider.ConnectionString);
            await connection.OpenAsync(ct);

            await using var cmd = new SqlCommand("GetRecentTransactionsByCashier", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add("@CashierId", SqlDbType.Int).Value = cashierId;
            cmd.Parameters.Add("@ShiftId", SqlDbType.Int).Value = shiftId;
            cmd.Parameters.Add("@Limit", SqlDbType.Int).Value = take;

            using var reader = await cmd.ExecuteReaderAsync(ct);
            List<RecentTransactionDto> recentTransactions = new();

            while (await reader.ReadAsync())
            {
                var transactionId = Convert.ToInt32(reader["TransactionId"]);
                var receiptNumber = reader["ReceiptNumber"]?.ToString() ?? "";
                var transactionDate = Convert.ToDateTime(reader["TransactionDate"]);
                var paymentMethod = reader["PaymentMethod"]?.ToString() ?? "";
                var statusByte = Convert.ToByte(reader["Status"]);
                var status = statusByte switch
                {
                    1 => "Completed",
                    2 => "Voided",
                    _ => "Pending"
                };
                var total = Convert.ToDecimal(reader["GrandTotal"]);

                var recentSale = BuildRecentLine(
                    transactionId,
                    receiptNumber,
                    transactionDate,
                    paymentMethod,
                    status,
                    total
                );

                if (recentSale != null)
                    recentTransactions.Add(recentSale);
            }

            return recentTransactions;
        }

        private RecentTransactionDto BuildRecentLine(
            int transactionId,
            string receiptNumber,
            DateTime transactionDate,
            string paymentMethod,
            string status,
            decimal total)
        {
            return new RecentTransactionDto
            {
                TransactionId = transactionId,
                ReceiptNumber = receiptNumber,
                TransactionDate = transactionDate,
                PaymentMethod = paymentMethod,
                Status = status,
                GrandTotal = total
            };
        }
    }
}

