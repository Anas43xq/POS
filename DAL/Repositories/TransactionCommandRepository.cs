using System.Data;
using Contracts.Transactions;
using DAL.Entities.Data;
using DAL.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;

namespace DAL.Repositories
{
    public class TransactionCommandRepository : ITransactionCommandRepository
    {
        private readonly IDbContextFactory<PosDbContext> _contextFactory;

        public TransactionCommandRepository(IDbContextFactory<PosDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<int> CreateTransactionAsync(CreateTransactionRequest request)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            string connectionString = context.Database.GetConnectionString()
                ?? throw new InvalidOperationException("Connection string not found.");

            await using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();

            await using var command = new SqlCommand("SP_CreateTransaction", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            // Header
            command.Parameters.Add("@ShiftId", SqlDbType.Int).Value = request.ShiftId;
            command.Parameters.Add("@CashierId", SqlDbType.Int).Value = request.CashierId;
            AddDecimalParameter(command, "@Subtotal", request.Subtotal, 10, 2);
            AddDecimalParameter(command, "@TaxTotal", request.TaxTotal, 10, 2);
            AddDecimalParameter(command, "@GrandTotal", request.GrandTotal, 10, 2);
            command.Parameters.Add("@Notes", SqlDbType.NVarChar, 500).Value = (object?)request.Notes ?? DBNull.Value;

            // Payment
            command.Parameters.Add("@PaymentMethod", SqlDbType.NVarChar, 20).Value = request.PaymentMethod;
            AddDecimalParameter(command, "@AmountTendered", request.AmountTendered, 10, 2);
            AddDecimalParameter(command, "@ChangeGiven", request.ChangeGiven, 10, 2);
            command.Parameters.Add("@ReferenceNumber", SqlDbType.NVarChar, 100).Value = (object?)request.ReferenceNumber ?? DBNull.Value;

            // Items (table-valued parameter)
            SqlParameter itemsParameter = command.Parameters.Add("@Items", SqlDbType.Structured);
            itemsParameter.Value = BuildItemsTable(request.Items);
            itemsParameter.TypeName = "dbo.TransactionItemType";

            try
            {
                object? result = await command.ExecuteScalarAsync();
                return (int)(result ?? throw new InvalidOperationException("Transaction creation did not return an id."));
            }
            catch (SqlException ex)
            {
                throw TranslateSqlException(ex);
            }
        }

        private static Exception TranslateSqlException(SqlException ex)
        {
            // Messages raised by SP_CreateTransaction's RAISERROR calls.
            // SqlException.Number is 50000 for RAISERROR-raised errors without an explicit error number.
            if (ex.Number == 50000)
            {
                if (ex.Message.Contains("No open shift found", StringComparison.OrdinalIgnoreCase))
                    return new InvalidOperationException("No open shift found for this cashier.", ex);

                if (ex.Message.Contains("inactive or do not exist", StringComparison.OrdinalIgnoreCase))
                    return new InvalidOperationException("One or more product variants are inactive or do not exist.", ex);

                // Any other deliberately raised business-rule error from the SP.
                return new InvalidOperationException(ex.Message, ex);
            }
            return new InvalidOperationException("An error occurred while creating the transaction.", ex);
        }

        private static DataTable BuildItemsTable(IEnumerable<CreateTransactionItemRequest> items)
        {
            var table = new DataTable();
            table.Columns.Add("VariantId", typeof(int));
            table.Columns.Add("ProductName", typeof(string));
            table.Columns.Add("UnitPrice", typeof(decimal));
            table.Columns.Add("Quantity", typeof(int));
            table.Columns.Add("TaxRate", typeof(decimal));
            table.Columns.Add("LineSubtotal", typeof(decimal));
            table.Columns.Add("LineTax", typeof(decimal));
            table.Columns.Add("LineTotal", typeof(decimal));

            foreach (CreateTransactionItemRequest item in items)
            {
                table.Rows.Add(
                    item.VariantId,
                    item.ProductName,
                    item.UnitPrice,
                    item.Quantity,
                    item.TaxRate,
                    item.LineSubtotal,
                    item.LineTax,
                    item.LineTotal);
            }

            return table;
        }

        private static void AddDecimalParameter(SqlCommand command, string parameterName, decimal value, byte precision, byte scale)
        {
            SqlParameter parameter = command.Parameters.Add(parameterName, SqlDbType.Decimal);
            parameter.Precision = precision;
            parameter.Scale = scale;
            parameter.Value = value;
        }
    }
}