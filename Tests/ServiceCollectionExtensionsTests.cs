using Microsoft.Extensions.DependencyInjection;
using UseCasePipeline;
using UseCasePipeline.Extensions;
using UseCasePipeline.Infrastructure;
using Xunit;

namespace UseCasePipelineTests;

public class ServiceCollectionExtensionsTests
{
    // Minimal request/handler pair used purely to assert registration happened.
    public record FooResponse(int Value) : IUseCaseResponse;
    public class FooRequest : IUseCaseRequest<FooResponse> { }

    public class FooHandler : IUseCaseHandler<FooRequest, FooResponse>
    {
        public Task<FooResponse> Handle(FooRequest request, CancellationToken cancellationToken)
            => Task.FromResult(new FooResponse(42));
    }

    public class FooValidator : IUseCaseRequestValidator<FooRequest>
    {
        public Task Validate(FooRequest request, CancellationToken cancellationToken)
            => Task.CompletedTask;
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
}
