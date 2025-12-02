using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using DamslaApi.Services;
using DamslaApi.Dtos.Auth;
using System.Security.Claims;

namespace DamslaApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _auth;

        public AuthController(AuthService auth)
        {
            _auth = auth;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var token = await _auth.Login(dto);

            if (token == null)
                return Unauthorized("Credenciales incorrectas");

            return Ok(new { token });
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            var user = await _auth.Register(dto);

            if (user == null)
                return BadRequest("El correo ya está registrado o el rol no existe.");

            return Ok(new
            {
                message = "Usuario creado exitosamente",
                user = new { user.Id, user.Nombre, user.Email }
            });
        }

        [Authorize]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword(ChangePasswordDto dto)
        {
            // Obtener el ID del usuario del token JWT
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                return Unauthorized("Token inválido");

            var success = await _auth.ChangePassword(userId, dto);

            if (!success)
                return BadRequest("La contraseña actual es incorrecta");

            return Ok(new { message = "Contraseña cambiada exitosamente" });
        }
    }
}