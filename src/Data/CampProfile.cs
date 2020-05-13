using AutoMapper;
using CoreCodeCamp.Models;

namespace CoreCodeCamp.Data
{
  public class CampProfile : Profile
  {
    public CampProfile()
    {
      CreateMap<Camp, CampModel>()
        .ForMember(c => c.Venue, o => o.MapFrom(m => m.Location.VenueName))
        .ForMember(c => c.Address1, o => o.MapFrom(m => m.Location.Address1))
        .ForMember(c => c.Address2, o => o.MapFrom(m => m.Location.Address2))
        .ForMember(c => c.Address3, o => o.MapFrom(m => m.Location.Address3))
        .ForMember(c => c.CityTown, o => o.MapFrom(m => m.Location.CityTown))
        .ForMember(c => c.StateProvince, o => o.MapFrom(m => m.Location.StateProvince))
        .ForMember(c => c.PostalCode, o => o.MapFrom(m => m.Location.PostalCode))
        .ForMember(c => c.Country, o => o.MapFrom(m => m.Location.Country))
        .ReverseMap();
      CreateMap<Talk, TalkModel>()
        .ReverseMap()
        .ForMember(t => t.Camp, opt => opt.Ignore())
        .ForMember(t => t.Speaker, opt => opt.Ignore());
      CreateMap<Speaker, SpeakerModel>().ReverseMap();

    }
  }
}
