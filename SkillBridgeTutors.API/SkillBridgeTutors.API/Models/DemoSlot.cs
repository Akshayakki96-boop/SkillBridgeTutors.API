namespace SkillBridgeTutors.API.Models
{
    public class DemoSlot
    {
        public int Id { get; set; }
        public DateTime SlotDateTime { get; set; }
        public bool IsBooked { get; set; } = false;
        public string? TutorName { get; set; }
    }
}
