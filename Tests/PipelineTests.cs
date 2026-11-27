using Microsoft.Extensions.DependencyInjection;
using UseCasePipeline;
using UseCasePipeline.Exceptions;
using UseCasePipeline.Extensions;
using UseCasePipeline.Infrastructure;
using UseCasePipeline.Services;
using Xunit;

namespace UseCasePipelineTests;

public class PipelineTests
{
    // ---- Test use case: a request with a response, exercised end-to-end ----

    public record PingResponse(string Message) : IUseCaseResponse;

    public class PingRequest : IUseCaseRequest<PingResponse>
    {
        public string Name { get; set; } = "";
        public List<string> Trace { get; } = new();
    }

    public class PingValidator : UseCaseRequestValidator<PingRequest>
    {
        public override Task Validate(PingRequest request, CancellationToken cancellationToken)
        {
            request.Trace.Add(nameof(PingValidator));
            if (string.IsNullOrWhiteSpace(request.Name))
                throw new UseCaseValidationException("Name required");
            return Task.CompletedTask;
        }
    }

    public class PingAuthoriser : UseCaseAuthoriser<PingRequest>
    {
        public override Task Authorise(PingRequest request, CancellationToken cancellationToken)
        {
            request.Trace.Add(nameof(PingAuthoriser));
            return Task.CompletedTask;
        }
    }

    public class PingHandler : UseCaseHandler<PingRequest, PingResponse>
    {
        public override Task<PingResponse> Handle(PingRequest request, CancellationToken cancellationToken)
        {
            request.Trace.Add(nameof(PingHandler));
            return Task.FromResult(new PingResponse($"hello {request.Name}"));
        }
    }

    public class PingAfterHandlerPipe : UseCaseCustomPipe<PingRequest>
    {
        public override int Order { get; set; } = 70;

        public override Task InvokeAsync(PingRequest request, CancellationToken cancellationToken)
        {
            request.Trace.Add(nameof(PingAfterHandlerPipe));
            return Task.CompletedTask;
        }
    }

    public class UnhandledRequest : IUseCaseRequest<PingResponse> { }

    public class SharedOrderRequest : IUseCaseRequest
    {
        public List<string> Trace { get; } = new();
    }

    public abstract class SharedOrderTracingPipe : UseCaseCustomPipe<SharedOrderRequest>
    {
        public override Task InvokeAsync(SharedOrderRequest request, CancellationToken cancellationToken)
        {
            request.Trace.Add(GetType().Name);
            return Task.CompletedTask;
        }
    }

    public class FirstSharedOrderPipe : SharedOrderTracingPipe { }

    public class SecondSharedOrderPipe : SharedOrderTracingPipe { }

    public class SharedOrderAuthoriser : UseCaseAuthoriser<SharedOrderRequest>
    {
        public override Task Authorise(SharedOrderRequest request, CancellationToken cancellationToken)
        {
            request.Trace.Add(nameof(SharedOrderAuthoriser));
            return Task.CompletedTask;
        }
    }

    private static UseCaseMediator BuildMediator()
    {
        var services = new ServiceCollection();
        services.AddUseCasePipeline(typeof(PipelineTests).Assembly);
        return services.BuildServiceProvider().GetRequiredService<UseCaseMediator>();
    }

    [Fact]
    public async Task Send_RunsPipesInOrderThenHandler_AndReturnsResponse()
    {
        var mediator = BuildMediator();
        var request = new PingRequest { Name = "world" };

        var response = await mediator.Send(request);

        Assert.Equal("hello world", response.Message);
        Assert.Equal(
            new[] { nameof(PingAuthoriser), nameof(PingValidator), nameof(PingHandler) },
            request.Trace);
    }

    [Fact]
    public async Task Send_WhenValidatorThrows_HandlerIsNotInvoked()
    {
        var mediator = BuildMediator();
        var request = new PingRequest { Name = "" };

        await Assert.ThrowsAsync<UseCaseValidationException>(() => mediator.Send(request));

        Assert.Contains(nameof(PingValidator), request.Trace);
        Assert.DoesNotContain(nameof(PingHandler), request.Trace);
    }

    [Fact]
    public async Task Send_WhenPipeIsOrderedAfterHandler_ReturnsResponseWithoutInvokingIt()
    {
        var mediator = BuildMediator();
        var request = new PingRequest { Name = "world" };

        var response = await mediator.Send(request);

        Assert.Equal("hello world", response.Message);
        Assert.DoesNotContain(nameof(PingAfterHandlerPipe), request.Trace);
    }

    [Fact]
    public async Task Send_WhenNoHandlerIsRegistered_ThrowsConfigurationException()
    {
        var mediator = BuildMediator();

        await Assert.ThrowsAsync<UseCaseConfigurationException>(() => mediator.Send(new UnhandledRequest()));
    }

    [Fact]
    public async Task Send_WhenPipesShareAnOrder_RunsThemInRegistrationOrder()
    {
        var services = new ServiceCollection();
        services.AddUseCasePipeline(Array.Empty<System.Reflection.Assembly>());
        services.AddTransient<IUseCasePipe<SharedOrderRequest>, SecondSharedOrderPipe>();
        services.AddTransient<IUseCasePipe<SharedOrderRequest>, FirstSharedOrderPipe>();
        services.AddTransient<IUseCasePipe<SharedOrderRequest>, SharedOrderAuthoriser>();
        var mediator = services.BuildServiceProvider().GetRequiredService<UseCaseMediator>();
        var request = new SharedOrderRequest();

        await mediator.Send(request);

        Assert.Equal(
            new[] { nameof(SharedOrderAuthoriser), nameof(SecondSharedOrderPipe), nameof(FirstSharedOrderPipe) },
            request.Trace);
    }

    [Fact]
    public async Task Send_WhenAnAssemblyIsRegisteredTwice_RunsEachPipeOnce()
    {
        var services = new ServiceCollection();
        services.AddUseCasePipeline(typeof(PipelineTests).Assembly);
        services.AddUseCasePipeline(typeof(PipelineTests).Assembly);
        var mediator = services.BuildServiceProvider().GetRequiredService<UseCaseMediator>();
        var request = new PingRequest { Name = "world" };

        await mediator.Send(request);

        Assert.Equal(
            new[] { nameof(PingAuthoriser), nameof(PingValidator), nameof(PingHandler) },
            request.Trace);
    }
}
