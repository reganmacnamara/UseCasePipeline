# UseCasePipeline

A lightweight UseCase pipeline library for .NET 8+. Implement clean, structured use cases with built-in support for request validation, entity validation, authorisation, and a mediator — all wired up automatically via DI.

## Installation

```
dotnet add package UseCasePipeline
```

## Getting Started

### 1. Register the pipeline

In `Program.cs`, call `AddUseCasePipeline()` with no arguments. It automatically scans every assembly in your app that references this library:

```csharp
using UseCasePipeline.Extensions;
using UseCasePipeline.Middleware;

builder.Services.AddUseCasePipeline();

var app = builder.Build();

// Optional: maps pipeline exceptions to HTTP responses automatically
app.UseUseCasePipelineExceptionHandler();
```

### 2. Define a request and response

```csharp
using UseCasePipeline.Infrastructure;

public class CreateOrderRequest : IUseCaseRequest<CreateOrderResponse>
{
    public Guid CustomerId { get; set; }
    public List<Guid> ProductIds { get; set; } = [];
}

public class CreateOrderResponse : IUseCaseResponse
{
    public Guid OrderId { get; set; }
}
```

### 3. Implement the handler

```csharp
using UseCasePipeline.Infrastructure;

public class CreateOrderHandler : IUseCaseHandler<CreateOrderRequest, CreateOrderResponse>
{
    public async Task<CreateOrderResponse> Handle(CreateOrderRequest request, CancellationToken cancellationToken)
    {
        // core use case logic
        return new CreateOrderResponse { OrderId = Guid.NewGuid() };
    }
}
```

### 4. Add optional pipeline stages

All stages are optional. Register as many as you need per use case.

**Request Validator** — validates the incoming request data:

```csharp
public class CreateOrderRequestValidator : IUseCaseRequestValidator<CreateOrderRequest>
{
    public Task Validate(CreateOrderRequest request, CancellationToken cancellationToken)
    {
        if (request.ProductIds.Count == 0)
            throw new UseCaseValidationException("At least one product is required.");

        return Task.CompletedTask;
    }
}
```

**Entity Validator** — checks required entities exist before the handler runs:

```csharp
public class CreateOrderEntityValidator : IUseCaseEntityValidator<CreateOrderRequest>
{
    public async Task Validate(CreateOrderRequest request, CancellationToken cancellationToken)
    {
        var customer = await _repo.FindCustomerAsync(request.CustomerId);
        if (customer is null)
            throw new UseCaseEntityNotFoundException(nameof(Customer), request.CustomerId);
    }
}
```

**Authoriser** — enforces permissions:

```csharp
public class CreateOrderAuthoriser : IUseCaseAuthoriser<CreateOrderRequest>
{
    public Task Authorise(CreateOrderRequest request, CancellationToken cancellationToken)
    {
        if (!_currentUser.CanCreateOrders)
            throw new UseCaseAuthorisationException();

        return Task.CompletedTask;
    }
}
```

### 5. Send a request

Inject `UseCaseMediator` and call `Send`. The response type is inferred automatically:

```csharp
public class OrdersController(UseCaseMediator mediator) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create(CreateOrderRequest request, CancellationToken cancellationToken)
    {
        var response = await mediator.Send(request, cancellationToken);
        return Ok(response);
    }
}
```

## Pipeline Execution Order

For every `Send` call the pipeline runs stages in this order:

```
Authoriser(s) → Request Validator(s) → Entity Validator(s) → Handler
```

All stages are optional. Multiple implementations of the same stage are all executed in registration order.

## Exception Middleware

`UseUseCasePipelineExceptionHandler()` catches pipeline exceptions and maps them to HTTP responses:

| Exception | Status Code |
|---|---|
| `UseCaseValidationException` | `400 Bad Request` |
| `UseCaseEntityNotFoundException` | `404 Not Found` |
| `UseCaseAuthorisationException` | `403 Forbidden` |
| Any other exception | Propagates unhandled |

## Command-style Use Cases (no response)

For use cases that produce no result, implement `IUseCaseRequest` and `IUseCaseHandler<TRequest>` directly:

```csharp
public class DeleteOrderRequest : IUseCaseRequest
{
    public Guid OrderId { get; set; }
}

public class DeleteOrderHandler : IUseCaseHandler<DeleteOrderRequest>
{
    public async Task Handle(DeleteOrderRequest request, CancellationToken cancellationToken)
    {
        // delete logic
    }
}

// Send with no return value
await mediator.Send(new DeleteOrderRequest { OrderId = id });
```

## License

MIT
