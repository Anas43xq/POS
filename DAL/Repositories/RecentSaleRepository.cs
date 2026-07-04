using Contracts.Sales;
using DAL.Entities.Data;
using DAL.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System.Data;

namespace DAL.Repositories
{
    public class RecentSaleRepository : IRecentSaleRepository
    {
        private readonly IDbContextFactory<PosDbContext> _contextFactory;

        public RecentSaleRepository(IDbContextFactory<PosDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<List<RecentTransactionDto>> GetRecentTransactionsByCashierId(int cashierId, int shiftId, int take = 10,CancellationToken ct= default)
        {
            await using var context = await _contextFactory.CreateDbContextAsync(ct);
            string connectionString = context.Database.GetConnectionString()
                ?? throw new InvalidOperationException("Connection string not found.");

            await using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync(ct);

            await using var cmd = new SqlCommand("GetRecentTransactionsByCashier", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add("@CashierId", SqlDbType.Int).Value = cashierId;
            cmd.Parameters.Add("@ShiftId", SqlDbType.Int).Value = shiftId;
            cmd.Parameters.Add("@Limit", SqlDbType.Int).Value = take;

            using var reader = await cmd.ExecuteReaderAsync();
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

