﻿namespace RxDBDotNet.Documents;

/// <summary>
/// Represents the minimum requirements for a document to be replicated using this extension.
/// </summary>
public interface IReplicatedDocument
{
    /// <summary>
    /// The unique identifier for the document.
    /// </summary>
    Guid DocumentId { get; init; }

    /// <summary>
    /// The timestamp of the last update to the document.
    /// </summary>
    DateTimeOffset UpdatedAt { get; init; }

    /// <summary>
    /// Indicates whether the document has been marked as deleted.
    /// </summary>
    bool IsDeleted { get; init; }
}
