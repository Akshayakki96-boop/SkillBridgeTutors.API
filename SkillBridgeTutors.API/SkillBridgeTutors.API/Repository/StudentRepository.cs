using Microsoft.EntityFrameworkCore;
using SkillBridgeTutors.API.Data;
using SkillBridgeTutors.API.Interfaces;
using SkillBridgeTutors.API.Models;

namespace SkillBridgeTutors.API.Repository
{
    public class StudentRepository : IStudentRepository
    {
        private readonly ApplicationDbContext _context;

        public StudentRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Student>> GetAllAsync()
        {
            return await _context.Students
                .OrderBy(s => s.FullName)
                .ToListAsync();
        }

        public async Task<Student?> GetByIdAsync(int studentId)
        {
            return await _context.Students
                .Include(s => s.RegularClasses)
                .ThenInclude(c => c.Teacher)
                .Include(s => s.Payments)
                .FirstOrDefaultAsync(s => s.Id == studentId);
        }

        public async Task<Student?> GetByEmailAsync(string email)
        {
            return await _context.Students
                .Include(s => s.RegularClasses)
                .ThenInclude(c => c.Teacher)
                .FirstOrDefaultAsync(s => s.Email == email);
        }

        public async Task<Student> CreateAsync(Student student)
        {
            _context.Students.Add(student);
            await _context.SaveChangesAsync();
            return student;
        }

        public async Task UpdateAsync(Student student)
        {
            _context.Students.Update(student);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ExistsByEmailAsync(string email)
        {
            return await _context.Students.AnyAsync(s => s.Email == email);
        }
    }
}
