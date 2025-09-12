using AutoMapper;
using Papernote.Notes.Core.Application.DTOs;
using Papernote.Notes.Core.Domain.Entities;

namespace Papernote.Notes.Core.Application.Mappings;

public class NoteMappingProfile : Profile
{
    public NoteMappingProfile()
    {
        CreateMap<Note, NoteDto>()
            .ForMember(dest => dest.Tags, opt => opt.MapFrom(src => src.NoteTags.Select(nt => nt.TagName).ToList()))
            .ForMember(dest => dest.OwnerUsername, opt => opt.Ignore())
            .ForMember(dest => dest.SharedWithUsernames, opt => opt.Ignore());

        CreateMap<Note, NoteSummaryDto>()
            .ForMember(dest => dest.ContentPreview, opt => opt.MapFrom(src => CreatePreview(src.Content)))
            .ForMember(dest => dest.Tags, opt => opt.MapFrom(src => src.NoteTags.Select(nt => nt.TagName).ToList()))
            .ForMember(dest => dest.OwnerUsername, opt => opt.Ignore());

        CreateMap<NoteShare, NoteShareDto>()
            .ForMember(dest => dest.SharedAt, opt => opt.MapFrom(src => src.SharedAt));
    }

    private static string CreatePreview(string content)
    {
        const int MaxPreviewLength = 100;
        return content.Length > MaxPreviewLength ? content[..MaxPreviewLength] + "..." : content;
    }
}