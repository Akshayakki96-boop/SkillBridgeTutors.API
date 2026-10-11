using Microsoft.EntityFrameworkCore;
using SkillBridgeTutors.API.Data;
using SkillBridgeTutors.API.Interfaces;
using SkillBridgeTutors.API.Models;

namespace SkillBridgeTutors.API.Repository
{
    public class RegularClassRepository : IRegularClassRepository
    {
        private readonly ApplicationDbContext _context;

        public RegularClassRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<RegularClass> CreateAsync(RegularClass regularClass)
        {
            _context.RegularClasses.Add(regularClass);
            await _context.SaveChangesAsync();
            return regularClass;
        }

        public async Task<RegularClass?> GetByIdAsync(long classId)
        {
            return await _context.RegularClasses
                .Include(c => c.Student)
                .Include(c => c.Teacher)
                .FirstOrDefaultAsync(c => c.ClassId == classId);
        }

        public async Task<IEnumerable<RegularClass>> GetByTeacherAsync(long teacherId)
        {
            return await _context.RegularClasses
                .Include(c => c.Student)
                .Include(c => c.Teacher)
                .Where(c => c.TeacherId == teacherId && c.Status != "Cancelled")
                .OrderBy(c => c.StartTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<RegularClass>> GetByStudentAsync(int studentId)
        {
            return await _context.RegularClasses
                .Include(c => c.Student)
                .Include(c => c.Teacher)
                .Where(c => c.StudentId == studentId && c.Status != "Cancelled")
                .OrderBy(c => c.StartTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<RegularClass>> GetUpcomingAsync(int days = 30)
        {
            var futureDate = DateTime.UtcNow.AddDays(days);
            return await _context.RegularClasses
                .Include(c => c.Student)
                .Include(c => c.Teacher)
                .Where(c => c.StartTime >= DateTime.UtcNow && 
                           c.StartTime <= futureDate && 
                           c.Status != "Cancelled")
                .OrderBy(c => c.StartTime)
                .ToListAsync();
        }

        public async Task UpdateAsync(RegularClass regularClass)
        {
            _context.RegularClasses.Update(regularClass);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(RegularClass regularClass)
        {
            _context.RegularClasses.Remove(regularClass);
            await _context.SaveChangesAsync();
        }
    }
}
