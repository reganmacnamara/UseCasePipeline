using Microsoft.Extensions.DependencyInjection;
using UseCasePipeline;
using UseCasePipeline.Extensions;
using UseCasePipeline.Infrastructure;
using UseCasePipeline.Services;
using Xunit;

namespace UseCasePipelineTests;

public class ServiceCollectionExtensionsTests
{
    // Minimal request/handler pair used purely to assert registration happened.
    public record FooResponse(int Value) : IUseCaseResponse;
    public class FooRequest : IUseCaseRequest<FooResponse> { }

    public class FooHandler : UseCaseHandler<FooRequest, FooResponse>
    {
        public override Task<FooResponse> Handle(FooRequest request, CancellationToken cancellationToken)
            => Task.FromResult(new FooResponse(42));
    }

    public class FooValidator : UseCaseRequestValidator<FooRequest>
    {
        public override Task Validate(FooRequest request, CancellationToken cancellationToken)
            => Task.CompletedTask;
    }

    public class FixedAssemblyLocator(params System.Reflection.Assembly[] assemblies) : IUseCaseAssemblyLocator
    {
        public IReadOnlyList<System.Reflection.Assembly> LocateAssemblies() => assemblies;
    }

    [Fact]
    public void AddUseCasePipeline_RegistersMediatorAndDiscoveredComponents()
    {
        var services = new ServiceCollection();

        // ↓ Breakpoint inside ServiceCollectionExtensions.AddUseCasePipeline to step through scanning.
        services.AddUseCasePipeline(typeof(ServiceCollectionExtensionsTests).Assembly);

        var provider = services.BuildServiceProvider();

        Assert.NotNull(provider.GetService<UseCaseMediator>());

        var handler = provider.GetService<IUseCaseHandler<FooRequest, FooResponse>>();
        Assert.NotNull(handler);
        Assert.IsType<FooHandler>(handler);

        var validators = provider.GetServices<IUseCaseRequestValidator<FooRequest>>().ToList();
        Assert.Single(validators);
        Assert.IsType<FooValidator>(validators[0]);
    }

    [Fact]
    public void AddUseCasePipeline_WithNoAssemblies_StillRegistersMediator()
    {
        var services = new ServiceCollection();

        services.AddUseCasePipeline(Array.Empty<System.Reflection.Assembly>());

        var provider = services.BuildServiceProvider();
        Assert.NotNull(provider.GetService<UseCaseMediator>());
        Assert.Null(provider.GetService<IUseCaseHandler<FooRequest, FooResponse>>());
    }

    [Fact]
    public void AddUseCasePipeline_WhenAnAssemblyIsScannedTwice_RegistersEachComponentOnce()
    {
        var services = new ServiceCollection();
        var assembly = typeof(ServiceCollectionExtensionsTests).Assembly;

        services.AddUseCasePipeline(assembly);
        services.AddUseCasePipeline(assembly, assembly);

        var provider = services.BuildServiceProvider();
        Assert.Single(provider.GetServices<IUseCaseRequestValidator<FooRequest>>());
        Assert.Single(provider.GetServices<IUseCaseHandler<FooRequest, FooResponse>>());
        Assert.Single(provider.GetServices<IUseCasePipe<FooRequest>>().OfType<FooValidator>());
        Assert.Single(services, descriptor => descriptor.ServiceType == typeof(UseCaseMediator));
    }

    [Fact]
    public void AddUseCasePipeline_WithNoArguments_RegistersComponentsFromTheApplicationsAssemblies()
    {
        var services = new ServiceCollection();

        services.AddUseCasePipeline();

        var provider = services.BuildServiceProvider();
        Assert.IsType<FooHandler>(provider.GetService<IUseCaseHandler<FooRequest, FooResponse>>());
        Assert.Single(provider.GetServices<IUseCaseRequestValidator<FooRequest>>());
    }

    [Fact]
    public void AddUseCasePipeline_WithAnAssemblyLocator_ScansOnlyTheAssembliesItReturns()
    {
        var withTestAssembly = new ServiceCollection();
        var withNoAssemblies = new ServiceCollection();

        withTestAssembly.AddUseCasePipeline(new FixedAssemblyLocator(typeof(ServiceCollectionExtensionsTests).Assembly));
        withNoAssemblies.AddUseCasePipeline(new FixedAssemblyLocator());

        Assert.NotNull(withTestAssembly.BuildServiceProvider().GetService<IUseCaseHandler<FooRequest, FooResponse>>());
        Assert.Null(withNoAssemblies.BuildServiceProvider().GetService<IUseCaseHandler<FooRequest, FooResponse>>());
    }
}
