using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RequestsManagement.Domain.Requests;
using RequestsManagement.Infrastructure.Persistence;

namespace RequestsManagement.Seeder;

internal sealed partial class DatabaseSeeder(
    RequestsDbContext dbContext,
    RequestDataGenerator generator,
    IOptions<SeedOptions> options,
    ILogger<DatabaseSeeder> logger)
{
    private readonly SeedOptions _options = options.Value;

    public async Task SeedAsync(CancellationToken cancellationToken)
    {
        if (_options.Reset)
        {
            LogResettingDatabase(logger);
            await dbContext.Database.EnsureDeletedAsync(cancellationToken);
        }

        await dbContext.Database.MigrateAsync(cancellationToken);

        var existingCount = await dbContext.Requests.CountAsync(cancellationToken);
        if (existingCount > 0)
        {
            LogSeedSkipped(logger, existingCount);
            return;
        }

        await BulkInsertRequestsAsync(cancellationToken);
    }

    private async Task BulkInsertRequestsAsync(CancellationToken cancellationToken)
    {
        var columns = RequestColumns.From(dbContext.Model);
        var connection = (SqlConnection)dbContext.Database.GetDbConnection();
        await connection.OpenAsync(cancellationToken);

        using var bulkCopy = new SqlBulkCopy(connection, SqlBulkCopyOptions.TableLock, externalTransaction: null)
        {
            DestinationTableName = columns.TableName,
            BatchSize = _options.BatchSize,
            BulkCopyTimeout = 0
        };

        using var batchTable = columns.CreateDataTable();
        foreach (DataColumn column in batchTable.Columns)
        {
            bulkCopy.ColumnMappings.Add(column.ColumnName, column.ColumnName);
        }

        var insertedCount = 0;
        foreach (var batch in generator.Generate().Chunk(_options.BatchSize))
        {
            batchTable.Clear();
            foreach (var request in batch)
            {
                batchTable.Rows.Add(
                    request.Title,
                    request.OrganizationName,
                    (byte)request.Status,
                    (byte)request.Priority,
                    (object?)request.AssignedTo ?? DBNull.Value,
                    request.CreatedAt,
                    request.UpdatedAt);
            }

            await bulkCopy.WriteToServerAsync(batchTable, cancellationToken);
            insertedCount += batch.Length;
            LogBatchInserted(logger, insertedCount, _options.RecordCount);
        }

        LogSeedCompleted(logger, insertedCount);
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "Reset requested, dropping the existing database")]
    private static partial void LogResettingDatabase(ILogger logger);

    [LoggerMessage(Level = LogLevel.Information, Message = "Database already contains {Count} requests, seeding skipped")]
    private static partial void LogSeedSkipped(ILogger logger, int count);

    [LoggerMessage(Level = LogLevel.Information, Message = "Inserted {Inserted}/{Total} requests")]
    private static partial void LogBatchInserted(ILogger logger, int inserted, int total);

    [LoggerMessage(Level = LogLevel.Information, Message = "Seeding completed with {Count} requests")]
    private static partial void LogSeedCompleted(ILogger logger, int count);

    private sealed record RequestColumns(
        string TableName,
        string Title,
        string OrganizationName,
        string Status,
        string Priority,
        string AssignedTo,
        string CreatedAt,
        string UpdatedAt)
    {
        public static RequestColumns From(IModel model)
        {
            var entityType = model.FindEntityType(typeof(Request))
                ?? throw new InvalidOperationException($"Entity '{nameof(Request)}' is not part of the model.");
            var table = StoreObjectIdentifier.Table(entityType.GetTableName()!, entityType.GetSchema());

            string ColumnOf(string propertyName) =>
                entityType.FindProperty(propertyName)!.GetColumnName(table)!;

            return new RequestColumns(
                TableName: entityType.GetSchemaQualifiedTableName()!,
                Title: ColumnOf(nameof(Request.Title)),
                OrganizationName: ColumnOf(nameof(Request.OrganizationName)),
                Status: ColumnOf(nameof(Request.Status)),
                Priority: ColumnOf(nameof(Request.Priority)),
                AssignedTo: ColumnOf(nameof(Request.AssignedTo)),
                CreatedAt: ColumnOf(nameof(Request.CreatedAt)),
                UpdatedAt: ColumnOf(nameof(Request.UpdatedAt)));
        }

        public DataTable CreateDataTable()
        {
            var table = new DataTable();
            table.Columns.Add(Title, typeof(string));
            table.Columns.Add(OrganizationName, typeof(string));
            table.Columns.Add(Status, typeof(byte));
            table.Columns.Add(Priority, typeof(byte));
            table.Columns.Add(AssignedTo, typeof(string));
            table.Columns.Add(CreatedAt, typeof(DateTime));
            table.Columns.Add(UpdatedAt, typeof(DateTime));
            return table;
        }
    }
}
