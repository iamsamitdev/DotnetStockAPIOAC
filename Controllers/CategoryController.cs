using Microsoft.AspNetCore.Mvc;
using StockAPI.Data;
using StockAPI.Models;

namespace StockAPI.Controllers;

[ApiController] // กำหนดให้ Class นี้เป็น API Controller
[Route("api/[controller]")] // กำหนด Route ของ API Controller
public class CategoryController: ControllerBase
{
    // สร้าง Object ของ ApplicationDbContext
    private readonly ApplicationDbContext _context;

    // สร้าง Constructor รับค่า ApplicationDbContext เข้ามา
    public CategoryController(ApplicationDbContext context)
    {
        _context = context;
    }

    // ทดสอบเขียนฟังก์ชันการเชื่อมต่อ database
    [HttpGet("testconnectdb")]
    public void TestConnectDB()
    {
        // ทดสอบเชื่อมต่อ Database
        // ถ้าเชื่อมต่อได้จะแสดงข้อความ "Connected"
        if (_context.Database.CanConnect()){
            Response.WriteAsync("Connected");
        } else {
            Response.WriteAsync("Not Connected");
        }
    }

    // ฟังก์ชันสำหรับการดึงข้อมูล Category ทั้งหมด
    // GET: /api/category
    [HttpGet]
    public ActionResult<Category> GetCategory()
    {
        // LINQ สำหรับการดึงข้อมูลจากตาราง Category ทั้งหมด
        // var categories = _context.Category.ToList(); // select * from Category

        // LINQ สำหรับการดึงข้อมูลจากตาราง Category ระบุเฉพาะ Column ที่ต้องการ
        // var categories = _context.Category.Select(
        //     c => new {
        //         c.CategoryName,
        //         c.CategoryStatus
        //     }
        // ).ToList();

        // LINQ สำหรับการดึงข้อมูลจากตาราง Category กำหนดเงื่อนไข
        // select * from Category where CategoryStatus = 1 and CategoryName = 'Mobile'
        var categories = _context.Category.Select(
            c => new {
                c.CategoryName,
                c.CategoryStatus
            }
        ).Where(
            c => c.CategoryStatus == 1 && c.CategoryName == "Mobile"
        ).ToList();

        // ส่งข้อมูลกลับไปในรูปแบบของ JSON
        return Ok(categories);
    }

    // ฟังก์ชันสำหรับการดึงข้อมูล Category ตาม ID
    // GET: /api/category/1
    [HttpGet("{id}")]
    public ActionResult<Category> GetCategory(int id)
    {
        // LINQ สำหรับการดึงข้อมูลจากตาราง Category ตาม ID
        var category = _context.Category.Find(id);

        // ถ้าไม่พบข้อมูลจะแสดงข้อความ Not Found
        if (category == null)
        {
            return NotFound();
        }

        // ส่งข้อมูลกลับไปในรูปแบบของ JSON
        return Ok(category);
    }

}