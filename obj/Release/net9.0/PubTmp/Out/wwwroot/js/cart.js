// Hàm cập nhật số lượng sản phẩm trong giỏ hàng
function updateCartItemCount() {
    fetch('/ShoppingCart/GetCartItemCount')
        .then(response => response.json())
        .then(data => {
            const cartBadge = document.getElementById('cartItemCount');
            if (cartBadge) {
                if (data > 0) {
                    cartBadge.textContent = data;
                    cartBadge.style.display = 'inline';
                } else {
                    cartBadge.style.display = 'none';
                }
            }
        })
        .catch(error => console.error('Error updating cart count:', error));
}

// Cập nhật khi trang được tải
document.addEventListener('DOMContentLoaded', function() {
    updateCartItemCount();
}); 