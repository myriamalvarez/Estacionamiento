using Estacionamiento.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Estacionamiento.Servicios
{
    // Hash de contraseñas (PBKDF2 + sal) y generación del JWT. Lo usan tanto el login de la web
    // (UsuarioController) como el de la API (ApiAuthController), por eso vive en una clase aparte
    // en vez de duplicarse en cada controller.
    public class ServicioSeguridad
    {
        private const int Iteraciones = 100_000;
        private const int LargoHashBytes = 32; // 256 bits

        private readonly IConfiguration _configuration;

        public ServicioSeguridad(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string HashearPassword(string password)
        {
            var salt = _configuration["Salt"]
                ?? throw new InvalidOperationException("Falta configurar \"Salt\" en appsettings.json.");

            var hash = Rfc2898DeriveBytes.Pbkdf2(
                password: Encoding.UTF8.GetBytes(password),
                salt: Encoding.UTF8.GetBytes(salt),
                iterations: Iteraciones,
                hashAlgorithm: HashAlgorithmName.SHA256,
                outputLength: LargoHashBytes);

            return Convert.ToBase64String(hash);
        }

        public bool VerificarPassword(string password, string hashAlmacenado) =>
            HashearPassword(password) == hashAlmacenado;

        public string GenerarToken(Usuario usuario)
        {
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration["TokenAuthentication:SecretKey"]!));
            var credenciales = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // NameIdentifier guarda el Id: ControladorBase lo usa para saber quién está logueado.
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                new(ClaimTypes.Name, usuario.NombreUsuario),
                new("FullName", $"{usuario.Nombre} {usuario.Apellido}"),
                new(ClaimTypes.Role, usuario.Rol)
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["TokenAuthentication:Issuer"],
                audience: _configuration["TokenAuthentication:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(8),
                signingCredentials: credenciales);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}