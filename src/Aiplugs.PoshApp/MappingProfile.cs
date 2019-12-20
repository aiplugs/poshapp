using Aiplugs.PoshApp.Models;
using Aiplugs.PoshApp.ViewModels;
using AutoMapper;
using Microsoft.AspNetCore.Builder;

namespace Aiplugs.PoshApp
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Script, ScriptListItemViewModel>();
            CreateMap<ListScriptViewModel, ListScript>();
            CreateMap<ListScript, ListScriptViewModel>();
            CreateMap<DetailScriptViewModel, DetailScript>();
            CreateMap<DetailScript, DetailScriptViewModel>();
            CreateMap<SingletonScriptViewModel, SingletonScript>();
            CreateMap<SingletonScript, SingletonScriptViewModel>();
            CreateMap<ActionScriptViewModel, ActionScript>();
            CreateMap<ActionScript, ActionScriptViewModel>();
            CreateMap<RepositoryViewModel, Repository>();
            CreateMap<Repository, RepositoryViewModel>();
        }
    }
}