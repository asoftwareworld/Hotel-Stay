using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace HotelStay.Tests.Integration;

/// <summary>
/// Shared WebApplicationFactory across all integration test classes.
/// Prevents parallel host creation which causes "entry point exited without building IHost".
/// </summary>
[CollectionDefinition("Integration")]
public sealed class IntegrationCollection : ICollectionFixture<WebApplicationFactory<Program>> { }
