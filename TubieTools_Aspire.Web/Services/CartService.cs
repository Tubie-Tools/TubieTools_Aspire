using TubieTools_Aspire.Web.Models;

namespace TubieTools_Aspire.Web.Services
{
    public class CartService
    {
        private List<CartItem> cartItems = new();

        public event Action OnCartChanged;

        public void AddToCart(Product product, int quantity)
        {
            var existingItem = cartItems.FirstOrDefault(x => x.ProductId == product.Id);

            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
            }
            else
            {
                cartItems.Add(new CartItem
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    Price = product.Price,
                    Quantity = quantity,
                    ImagePath = product.ImagePath
                });
            }

            NotifyCartChanged();
        }

        public void RemoveFromCart(int productId)
        {
            cartItems.RemoveAll(x => x.ProductId == productId);
            NotifyCartChanged();
        }

        public void UpdateQuantity(int productId, int quantity)
        {
            var item = cartItems.FirstOrDefault(x => x.ProductId == productId);
            if (item != null)
            {
                if (quantity <= 0)
                {
                    RemoveFromCart(productId);
                }
                else
                {
                    item.Quantity = quantity;
                    NotifyCartChanged();
                }
            }
        }

        public List<CartItem> GetCartItems() => cartItems;

        public int GetCartItemCount() => cartItems.Sum(x => x.Quantity);

        public decimal GetCartTotal() => cartItems.Sum(x => x.TotalPrice);

        public void ClearCart()
        {
            cartItems.Clear();
            NotifyCartChanged();
        }

        private void NotifyCartChanged()
        {
            OnCartChanged?.Invoke();
        }

        internal async Task ClearCartAsync()
        {
            throw new NotImplementedException();
        }

        internal async Task<IEnumerable<CartItem>> GetCartItemsAsync() => cartItems;
    }
}
