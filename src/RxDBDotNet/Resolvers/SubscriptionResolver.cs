﻿using System.Runtime.CompilerServices;
using HotChocolate.Execution;
using HotChocolate.Subscriptions;
using Microsoft.Extensions.Logging;
using RxDBDotNet.Documents;
using RxDBDotNet.Models;

namespace RxDBDotNet.Resolvers;

/// <summary>
///     Provides subscription functionality for real-time updates of replicated documents.
///     This class implements the 'event observation' mode of the RxDB replication protocol.
/// </summary>
/// <typeparam name="TDocument">The type of document being replicated. Must implement <see cref="IReplicatedDocument" />.</typeparam>
/// <remarks>
///     Note that this class must not use constructor injection per:
///     https://chillicream.com/docs/hotchocolate/v13/server/dependency-injection#constructor-injection
/// </remarks>
public sealed class SubscriptionResolver<TDocument> where TDocument : class, IReplicatedDocument
{
    private const int RetryDelayMilliseconds = 5000;

    /// <summary>
    ///     Provides a stream of document changes for subscription.
    ///     This method is the entry point for GraphQL subscriptions and implements
    ///     the server-side push mechanism of the RxDB replication protocol.
    /// </summary>
    /// <param name="eventReceiver">The event receiver used for subscribing to document changes.</param>
    /// <param name="logger">The logger used for logging information and errors.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>
    ///     An asynchronous enumerable of <see cref="DocumentPullBulk{TDocument}" /> representing the stream of document
    ///     changes.
    /// </returns>
#pragma warning disable CA1822 // disable Mark members as static since this is a class instantiated by DI
    internal IAsyncEnumerable<DocumentPullBulk<TDocument>> DocumentChangedStream(
        ITopicEventReceiver eventReceiver,
        ILogger<SubscriptionResolver<TDocument>> logger,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(eventReceiver);
        ArgumentNullException.ThrowIfNull(logger);

        return DocumentChangedStreamInternal(eventReceiver, logger, cancellationToken);
    }

    private static async IAsyncEnumerable<DocumentPullBulk<TDocument>> DocumentChangedStreamInternal(
        ITopicEventReceiver eventReceiver,
        ILogger<SubscriptionResolver<TDocument>> logger,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var streamName = $"Stream_{typeof(TDocument).Name}";

        while (!cancellationToken.IsCancellationRequested)
        {
            ISourceStream<DocumentPullBulk<TDocument>>? documentStream;

            try
            {
                documentStream = await eventReceiver.SubscribeAsync<DocumentPullBulk<TDocument>>(streamName, cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                logger.LogInformation("Document change stream for {DocumentType} was cancelled.", typeof(TDocument).Name);
                yield break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while subscribing to the document change stream for {DocumentType}. Retrying in {Delay} ms.",
                    typeof(TDocument).Name, RetryDelayMilliseconds);
                await Task.Delay(RetryDelayMilliseconds, cancellationToken)
                    .ConfigureAwait(false);
                continue;
            }

            await foreach (var pullDocumentResult in documentStream.ReadEventsAsync()
                               .WithCancellation(cancellationToken)
                               .ConfigureAwait(false))
            {
                yield return pullDocumentResult;
            }

            // If we reach here, it means the stream has completed normally.
            // We'll log this and continue the outer loop to resubscribe.
            logger.LogInformation("Document change stream for {DocumentType} completed. Resubscribing.", typeof(TDocument).Name);
        }
    }
}
