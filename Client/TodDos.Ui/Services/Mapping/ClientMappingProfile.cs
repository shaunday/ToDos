using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TodDos.Ui.Models;
using ToDos.DotNet.Common;
using ToDos.Ui.Models;

namespace TodDos.Ui.Services.Mapping
{
    public class ClientMappingProfile : Profile
    {
        public ClientMappingProfile()
        {
            CreateMap<TaskDTO, TaskModel>()
                .ReverseMap();

            CreateMap<TagDTO, TagModel>()
                .ReverseMap();
        }
    }

}
