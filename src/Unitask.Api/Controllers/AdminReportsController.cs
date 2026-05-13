using AutoMapper;
using Unitask.Api.Controllers.Common;
using Unitask.Application.Common.Interfaces;
using Unitask.Application.DTOs;
using Unitask.Domain.Entities;

namespace Unitask.Api.Controllers;

public class AdminReportsController : CrudController<AdminReport, AdminReportDto>
{
    public AdminReportsController(IGenericRepository<AdminReport> repository, IMapper mapper)
        : base(repository, mapper)
    {
    }
}

