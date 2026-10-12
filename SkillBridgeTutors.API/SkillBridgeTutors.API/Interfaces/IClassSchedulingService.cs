using SkillBridgeTutors.API.DTOs;
using SkillBridgeTutors.API.Models;

namespace SkillBridgeTutors.API.Interfaces
{
    public interface IClassSchedulingService
    {
        Task<IEnumerable<ScheduleSuggestionDto>> GetSuggestionsAsync(ScheduleSuggestionRequestDto request);
        Task<(RegularClass? Class, string? Error)> CreateRegularClassAsync(CreateRegularClassDto dto);
        Task<(RegularClass? Class, string? Error)> RescheduleRegularClassAsync(long classId, RescheduleRegularClassDto dto);
    }
}
