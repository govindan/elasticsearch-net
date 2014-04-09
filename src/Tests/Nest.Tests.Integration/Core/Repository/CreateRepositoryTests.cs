﻿using System;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using Nest.Tests.MockData.Domain;
using Nest.Resolvers;
using Elasticsearch.Net;

namespace Nest.Tests.Integration.Core.Repository
{
	[TestFixture]
	public class CreateRepositoryTests : IntegrationTests
	{
		[Test]
		public void CreateAndDeleteRepository_ThenSnapshotWithWait()
		{
			var repositoryName = ElasticsearchConfiguration.NewUniqueIndexName();
			var createReposResult = this._client.CreateRepository(repositoryName, r => r
				.FileSystem(@"local\\path", o => o
					.Compress()
					.ConcurrentStreams(10)
				)
			);
			createReposResult.IsValid.Should().BeTrue();
			createReposResult.Acknowledged.Should().BeTrue();

			var backupName = ElasticsearchConfiguration.NewUniqueIndexName();
			var snapshotResponse = this._client.Snapshot(backupName, repositoryName, f => f
				.WaitForCompletion(true)
				.Index(ElasticsearchConfiguration.NewUniqueIndexName())
				.IgnoreUnavailable()
				.Partial()
			);
			snapshotResponse.IsValid.Should().BeTrue();
			snapshotResponse.Accepted.Should().BeTrue();
			snapshotResponse.Snapshot.Should().NotBeNull();
			snapshotResponse.Snapshot.DurationInMilliseconds.Should().BeGreaterThan(0);
			snapshotResponse.Snapshot.EndTimeInMilliseconds.Should().BeGreaterThan(0);
			snapshotResponse.Snapshot.StartTime.Should().BeAfter(DateTime.UtcNow.AddDays(-1));

			var deleteReposResult = this._client.DeleteRepository(repositoryName);
			deleteReposResult.IsValid.Should().BeTrue();
			deleteReposResult.Acknowledged.Should().BeTrue();
		}
		
		[Test]
		public void CreateAndDeleteRepository_ThenSnapshotWithoutWait()
		{
			var repositoryName = ElasticsearchConfiguration.NewUniqueIndexName();
			var createReposResult = this._client.CreateRepository(repositoryName, r => r
				.FileSystem(@"local\\path", o => o
					.Compress()
					.ConcurrentStreams(10)
				)
			);
			createReposResult.IsValid.Should().BeTrue();
			createReposResult.Acknowledged.Should().BeTrue();

			var backupName = ElasticsearchConfiguration.NewUniqueIndexName();
			var snapshotResponse = this._client.Snapshot(backupName, repositoryName, f => f
				.Index(ElasticsearchConfiguration.NewUniqueIndexName())
				.IgnoreUnavailable()
				.Partial()
			);
			snapshotResponse.IsValid.Should().BeTrue();
			snapshotResponse.Accepted.Should().BeTrue();
			snapshotResponse.Snapshot.Should().BeNull();

			var deleteReposResult = this._client.DeleteRepository(repositoryName);
			deleteReposResult.IsValid.Should().BeTrue();
			deleteReposResult.Acknowledged.Should().BeTrue();
		}
	}
}
