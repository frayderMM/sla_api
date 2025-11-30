using DamslaApi.Data;
using DamslaApi.Dtos.Auth;
using DamslaApi.Models;
using DamslaApi.Utils;
using Microsoft.EntityFrameworkCore;

namespace DamslaApi.Services
{
    public class AuthService
    {
        private readonly DamslaDbContext _db;
        private readonly JwtService _jwt;

        public AuthService(DamslaDbContext db, JwtService jwt)
        {
            _db = db;
            _jwt = jwt;
        }

        public async Task<string?> Login(LoginDto dto)
        {
            var user = await _db.Usuarios
                .Include(r => r.Rol)
                .FirstOrDefaultAsync(x => x.Email == dto.Email);

            if (user == null)
                return null;

            if (!PasswordHasher.Verify(dto.Password, user.Password_Hash))
                return null;

            return _jwt.GenerateToken(user);
        }

        public async Task<Usuario?> Register(RegisterDto dto)
        {
            if (await _db.Usuarios.AnyAsync(x => x.Email == dto.Email))
                return null;

            // Validar que el rol existe
            var rolExiste = await _db.Roles.AnyAsync(r => r.Id == dto.RolId);
            if (!rolExiste)
                return null;

            var nuevo = new Usuario
            {
                Nombre = dto.Nombre,
                Email = dto.Email,
                Password_Hash = PasswordHasher.Hash(dto.Password),
                RolId = dto.RolId
            };

            _db.Usuarios.Add(nuevo);
            await _db.SaveChangesAsync();
            return nuevo;
        }
    }
}