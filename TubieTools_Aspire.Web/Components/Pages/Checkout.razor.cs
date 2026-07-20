using Microsoft.AspNetCore.Components;
using TubieTools_Aspire.Web.Models;
using TubieTools_Aspire.Web.Services;

namespace TubieTools_Aspire.Web.Components.Pages
{
    /// <summary>
    /// Checkout component for processing payments via Authorize.Net
    /// </summary>
    public partial class Checkout : ComponentBase
    {
        [Inject]
        private IPaymentService? PaymentService { get; set; }

        [Inject]
        private CartService? CartService { get; set; }

        [Inject]
        private NavigationManager? NavigationManager { get; set; }

        [Inject]
        private ILogger<Checkout>? Logger { get; set; }

        [Parameter]
        public string? ReturnUrl { get; set; }

        private Order? CurrentOrder { get; set; }
        private PaymentRequest PaymentRequest { get; set; } = new();
        private bool IsProcessing { get; set; } = false;
        private bool ShowPaymentForm { get; set; } = true;
        private string PaymentMessage { get; set; } = string.Empty;
        private bool IsPaymentError { get; set; } = false;
        private PaymentResponse? LastPaymentResponse { get; set; }

        protected override async Task OnInitializedAsync()
        {
            try
            {
                if (CartService == null)
                {
                    PaymentMessage = "Cart service not available";
                    IsPaymentError = true;
                    return;
                }

                var cartItems = await CartService.GetCartItemsAsync();
                if (!cartItems.Any())
                {
                    if (NavigationManager != null)
                    {
                        NavigationManager.NavigateTo("/shop", replace: true);
                    }
                    return;
                }

                // Initialize order from cart
                CurrentOrder = new Order
                {
                    OrderId = Guid.NewGuid().ToString(),
                    CreatedDate = DateTime.UtcNow,
                    ModifiedDate = DateTime.UtcNow
                };

                // Map cart items to order items
                foreach (var item in cartItems)
                {
                    CurrentOrder.OrderItems.Add(new OrderItem
                    {
                        ProductId = item.ProductId,
                        ProductName = item.ProductName,
                        ProductDescription = item.ProductDescription ?? string.Empty,
                        UnitPrice = item.Price,
                        Quantity = item.Quantity
                    });
                }

                // Initialize payment request
                PaymentRequest = new PaymentRequest
                {
                    OrderId = CurrentOrder.OrderId,
                    Amount = CurrentOrder.TotalAmount,
                    Description = $"Order {CurrentOrder.OrderId}",
                    InvoiceNumber = CurrentOrder.OrderId
                };
            }
            catch (Exception ex)
            {
                Logger?.LogError(ex, "Error initializing checkout");
                PaymentMessage = "Error loading checkout. Please try again.";
                IsPaymentError = true;
            }
        }

        private async Task HandlePaymentSubmitAsync()
        {
            if (!ValidatePaymentForm())
            {
                return;
            }

            IsProcessing = true;
            IsPaymentError = false;
            PaymentMessage = "Processing payment...";

            try
            {
                if (CurrentOrder == null || PaymentService == null)
                {
                    PaymentMessage = "Order or payment service not available";
                    IsPaymentError = true;
                    IsProcessing = false;
                    return;
                }

                PaymentRequest.OrderId = CurrentOrder.OrderId;
                PaymentRequest.Amount = CurrentOrder.TotalAmount;
                PaymentRequest.InvoiceNumber = CurrentOrder.OrderId;

                LastPaymentResponse = await PaymentService.ProcessPaymentAsync(PaymentRequest);

                if (LastPaymentResponse.IsSuccessful)
                {
                    // Update order status
                    CurrentOrder.PaymentStatus = PaymentStatus.Approved;
                    CurrentOrder.TransactionId = LastPaymentResponse.TransactionId;
                    CurrentOrder.ModifiedDate = DateTime.UtcNow;

                    // Clear cart
                    if (CartService != null)
                    {
                        await CartService.ClearCartAsync();
                    }

                    PaymentMessage = $"Payment successful! Transaction ID: {LastPaymentResponse.TransactionId}";
                    IsPaymentError = false;
                    ShowPaymentForm = false;

                    // Redirect to success page after delay
                    await Task.Delay(2000);
                    if (NavigationManager != null)
                    {
                        NavigationManager.NavigateTo($"/order-confirmation/{CurrentOrder.OrderId}");
                    }
                }
                else
                {
                    CurrentOrder.PaymentStatus = PaymentStatus.Declined;
                    PaymentMessage = $"Payment declined: {LastPaymentResponse.ResponseText}";
                    IsPaymentError = true;
                    Logger?.LogWarning($"Payment declined for order {CurrentOrder.OrderId}: {LastPaymentResponse.ResponseText}");
                }
            }
            catch (Exception ex)
            {
                if (CurrentOrder != null)
                {
                    CurrentOrder.PaymentStatus = PaymentStatus.Error;
                }
                PaymentMessage = $"Payment processing error: {ex.Message}";
                IsPaymentError = true;
                Logger?.LogError(ex, $"Payment processing error for order {CurrentOrder?.OrderId}");
            }
            finally
            {
                IsProcessing = false;
            }
        }

        private bool ValidatePaymentForm()
        {
            if (string.IsNullOrWhiteSpace(PaymentRequest.CustomerName))
            {
                PaymentMessage = "Please enter customer name";
                IsPaymentError = true;
                return false;
            }

            if (string.IsNullOrWhiteSpace(PaymentRequest.CustomerEmail))
            {
                PaymentMessage = "Please enter customer email";
                IsPaymentError = true;
                return false;
            }

            if (string.IsNullOrWhiteSpace(PaymentRequest.BillingAddress))
            {
                PaymentMessage = "Please enter billing address";
                IsPaymentError = true;
                return false;
            }

            if (string.IsNullOrWhiteSpace(PaymentRequest.BillingCity))
            {
                PaymentMessage = "Please enter billing city";
                IsPaymentError = true;
                return false;
            }

            if (string.IsNullOrWhiteSpace(PaymentRequest.BillingState))
            {
                PaymentMessage = "Please enter billing state";
                IsPaymentError = true;
                return false;
            }

            if (string.IsNullOrWhiteSpace(PaymentRequest.BillingZip))
            {
                PaymentMessage = "Please enter billing ZIP code";
                IsPaymentError = true;
                return false;
            }

            if (string.IsNullOrWhiteSpace(PaymentRequest.DataValue) || string.IsNullOrWhiteSpace(PaymentRequest.DataDescriptor))
            {
                PaymentMessage = "Please complete card information";
                IsPaymentError = true;
                return false;
            }

            return true;
        }

        private void CancelCheckout()
        {
            if (NavigationManager != null)
            {
                NavigationManager.NavigateTo("/cart");
            }
        }
    }
}
