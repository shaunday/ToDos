using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TodDos.Ui.Models;
using ToDos.DotNet.Common;
using ToDos.Ui.Models;
using static ToDos.DotNet.Common.GlobalTypes;

namespace TodDos.Ui.Services.Mapping
{
    public class ClientMappingProfile : Profile
    {
        public ClientMappingProfile()
        {
            CreateMap<TaskDTO, TaskModel>()
                .ForMember(dest => dest.Priority, opt => opt.MapFrom(src => 
                    string.IsNullOrEmpty(src.Priority) ? TaskPriority.Medium : 
                    (TaskPriority)Enum.Parse(typeof(TaskPriority), src.Priority)))
                .ReverseMap()
                .ForMember(dest => dest.Priority, opt => opt.MapFrom(src => src.Priority.ToString()));

            CreateMap<TagDTO, TagModel>()
                .ReverseMap();
        }
    }

}
