using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToDos.DotNet.Common;
using Todos.Ui.Models;
using Todos.Client.UserService.Models;

namespace TodDos.Ui.Services.Mapping
{
    public class ClientMappingProfile : Profile
    {
        public ClientMappingProfile()
        {
            CreateMap<TaskDTO, TaskModel>()
                .ForMember(dest => dest.Tags, opt => opt.MapFrom(src => src.Tags == null ? string.Empty : string.Join(", ", src.Tags.Select(t => t.Name))))
                .ReverseMap()
                .ForMember(dest => dest.Tags, opt => opt.MapFrom(src => string.IsNullOrWhiteSpace(src.Tags) ? new List<TagDTO>() : src.Tags.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries).Select(tag => tag.Trim()).Where(tag => !string.IsNullOrEmpty(tag)).Distinct().Select(tag => new TagDTO { Name = tag }).ToList()));

            CreateMap<TagDTO, TagModel>()
                .ReverseMap();

            CreateMap<UserDTO, UserModel>()
                .ReverseMap();
        }
    }

}
