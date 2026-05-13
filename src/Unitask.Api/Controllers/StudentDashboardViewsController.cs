using AutoMapper;
using Unitask.Api.Controllers.Common;
using Unitask.Application.Common.Interfaces;
using Unitask.Application.DTOs;
using Unitask.Domain.Entities;

namespace Unitask.Api.Controllers;

public class StudentDashboardViewsController : ReadOnlyController<StudentDashboardView, StudentDashboardViewDto>
{
    public StudentDashboardViewsController(IGenericRepository<StudentDashboardView> repository, IMapper mapper)
        : base(repository, mapper)
    {
    }
}

