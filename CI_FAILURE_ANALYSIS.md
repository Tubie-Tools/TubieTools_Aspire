# CI Failure Analysis

## Isolated Root Cause: `SetTenantContext` Not Implemented

### The Bug

`TenantContextAccessor.SetTenantContext` throws `NotImplementedException`:

```csharp
// TenantContext.cs — line 70
public void SetTenantContext(TenantContext tenantContext)
{
    throw new NotImplementedException();
}
```

`TenantResolverMiddleware` sets the context via **property assignment** instead of calling the method:

```csharp
// TenantResolverMiddleware.cs — line 73
tenantContextAccessor.TenantContext = tenantContext;  // ← wrong
```

But the tests verify the **method** was called:

```csharp
// TenantResolverMiddlewareTests.cs — line 257
_mockContextAccessor.Verify(ca => ca.SetTenantContext(It.IsAny<TenantContext>()), Times.Once);
```

### Why It Fails

| What tests mock | What middleware does | Result |
|---|---|---|
| `SetTenantContext(ctx)` | `TenantContext = ctx` (property set) | Moq sees 0 calls to `SetTenantContext` → `MockException` |

The Moq mock intercepts method calls, not property setters on a different interface member. So even though the property is set, the method is never called, and `Verify(..., Times.Once)` throws.

### The Fix

**Step 1** — Implement `SetTenantContext` in `TenantContextAccessor`:

```csharp
public void SetTenantContext(TenantContext tenantContext)
{
    TenantContext = tenantContext;
}
```

**Step 2** — Call `SetTenantContext` in the middleware:

```csharp
// Before
tenantContextAccessor.TenantContext = tenantContext;

// After
tenantContextAccessor.SetTenantContext(tenantContext);
```

### Tests Fixed by This Change

| Test | Failure reason |
|---|---|
| `InvokeAsync_WithValidTenant_SetsUpContext` | `SetTenantContext` never called |
| `InvokeAsync_BuildsCorrectFeatureFlags_ForFreeTier` | `SetTenantContext` never called |
| `InvokeAsync_BuildsCorrectFeatureFlags_ForProfessionalTier` | `SetTenantContext` never called |
| `InvokeAsync_WithQuotaExceeded_SetsContinuesButMarksInContext` | `SetTenantContext` never called |
| `InvokeAsync_WithValidQuota_MarksQuotaAsNotExceeded` | `SetTenantContext` never called |

### Broader Pattern in This PR

The same mismatch — **interface defines a method, implementation ignores it, middleware bypasses it** — appears in several other places in this codebase and accounts for the majority of the 19 failing tests. Each fix follows the same two-step pattern above: implement the method, then call it instead of going around it.
