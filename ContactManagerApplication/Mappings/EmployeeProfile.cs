using AutoMapper;
using ContactManagerApplication.Models.Domain;
using ContactManagerApplication.Models.ViewModel;

namespace ContactManagerApplication.Mappings
{
    public class EmployeeProfile : Profile
    {
        public EmployeeProfile()
        {
            CreateMap<CreateEmployeeVM, Employee>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(_ => Guid.NewGuid()))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name.Trim()))
                .ForMember(dest => dest.Phone, opt => opt.MapFrom(src => src.Phone == null ? null : src.Phone.Trim()));

            CreateMap<Employee, CreateEmployeeVM>();

            CreateMap<UpdateEmployeeVM, Employee>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name.Trim()))
                .ForMember(dest => dest.Phone, opt => opt.MapFrom(src => src.Phone == null ? null : src.Phone.Trim()));

            CreateMap<Employee, UpdateEmployeeVM>();
        }
    }
}
