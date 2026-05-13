using AutoMapper;
using Unitask.Api.Controllers.Common;
using Unitask.Application.Common.Interfaces;
using Unitask.Application.DTOs;
using Unitask.Domain.Entities;

namespace Unitask.Api.Controllers;

public class WithdrawalRequestsController : CrudController<WithdrawalRequest, WithdrawalRequestDto>
{
    public WithdrawalRequestsController(IGenericRepository<WithdrawalRequest> repository, IMapper mapper)
        : base(repository, mapper)
    {
    }
}

