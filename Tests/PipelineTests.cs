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
}
