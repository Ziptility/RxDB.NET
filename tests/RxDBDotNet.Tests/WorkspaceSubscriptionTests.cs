// using System.Diagnostics;
// using System.Reactive.Linq;
// using System.Reactive.Threading.Tasks;
// using System.Text.Json;
// using FluentAssertions;
// using GraphQL;
// using GraphQL.Client.Abstractions.Websocket;
// using GraphQL.Client.Http;
// using GraphQL.Client.Http.Websocket;
// using GraphQL.Client.Serializer.SystemTextJson;
// using Microsoft.AspNetCore.TestHost;
// using RxDBDotNet.Tests.Model;
// using RxDBDotNet.Tests.Utils;
// using Xunit.Abstractions;
// using GraphQLRequest = GraphQL.GraphQLRequest;
//
// namespace RxDBDotNet.Tests
// {
//     public class WorkspaceSubscriptionTests(ITestOutputHelper output) : TestBase(output)
//     {
//         private static readonly JsonSerializerOptions JsonOptions = new()
//         {
//             PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
//             PropertyNameCaseInsensitive = true,
//         };
//
//         [Fact]
//         public async Task WorkspaceCreation_ShouldBeEmittedThroughSubscription()
//         {
//             // Arrange
//             //var webSocketClient = Server.CreateWebSocketClient();
//             using var subscriptionClient = await CreateSubscriptionClientAsync();
//             var subscriptionResult = StartWorkspaceSubscription(subscriptionClient);
//
//             // Act
//             var newWorkspace = await HttpClient.CreateNewWorkspaceAsync();
//
//             // Assert
//             var receivedWorkspacePullBulk = await WaitForSubscriptionUpdateAsync(subscriptionResult);
//             Debug.Assert(receivedWorkspacePullBulk != null, nameof(receivedWorkspacePullBulk) + " != null");
//             receivedWorkspacePullBulk.Documents.Should().NotBeNullOrEmpty();
//             receivedWorkspacePullBulk.Documents.Should().HaveCount(1);
//             var recievedWorkspace = receivedWorkspacePullBulk.Documents?.Single();
//             Debug.Assert(recievedWorkspace != null, nameof(recievedWorkspace) + " != null");
//             Debug.Assert(newWorkspace.Id != null, "newWorkspace.Id != null");
//             recievedWorkspace.Id.Should().Be(newWorkspace.Id.Value);
//             recievedWorkspace.Name.Should().Be(newWorkspace.Name?.Value);
//             recievedWorkspace.IsDeleted.Should().Be(newWorkspace.IsDeleted?.Value);
//             recievedWorkspace.UpdatedAt.Should().BeCloseTo(newWorkspace.UpdatedAt?.Value ?? default, TimeSpan.FromSeconds(1));
//         }
//
//         private async Task<GraphQLHttpClient> CreateSubscriptionClientAsync()
//         {
//             var options = new GraphQLHttpClientOptions
//             {
//                 EndPoint = new Uri(HttpClient.BaseAddress!, "graphql"),
//                 WebSocketEndPoint = new Uri($"ws://{HttpClient.BaseAddress!.Host}:{HttpClient.BaseAddress.Port}/graphql"),
//                 //WebSocketProtocol = WebSocketProtocols.GRAPHQL_TRANSPORT_WS,
//                 ConfigureWebsocketOptions = wsOptions =>
//                 {
//                     //wsOptions.AddSubProtocol("graphql-transport-ws");
//                     wsOptions.SetRequestHeader("Authorization", "Bearer test-token");
//                 },
//                 ConfigureWebSocketConnectionInitPayload = _ => new
//                 {
//                     Authorization = "Bearer test-token",
//                 },
//             };
//
//             var jsonSerializer = new SystemTextJsonSerializer(JsonOptions);
//
//             var client = new GraphQLHttpClient(options, jsonSerializer, HttpClient);
//
//             await client.InitializeWebsocketConnection();
//
//             return client;
//         }
//
//         private static IObservable<GraphQLResponse<GqlQueryResponse>> StartWorkspaceSubscription(GraphQLHttpClient client)
//         {
//             var subscription = new SubscriptionQueryBuilderGql()
//                 .WithStreamWorkspace(
//                     new WorkspacePullBulkQueryBuilderGql()
//                         .WithDocuments(
//                             new WorkspaceQueryBuilderGql()
//                                 .WithId()
//                                 .WithName()
//                                 .WithUpdatedAt()
//                                 .WithIsDeleted()
//                         )
//                         .WithCheckpoint(
//                             new CheckpointQueryBuilderGql()
//                                 .WithLastDocumentId()
//                                 .WithUpdatedAt()
//                         ),
//                     new WorkspaceInputHeadersGql
//                     {
//                         Authorization = "Bearer test-token",
//                     }
//                 );
//
//             var request = new GraphQLRequest
//             {
//                 Query = subscription.Build(),
//             };
//
//             return client.CreateSubscriptionStream<GqlQueryResponse>(request);
//         }
//
//         private static async Task<WorkspacePullBulkGql?> WaitForSubscriptionUpdateAsync(IObservable<GraphQLResponse<GqlQueryResponse>> subscription)
//         {
//             using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
//
//             try
//             {
//                 var subscriptionTask = subscription
//                     .Select(graphQLResponse => graphQLResponse.Data.Data.PullWorkspace)
//                     .FirstOrDefaultAsync()
//                     .Timeout(TimeSpan.FromSeconds(5))
//                     .ToTask(cts.Token);
//
//                 return await subscriptionTask;
//             }
//             catch (TimeoutException)
//             {
//                 throw new TimeoutException("Subscription update not received within the expected timeframe.");
//             }
//         }
//     }
//
//     public class TestGraphQLClient : GraphQLHttpClient
//     {
//         public TestGraphQLClient(GraphQLHttpClientOptions options, IGraphQLWebsocketJsonSerializer serializer, TestWebSocketClient webSocketClient)
//             : base(options, serializer)
//         {
//             WebSocketClient = webSocketClient;
//         }
//
//         public override WebSocketClient WebSocketClient { get; }
//     }
// }
