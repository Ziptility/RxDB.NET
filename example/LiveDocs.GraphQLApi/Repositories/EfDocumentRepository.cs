﻿using LiveDocs.GraphQLApi.Models;
using Microsoft.EntityFrameworkCore;
using RxDBDotNet.Documents;
using RxDBDotNet.Repositories;
using RxDBDotNet.Services;

namespace LiveDocs.GraphQLApi.Repositories;

/// <summary>
/// An implementation of IDocumentRepository using Entity Framework Core.
/// This class provides optimized database access for document operations required by the RxDB replication protocol.
/// </summary>
/// <typeparam name="TDocument">The type of document being managed, which must implement IReplicatedDocument.</typeparam>
/// <typeparam name="TContext">The type of DbContext to use for data access.</typeparam>
/// <remarks>
/// Initializes a new instance of the EfDocumentRepository class.
/// </remarks>
/// <param name="context">The DbContext to use for data access.</param>
/// <param name="eventPublisher">The event publisher used to publish document change events.</param>
/// <param name="logger">The logger to use for logging operations and errors.</param>
public class EfDocumentRepository<TDocument, TContext>(TContext context, IEventPublisher eventPublisher, ILogger<EfDocumentRepository<TDocument, TContext>> logger) : BaseDocumentRepository<TDocument>(eventPublisher, logger)
    where TDocument : class, IReplicatedDocument
    where TContext : DbContext
{
    private readonly TContext _context = context ?? throw new ArgumentNullException(nameof(context));

    /// <inheritdoc/>
    public override IQueryable<TDocument> GetQueryableDocuments()
    {
        return _context.Set<TDocument>().AsNoTracking();
    }

    /// <inheritdoc/>
    public override Task<List<TDocument>> ExecuteQueryAsync(IQueryable<TDocument> query, CancellationToken cancellationToken)
    {
        return query.ToListAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public override Task<TDocument?> GetDocumentByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return _context.Set<TDocument>().FindAsync([id], cancellationToken).AsTask();
    }

    /// <inheritdoc/>
    protected override async Task<TDocument> CreateDocumentInternalAsync(TDocument document, CancellationToken cancellationToken)
    {
        await _context.Set<TDocument>().AddAsync(document, cancellationToken).ConfigureAwait(false);
        return document;
    }

    /// <inheritdoc/>
    protected override async Task<TDocument> UpdateDocumentInternalAsync(TDocument document, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(document);
        
        var existingDocument = await _context.Set<TDocument>().FindAsync([document.Id], cancellationToken).ConfigureAwait(false)
                               ?? throw new InvalidOperationException($"Document with ID {document.Id} not found for update.");

        if (!AreDocumentsEqual(existingDocument, document))
        {
            _context.Entry(existingDocument).CurrentValues.SetValues(document);
        }

        return existingDocument;
    }

    /// <inheritdoc/>
    protected override async Task<TDocument> MarkAsDeletedInternalAsync(TDocument document, CancellationToken cancellationToken)
    {
        var documentToDelete = await GetDocumentByIdAsync(document.Id, cancellationToken).ConfigureAwait(false);

        if (documentToDelete != null)
        {
            documentToDelete.IsDeleted = true;
            // this should be set from the updated document from the client
            //document.UpdatedAt = DateTimeOffset.UtcNow;
            _context.Update(documentToDelete);
        }

        return documentToDelete;
    }

    /// <inheritdoc/>
    protected override async Task SaveChangesInternalAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            throw new ConcurrencyException("Concurrency conflict occurred while saving changes", ex);
        }
        catch (DbUpdateException ex)
        {
            throw new ConcurrencyException("Error occurred while saving changes", ex);
        }
    }

    /// <inheritdoc/>
    public override bool AreDocumentsEqual(TDocument existingDocument, TDocument assumedMasterState)
    {
        var entry1 = _context.Entry(existingDocument);
        var entry2 = _context.Entry(assumedMasterState);

        foreach (var property in entry1.Properties)
        {
            var name = property.Metadata.Name;
            if (name != nameof(IReplicatedDocument.UpdatedAt)) // Ignore UpdatedAt for comparison
            {
                var value1 = property.CurrentValue;
                var value2 = entry2.Property(name).CurrentValue;

                if (!Equals(value1, value2))
                {
                    return false;
                }
            }
        }

        return true;
    }
}
