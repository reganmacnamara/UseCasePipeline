# UseCasePipeline

A lightweight UseCase pipeline library for .NET 8+. Implement clean, structured use cases with built-in support for request validation, entity validation, rule enforcement, authorisation, custom pipes, and a mediator — all discovered and wired up automatically via DI. Every stage runs through one pipeline in an order you can configure, and each stage has a base class to inherit.

## Installation

```
dotnet add package UseCasePipeline
```

Upgrading from 1.x? See [Upgrading from 1.x](#upgrading-from-1x).

## Getting Started

### 1. Register the pipeline

In `Program.cs`, call `AddUseCasePipeline()` with no arguments. It scans every assembly your app references that references this library, loading any that aren't loaded yet:

```csharp
using UseCasePipeline.Extensions;
using UseCasePipeline.Middleware;

builder.Services.AddUseCasePipeline();

var app = builder.Build();

// Optional: maps pipeline exceptions to HTTP responses automatically
app.UseUseCasePipelineExceptionHandler();
```

To scan specific assemblies instead, see [Choosing which assemblies are scanned](#choosing-which-assemblies-are-scanned).

### 2. Define a request and response

```csharp
using UseCasePipeline.Services;

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

public class CreateOrderHandler : UseCaseHandler<CreateOrderRequest, CreateOrderResponse>
{
    public override async Task<CreateOrderResponse> Handle(CreateOrderRequest request, CancellationToken cancellationToken)
    {
        // core use case logic
        return new CreateOrderResponse { OrderId = Guid.NewGuid() };
    }
}
```

Every request needs a handler. If none is registered, `Send` throws `UseCaseConfigurationException`.

### 4. Add optional pipeline stages

All stages are optional. Register as many as you need per use case. The stage base classes are in `UseCasePipeline.Infrastructure`, and the exceptions they throw are in `UseCasePipeline.Exceptions`.

**Request Validator** — validates the incoming request data:

```csharp
public class CreateOrderRequestValidator : UseCaseRequestValidator<CreateOrderRequest>
{
    public override Task Validate(CreateOrderRequest request, CancellationToken cancellationToken)
    {
        if (request.ProductIds.Count == 0)
            throw new UseCaseValidationException("At least one product is required.");

        return Task.CompletedTask;
    }
}
```

**Entity Validator** — checks required entities exist before the handler runs:

```csharp
public class CreateOrderEntityValidator : UseCaseEntityValidator<CreateOrderRequest>
{
    public override async Task Validate(CreateOrderRequest request, CancellationToken cancellationToken)
    {
        var customer = await _repo.FindCustomerAsync(request.CustomerId);
        if (customer is null)
            throw new UseCaseEntityNotFoundException(nameof(Customer), request.CustomerId);
    }
}
```

**Authoriser** — enforces permissions:

```csharp
public class CreateOrderAuthoriser : UseCaseAuthoriser<CreateOrderRequest>
{
    public override Task Authorise(CreateOrderRequest request, CancellationToken cancellationToken)
    {
        if (!_currentUser.CanCreateOrders)
            throw new UseCaseAuthorisationException();

        return Task.CompletedTask;
    }
}
```

**Rule Enforcer** — enforces business rules after validation and before the handler runs:

```csharp
public class CreateOrderRuleEnforcer : UseCaseRuleEnforcer<CreateOrderRequest>
{
    public override Task Enforce(CreateOrderRequest request, CancellationToken cancellationToken)
    {
        if (!_orderingRules.CustomerCanPlaceOrder(request.CustomerId))
            throw new UseCaseRuleViolationException("Customer cannot place orders at this time.");

        return Task.CompletedTask;
    }
}
```

**Custom Pipe** — runs your own pipeline logic. By default it runs after the built-in authorisation, validation, entity validation, and rule enforcement stages, and before the handler:

```csharp
public class CreateOrderAuditPipe : UseCaseCustomPipe<CreateOrderRequest>
{
    public override Task InvokeAsync(CreateOrderRequest request, CancellationToken cancellationToken)
    {
        // custom pipeline logic, such as auditing, enrichment, or instrumentation
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

The mediator works out the pipeline call for each request type once and caches it, so `Send` doesn't use reflection after the first request of each type.

## Pipeline Execution Order

Every pipe registered for a request, including its handler, runs in ascending `Order`. Each base class sets a default, which gives this order:

```
Authoriser(s) -> Request Validator(s) -> Entity Validator(s) -> Rule Enforcer(s) -> Custom Pipe(s) -> Handler
```

| Stage | Base class | Default `Order` |
|---|---|---|
| Authoriser | `UseCaseAuthoriser<TRequest>` | `10` |
| Request Validator | `UseCaseRequestValidator<TRequest>` | `20` |
| Entity Validator | `UseCaseEntityValidator<TRequest>` | `30` |
| Rule Enforcer | `UseCaseRuleEnforcer<TRequest>` | `40` |
| Custom Pipe | `UseCaseCustomPipe<TRequest>` | `50` |
| Handler | `UseCaseHandler<TRequest>`, `UseCaseHandler<TRequest, TResponse>` | `60` |

- **Pipes with the same `Order`** run in registration order.
- **To move a pipe**, override `Order`. This custom pipe runs before the authoriser:

  ```csharp
  public class CreateOrderRateLimitPipe : UseCaseCustomPipe<CreateOrderRequest>
  {
      public override int Order { get; set; } = 5;

      public override Task InvokeAsync(CreateOrderRequest request, CancellationToken cancellationToken)
      {
          // rate limiting logic
          return Task.CompletedTask;
      }
  }
  ```

- **A class that implements a stage interface directly**, such as `IUseCaseAuthoriser<TRequest>`, instead of inheriting its base class, must declare `Order` itself. Use the defaults above to keep the standard order. A pipe left at `0` runs before every base-class stage.
- **For a request with a response**, the pipeline returns the handler's response as soon as it reaches the handler, so pipes ordered after the handler don't run.
- **For a command**, every pipe runs, including any ordered after the handler.

## Choosing which assemblies are scanned

| Call | Scans |
|---|---|
| `AddUseCasePipeline()` | Every assembly your app references that references UseCasePipeline, loading any that aren't loaded yet |
| `AddUseCasePipeline(params Assembly[] assemblies)` | Only the assemblies you pass |
| `AddUseCasePipeline(IUseCaseAssemblyLocator assemblyLocator)` | The assemblies your locator returns |

The scan follows the assemblies your app's code uses. If no code in your app uses a type from a project (for example a module wired up only through DI), pass its assembly explicitly:

```csharp
builder.Services.AddUseCasePipeline(typeof(CreateOrderHandler).Assembly);
```

To decide which assemblies are scanned yourself, implement `IUseCaseAssemblyLocator`:

```csharp
using System.Reflection;
using UseCasePipeline.Services;

public class ModuleAssemblyLocator : IUseCaseAssemblyLocator
{
    public IReadOnlyList<Assembly> LocateAssemblies()
        => [typeof(CreateOrderHandler).Assembly, typeof(SendInvoiceHandler).Assembly];
}

builder.Services.AddUseCasePipeline(new ModuleAssemblyLocator());
```

Calling `AddUseCasePipeline` more than once, or passing the same assembly twice, is safe: nothing is registered twice.

## Exception Middleware

`UseUseCasePipelineExceptionHandler()` catches pipeline exceptions and maps them to HTTP responses:

| Exception | Status Code |
|---|---|
| `UseCaseValidationException` | `400 Bad Request` |
| `UseCaseEntityNotFoundException` | `404 Not Found` |
| `UseCaseAuthorisationException` | `403 Forbidden` |
| `UseCaseRuleViolationException` | `400 Bad Request` |
| `UseCaseConfigurationException` (no handler registered) | Propagates unhandled |
| Any other exception | Propagates unhandled |

## Command-style Use Cases (no response)

For use cases that produce no result, implement `IUseCaseRequest` and inherit `UseCaseHandler<TRequest>`:

```csharp
public class DeleteOrderRequest : IUseCaseRequest
{
    public Guid OrderId { get; set; }
}

public class DeleteOrderHandler : UseCaseHandler<DeleteOrderRequest>
{
    public override async Task Handle(DeleteOrderRequest request, CancellationToken cancellationToken)
    {
        // delete logic
    }
}

// Send with no return value
await mediator.Send(new DeleteOrderRequest { OrderId = id });
```

A command needs a handler too: without one, `Send` throws `UseCaseConfigurationException` before any pipe runs. For a command that should do nothing, register a handler whose `Handle` returns `Task.CompletedTask`.

## Upgrading from 1.x

2.0 has breaking changes. These show up as compile errors:

- **Namespaces:** the interfaces moved from `UseCasePipeline.Infrastructure` to `UseCasePipeline.Services`. `UseCasePipeline.Infrastructure` now holds the stage base classes.
- **`Order`:** every pipe now has one, and a class that implements a stage or handler interface directly must declare it. The simplest fix is to inherit the matching base class, which keeps the 1.x order. If you declare `Order` yourself, use the defaults above; pipes all left at `0` run in registration order, so a validator or handler could run before an authoriser.
- **Classes with two stages:** a class that implements two stage interfaces for the same request must implement `InvokeAsync` itself, or be split into one class per stage.
- **`UseCaseMediator` constructor:** it now takes different dependencies. Resolve it from DI instead of creating it yourself.

These change behaviour without a compile error:

- **More pipes may run:** `AddUseCasePipeline()` now also finds assemblies that weren't loaded yet, so pipes the 1.x scan skipped will start running.
- **No double registration:** calling `AddUseCasePipeline` more than once no longer registers each pipe twice, so pipes that ran twice now run once.
- **Pipes after a response handler don't run:** for requests with a response, pipes ordered after the handler are skipped.
- **A missing handler throws a different exception:** `Send` now throws `UseCaseConfigurationException` instead of DI's `InvalidOperationException`. For commands, it throws before any pipe runs.

## License

MIT
