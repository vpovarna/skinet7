using API.Dtos;
using API.Errors;
using AutoMapper;
using Core.Entities;
using Core.Interfaces;
using Core.Specifications;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class ProductsController : BaseApiController
{
    private readonly IGenericRepository<Product> _productsRepository;
    private readonly IGenericRepository<ProductBrand> _productBrandsRepository;
    private readonly IGenericRepository<ProductType> _productTypesRepository;
    private readonly IMapper _mapper;

    public ProductsController(
        IGenericRepository<Product> productsRepository,
        IGenericRepository<ProductBrand> productBrandsRepository,
        IGenericRepository<ProductType> productTypesRepository,
        IMapper mapper
    )
    {
        _productsRepository = productsRepository;
        _productBrandsRepository = productBrandsRepository;
        _productTypesRepository = productTypesRepository;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<Pagination<ProductToReturnDto>>> GetProducts(
        [FromQuery] ProductSpecParams productParams)
    {
        var spec = new ProductsWithTypesAndBrandsSpecification(productParams);
        var countSpec = new ProductsWithFiltersForCountSpecification(productParams);
        var totalItems = await _productsRepository.CountAsync(countSpec);
        var products = await _productsRepository.ListAsync(spec);
        var data = _mapper.Map<IReadOnlyList<Product>, IReadOnlyList<ProductToReturnDto>>(products);
        return Ok(new Pagination<ProductToReturnDto>(productParams.PageIndex, productParams.PageSize, totalItems,
            data));
    }

    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductToReturnDto>> GetProduct(int id)
    {
        var spec = new ProductsWithTypesAndBrandsSpecification(id);
        var product = await _productsRepository.GetEntityWithSpec(spec);
        if (product == null) return NotFound(new ApiResponse(404));
        return _mapper.Map<Product, ProductToReturnDto>(product);
    }

    [HttpGet("brands")]
    public async Task<ActionResult<IReadOnlyList<ProductBrand>>> GetProductBrands()
    {
        return Ok(await _productBrandsRepository.ListAllAsync());
    }

    [HttpGet("types")]
    public async Task<ActionResult<IReadOnlyList<ProductType>>> GetProductTypes()
    {
        return Ok(await _productTypesRepository.ListAllAsync());
    }
}