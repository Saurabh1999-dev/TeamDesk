using TeamDesk.Services.Interfaces;

namespace TeamDesk.Services
{

    public class FaqService : IFaqService
    {
        private readonly List<(string Question, string Answer)> _faqs = new()
        {
            ("How do I reset my password?", "Go to Account Settings and click on \"Reset Password.\"."),
            ("How can I contact support?", "Visit the \"Contact Us\" page or email us at support@example.com."),
            ("How do I create a project?", "Go to the Projects tab on the left side and click on \"New Project.\""),
            ("How do I log out?", "Click on your profile photo, then select \"Sign Out.\""),
            ("How do I view the client?", "Click on \"Client\" on the right side.\r\nNote: Only admins can change client details."),
            // ... more
        };

        public List<string> Search(string query)
        {
            return _faqs
                .Where(faq => faq.Question.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                              faq.Answer.Contains(query, StringComparison.OrdinalIgnoreCase))
                .Select(faq => faq.Answer)
                .ToList();
        }
    }
}