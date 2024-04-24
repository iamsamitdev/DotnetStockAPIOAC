using System.Data;
using System.Dynamic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using StockAPI.Data;
using StockAPI.Models;

namespace StockAPI.Controllers;

// Admin and Manager can access this controller
// [Authorize(Roles = UserRolesModel.Admin + "," + UserRolesModel.Manager)]
[ApiController] // กำหนดให้ Class นี้เป็น API Controller
[Route("api/[controller]")] // กำหนด Route ของ API Controller

public class ProductController: ControllerBase
{
    // สร้าง Object ของ ApplicationDbContext
    private readonly ApplicationDbContext _context;

    // สร้าง Object อ่าน Path ของไฟล์
    private readonly IWebHostEnvironment _env;

    // สร้าง Constructor รับค่า ApplicationDbContext เข้ามา
    public ProductController(
        ApplicationDbContext context, 
        IWebHostEnvironment env)
    {
        _context = context;
        _env = env;
    }

    // ฟังก์ชันสำหรับการดึงข้อมูล Product ทั้งหมด
    // GET: /api/product?page=1&limit=100
    [HttpGet]
    public ActionResult<Product> GetProduct(
        [FromQuery] int page=1, 
        [FromQuery] int limit=100,
        [FromQuery] string? searchQuery=null,
        [FromQuery] int? categoryID=null
    )
    {
        

        // LINQ สำหรับการดึงข้อมูลจากตาราง Product ทั้งหมด
        // var products = _context.Product.ToList(); // select * from Product

        // LINQ สำหรับการดึงข้อมูลจากตาราง Product ระบุเฉพาะ Column ที่ต้องการ
        // var products = _context.Product.Select(
        //     p => new {
        //         p.ProductName,
        //         p.ProductPrice
        //     }
        // ).ToList();

        // LINQ สำหรับการดึงข้อมูลจากตาราง Product กำหนดเงื่อนไข
        // select * from Product where ProductStatus = 1 and ProductName = 'iPhone 12'
        // var products = _context.Product.Select(
        //     p => new {
        //         p.ProductName,
        //         p.ProductPrice
        //     }
        // ).Where(
        //     p => p.ProductStatus == 1 && p.ProductName == "iPhone 12"
        // ).ToList();

        // แบบเชื่อมกับตารางอื่น product เชื่อมกับ category แบบ inner join

        // Read from views in database
        var query = _context.Product.Join(
            _context.Category,
            p => p.CategoryID,
            c => c.CategoryID,
            (p, c) => new {
                p.ProductID,
                p.ProductName,
                p.UnitPrice,
                p.UnitInStock,
                p.CategoryID,
                p.ProductPicture,
                p.ModifiedDate,
                c.CategoryName
            }
        );

        // ตรวจสอบว่ามีการค้นหาข้อมูลหรือไม่
        if(!string.IsNullOrEmpty(searchQuery)){
            query = query.Where(
                p => EF.Functions.Like(
                    p.ProductName!, $"%{searchQuery}%"
                )
            );
        }

        // ตรวจสอบว่ามีการค้นหาข้อมูลตาม CategoryID หรือไม่
        if(categoryID.HasValue){
            query = query.Where(
                p => p.CategoryID == categoryID
            );
        }

        // ดึงข้อมูลจาก query และแปลงเป็น List
        var products = query
        .OrderByDescending(p => p.ProductID)
        .Skip((page - 1) * limit) // ข้ามข้อมูล
        .Take(limit) // จำกัดจำนวนข้อมูล
        .ToList();

        // คำนวณจำนวนข้อมูลทั้งหมด
        var totalRecords = products.Count();
        
        // ส่งข้อมูลกลับไปในรูปแบบของ JSON
        return Ok(new {
            Total = totalRecords,
            data = products,
        });
    }

    // ฟังก์ชันสำหรับการดึงข้อมูล Product ตาม ID
    // GET: /api/product/1
    [HttpGet("{id}")]
    public ActionResult<Product> GetProductById(int id)
    {
        // LINQ สำหรับการดึงข้อมูลจากตาราง Product ตาม ID
        // var product = _context.Product.Join(
        //     _context.Category,
        //     p => p.CategoryID,
        //     c => c.CategoryID,
        //     (p, c) => new {
        //         p.ProductID,
        //         p.ProductName,
        //         p.UnitPrice,
        //         p.UnitInStock,
        //         p.CategoryID,
        //         p.ProductPicture,
        //         p.ModifiedDate,
        //         c.CategoryName
        //     }
        // )
        // .FirstOrDefault(p => p.ProductID == id);
        
        // Raw SQL Query
        // var product = _context.Product.FromSqlRaw(
        //     "SELECT p.ProductID, p.ProductName, p.UnitPrice, p.UnitInStock, p.CategoryID, p.ProductPicture, p.CreatedDate, p.ModifiedDate, c.CategoryName FROM Product p INNER JOIN Category c ON p.CategoryID = c.CategoryID WHERE p.ProductID = {0}",
        // id).FirstOrDefault();

        // Read from view
        // var product = _context.ProductDetails.FromSqlRaw("SELECT * FROM vw_ProductDetails WHERE ProductID = {0}", id).FirstOrDefault();

        // Read from views in database
        // using (var command = _context.Database.GetDbConnection().CreateCommand())
        // {
        //     command.CommandText = "SELECT * FROM vw_ProductDetails WHERE ProductID = @productId";
        //     command.Parameters.Add(new SqlParameter("@productId", id));
        //     _context.Database.OpenConnection();
            
        //     using (var result = command.ExecuteReader())
        //     {
        //         var list = new List<dynamic>();
        //         while (result.Read())
        //         {
        //             dynamic item = new ExpandoObject();
        //             for (int i = 0; i < result.FieldCount; i++)
        //             {
        //                 ((IDictionary<string, object>)item).Add(result.GetName(i), result[i]);
        //             }
        //             list.Add(item);
        //         }
        //         return Ok(list);
        //     }
        // }

        // Call Stored Procedure
        var param = new SqlParameter("@ProductID", id);

        var product = _context.Product
            .FromSqlRaw("EXEC GetProductDetails @ProductID", param)
            .AsEnumerable()
            .FirstOrDefault();

        // using (var command = _context.Database.GetDbConnection().CreateCommand())
        // {
        //     command.CommandText = "EXEC GetProductDetails @ProductID";
        //     command.CommandType = CommandType.Text;
        //     command.Parameters.Add(new SqlParameter("@ProductID", id));

        //     _context.Database.OpenConnection();
        //     using (var result = command.ExecuteReader())
        //     {
        //         if(result.Read()){
        //             var product = new {
        //                 ProductID = result.GetInt32(0),
        //                 ProductName = result.GetString(1),
        //                 UnitPrice = result.GetDecimal(2),
        //                 UnitInStock = result.GetInt32(3),
        //                 CategoryID = result.GetInt32(4),
        //                 ProductPicture = result.GetString(5),
        //                 CreatedDate = result.GetDateTime(6),
        //                 ModifiedDate = result.GetDateTime(7),
        //                 CategoryName = result.GetString(8)
        //             };

        //             return Ok(product);
        //         }
        //     }
        // }
                
        // ส่งข้อมูลกลับไปในรูปแบบของ JSON
        return Ok(product);
        // return NotFound();
    }

    // ฟังก์ชันสำหรับการเพิ่มข้อมูล Product
    // POST: /api/product
    [HttpPost]
    public async Task<ActionResult<Product>> AddProduct([FromForm] Product product, IFormFile? image)
    {
        // เพิ่มข้อมูลลงในตาราง Product
        _context.Product.Add(product);

        // ตรวจสอบว่ามีการอัพโหลดไฟล์รูปภาพหรือไม่
        if(image != null){
            // กำหนดชื่อไฟล์รูปภาพใหม่
            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);

            // บันทึกไฟล์รูปภาพลงในโฟลเดอร์ wwwroot/uploads
            string uploadFolder = Path.Combine(_env.WebRootPath, "uploads");

            // ตรวจสอบว่าโฟลเดอร์ uploads ไม่มีอยู่ให้สร้างโฟลเดอร์
            if(!Directory.Exists(uploadFolder)){
                Directory.CreateDirectory(uploadFolder);
            }

            // Upload ไฟล์รูปภาพ
            using (var fileStream = new FileStream(Path.Combine(uploadFolder, fileName), FileMode.Create))
            {
                await image.CopyToAsync(fileStream);
            }

            // บันทึกชื่อไฟล์รูปภาพลงในฐานข้อมูล
            product.ProductPicture = fileName;
        } else {
            product.ProductPicture = "noimg.jpg";
        }

        _context.SaveChanges();

        // ส่งข้อมูลกลับไปในรูปแบบของ JSON
        return Ok(product);
    }

    // ฟังก์ชันสำหรับการแก้ไขข้อมูล Product
    // PUT /api/Product/1
    [HttpPut("{id}")]
    public async Task<ActionResult<Product>> UpdateProduct(int id, [FromForm] Product product, IFormFile? image)
    {
         // ดึงข้อมูลสินค้าตาม id
        var existingProduct = _context.Product.FirstOrDefault(p => p.ProductID == id);

        // ถ้าไม่พบข้อมูลจะแสดงข้อความ Not Found
        if (existingProduct == null)
        {
            return NotFound();
        }

        // แก้ไขข้อมูลสินค้า
        existingProduct.ProductName = product.ProductName;
        existingProduct.UnitPrice = product.UnitPrice;
        existingProduct.UnitInStock = product.UnitInStock;
        existingProduct.CategoryID = product.CategoryID;
        existingProduct.ModifiedDate = product.ModifiedDate;

        // ตรวจสอบว่ามีการอัพโหลดไฟล์รูปภาพหรือไม่
        if(image != null){
            // กำหนดชื่อไฟล์รูปภาพใหม่
            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);

            // บันทึกไฟล์รูปภาพ
            string uploadFolder = Path.Combine(_env.WebRootPath, "uploads");

            // ตรวจสอบว่าโฟลเดอร์ uploads มีหรือไม่
            if (!Directory.Exists(uploadFolder))
            {
                Directory.CreateDirectory(uploadFolder);
            }

            using (var fileStream = new FileStream(Path.Combine(uploadFolder, fileName), FileMode.Create))
            {
                await image.CopyToAsync(fileStream);
            }

            // ลบไฟล์รูปภาพเดิม ถ้ามีการอัพโหลดรูปภาพใหม่ และรูปภาพเดิมไม่ใช่ noimg.jpg
            if(existingProduct.ProductPicture != "noimg.jpg"){
                System.IO.File.Delete(
                    Path.Combine(uploadFolder, existingProduct.ProductPicture!)
                );
            }

            // บันทึกชื่อไฟล์รูปภาพลงในฐานข้อมูล
            existingProduct.ProductPicture = fileName;
        }

        // บันทึกข้อมูลลงในฐานข้อมูล
        _context.SaveChanges();

        // ส่งข้อมูลกลับไปในรูปแบบของ JSON
        return Ok(existingProduct);
    }

    // ฟังก์ชันสำหรับการลบข้อมูล Product
    // DELETE /api/Product/1
    [HttpDelete("{id}")]
    public ActionResult<Product> DeleteProduct(int id)
    {
        // ดึงข้อมูลสินค้าตาม id
        var existingProduct = _context.Product.FirstOrDefault(p => p.ProductID == id);

        // ถ้าไม่พบข้อมูลจะแสดงข้อความ Not Found
        if (existingProduct == null)
        {
            return NotFound();
        }

        // ตรวจสอบว่ามีไฟล์รูปภาพหรือไม่
        if(existingProduct.ProductPicture != "noimg.jpg"){
            // string uploadFolder = Path.Combine(_env.ContentRootPath, "uploads");
            string uploadFolder = Path.Combine(_env.WebRootPath, "uploads");

            // ลบไฟล์รูปภาพ
            System.IO.File.Delete(Path.Combine(uploadFolder, existingProduct.ProductPicture!));
        }

        // ลบข้อมูลสินค้า
        _context.Product.Remove(existingProduct);

        // บันทึกข้อมูลลงในฐานข้อมูล
        _context.SaveChanges();

        // ส่งข้อมูลกลับไปในรูปแบบของ JSON
        return Ok(existingProduct);
    }

}