# TubieTools_Aspire Solution Architecture

## High-Level System Overview

```
┌─────────────────────────────────────────────────────────────────┐
│                    TubieTools_Aspire Solution                   │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  ┌──────────────────────┐  ┌──────────────────────┐             │
│  │   TubieTools_Map     │  │  TubieTools_Web      │             │
│  │  (Blazor Server)     │  │  (Blazor Server)     │             │
│  │  Port: 7264          │  │  Port: 7263          │             │
│  └──────────┬───────────┘  └──────────┬───────────┘             │
│             │                         │                        │
│             └───────────┬─────────────┘                        │
│                         │                                      │
│            ┌────────────▼────────────┐                         │
│            │   AppHost (Orchestrator) │                         │
│            │  (Port: 5000, 5001)     │                         │
│            └────────────┬────────────┘                         │
│                         │                                      │
│     ┌───────────────────┼───────────────────┐                 │
│     │                   │                   │                 │
│  ┌──▼──────┐  ┌────────▼──────┐  ┌────────▼──┐               │
│  │  SQL DB  │  │  ASP.NET Core  │  │  Services  │              │
│  │ (MapApp) │  │   API & Auth   │  │            │              │
│  └──────────┘  └────────────────┘  └────────────┘              │
│                                                                 │
│  ┌──────────────────────────────────────────────────────┐     │
│  │    TubieTools_Aspire.Tests (MSTest Suite)           │      │
│  │  ├─ PaymentIntegrations/                            │      │
│  │  │  ├─ PayPalPaymentServiceTests                   │      │
│  │  │  ├─ GooglePayPaymentServiceTests                │      │
│  │  │  ├─ ApplePayPaymentServiceTests                 │      │
│  │  │  ├─ AuthorizeNetPaymentServiceTests             │      │
│  │  │  ├─ PaymentWebhookIntegrationTests              │      │
│  │  │  └─ PaymentServiceTestBase (Shared Fixture)    │      │
│  └──────────────────────────────────────────────────────┘     │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

---

## Project Structure

### 1. **TubieTools_Map** (Blazor Server Web App)
**Purpose:** Logistics & Map Management UI  
**Port:** 7264  
**Stack:** .NET 10.0, Blazor Server, Entity Framework Core

#### Components:
```
TubieTools_Map/
├── Program.cs
│   └── Blazor Server configuration
│       ├── AddServerSideBlazor()
│       ├── MapBlazorHub()
│       └── MapFallbackToPage("/_Host")
│
├── Pages/
│   └── _Host.cshtml
│       ├── HTML Document Shell
│       ├── <component type="typeof(App)" render-mode="ServerPrerendered" />
│       └── Leaflet JS Integration
│
├── Components/
│   ├── App.razor
│   │   └── Root Blazor Component → <Routes />
│   │
│   ├── Routes.razor
│   │   ├── Router with AppAssembly
│   │   ├── CascadingAuthenticationState
│   │   ├── AuthorizeRouteView
│   │   └── FocusOnNavigate
│   │
│   └── Layout/
│       ├── MainLayout.razor (Primary Layout)
│       └── MainLayout.razor.css
│
├── Services/
│   ├── MapService.cs
│   └── LocationService.cs
│
├── Models/
│   ├── MapMarker.cs
│   ├── RoutePoint.cs
│   └── Location.cs
│
└── wwwroot/
	├── css/
	│   ├── app.css
	│   └── site.css
	├── js/
	│   ├── leaflet.js (External)
	│   └── mapManager.js
	└── lib/
```

---

### 2. **TubieTools_Web** (Blazor Server Web App)
**Purpose:** Core Web Application  
**Port:** 7263  
**Stack:** .NET 10.0, Blazor Server, EF Core, Identity

#### Components:
```
TubieTools_Web/
├── Program.cs
│   ├── Blazor Server configuration
│   ├── Identity & Authentication
│   └── Database initialization
│
├── Pages/
│   ├── _Host.cshtml
│   ├── Identity Pages (Login, Register, etc.)
│   └── Error.cshtml
│
├── Components/
│   ├── App.razor (Root)
│   ├── Routes.razor (Routing)
│   └── Layout/
│       └── MainLayout.razor
│
├── Services/
│   ├── AuthService.cs
│   └── UserService.cs
│
├── Models/
│   ├── User.cs
│   └── ApplicationUser.cs
│
└── Data/
	└── ApplicationDbContext.cs
```

---

### 3. **TubieTools_Aspire.Web** (Shared Business Logic)
**Purpose:** Shared Models, Services, and Payment Integration  
**Stack:** .NET 10.0 Class Library

#### Structure:
```
TubieTools_Aspire.Web/
├── Models/
│   ├── Payment Models/
│   │   ├── PaymentRequest.cs
│   │   │   ├── Amount
│   │   │   ├── Currency
│   │   │   ├── PaymentMethod
│   │   │   ├── PaymentToken
│   │   │   └── LineItems[]
│   │   │
│   │   ├── PaymentResponse.cs
│   │   │   ├── IsSuccessful
│   │   │   ├── TransactionId
│   │   │   ├── OrderId
│   │   │   ├── ErrorMessage
│   │   │   └── CustomerProfileId
│   │   │
│   │   ├── Order.cs
│   │   │   ├── OrderId
│   │   │   ├── CustomerId
│   │   │   ├── OrderItems[]
│   │   │   ├── Payments[]
│   │   │   ├── BillingAddress
│   │   │   ├── ShippingAddress
│   │   │   ├── Subtotal
│   │   │   └── TotalAmount
│   │   │
│   │   ├── OrderItem.cs
│   │   │   ├── ProductId
│   │   │   ├── Quantity
│   │   │   ├── UnitPrice
│   │   │   └── LineTotal
│   │   │
│   │   ├── Payment.cs
│   │   │   ├── Amount
│   │   │   ├── PaymentToken
│   │   │   ├── TransactionId
│   │   │   ├── Status (Pending, Completed, Failed)
│   │   │   └── CreatedDate
│   │   │
│   │   ├── PaymentSettings.cs
│   │   │   ├── ApiKey
│   │   │   ├── ApiSecret
│   │   │   ├── Environment (Sandbox/Production)
│   │   │   └── Timeout
│   │   │
│   │   └── PaymentStatus.cs (Enum)
│   │       ├── Pending
│   │       ├── Completed
│   │       └── Failed
│   │
│   ├── Address.cs
│   │   ├── Street
│   │   ├── City
│   │   ├── State
│   │   ├── ZipCode
│   │   └── Country
│   │
│   ├── Customer.cs
│   │   ├── CustomerId
│   │   ├── Name
│   │   ├── Email
│   │   └── Phone
│   │
│   └── Location.cs
│
├── Services/
│   ├── IPaymentService.cs (Interface)
│   │   ├── ProcessPaymentAsync()
│   │   ├── RefundPaymentAsync()
│   │   ├── ValidatePaymentAsync()
│   │   └── GetTransactionStatusAsync()
│   │
│   ├── PaymentService.cs (Base Implementation)
│   │   ├── Service locator / routing logic
│   │   └── Common payment operations
│   │
│   ├── Providers/
│   │   ├── IPayPalPaymentService.cs
│   │   ├── PayPalPaymentService.cs
│   │   │   ├── API integration with PayPal REST API
│   │   │   ├── Create payment, approve, execute
│   │   │   └── Direct credit card processing
│   │   │
│   │   ├── IGooglePayPaymentService.cs
│   │   ├── GooglePayPaymentService.cs
│   │   │   ├── Token validation
│   │   │   └── Transaction processing
│   │   │
│   │   ├── IApplePayPaymentService.cs
│   │   ├── ApplePayPaymentService.cs
│   │   │   ├── Token validation
│   │   │   └── Transaction processing
│   │   │
│   │   ├── IAuthorizeNetPaymentService.cs
│   │   └── AuthorizeNetPaymentService.cs
│   │       ├── CIM (Customer Information Manager)
│   │       ├── AIM (Advanced Integration Method)
│   │       └── Webhook handling
│   │
│   ├── IPaymentServiceFactory.cs
│   ├── PaymentServiceFactory.cs
│   │   └── Routes to correct provider based on PaymentMethod
│   │
│   ├── IWebhookService.cs
│   ├── WebhookService.cs
│   │   ├── Validate webhook signatures
│   │   ├── Process payment callbacks
│   │   └── Update order status
│   │
│   └── DatabaseContext/
│       └── MapAppDbContext.cs
│           ├── DbSet<Order>
│           ├── DbSet<OrderItem>
│           ├── DbSet<Payment>
│           ├── DbSet<Customer>
│           └── OnModelCreating() (seed data)
│
└── Controllers/
	├── PaymentController.cs
	│   ├── POST /api/payments/process
	│   ├── POST /api/payments/refund
	│   ├── GET /api/payments/{id}
	│   ├── POST /api/payments/validate
	│   └── POST /api/webhooks/payment (webhook endpoint)
	│
	└── OrderController.cs
		├── GET /api/orders/{id}
		├── POST /api/orders
		└── GET /api/orders
```

---

### 4. **TubieTools_Aspire.Tests** (Integration Test Suite)
**Purpose:** Validate payment provider integrations with sandbox credentials  
**Framework:** MSTest  
**Configuration:** Shared DI container, sandbox payment settings

#### Structure:
```
TubieTools_Aspire.Tests/
├── PaymentIntegrations/
│   ├── PaymentServiceTestBase.cs (Shared Fixture)
│   │   ├── [TestInitialize] Setup()
│   │   │   ├── Build DI container
│   │   │   ├── Register PaymentService + Providers
│   │   │   ├── Create sandbox PaymentSettings
│   │   │   └── Initialize HttpClient
│   │   │
│   │   ├── [TestCleanup] TearDown()
│   │   │   └── Dispose resources
│   │   │
│   │   ├── Helper: CreateTestPaymentRequest()
│   │   │   └── Returns pre-configured PaymentRequest with test data
│   │   │
│   │   ├── Helper: CreateTestOrder()
│   │   │   └── Returns Order with OrderItems and Payments array
│   │   │
│   │   ├── Helper: AssertPaymentSuccess()
│   │   │   └── Validates successful response
│   │   │
│   │   └── Helper: AssertPaymentFailure()
│   │       └── Validates failed response
│   │
│   ├── PayPalPaymentServiceTests.cs
│   │   ├── [TestMethod] TestProcessPaymentSuccess()
│   │   ├── [TestMethod] TestProcessPaymentFailure()
│   │   ├── [TestMethod] TestRefundPayment()
│   │   ├── [TestMethod] TestValidateToken()
│   │   └── [TestMethod] TestGetTransactionStatus()
│   │
│   ├── GooglePayPaymentServiceTests.cs
│   │   ├── [TestMethod] TestProcessPaymentSuccess()
│   │   ├── [TestMethod] TestTokenValidation()
│   │   ├── [TestMethod] TestRefund()
│   │   └── [TestMethod] TestInvalidToken()
│   │
│   ├── ApplePayPaymentServiceTests.cs
│   │   ├── [TestMethod] TestProcessPaymentSuccess()
│   │   ├── [TestMethod] TestTokenValidation()
│   │   └── [TestMethod] TestRefund()
│   │
│   ├── AuthorizeNetPaymentServiceTests.cs
│   │   ├── [TestMethod] TestCreateCustomerProfile()
│   │   ├── [TestMethod] TestCreatePaymentProfile()
│   │   ├── [TestMethod] TestChargeCard()
│   │   ├── [TestMethod] TestRefund()
│   │   └── [TestMethod] TestCIMOperations()
│   │
│   └── PaymentWebhookIntegrationTests.cs
│       ├── [TestMethod] TestPayPalIPNWebhook()
│       ├── [TestMethod] TestAuthorizeNetWebhook()
│       ├── [TestMethod] TestWebhookSignatureValidation()
│       └── [TestMethod] TestOrderStatusUpdate()
│
└── appsettings.json
	├── Sandbox API credentials
	├── Connection strings
	└── Test configuration
```

---

### 5. **AppHost** (Orchestrator/Service Discovery)
**Purpose:** Manage microservices, local development orchestration  
**Port:** 5000 (gRPC), 5001 (HTTP)  
**Stack:** .NET Aspire

#### Configuration:
```
AppHost/
├── Program.cs
│   ├── CreateDistributedApplicationBuilder()
│   ├── AddSqlServer() → MapApp Database
│   ├── AddProject("TubieTools_Web")
│   │   └── Port 7263
│   ├── AddProject("TubieTools_Map")
│   │   └── Port 7264
│   ├── AddProject("TubieTools_Forecasting_API")
│   │   └── Port 7262
│   └── Build().RunAsync()
│
└── appsettings.json
	├── Service endpoints
	├── Database connection strings
	└── Environment configuration
```

---

## Data Flow Diagram

### Payment Processing Flow

```
┌─────────────────────────────────────────────────────────────────┐
│ User Browser (Blazor UI)                                        │
└────────────────────┬────────────────────────────────────────────┘
					 │
					 │ 1. Enter payment details
					 │ 2. Select payment method
					 │
		┌────────────▼────────────┐
		│  TubieTools_Web/Map      │
		│  (Blazor Component)      │
		│  PaymentComponent.razor  │
		└────────────┬────────────┘
					 │
					 │ 3. POST /api/payments/process
					 │    { PaymentRequest }
					 │
		┌────────────▼────────────┐
		│  PaymentController       │
		│  (ASP.NET Core)          │
		│  ProcessPaymentAsync()   │
		└────────────┬────────────┘
					 │
					 │ 4. Route to provider
					 │
		┌────────────▼─────────────────────┐
		│  PaymentServiceFactory           │
		│  Resolve(PaymentMethod)          │
		└────────────┬─────────────────────┘
					 │
		┌────────────┴─────────────┬────────────┬─────────────┐
		│                          │            │             │
   ┌────▼────────┐   ┌────────────▼──┐  ┌──────▼─────┐  ┌────▼───────┐
   │   PayPal    │   │   Google Pay   │  │ Apple Pay  │  │ Authorize  │
   │  Service    │   │    Service     │  │  Service   │  │   .Net     │
   │             │   │                │  │            │  │   Service  │
   │ HTTP Client │   │  HTTP Client   │  │ HTTP Client│  │ HTTP Client│
   │ to API      │   │  to API        │  │ to API     │  │ to API     │
   └────┬────────┘   └────────┬───────┘  └───┬────────┘  └────┬───────┘
		│                     │              │                │
		│ 5. Call provider    │              │                │
		│    API              │              │                │
		│                     │              │                │
   ┌────▼─────────┐   ┌─────▼─────────┐ ┌──▼──────────┐ ┌───▼────────┐
   │ PayPal REST  │   │ Google Pay    │ │ Apple Pay  │ │ Authorize  │
   │ API          │   │ API           │ │ API        │ │ .Net API   │
   │ Sandbox      │   │ Sandbox       │ │ Sandbox    │ │ Sandbox    │
   └────┬─────────┘   └─────┬─────────┘ └──┬─────────┘ └───┬────────┘
		│                   │              │                │
		│ 6. Response       │              │                │
		│                   │              │                │
		└────────┬──────────┴──────────┬───┴────────────┬───┘
				 │                    │                │
		┌────────▼────────────────────▼────────────────▼──┐
		│  PaymentResponse (Success/Failure)              │
		│  ├─ TransactionId                              │
		│  ├─ OrderId                                    │
		│  ├─ IsSuccessful                               │
		│  └─ ErrorMessage (if failed)                   │
		└────────┬──────────────────────────────────────┘
				 │
				 │ 7. Save transaction
				 │ 8. Update order status
				 │
		┌────────▼──────────────────┐
		│  MapAppDbContext          │
		│  ├─ Orders                │
		│  ├─ OrderItems            │
		│  ├─ Payments              │
		│  └─ Customers             │
		│                           │
		│  SQL Server Database      │
		└───────────────────────────┘
				 │
				 │ 9. Return response
				 │
		┌────────▼──────────────────┐
		│  Blazor UI                │
		│  Show success/error       │
		│  Redirect to order        │
		└───────────────────────────┘
```

---

## Webhook/Callback Flow

```
┌──────────────────────┐
│  Payment Provider    │
│  (PayPal, Auth.Net)  │
│                      │
│  Event occurs:       │
│  -Payment captured   │
│  -Refund processed   │
│  -Dispute filed      │
└──────────┬───────────┘
		   │
		   │ 1. HTTPS POST
		   │    Signed payload
		   │
		┌──▼──────────────────────────┐
		│  /api/webhooks/payment       │
		│  PaymentController           │
		│  HandleWebhookAsync()        │
		└──┬───────────────────────────┘
		   │
		   │ 2. Validate signature
		   │
		┌──▼──────────────────────────┐
		│  WebhookService              │
		│  ValidateSignature()         │
		│  ✓ Authentic / ✗ Rejected    │
		└──┬───────────────────────────┘
		   │
		   │ 3. Parse & process
		   │
		┌──▼──────────────────────────┐
		│  Parse provider event JSON   │
		│  Extract:                    │
		│  - TransactionId             │
		│  - OrderId                   │
		│  - Status                    │
		│  - Timestamp                 │
		└──┬───────────────────────────┘
		   │
		   │ 4. Update database
		   │
		┌──▼──────────────────────────┐
		│  MapAppDbContext             │
		│  Update Payment.Status       │
		│  Update Order.Status         │
		└──┬───────────────────────────┘
		   │
		   │ 5. Send notification
		   │
		┌──▼──────────────────────────┐
		│  NotificationService         │
		│  Send email to customer      │
		│  Notify order system         │
		└──────────────────────────────┘
```

---

## Authentication Flow

```
┌──────────────────┐
│  User Browser    │
└────────┬─────────┘
		 │
		 │ 1. Navigate to app
		 │    https://localhost:7263
		 │
	┌────▼──────────────┐
	│  _Host.cshtml      │
	│  (Razor Page)      │
	│  Serve HTML        │
	└────┬───────────────┘
		 │
		 │ 2. Load Blazor
		 │
	┌────▼──────────────┐
	│  App.razor         │
	│  Routes.razor      │
	└────┬───────────────┘
		 │
		 │ 3. Check auth status
		 │
	┌────▼──────────────────────────┐
	│  AuthenticationStateProvider   │
	│  (Blazor Server Auth)          │
	└────┬───────────────────────────┘
		 │
	┌────┴──────────────┐
	│                   │
	│ Authenticated?    │
	│                   │
	NO                  YES
	│                   │
	│            ┌──────▼─────────┐
	│            │  MainLayout     │
	│            │  NavigationUI   │
	│            │  Routes: Allow  │
	│            │  AuthorizeRouteView
	│            └─────────────────┘
	│
	└──────────┬──────────────┐
			   │              │
		 ┌─────▼─────┐    ┌───▼────────┐
		 │ LoginPage │    │ Register   │
		 │ Form      │    │ Page       │
		 └─────┬─────┘    └───┬────────┘
			   │              │
			   │ POST /login  │
			   │              │
		 ┌─────┴──────────────┘
		 │
	┌────▼──────────────────┐
	│  Identity System       │
	│  (ASP.NET Core)        │
	│  Validate credentials  │
	└────┬───────────────────┘
		 │
		 │ Create Auth Cookie
		 │ or Token
		 │
	┌────▼──────────────────┐
	│  Browser Cookie       │
	│  HttpOnly, Secure     │
	└────┬───────────────────┘
		 │
		 │ Return to app
		 │
	┌────▼──────────────────┐
	│  Authenticated ✓       │
	│  Can access Routes     │
	│  [Authorize] enforced  │
	└───────────────────────┘
```

---

## Database Schema

```
┌──────────────────────────────────┐
│     MapAppDbContext              │
│     SQL Server Database          │
└──────────────────────────────────┘

┌─────────────────────┐
│   Customers         │
├─────────────────────┤
│ * CustomerId (PK)   │
│  Name               │
│  Email              │
│  Phone              │
│  CreatedDate        │
└──────────┬──────────┘
		   │ 1:N
		   │
		┌──▼──────────────────┐
		│   Orders            │
		├─────────────────────┤
		│ * OrderId (PK)      │
		│ + CustomerId (FK)   │
		│  OrderDate          │
		│  Status             │
		│  Subtotal           │
		│  TotalAmount        │
		│  BillingAddressId   │
		│  ShippingAddressId  │
		│  CreatedDate        │
		└──┬─────────┬────────┘
		   │         │
		1:N│         │1:N
		   │         │
	  ┌────▼──┐  ┌───▼──────────┐
	  │Orders │  │  Payments    │
	  │Items  │  ├──────────────┤
	  ├───────┤  │ * PaymentId  │
	  │ * OII │  │ + OrderId FK │
	  │ + OId │  │  Amount      │
	  │  PId  │  │  Token       │
	  │  Qty  │  │  TransactionId
	  │  Price   │  Status      │
	  │  Total   │  CreatedDate │
	  └───────┘  └──────────────┘
						│
					1:1 │
						│
			  ┌─────────▼──────────┐
			  │  Addresses         │
			  ├────────────────────┤
			  │ * AddressId (PK)   │
			  │  Street            │
			  │  City              │
			  │  State             │
			  │  ZipCode           │
			  │  Country           │
			  └────────────────────┘
```

---

## Technology Stack Summary

| Component | Technology | Version | Purpose |
|-----------|-----------|---------|---------|
| **Web Framework** | ASP.NET Core Blazor Server | .NET 10.0 | Server-side rendering UI |
| **Database** | SQL Server | 2022+ | Data persistence |
| **ORM** | Entity Framework Core | Latest | Database mapping |
| **Auth** | ASP.NET Core Identity | Built-in | User authentication |
| **Testing** | MSTest | Latest | Integration tests |
| **Payment Providers** | REST APIs | Latest | Multiple payment gateways |
| **Frontend Maps** | Leaflet.js | 1.9.4 | Map/Logistics visualization |
| **Orchestration** | .NET Aspire | Latest | Service discovery & management |
| **Hosting** | Kestrel / IIS | .NET 10.0 | Web server runtime |

---

## Key Integration Points

### 1. **Payment Service Factory Pattern**
```csharp
IPaymentService service = factory.Resolve(paymentMethod);
// Routes to: PayPal, GooglePay, ApplePay, or AuthorizeNet
```

### 2. **Dependency Injection**
```csharp
services.AddScoped<IPaymentService, PaymentService>();
services.AddScoped<IPaymentServiceFactory, PaymentServiceFactory>();
services.AddScoped<PayPalPaymentService>();
services.AddScoped<GooglePayPaymentService>();
services.AddScoped<ApplePayPaymentService>();
services.AddScoped<AuthorizeNetPaymentService>();
```

### 3. **Configuration Management**
- `appsettings.json` → Production settings
- `appsettings.Development.json` → Dev/test overrides
- Sandbox payment credentials injected at runtime

### 4. **Webhooks Security**
- Signature validation on webhook endpoints
- HMAC verification per provider
- Provider-specific event parsing

---

## Deployment Architecture (Future)

```
┌─────────────────────────────────────────┐
│          Cloud Environment              │
│  (Azure / AWS / On-Premises)            │
├─────────────────────────────────────────┤
│                                         │
│  ┌──────────────────────────────┐      │
│  │  Azure App Service           │      │
│  │  (TubieTools_Web)            │      │
│  │  Port: 7263                  │      │
│  └──────────────────────────────┘      │
│                                         │
│  ┌──────────────────────────────┐      │
│  │  Azure App Service           │      │
│  │  (TubieTools_Map)            │      │
│  │  Port: 7264                  │      │
│  └──────────────────────────────┘      │
│                                         │
│  ┌──────────────────────────────┐      │
│  │  Azure Container Instances   │      │
│  │  (Background Services)       │      │
│  └──────────────────────────────┘      │
│                                         │
│  ┌──────────────────────────────┐      │
│  │  Azure SQL Database          │      │
│  │  (Geo-redundant backup)      │      │
│  └──────────────────────────────┘      │
│                                         │
│  ┌──────────────────────────────┐      │
│  │  Azure Key Vault             │      │
│  │  (Secrets & Certificates)    │      │
│  └──────────────────────────────┘      │
│                                         │
│  ┌──────────────────────────────┐      │
│  │  Azure CDN                   │      │
│  │  (Static files, leaflet.js)  │      │
│  └──────────────────────────────┘      │
│                                         │
└─────────────────────────────────────────┘
```

---

## Quick Reference: File Structure

```
TubieTools_Aspire/
├── TubieTools_Map/                          [Blazor Server - Map/Logistics]
├── TubieTools_Web/                          [Blazor Server - Core Web]
├── TubieTools_Aspire.Web/                   [Shared Business Logic & Services]
├── TubieTools_Aspire.Tests/                 [Integration Test Suite]
│   └── PaymentIntegrations/
│       ├── PaymentServiceTestBase.cs
│       ├── PayPalPaymentServiceTests.cs
│       ├── GooglePayPaymentServiceTests.cs
│       ├── ApplePayPaymentServiceTests.cs
│       ├── AuthorizeNetPaymentServiceTests.cs
│       └── PaymentWebhookIntegrationTests.cs
├── AppHost/                                 [.NET Aspire Orchestrator]
├── TubieTools_Forecasting_API/              [Forecasting Microservice]
└── .sln                                     [Solution file]
```

---

## Next Steps for Visio Document

Use this architecture document to create your Visio diagram with:

1. **Container Diagram** (Systems & Services)
2. **Component Diagram** (TubieTools_Aspire.Web classes & layer structure)
3. **Sequence Diagram** (Payment processing flow)
4. **Deployment Diagram** (Cloud architecture)
5. **ER Diagram** (Database schema)
6. **Class Diagram** (Key classes & relationships)

All structures and relationships are documented above for easy Visio translation.
