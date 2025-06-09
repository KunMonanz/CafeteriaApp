using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

public class Cart
{
    [Key]
    public int CartId { get; set; }

    [Required]
    public string? UserId { get; set; }
    
    public ICollection<CartItem>    Items { get; set; } = new List<CartItem>();
}
