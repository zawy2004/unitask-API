using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Unitask.Application.Common.Interfaces;

namespace Unitask.Api.Controllers.Common;

[ApiController]
[Route("api/[controller]")]
public abstract class ReadOnlyController<TEntity, TDto> : ControllerBase
    where TEntity : class
    where TDto : class
{
    private readonly IGenericRepository<TEntity> _repository;
    private readonly IMapper _mapper;

    protected ReadOnlyController(IGenericRepository<TEntity> repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<TDto>>> GetAll()
    {
        var entities = await _repository.GetAllAsync();
        var dtos = _mapper.Map<IReadOnlyList<TDto>>(entities);

        return Ok(dtos);
    }
}
