using Microsoft.AspNetCore.Mvc;
using StockAPI.Data;
using StockAPI.Models;

namespace StockAPI.Controllers;

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
    // GET: /api/product
    [HttpGet]
    public ActionResult<Product> GetProduct()
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

        var products = _context.Product.Join(
            _context.Category,
            p => p.CategoryID,
            c => c.CategoryID,
            (p, c) => new {
                p.ProductID,
                p.ProductName,
                p.UnitPrice,
                p.UnitInStock,
                p.CategoryID,
                p.ModifiedDate,
                c.CategoryName
            }
        ).ToList();
        
        // ส่งข้อมูลกลับไปในรูปแบบของ JSON
        return Ok(products);
    }

    // ฟังก์ชันสำหรับการดึงข้อมูล Product ตาม ID
    // GET: /api/product/1
    [HttpGet("{id}")]
    public ActionResult<Product> GetProductById(int id)
    {
        // LINQ สำหรับการดึงข้อมูลจากตาราง Product ตาม ID
        var product = _context.Product.Find(id); // select * from Product where ProductID = id

        // ส่งข้อมูลกลับไปในรูปแบบของ JSON
        return Ok(product);
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
    public Task<ActionResult<Product>> UpdateProduct(int id, [FromForm] Product product)
    {
         // ดึงข้อมูลสินค้าตาม id
        var existingProduct = _context.Product.FirstOrDefault(p => p.ProductID == id);

        // ถ้าไม่พบข้อมูลจะแสดงข้อความ Not Found
        if (existingProduct == null)
        {
            return Task.FromResult<ActionResult<Product>>(NotFound());
        }

        // แก้ไขข้อมูลสินค้า
        existingProduct.ProductName = product.ProductName;
        existingProduct.UnitPrice = product.UnitPrice;
        existingProduct.UnitInStock = product.UnitInStock;
        existingProduct.CategoryID = product.CategoryID;
        existingProduct.ModifiedDate = product.ModifiedDate;

        // บันทึกข้อมูลลงในฐานข้อมูล
        _context.SaveChanges();

        // ส่งข้อมูลกลับไปในรูปแบบของ JSON
        return Task.FromResult<ActionResult<Product>>(Ok(existingProduct));
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

        // ลบข้อมูลสินค้า
        _context.Product.Remove(existingProduct);

        // บันทึกข้อมูลลงในฐานข้อมูล
        _context.SaveChanges();

        // ส่งข้อมูลกลับไปในรูปแบบของ JSON
        return Ok(existingProduct);
    }

}