using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Unitask.Application.Common.Interfaces;

namespace Unitask.Api.Controllers.Common;

[ApiController]
[Route("api/[controller]")]
public abstract class CrudController<TEntity, TDto> : ControllerBase
    where TEntity : class
    where TDto : class
{
    private readonly IGenericRepository<TEntity> _repository;
    private readonly IMapper _mapper;

    protected CrudController(IGenericRepository<TEntity> repository, IMapper mapper)
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

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TDto>> GetById(Guid id)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity is null)
        {
            return NotFound();
        }

        return Ok(_mapper.Map<TDto>(entity));
    }

    [HttpPost]
    public async Task<ActionResult<TDto>> Create([FromBody] TDto dto)
    {
        var entity = _mapper.Map<TEntity>(dto);
        await _repository.AddAsync(entity);
        await _repository.SaveChangesAsync();

        var createdId = GetEntityId(entity);
        if (createdId is null)
        {
            return Ok(_mapper.Map<TDto>(entity));
        }

        return CreatedAtAction(nameof(GetById), new { id = createdId }, _mapper.Map<TDto>(entity));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] TDto dto)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity is null)
        {
            return NotFound();
        }

        _mapper.Map(dto, entity);
        SetEntityId(entity, id);
        _repository.Update(entity);
        await _repository.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity is null)
        {
            return NotFound();
        }

        _repository.Remove(entity);
        await _repository.SaveChangesAsync();

        return NoContent();
    }

    private static Guid? GetEntityId(TEntity entity)
    {
        var property = typeof(TEntity).GetProperty("Id");
        if (property is null)
        {
            return null;
        }

        var value = property.GetValue(entity);
        if (value is Guid id)
        {
            return id;
        }

        return value as Guid?;
    }

    private static void SetEntityId(TEntity entity, Guid id)
    {
        var property = typeof(TEntity).GetProperty("Id");
        if (property is null || !property.CanWrite)
        {
            return;
        }

        property.SetValue(entity, id);
    }
}
