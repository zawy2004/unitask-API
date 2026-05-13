using System.Linq;
using AutoMapper;
using Unitask.Application.DTOs;
using Unitask.Domain.Entities;

namespace Unitask.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        var entityTypes = typeof(User).Assembly
            .GetTypes()
            .Where(t => t.IsClass && t.Namespace == "Unitask.Domain.Entities");

        var dtoTypes = typeof(ActivityLogDto).Assembly
            .GetTypes()
            .Where(t => t.IsClass && t.Namespace == "Unitask.Application.DTOs");

        foreach (var entityType in entityTypes)
        {
            var dtoType = dtoTypes.FirstOrDefault(t => t.Name == $"{entityType.Name}Dto");
            if (dtoType is null)
            {
                continue;
            }

            CreateMap(entityType, dtoType).ReverseMap();
        }
    }
}
