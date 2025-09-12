using AutoMapper;
using Papernote.Notes.Core.Application.DTOs;
using Papernote.Notes.Core.Domain.Entities;

namespace Papernote.Notes.Core.Application.Mappings;

/// <summary>
/// AutoMapper profile for Note entity mappings
/// </summary>
public class NoteMappingProfile : Profile
{
    public NoteMappingProfile()
    {
        CreateMap<Note, NoteDto>()
            .ForMember(dest => dest.Tags, opt => opt.MapFrom(src => src.NoteTags.Select(nt => nt.TagName).ToList()));

        CreateMap<Note, NoteSummaryDto>()
            .ForMember(dest => dest.ContentPreview, opt => opt.MapFrom(src => CreatePreview(src.Content)))
            .ForMember(dest => dest.Tags, opt => opt.MapFrom(src => src.NoteTags.Select(nt => nt.TagName).ToList()));

        // DTOs to Note mappings
        CreateMap<CreateNoteDto, Note>()
            .ConstructUsing(src => new Note(src.Title, src.Content, src.Tags));

        CreateMap<UpdateNoteDto, Note>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
            .ForMember(dest => dest.Content, opt => opt.MapFrom(src => src.Content))
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.NoteTags, opt => opt.Ignore());
    }

    private static string CreatePreview(string content)
    {
        return content.Length > 100 ? content[..100] + "..." : content;
    }
}