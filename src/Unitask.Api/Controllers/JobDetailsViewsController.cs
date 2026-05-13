using AutoMapper;
using Unitask.Api.Controllers.Common;
using Unitask.Application.Common.Interfaces;
using Unitask.Application.DTOs;
using Unitask.Domain.Entities;

namespace Unitask.Api.Controllers;

public class JobDetailsViewsController : ReadOnlyController<JobDetailsView, JobDetailsViewDto>
{
    public JobDetailsViewsController(IGenericRepository<JobDetailsView> repository, IMapper mapper)
        : base(repository, mapper)
    {
    }
}

