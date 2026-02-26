using AutoMapper;
using Business.Logic.ProfileLogic;
using Core.Dtos.Profile;
using Core.Interface;
using Core.Specification;
using Microsoft.AspNetCore.Mvc;
using Profile = Core.Entities.Profile;

namespace EcomerceApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProfileController : ControllerBase
    {
        private readonly IGenericRepository<Profile> _profileRepository;
        private readonly ProfileResponse _response;
        private readonly IMapper _mapper;

        public ProfileController(
            IGenericRepository<Profile> profileRepository,
            ProfileResponse response,
            IMapper mapper)
        {
            _profileRepository = profileRepository;
            _response = response;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<ProfileResponse>> GetAll()
        {
            try
            {
                var spec = new ProfileSpecification();
                var profiles = await _profileRepository.GetAllWhitSpec(spec);
                _response.ListDataObject = _mapper.Map<List<ProfileDto>>(profiles);
            }
            catch (Exception ex)
            {
                _response.statusCode = 500;
                _response.Message = string.Concat(ex.Message, ex.InnerException?.Message, ex.StackTrace);
            }

            return Ok(_response);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ProfileResponse>> GetById(Guid id)
        {
            try
            {
                var spec = new ProfileSpecification();
                var profiles = await _profileRepository.GetAllWhitSpec(spec);
                var profile = profiles.FirstOrDefault(p => p.Id == id);
                _response.DataObject = _mapper.Map<ProfileDto>(profile);

                if (profile == null)
                {
                    _response.statusCode = 404;
                    _response.Message = "El perfil no se encontró.";
                }
            }
            catch (Exception ex)
            {
                _response.statusCode = 500;
                _response.Message = string.Concat(ex.Message, ex.InnerException?.Message, ex.StackTrace);
            }

            return Ok(_response);
        }

        [HttpPost]
        public async Task<ActionResult<ProfileResponse>> Post(ProfileCreateDto profile)
        {
            try
            {
                var newProfile = _mapper.Map<Profile>(profile);
                var result = await _profileRepository.Insert(newProfile);
                _response.DataObject = _mapper.Map<ProfileDto>(result);
            }
            catch (Exception ex)
            {
                _response.statusCode = 500;
                _response.Message = string.Concat(ex.Message, ex.InnerException?.Message, ex.StackTrace);
            }

            return Ok(_response);
        }

        [HttpPut]
        public async Task<ProfileResponse> Update(ProfileUpdateDto profile)
        {
            try
            {
                var existingProfile = await _profileRepository.GetByGuidAsync(profile.Id);
                if (existingProfile == null)
                {
                    _response.statusCode = 404;
                    _response.Message = "El perfil no se encontró.";
                    return _response;
                }

                var updatedProfile = _mapper.Map(profile, existingProfile);
                _profileRepository.Update(updatedProfile);
                await _profileRepository.SaveAsync();

                _response.Message = "El perfil se actualizó con éxito.";
            }
            catch (Exception ex)
            {
                _response.statusCode = 500;
                _response.Message = string.Concat(ex.Message, ex.InnerException?.Message, ex.StackTrace);
            }

            return _response;
        }

        [HttpDelete("{id}")]
        public async Task<ProfileResponse> Delete(Guid id)
        {
            try
            {
                await _profileRepository.Delete(id);
                _response.Message = "El perfil se eliminó con éxito.";
            }
            catch (Exception ex)
            {
                _response.statusCode = 500;
                _response.Message = string.Concat(ex.Message, ex.InnerException?.Message, ex.StackTrace);
            }

            return _response;
        }
    }
}
