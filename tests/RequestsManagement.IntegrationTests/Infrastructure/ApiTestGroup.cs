namespace RequestsManagement.IntegrationTests.Infrastructure;

[CollectionDefinition(Name)]
public sealed class ApiTestGroup : ICollectionFixture<RequestsApiFactory>
{
    public const string Name = "Api";
}
