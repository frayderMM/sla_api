using Microsoft.AspNetCore.Mvc;
using DamslaApi.Services;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace DamslaApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UploadController : ControllerBase
    {
        private readonly ExcelService _excelService;

        public UploadController(ExcelService excelService)
        {
            _excelService = excelService;
        }

        // SOLO ROL ANALISTA
        [Authorize(Roles = "analista")]
        [HttpPost("excel")]
        public async Task<IActionResult> UploadExcel(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Debe subir un archivo Excel");

            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            using var stream = file.OpenReadStream();
            var errores = await _excelService.ImportarExcel(stream, userId);

            return Ok(new
            {
                message = "Archivo procesado correctamente",
                errores
            });
        }
    }
}