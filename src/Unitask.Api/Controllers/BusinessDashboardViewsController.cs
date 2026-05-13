using AutoMapper;
using Unitask.Api.Controllers.Common;
using Unitask.Application.Common.Interfaces;
using Unitask.Application.DTOs;
using Unitask.Domain.Entities;

namespace Unitask.Api.Controllers;

public class BusinessDashboardViewsController : ReadOnlyController<BusinessDashboardView, BusinessDashboardViewDto>
{
    public BusinessDashboardViewsController(IGenericRepository<BusinessDashboardView> repository, IMapper mapper)
        : base(repository, mapper)
    {
    }
}

