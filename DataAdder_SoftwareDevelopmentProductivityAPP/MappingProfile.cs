using AutoMapper;
using ClassLibrary_SoftwareDevelopmentProductivityAPP.DataTransferObjects_DTOs;
using ClassLibrary_SoftwareDevelopmentProductivityAPP.Models;

namespace DataAdder_SoftwareDevelopmentProductivityAPP
{
    public class MappingProfile :Profile
    {
        public MappingProfile()
        {

            CreateMap<Firme, partialFirme>()
                .ForMember(dest => dest.Denumire, opt => opt.MapFrom(src => src.Denumire))
                .ForMember(dest => dest.CODFirma, opt => opt.MapFrom(src => src.CODFirma));

            CreateMap<Proiecte, partialProiecte>()
                 .ForMember(dest => dest.Denumire, opt => opt.MapFrom(src => src.Denumire))
                 .ForMember(dest => dest.NrProiect, opt => opt.MapFrom(src => src.NrProiect));

            CreateMap<Functionalitati, partialFunctionalitati>()
                .ForMember(dest => dest.IDFunctionalitate, opt => opt.MapFrom(src => src.IDFunctionalitate))
                .ForMember(dest => dest.Denumire, opt => opt.MapFrom(src => src.Denumire))
                .ForMember(dest => dest.Descriere, opt => opt.MapFrom(src => src.Descriere))
                .ForMember(dest => dest.DenumireProiect, opt => opt.MapFrom(src => src.Proiect != null ? src.Proiect.Denumire : null));


            CreateMap<Angajati, partialAngajati>()
                .ForMember(dest => dest.NumeSiPrenume, opt => opt.MapFrom(src => src.NumeSiPrenume))
                .ForMember(dest => dest.MarcaAngajat, opt => opt.MapFrom(src => src.MarcaAngajat))
                .ForMember(dest => dest.Post, opt => opt.MapFrom(src => src.Post.Denumire));

            CreateMap<Departamente, partialDepartamente>()
                .ForMember(dest => dest.IDDepartament, opt => opt.MapFrom(src => src.IDDepartament))
                .ForMember(dest => dest.Denumire, opt => opt.MapFrom(src => src.Denumire));

            CreateMap<Posturi, partialPosturi>()
                .ForMember(dest => dest.IDPost, opt => opt.MapFrom(src => src.IDPost))
                .ForMember(dest => dest.Denumire, opt => opt.MapFrom(src => src.Denumire));

            CreateMap<Sarcini, partialSarcini>()
                .ForMember(dest => dest.IDSarcina, opt => opt.MapFrom(src => src.IDSarcina))
                .ForMember(dest => dest.Denumire, opt => opt.MapFrom(src => src.Denumire));

            CreateMap<GradeDeDificultate, partialGradeDificultate>()
                .ForMember(dest => dest.IDGradDificultate, opt => opt.MapFrom(src => src.IDGradDificultate))
                .ForMember(dest => dest.Denumire, opt => opt.MapFrom(src => src.Denumire));

            CreateMap<GradeUrgentaSarcini, partialGradeUrgentaSarcini>()
                .ForMember(dest => dest.IDGradUrgentaSarcina, opt => opt.MapFrom(src => src.IDGradUrgentaSarcina))
                .ForMember(dest => dest.Denumire, opt => opt.MapFrom(src => src.Denumire));

            CreateMap<DocumenteInterne, partialDocumenteInterne>()
                .ForMember(dest => dest.Denumire, opt => opt.MapFrom(src => src.Denumire))
                .ForMember(dest => dest.NumarDocument, opt => opt.MapFrom(src => src.NumarDocument));
        }

    }
}
