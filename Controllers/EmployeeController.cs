using ClosedXML.Excel;
using EmpAppApi.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Route("api/[controller]")]
[ApiController]
public class EmployeesController : ControllerBase
{
    private readonly EmployeeDbContext _context;

    public EmployeesController(EmployeeDbContext context)
    {
        _context = context;
    }

    [HttpPost]
    public async Task<IActionResult> AddEmployee([FromBody] Employee employee)
    {
        if (employee == null) return BadRequest("Invalid employee data.");
        _context.Employees.Add(employee);
        await _context.SaveChangesAsync();
        return Ok(employee);
    }

    [HttpGet]
    public async Task<IActionResult> GetAllEmployees()
    {
        var employees = await _context.Employees.ToListAsync();
        return Ok(employees);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteEmployee(int id)
    {
        var employee = await _context.Employees.FindAsync(id);
        if (employee == null)
        {
            return NotFound();
        }

        _context.Employees.Remove(employee);
        await _context.SaveChangesAsync();

        return NoContent();  // 204 No Content response
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateEmployee(int id, [FromBody] Employee employee)
    {
        if (id != employee.EmployeeId)
        {
            return BadRequest("Employee ID mismatch.");
        }

        _context.Entry(employee).State = EntityState.Modified;
        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!_context.Employees.Any(e => e.EmployeeId == id))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }
        return NoContent();
    }

    [HttpPost("AssignWork")]
    public async Task<IActionResult> AssignWork([FromBody] AssignWorkRequest request)
    {
        var employee = await _context.Employees.FindAsync(request.EmployeeId);

        if (employee == null)
        {
            return NotFound(new { message = "Employee not found." });
        }

        // Ensure AssignedWork is not null before assigning work
        if (employee.AssignedWork == null)
        {
            employee.AssignedWork = request.Work;
        }
        else
        {
            employee.AssignedWork = request.Work ?? employee.AssignedWork; // Use existing work if new work is null
        }

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while assigning the work.", error = ex.Message });
        }

        return Ok(new { message = "Work assigned successfully!" });
    }

    // Export Employee Data to Excel
    [HttpGet("ExportToExcel")]
    public async Task<IActionResult> ExportToExcel()
    {
        var employees = await _context.Employees.ToListAsync();
        using (var workbook = new XLWorkbook())
        {
            var worksheet = workbook.AddWorksheet("Employee Report");
            worksheet.Cell(1, 1).Value = "Employee ID";
            worksheet.Cell(1, 2).Value = "Employee Name";
            worksheet.Cell(1, 3).Value = "Assigned Work";

            for (int i = 0; i < employees.Count; i++)
            {
                worksheet.Cell(i + 2, 1).Value = employees[i].EmployeeId;
                worksheet.Cell(i + 2, 2).Value = $"{employees[i].FirstName} {employees[i].LastName}";
                worksheet.Cell(i + 2, 3).Value = employees[i].AssignedWork ?? "N/A";
            }

            using (var stream = new MemoryStream())
            {
                workbook.SaveAs(stream);
                var content = stream.ToArray();
                return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "EmployeeReport.xlsx");
            }
        }
    }

    public class AssignWorkRequest
    {
        public int EmployeeId { get; set; }
        public string Work { get; set; }
    }
}
