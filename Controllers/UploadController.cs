using Microsoft.AspNetCore.Mvc;
using DamslaApi.Services;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

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
        [RequestSizeLimit(30_000_000)] // 30 MB
        [RequestFormLimits(MultipartBodyLengthLimit = 30_000_000)]
        public async Task<IActionResult> UploadExcel([FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { success = false, message = "Debe subir un archivo Excel" });

            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            using var stream = file.OpenReadStream();
            var errores = await _excelService.ImportarExcel(stream, userId);

            return Ok(new
            {
                success = true,
                message = "Archivo procesado correctamente",
                errores
            });
        }
    }
}