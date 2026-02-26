using AutoMapper;
using Business.Data;
using Business.Logic.AuthLogic;
using Core.Common.Constans;
using Core.Dtos.Auth;
using Core.Dtos.User;
using Core.Entities;
using Core.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EcomerceApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AuthResponse _response;
        private readonly IMapper _mapper;
        private readonly IJwtGenerator _jwtGenerator;
        private readonly IGenericRepository<User> _userRepository;
        private readonly EcomerceDbContext _context;

        public AuthController(AuthResponse response, IMapper mapper, IJwtGenerator jwtGenerator, IGenericRepository<User> userRepository, EcomerceDbContext context)
        {
            _response = response;
            _mapper = mapper;
            _jwtGenerator = jwtGenerator;
            _userRepository = userRepository;
            _context = context;
        }

        [HttpPost("Login")]
        [AllowAnonymous]
        public async Task<AuthResponse> SignIn(LoginDto request)
        {
            try
            {
                var result = await _context.User.Include(u => u.Profile).FirstAsync(u => u.Email == request.Email);
                
                if (result is null)
                {
                    _response.Message = "Credenciales incorrectas";
                    _response.statusCode = 404;
                    return _response;
                }
                
                var passwordHasher = new PasswordHasher<User>();
                var isPasswordValid = passwordHasher.VerifyHashedPassword(result, result.Password, request.Password);

                if (isPasswordValid == PasswordVerificationResult.Failed)
                {
                    _response.Message = "Credenciales incorrectas";
                    _response.statusCode = 404;
                    return _response;
                }
                
                _response.DataObject = _mapper.Map<UserDto>(result);
                string jwtToken = _jwtGenerator.GenerateToken(result);
                _response.Token = jwtToken;
            }
            catch (Exception ex)
            {
                _response.statusCode = 500;
                _response.Message = string.Concat(ex.Message, ex.InnerException, ex.StackTrace);
                throw;
            }
            
            return _response;
        }
        
        [HttpPost("Register")]
        [AllowAnonymous]
        public async Task<AuthResponse> Register(RegisterDto request)
        {
            try
            {
                var passwordHasher = new PasswordHasher<User>();

                var customerProfile = await _context.Profile.FirstAsync(p => p.Title == EndlezConstants.CustomerRole);
                
                if (customerProfile is null)
                {
                    _response.Message = "Error de servidor";
                    _response.statusCode = 500;
                    return _response;
                }
                
                var entity = new User
                {
                    Email = request.Email,
                    Password = request.Password,
                    Name = request.Name,
                    LastName = request.LastName,
                    CreatedDate = DateTime.UtcNow,
                    ProfileId = customerProfile.Id,
                    Phone = request.Phone
                };
                
                entity.Password = passwordHasher.HashPassword(entity, entity.Password);

                var result = await _userRepository.Insert(entity);
                
                if (result is null)
                {
                    _response.Message = "No se pudo registrar el usuario";
                    _response.statusCode = 404;
                    return _response;
                }
                
                _response.Message = "Usuario registrado con éxito";
                _response.statusCode = 200;
            }
            catch (Exception ex)
            {
                _response.statusCode = 500;
                _response.Message = string.Concat(ex.Message, ex.InnerException, ex.StackTrace);
                throw;
            }
            
            return _response;
        }

        [HttpGet("token")]
        public async Task<AuthResponse> CheckToken()
        {
            try
            {
                var userId = User.FindFirst("id")!.Value;
                var user = await _context.User
                    .Include(u => u.Profile)
                    .FirstOrDefaultAsync(u => u.Id == Guid.Parse(userId));

                if (user is null)
                {
                    _response.Message = "No existe el usuario con los datos proporcionados";
                    _response.statusCode = 404;
                    return _response;
                }

                _response.DataObject = _mapper.Map<UserDto>(user);
            }
            catch (Exception ex)
            {
                _response.statusCode = 500;
                _response.Message = string.Concat(ex.Message, ex.InnerException, ex.StackTrace);
                throw;
            }
            
            return _response;
        }
    }
}
