﻿using FluentAssertions;
using RT.Comb;
using RxDBDotNet.Tests.Model;
using RxDBDotNet.Tests.Utils;
using Xunit.Abstractions;

namespace RxDBDotNet.Tests;

public class BasicDocumentOperationsTests(ITestOutputHelper output) : TestBase(output)
{
    [Fact]
    public async Task TestCase1_1_PushNewRowShouldCreateSingleDocument()
    {
        // Arrange
        var newWorkspace = new WorkspaceInputGql
        {
            Id = Provider.Sql.Create(),
            Name = Strings.CreateString(),
            UpdatedAt = DateTimeOffset.UtcNow,
            IsDeleted = false,
        };

        var workspaceInput = new WorkspaceInputPushRowGql
        {
            AssumedMasterState = null,
            NewDocumentState = newWorkspace,
        };

        var pushWorkspaceInputGql = new PushWorkspaceInputGql
        {
            WorkspacePushRow = new List<WorkspaceInputPushRowGql?>
            {
                workspaceInput,
            },
        };

        var createWorkspace =
            new MutationQueryBuilderGql().WithPushWorkspace(new PushWorkspacePayloadQueryBuilderGql().WithAllFields(), pushWorkspaceInputGql);

        // Act
        var response = await HttpClient.PostGqlMutationAsync(createWorkspace);

        // Assert
        response.Errors.Should()
            .BeNullOrEmpty();
        response.Data.PushWorkspace?.Workspace.Should()
            .BeNullOrEmpty();

        // Verify the workspace exists in the database
        await HttpClient.VerifyWorkspaceExists(newWorkspace);
    }

    [Fact]
    public async Task TestCase1_2_PullBulkByDocumentIdShouldReturnSingleDocument()
    {
        // Arrange
        var newWorkspace = await HttpClient.CreateNewWorkspaceAsync();

        var query = new QueryQueryBuilderGql().WithPullWorkspace(new WorkspacePullBulkQueryBuilderGql().WithAllFields()
            .WithDocuments(new WorkspaceQueryBuilderGql().WithAllFields(), new WorkspaceFilterInputGql
            {
                Id = new UuidOperationFilterInputGql
                {
                    Eq = newWorkspace.Id?.Value,
                },
            }), 10);

        // Act
        var response = await HttpClient.PostGqlQueryAsync(query);

        // Assert
        response.Errors.Should()
            .BeNullOrEmpty();

        response.Data.PullWorkspace?.Documents.Should()
            .HaveCount(1);
    }
}
