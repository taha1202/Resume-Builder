using System.Net.Http.Json;
using ResumeBuilder.Models;

namespace ResumeBuilder.Services
{
    public class ResumeService
    {
        private readonly HttpClient _http;
        private readonly PdfService _pdfService;
        private readonly List<ResumeTemplate> _templates = new();

        public ResumeService(HttpClient http, PdfService pdfService, IConfiguration config)
        {
            _http = http;
            _pdfService = pdfService;

            // Set API Base URL (reads from appsettings.json)
            var apiBase = config["ApiBaseUrl"] ?? "http://localhost:7071/api/";
            if (_http.BaseAddress == null) _http.BaseAddress = new Uri(apiBase);

            InitializeTemplates();
        }

        // --- API CALLS ---

        public async Task<Resume> SaveResume(Resume resume)
        {
            // Calls Azure Function: POST /api/SaveResume
            var response = await _http.PostAsJsonAsync("SaveResume", resume);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<Resume>() ?? resume;
            }

            // Log error (or handle globally)
            Console.WriteLine($"Save failed: {response.StatusCode}");
            return resume;
        }

        public async Task<List<Resume>> GetUserResumes(string userId)
        {
            try
            {
                // Calls Azure Function: GET /api/resumes/{userId}
                return await _http.GetFromJsonAsync<List<Resume>>($"resumes/{userId}") ?? new List<Resume>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fetch failed: {ex.Message}");
                return new List<Resume>();
            }
        }

        public async Task<Resume?> GetResume(string resumeId, string userId)
        {
            // Fetch all and find one (Efficient enough for resume lists < 100 items)
            var all = await GetUserResumes(userId);
            return all.FirstOrDefault(r => r.Id == resumeId);
        }

        public async Task<bool> DeleteResume(string resumeId, string userId)
        {
            // Calls Azure Function: DELETE /api/resumes/{userId}/{id}
            var response = await _http.DeleteAsync($"resumes/{userId}/{resumeId}");
            return response.IsSuccessStatusCode;
        }

        // --- PDF GENERATION (Local) ---

        public async Task<byte[]> GeneratePDF(Resume resume, ResumeTemplate? template)
        {
            if (template == null) template = GetTemplate(resume.TemplateId) ?? _templates.First();
            return _pdfService.GenerateResumePDF(resume, template);
        }

        // --- TEMPLATES (Static) ---
        public List<ResumeTemplate> GetTemplates() => _templates;
        public ResumeTemplate? GetTemplate(int templateId) => _templates.FirstOrDefault(t => t.Id == templateId);

        private void InitializeTemplates()
        {
            _templates.AddRange(new List<ResumeTemplate>
            {
                new ResumeTemplate { Id = 1, Name = "Modern Template", Description = "Clean and professional design with blue accents", ColorScheme = "#2563EB" },
                new ResumeTemplate { Id = 2, Name = "Executive Template", Description = "Bold and sophisticated with navy color scheme", ColorScheme = "#1e3a8a" },
                new ResumeTemplate { Id = 3, Name = "Creative Template", Description = "Vibrant and eye-catching with purple theme", ColorScheme = "#7c3aed" },
                new ResumeTemplate { Id = 4, Name = "Minimal Template", Description = "Simple and elegant with dark gray accents", ColorScheme = "#374151" },
                new ResumeTemplate { Id = 5, Name = "Tech Template", Description = "Modern tech-focused with teal highlights", ColorScheme = "#0d9488" },
                new ResumeTemplate { Id = 6, Name = "Professional Template", Description = "Traditional business style with green accents", ColorScheme = "#059669" }
            });
        }
    }
}