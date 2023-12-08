

namespace Deepgram.Tests.UnitTests.ExtensionsTests;
public class ServiceCollectionExtensionsTests
{
    #region With DeepgramClientOptions
    [Test]
    public void AddDeepgram_With_DeepgramClientOptions_Should_Add_IHttpClientFactory_ToServiceCollection()
    {
        // Arrange
        var services = new ServiceCollection();
        // Act
        ServiceCollectionExtensions.AddDeepgram(services, new DeepgramClientOptions("453t"));
        var registeredService = services.FirstOrDefault(descriptor => descriptor.ServiceType == typeof(IHttpClientFactory));

        // Assert
        registeredService.Should().NotBeNull();
    }


    [Test]
    public void AddDeepgram_With_DeepgramClientOptions_Should_Add_ManageClient_ToServiceCollection()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        ServiceCollectionExtensions.AddDeepgram(services, new DeepgramClientOptions("453t"));
        var registeredService = services.FirstOrDefault(descriptor => descriptor.ServiceType == typeof(ManageClient));

        // Assert
        registeredService.Should().NotBeNull();
    }

    [Test]
    public void AddDeepgram_With_DeepgramClientOptions_Should_Add_LiveClient_ToServiceCollection()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        ServiceCollectionExtensions.AddDeepgram(services, new DeepgramClientOptions("453t"));
        var registeredService = services.FirstOrDefault(descriptor => descriptor.ServiceType == typeof(LiveClient));

        // Assert
        registeredService.Should().NotBeNull();
    }

    [Test]
    public void AddDeepgram_With_DeepgramClientOptions_Should_Add_OnPremClient_ToServiceCollection()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        ServiceCollectionExtensions.AddDeepgram(services, new DeepgramClientOptions("453t"));
        var registeredService = services.FirstOrDefault(descriptor => descriptor.ServiceType == typeof(OnPremClient));

        // Assert
        registeredService.Should().NotBeNull();
    }

    [Test]
    public void AddDeepgram_With_DeepgramClientOptions_Should_Add_PrerecordedClient_ToServiceCollection()
    {
        // Arrange
        var services = new ServiceCollection();
        // Act
        ServiceCollectionExtensions.AddDeepgram(services, new DeepgramClientOptions("453t"));
        var registeredService = services.FirstOrDefault(descriptor => descriptor.ServiceType == typeof(PrerecordedClient));

        // Assert
        registeredService.Should().NotBeNull();
    }
    #endregion

    #region With ApiKey

    [Test]
    public void AddDeepgram_With_ApiKey_Should_Add_IHttpClientFactory_ToServiceCollection()
    {
        // Arrange
        var services = new ServiceCollection();
        // Act
        ServiceCollectionExtensions.AddDeepgram(services, "453t");
        var registeredService = services.FirstOrDefault(descriptor => descriptor.ServiceType == typeof(IHttpClientFactory));

        // Assert
        registeredService.Should().NotBeNull();
    }


    [Test]
    public void AddDeepgram_With_ApiKey_Should_Add_ManageClient_ToServiceCollection()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        ServiceCollectionExtensions.AddDeepgram(services, "453t");
        var registeredService = services.FirstOrDefault(descriptor => descriptor.ServiceType == typeof(ManageClient));

        // Assert
        registeredService.Should().NotBeNull();
    }

    [Test]
    public void AddDeepgram_With_ApiKey_Should_Add_LiveClient_ToServiceCollection()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        ServiceCollectionExtensions.AddDeepgram(services, "453t");
        var registeredService = services.FirstOrDefault(descriptor => descriptor.ServiceType == typeof(LiveClient));

        // Assert
        registeredService.Should().NotBeNull();
    }

    [Test]
    public void AddDeepgram_With_ApiKey_Should_Add_OnPremClient_ToServiceCollection()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        ServiceCollectionExtensions.AddDeepgram(services, "453t");
        var registeredService = services.FirstOrDefault(descriptor => descriptor.ServiceType == typeof(OnPremClient));

        // Assert
        registeredService.Should().NotBeNull();
    }

    [Test]
    public void AddDeepgram_With_ApiKey_Should_Add_PrerecordedClient_ToServiceCollection()
    {
        // Arrange
        var services = new ServiceCollection();
        // Act
        ServiceCollectionExtensions.AddDeepgram(services, "453t");
        var registeredService = services.FirstOrDefault(descriptor => descriptor.ServiceType == typeof(PrerecordedClient));

        // Assert
        registeredService.Should().NotBeNull();
    }
    #endregion
}
