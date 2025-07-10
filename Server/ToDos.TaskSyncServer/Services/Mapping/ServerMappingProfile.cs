using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ToDos.DotNet.Common;
using ToDos.Entities;
using AutoMapper;

namespace ToDos.TaskSyncServer.Mapping
{
    public class ServerMappingProfile : Profile
    {
        public ServerMappingProfile() 
        {
            CreateMap<TaskEntity, TaskDTO>().ReverseMap();
            CreateMap<TagEntity, TagDTO>().ReverseMap();
        }
       
    }
}