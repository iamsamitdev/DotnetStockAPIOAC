using System;
using System.Collections.Generic;

namespace StockAPI.Models;

public partial class Product
{
    public int ProductID { get; set; }

    public string? ProductName { get; set; }

    public decimal? UnitPrice { get; set; }

    public int? UnitInStock { get; set; }

    public string? ProductPicture { get; set; }

    public int? CategoryID { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? ModifiedDate { get; set; }
}
