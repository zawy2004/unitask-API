using AutoMapper;
using Unitask.Api.Controllers.Common;
using Unitask.Application.Common.Interfaces;
using Unitask.Application.DTOs;
using Unitask.Domain.Entities;

namespace Unitask.Api.Controllers;

public class ActivityLogsController : CrudController<ActivityLog, ActivityLogDto>
{
    public ActivityLogsController(IGenericRepository<ActivityLog> repository, IMapper mapper)
        : base(repository, mapper)
    {
    }
}

