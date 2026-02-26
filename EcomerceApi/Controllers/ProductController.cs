using AutoMapper;
using Business.Logic.ProductLogic;
using Core.Dtos.Product;
using Core.Entities;
using Core.Interface;
using Core.Specification;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcomerceApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IGenericRepository<Product> _productRepository;
        private readonly IGenericRepository<ProductImage> _productImageRepository;
        private readonly ProductResponse _response;
        private IMapper _mapper;

        [HttpGet("{productId}/images")]
        [AllowAnonymous]
        public async Task<IActionResult> GetProductImages(Guid productId)
        {
            try
            {
                // Obtener todas las imágenes asociadas al producto
                var images = await _productImageRepository.GetAllAsync();
                var productImages = images.Where(img => img.ProductId == productId).ToList();
                return Ok(productImages);
            }
            catch (Exception ex)
            {
                return Conflict(new { Message = "Error al obtener las imágenes", Error = ex.Message });
            }
        }
        public ProductController(
            IGenericRepository<Product> productRepository,
            IGenericRepository<ProductImage> productImageRepository,
            ProductResponse response,
            IMapper mapper)
        {
            _productRepository = productRepository;
            _productImageRepository = productImageRepository;
            _response = response;
            _mapper = mapper;
        }
        [HttpPost("images")]
        public async Task<IActionResult> UploadImages(List<IFormFile> images, [FromQuery] Guid productId)
        {
            try
            {
                var product = await _productRepository.GetByGuidAsync(productId);
                if (product is null)
                    return NotFound("El producto no se encontró.");

                foreach (var image in images)
                {
                    if (image != null && image.Length > 0)
                    {
                        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(image.FileName)}";
                        var path = Path.Combine(Directory.GetCurrentDirectory(), "images", "products", fileName);

                        if (!Directory.Exists(Path.Combine(Directory.GetCurrentDirectory(), "images", "products")))
                            Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), "images", "products"));

                        using (var stream = new FileStream(path, FileMode.Create))
                            await image.CopyToAsync(stream);

                        var productImage = new ProductImage
                        {
                            Id = Guid.NewGuid(),
                            ProductId = productId,
                            FileName = fileName,
                            CreatedDate = DateTime.UtcNow
                        };
                        await _productImageRepository.Insert(productImage);
                    }
                }
                await _productImageRepository.SaveAsync();
                return Ok();
            }
            catch (Exception)
            {
                return Conflict("Error al subir las imágenes");
            }
        }
        
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<ProductResponse>> GetAll()
        {
            try
            {
                var products = await _productRepository.GetAllAsync();
                _response.ListDataObject = _mapper.Map<IReadOnlyList<ProductDto>>(products);
            }
            catch (Exception ex)
            {
                _response.statusCode = 500;
                _response.Message = string.Concat(ex.Message, ex.InnerException, ex.StackTrace);
            }

            return _response;
        }
        
        [HttpGet("{ProductId}")]
        [AllowAnonymous]
        public async Task<ActionResult<ProductResponse>> GetByGuidId(Guid ProductId)
        {
            try
            {
                var products = await _productRepository.GetAllAsync();
                var product = products.FirstOrDefault(c => c.Id == ProductId);
                _response.DataObject = _mapper.Map<ProductDto>(product);
            }
            catch (Exception ex)
            {
                _response.statusCode = 500;
                _response.Message = string.Concat(ex.Message, ex.InnerException, ex.StackTrace);
            }

            return _response;
        }
        
        [HttpGet("image/{imageName}")]
        [AllowAnonymous]
        public IActionResult GetProductImage(string imageName)
        {
            var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "images", "products", imageName);

            if (!System.IO.File.Exists(imagePath))
            {
                return NotFound(new { Message = "La imagen no se encontró." });
            }

            var image = System.IO.File.OpenRead(imagePath);
            return File(image, "image/jpeg");
        }

        [HttpPost("image")]
        public async Task<IActionResult> UploadImage(IFormFile image, [FromQuery] Guid productId)
        {
            try
            {
                var product = await _productRepository.GetByGuidAsync(productId);

                if (product is null)
                {
                    return NotFound("El producto no se encontró.");
                }

                if (image != null && image.Length > 0)
                {
                    // Eliminar la imagen antigua si existe
                    if (!string.IsNullOrEmpty(product.ImagePath))
                    {
                        var oldImagePath = Path.Combine(Directory.GetCurrentDirectory(), product.ImagePath);
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }

                    // Guardar la nueva imagen
                    var fileName = $"{Guid.NewGuid()}{Path.GetExtension(image.FileName)}";
                    var path = Path.Combine(Directory.GetCurrentDirectory(), "images", "products", fileName);

                    if (!Directory.Exists(Path.Combine(Directory.GetCurrentDirectory(), "images", "products")))
                    {
                        Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), "images", "products"));
                    }

                    using (var stream = new FileStream(path, FileMode.Create))
                    {
                        await image.CopyToAsync(stream);
                    }

                    product.ImagePath = fileName;
                    // Guardar el producto actualizado en la base de datos
                    _productRepository.Update(product);
                    await _productRepository.SaveAsync();
                }

                return Ok();
            }
            catch (Exception)
            {
                return Conflict("Error al subir la imagen");
            }
        } 

        [HttpPost]
        public async Task<ProductResponse> Post(ProductCreateDto product)
        {
            try
            {
                var _product = _mapper.Map<Product>(product);

                var result = await _productRepository.Insert(_product);
                _response.DataObject = _mapper.Map<ProductDto>(result);
            }
            catch (Exception ex)
            {
                _response.statusCode = 500;
                _response.Message = string.Concat(ex.Message, ex.InnerException, ex.StackTrace);
            }

            return _response;
        }

        [HttpPut]
        public async Task<ProductResponse> Update([FromBody] ProductUpdateDto product)
        {
            try
            {
                var existingProduct = await _productRepository.GetByGuidAsync(product.Id);
                if (existingProduct == null)
                {
                    _response.statusCode = 404;
                    _response.Message = "El Producto no se encontró.";
                    return _response;
                }

                var originalCreatedDate = existingProduct.CreatedDate;

                _mapper.Map(product, existingProduct);
                existingProduct.CreatedDate = originalCreatedDate;

                _productRepository.Update(existingProduct);
                await _productRepository.SaveAsync();

                _response.Message = "El cambio se realizó con éxito.";
            }
            catch (Exception ex)
            {
                _response.statusCode = 500;
                _response.Message = string.Concat(ex.Message, ex.InnerException, ex.StackTrace);
            }

            return _response;
        }


        [HttpDelete("{id}")]
        public async Task<ProductResponse> SoftDelete(Guid id)
        {
            try
            {
                await _productRepository.Delete(id);
                _response.Message = "El Producto se eliminó.";
            }
            catch (Exception ex)
            {
                _response.statusCode = 500;
                _response.Message = string.Concat(ex.Message, ex.InnerException, ex.StackTrace);
            }

            return _response;
        }
    }
}