using AutoMapper;
using Unitask.Api.Controllers.Common;
using Unitask.Application.Common.Interfaces;
using Unitask.Application.DTOs;
using Unitask.Domain.Entities;

namespace Unitask.Api.Controllers;

public class StudentSkillsController : CrudController<StudentSkill, StudentSkillDto>
{
    public StudentSkillsController(IGenericRepository<StudentSkill> repository, IMapper mapper)
        : base(repository, mapper)
    {
    }
}

