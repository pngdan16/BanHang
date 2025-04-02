namespace BanHang.Models
{
  /// <summary>
  /// Model đại diện cho giỏ hàng của người dùng
  /// </summary>
  public class ShoppingCart
  {
    public List<CartItem> Items { get; set; } = new List<CartItem>();

    //Add Item 
    public void AddItem(CartItem item)
    {
      var existingItem = Items.FirstOrDefault(i => i.ProductId == item.ProductId);
      if (existingItem != null)
      {
        existingItem.Quantity += item.Quantity;
      }
      else
      {
        Items.Add(item);
      }
    }
    //Remove Item
    public void RemoveItem(int productId)
    {
      var item = Items.FirstOrDefault(i => i.ProductId == productId);
      if (item != null)
      {
        Items.Remove(item);
      }
    }
    //Cập nhật số lượng sản phẩm trong giỏ hàng 
    public bool UpdateQuantity(int productId, int quantity)
    {
      var item = Items.FirstOrDefault(i => i.ProductId == productId);
      if (item != null)
      {
        item.Quantity = quantity;
        return true;
      }
      return false;
    }
    //Lấy tổng tiền 
    public decimal GetTotalPrice()
    {
      return Items.Sum(i => i.Price * i.Quantity);
    }
    //Lấy tổng số lượng sản phẩm trong giỏ hàng 
    public int GetTotalQuantity()
    {
      return Items.Sum(i => i.Quantity);
    }
    //Tính tổng tiền của giỏ hàng 
    public decimal GetTotalAmount()
    {
      return Items.Sum(i => i.Price * i.Quantity);
    }
    //Kiểm tra xem giỏ hàng còn trống không 
    public bool IsEmpty()
    {
      return !Items.Any();
    }
    //Xóa toàn bộ sản phẩm trong giỏ hàng 
    public void Clear()
    {
      Items.Clear();
    }
    /// <summary>
    /// Tìm một sản phẩm trong giỏ hàng
    /// </summary>
    /// <param name="productId">ID của sản phẩm cần tìm</param>
    /// <returns>CartItem nếu tìm thấy, null nếu không tìm thấy</returns>
    public CartItem FindItem(int productId)
    {
      return Items.FirstOrDefault(i => i.ProductId == productId);
    }
  }
}
