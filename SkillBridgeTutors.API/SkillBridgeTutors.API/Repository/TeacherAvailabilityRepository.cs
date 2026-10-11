using Microsoft.EntityFrameworkCore;
using SkillBridgeTutors.API.Data;
using SkillBridgeTutors.API.Interfaces;
using SkillBridgeTutors.API.Models;

namespace SkillBridgeTutors.API.Repository
{
    public class TeacherAvailabilityRepository : ITeacherAvailabilityRepository
    {
        private readonly ApplicationDbContext _context;

        public TeacherAvailabilityRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<TeacherAvailability>> GetActiveAsync()
        {
            return await _context.TeacherAvailabilities
                .Where(a => a.IsActive)
                .ToListAsync();
        }

        public async Task<IEnumerable<TeacherAvailability>> GetByTeacherAsync(long teacherId)
        {
            return await _context.TeacherAvailabilities
                .Where(a => a.TeacherId == teacherId && a.IsActive)
                .OrderBy(a => a.DayOfWeek)
                .ThenBy(a => a.StartTime)
                .ToListAsync();
        }

        public async Task<TeacherAvailability?> GetByIdAsync(long availabilityId)
        {
            return await _context.TeacherAvailabilities
                .FirstOrDefaultAsync(a => a.AvailabilityId == availabilityId);
        }

        public async Task<TeacherAvailability> CreateAsync(TeacherAvailability availability)
        {
            _context.TeacherAvailabilities.Add(availability);
            await _context.SaveChangesAsync();
            return availability;
        }

        public async Task DeleteAsync(TeacherAvailability availability)
        {
            _context.TeacherAvailabilities.Remove(availability);
            await _context.SaveChangesAsync();
        }
    }
}
