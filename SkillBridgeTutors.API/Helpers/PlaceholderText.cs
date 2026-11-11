namespace SkillBridgeTutors.API.Helpers
{
    public static class PlaceholderText
    {
        // Returns null for blank values and for the "string" placeholder that API docs tools insert by default.
        public static string? Clean(string? value)
        {
            var trimmed = value?.Trim();
            if (string.IsNullOrEmpty(trimmed) || trimmed.Equals("string", StringComparison.OrdinalIgnoreCase))
                return null;

            return trimmed;
        }
    }
}
