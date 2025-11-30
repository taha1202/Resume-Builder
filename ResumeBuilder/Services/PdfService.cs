using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using ResumeBuilder.Models;
using System.Text.Json;
using Color = QuestPDF.Infrastructure.Color;

namespace ResumeBuilder.Services
{
    public class PdfService
    {
        // Helper classes for JSON Deserialization
        private class PdfEdu { public string Institution { get; set; } = ""; public string Degree { get; set; } = ""; public string FieldOfStudy { get; set; } = ""; public string StartDate { get; set; } = ""; public string EndDate { get; set; } = ""; public string GPA { get; set; } = ""; }
        private class PdfExp { public string Company { get; set; } = ""; public string Position { get; set; } = ""; public string Location { get; set; } = ""; public string StartDate { get; set; } = ""; public string EndDate { get; set; } = ""; public string Description { get; set; } = ""; }
        private class PdfProj { public string Name { get; set; } = ""; public string Description { get; set; } = ""; public string Technologies { get; set; } = ""; public string Link { get; set; } = ""; }
        private class PdfCert { public string Name { get; set; } = ""; public string Issuer { get; set; } = ""; public string Date { get; set; } = ""; public string Link { get; set; } = ""; }
        private class PdfAcademic { public string Name { get; set; } = ""; public string Course { get; set; } = ""; public string Grade { get; set; } = ""; public string Description { get; set; } = ""; }
        private class PdfVolunteer { public string Organization { get; set; } = ""; public string Role { get; set; } = ""; public string StartDate { get; set; } = ""; public string EndDate { get; set; } = ""; public string Description { get; set; } = ""; }
        private class PdfActivity { public string Organization { get; set; } = ""; public string Role { get; set; } = ""; public string StartDate { get; set; } = ""; public string EndDate { get; set; } = ""; public string Description { get; set; } = ""; }

        // SVG Path Constants
        private const string IconEmail = @"<path d=""M4 4h16c1.1 0 2 .9 2 2v12c0 1.1-.9 2-2 2H4c-1.1 0-2-.9-2-2V6c0-1.1.9-2 2-2z"" /><polyline points=""22,6 12,13 2,6"" />";
        private const string IconPhone = @"<path d=""M22 16.92v3a2 2 0 0 1-2.18 2 19.79 19.79 0 0 1-8.63-3.07 19.5 19.5 0 0 1-6-6 19.79 19.79 0 0 1-3.07-8.67A2 2 0 0 1 4.11 2h3a2 2 0 0 1 2 1.72 12.05 12.05 0 0 0 .7 2.81 2 2 0 0 1-.45 2.11L8.09 9.91a16 16 0 0 0 6 6l1.27-1.27a2 2 0 0 1 2.11-.45 12.05 12.05 0 0 0 2.81.7A2 2 0 0 1 22 16.92z"" />";
        private const string IconMap = @"<path d=""M21 10c0 7-9 13-9 13s-9-6-9-13a9 9 0 0 1 18 0z"" /><circle cx=""12"" cy=""10"" r=""3"" />";
        private const string IconLink = @"<path d=""M10 13a5 5 0 0 0 7.54.54l3-3a5 5 0 0 0-7.07-7.07l-1.72 1.71"" /><path d=""M14 11a5 5 0 0 0-7.54-.54l-3 3a5 5 0 0 0 7.07 7.07l1.71-1.71"" />";
        private const string IconGlobe = @"<circle cx=""12"" cy=""12"" r=""10"" /><line x1=""2"" y1=""12"" x2=""22"" y2=""12"" /><path d=""M12 2a15.3 15.3 0 0 1 4 10 15.3 15.3 0 0 1-4 10 15.3 15.3 0 0 1-4-10 15.3 15.3 0 0 1 4-10z"" />";

        public byte[] GenerateResumePDF(Resume resume, ResumeTemplate template)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.Letter);
                    page.Margin(0.75f, Unit.Inch);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10.5f).FontFamily("Arial").FontColor(Colors.Grey.Darken3));

                    page.Content().Column(column =>
                    {
                        column.Spacing(15);
                        column.Item().Element(c => RenderHeader(c, resume, template));

                        if (!string.IsNullOrWhiteSpace(resume.Summary))
                            column.Item().Element(c => RenderSection(c, "PROFESSIONAL SUMMARY", resume.Summary, template.ColorScheme));

                        if (!string.IsNullOrWhiteSpace(resume.Experience) && resume.Experience.Trim().StartsWith("["))
                            column.Item().Element(c => RenderExperience(c, resume.Experience, template.ColorScheme));

                        if (!string.IsNullOrWhiteSpace(resume.Education) && resume.Education.Trim().StartsWith("["))
                            column.Item().Element(c => RenderEducation(c, resume.Education, template.ColorScheme));

                        if (!string.IsNullOrWhiteSpace(resume.Skills))
                            column.Item().Element(c => RenderSkills(c, "TECHNICAL SKILLS", resume.Skills, template.ColorScheme));
                        if (!string.IsNullOrWhiteSpace(resume.SoftSkills))
                            column.Item().Element(c => RenderSkills(c, "SOFT SKILLS", resume.SoftSkills, template.ColorScheme));

                        if (!string.IsNullOrWhiteSpace(resume.Projects) && resume.Projects.Trim().StartsWith("["))
                            column.Item().Element(c => RenderProjects(c, resume.Projects, template.ColorScheme));

                        if (!string.IsNullOrWhiteSpace(resume.Certifications) && resume.Certifications.Trim().StartsWith("["))
                            column.Item().Element(c => RenderCertifications(c, resume.Certifications, template.ColorScheme));

                        if (!string.IsNullOrWhiteSpace(resume.Academic) && resume.Academic.Trim().StartsWith("["))
                            column.Item().Element(c => RenderAcademic(c, resume.Academic, template.ColorScheme));

                        if (!string.IsNullOrWhiteSpace(resume.Volunteer) && resume.Volunteer.Trim().StartsWith("["))
                            column.Item().Element(c => RenderVolunteer(c, resume.Volunteer, template.ColorScheme));

                        if (!string.IsNullOrWhiteSpace(resume.Activities) && resume.Activities.Trim().StartsWith("["))
                            column.Item().Element(c => RenderActivities(c, resume.Activities, template.ColorScheme));
                    });
                });
            });

            return document.GeneratePdf();
        }

        private void RenderHeader(IContainer container, Resume resume, ResumeTemplate template)
        {
            var color = HexToColor(template.ColorScheme);
            container.Column(column =>
            {
                column.Item().Text(resume.FullName).FontSize(24).Bold().FontColor(Colors.Black).LineHeight(1);
                column.Item().PaddingTop(8).Row(row =>
                {
                    row.Spacing(25);
                    void RenderItem(string text, string svgContent, string linkUrl = "")
                    {
                        if (string.IsNullOrWhiteSpace(text)) return;

                        row.AutoItem().Row(item => {
                            item.Spacing(6);

                            // Render Icon
                            item.AutoItem().AlignMiddle().Width(11).Height(11).Element(e => {
                                string xml = $@"<svg viewBox=""0 0 24 24"" fill=""none"" stroke=""#555555"" stroke-width=""2"" stroke-linecap=""round"" stroke-linejoin=""round"" xmlns=""http://www.w3.org/2000/svg"">{svgContent}</svg>";
                                e.Svg(xml);
                            });

                            // Render Text Container
                            var textContainer = item.AutoItem().AlignMiddle();

                            // FIX: Apply Hyperlink to the Container, NOT the Text descriptor
                            if (!string.IsNullOrEmpty(linkUrl))
                            {
                                var validUrl = linkUrl.StartsWith("http") ? linkUrl : "https://" + linkUrl;
                                textContainer = textContainer.Hyperlink(validUrl);
                            }

                            textContainer.Text(text).FontSize(9).FontColor(Colors.Grey.Darken2);
                        });
                    }
                    RenderItem(resume.Email, IconEmail, "mailto:" + resume.Email);
                    RenderItem(resume.Phone, IconPhone);
                    RenderItem(resume.Address, IconMap);
                    RenderItem(resume.LinkedIn, IconLink, resume.LinkedIn);
                    RenderItem(resume.Website, IconGlobe, resume.Website);
                });
                column.Item().PaddingTop(10).Height(2).Background(color);
            });
        }

        private void RenderProjects(IContainer container, string json, string colorHex)
        {
            var list = JsonSerializer.Deserialize<List<PdfProj>>(json);
            if (list == null || !list.Any()) return;
            var color = HexToColor(colorHex);
            container.Column(column =>
            {
                column.Item().PaddingBottom(5).Column(c => { c.Item().Text("PROJECTS").FontSize(12).Bold().FontColor(color).LetterSpacing(0.05f); c.Item().PaddingTop(2).Height(1).Background(Colors.Grey.Lighten2); });
                foreach (var item in list)
                {
                    column.Item().PaddingBottom(8).Column(entry => {

                        // FIX: Use a Row to separate Name and Link so we can apply Hyperlink to the Link container
                        entry.Item().Row(row => {
                            row.AutoItem().Text(item.Name).Bold().FontSize(11).FontColor(Colors.Black);

                            if (!string.IsNullOrEmpty(item.Link))
                            {
                                var validUrl = item.Link.StartsWith("http") ? item.Link : "https://" + item.Link;
                                // Apply Hyperlink to this specific container
                                row.AutoItem().PaddingLeft(5).Hyperlink(validUrl).Text("🔗 Link").FontSize(9).FontColor(Colors.Blue.Medium);
                            }
                        });

                        if (!string.IsNullOrEmpty(item.Technologies)) entry.Item().Text($"Tech: {item.Technologies}").FontSize(9).FontColor(Colors.Grey.Darken1);
                        if (!string.IsNullOrEmpty(item.Description)) entry.Item().Text(item.Description).FontSize(10);
                    });
                }
            });
        }

        private void RenderCertifications(IContainer container, string json, string colorHex)
        {
            var list = JsonSerializer.Deserialize<List<PdfCert>>(json);
            if (list == null || !list.Any()) return;
            var color = HexToColor(colorHex);
            container.Column(column =>
            {
                column.Item().PaddingBottom(5).Column(c => { c.Item().Text("CERTIFICATIONS").FontSize(12).Bold().FontColor(color).LetterSpacing(0.05f); c.Item().PaddingTop(2).Height(1).Background(Colors.Grey.Lighten2); });
                foreach (var item in list)
                {
                    // FIX: Broken into discrete AutoItems so we can link the Icon
                    column.Item().PaddingBottom(4).Row(row => {
                        // Name and Issuer
                        row.RelativeItem().Text(text => {
                            text.Span(item.Name).Bold().FontColor(Colors.Black);
                            if (!string.IsNullOrEmpty(item.Issuer)) text.Span($" - {item.Issuer}");
                        });

                        // Link Icon
                        if (!string.IsNullOrEmpty(item.Link))
                        {
                            var validUrl = item.Link.StartsWith("http") ? item.Link : "https://" + item.Link;
                            row.AutoItem().PaddingLeft(5).Hyperlink(validUrl).Text("🔗").FontSize(9).FontColor(Colors.Blue.Medium);
                        }

                        // Date
                        if (!string.IsNullOrEmpty(item.Date)) row.AutoItem().PaddingLeft(5).Text(item.Date).Bold();
                    });
                }
            });
        }

        private void RenderSkills(IContainer container, string title, string skillsCsv, string colorHex)
        {
            var themeColor = HexToColor(colorHex);
            var pillBgColor = themeColor.WithAlpha(32);
            var skills = skillsCsv.Split(',').Select(s => s.Trim()).Where(s => !string.IsNullOrEmpty(s)).ToList();
            if (!skills.Any()) return;
            container.Column(column =>
            {
                column.Item().PaddingBottom(8).Column(c => { c.Item().Text(title).FontSize(12).Bold().FontColor(themeColor).LetterSpacing(0.05f); c.Item().PaddingTop(2).Height(1).Background(Colors.Grey.Lighten2); });
                column.Item().Inlined(inlined => { inlined.Spacing(8); inlined.VerticalSpacing(8); foreach (var skill in skills) { inlined.Item().Background(pillBgColor).PaddingVertical(3).PaddingHorizontal(8).Text(t => t.Span(skill).FontSize(9).FontColor(themeColor).Medium()); } });
            });
        }

        // --- NEW RENDERERS ---
        private void RenderAcademic(IContainer container, string json, string colorHex)
        {
            var list = JsonSerializer.Deserialize<List<PdfAcademic>>(json);
            if (list == null || !list.Any()) return;
            var color = HexToColor(colorHex);
            container.Column(column =>
            {
                column.Item().PaddingBottom(5).Column(c => { c.Item().Text("ACADEMIC ACHIEVEMENTS").FontSize(12).Bold().FontColor(color).LetterSpacing(0.05f); c.Item().PaddingTop(2).Height(1).Background(Colors.Grey.Lighten2); });
                foreach (var item in list)
                {
                    column.Item().PaddingBottom(8).Column(entry => {
                        entry.Item().Text(item.Name).Bold().FontSize(11).FontColor(Colors.Black);
                        entry.Item().Text(t => { t.Span(item.Course).Italic().FontSize(10); if (!string.IsNullOrEmpty(item.Grade)) t.Span($" • Grade: {item.Grade}"); });
                        if (!string.IsNullOrEmpty(item.Description)) entry.Item().Text(item.Description).FontSize(10);
                    });
                }
            });
        }

        private void RenderVolunteer(IContainer container, string json, string colorHex)
        {
            var list = JsonSerializer.Deserialize<List<PdfVolunteer>>(json);
            if (list == null || !list.Any()) return;
            var color = HexToColor(colorHex);
            container.Column(column =>
            {
                column.Item().PaddingBottom(5).Column(c => { c.Item().Text("VOLUNTEER EXPERIENCE").FontSize(12).Bold().FontColor(color).LetterSpacing(0.05f); c.Item().PaddingTop(2).Height(1).Background(Colors.Grey.Lighten2); });
                foreach (var item in list)
                {
                    column.Item().PaddingBottom(10).Column(entry => {
                        entry.Item().Row(row => { row.RelativeItem().Text(item.Organization).Bold().FontSize(11).FontColor(Colors.Black); row.AutoItem().Text($"{item.StartDate} - {item.EndDate}").Bold().FontSize(10); });
                        entry.Item().Text(item.Role).Italic().FontSize(10.5f);
                        if (!string.IsNullOrWhiteSpace(item.Description)) entry.Item().PaddingTop(2).Text(item.Description).FontSize(10).LineHeight(1.4f);
                    });
                }
            });
        }

        private void RenderActivities(IContainer container, string json, string colorHex)
        {
            var list = JsonSerializer.Deserialize<List<PdfActivity>>(json);
            if (list == null || !list.Any()) return;
            var color = HexToColor(colorHex);
            container.Column(column =>
            {
                column.Item().PaddingBottom(5).Column(c => { c.Item().Text("ACTIVITIES & INTERESTS").FontSize(12).Bold().FontColor(color).LetterSpacing(0.05f); c.Item().PaddingTop(2).Height(1).Background(Colors.Grey.Lighten2); });
                foreach (var item in list)
                {
                    column.Item().PaddingBottom(10).Column(entry => {
                        entry.Item().Row(row => { row.RelativeItem().Text(item.Organization).Bold().FontSize(11).FontColor(Colors.Black); row.AutoItem().Text($"{item.StartDate} - {item.EndDate}").Bold().FontSize(10); });
                        entry.Item().Text(item.Role).Italic().FontSize(10.5f);
                        if (!string.IsNullOrWhiteSpace(item.Description)) entry.Item().PaddingTop(2).Text(item.Description).FontSize(10).LineHeight(1.4f);
                    });
                }
            });
        }

        private void RenderEducation(IContainer container, string json, string colorHex)
        {
            var list = JsonSerializer.Deserialize<List<PdfEdu>>(json);
            if (list == null || !list.Any()) return;
            var color = HexToColor(colorHex);
            container.Column(column => { column.Item().PaddingBottom(5).Column(c => { c.Item().Text("EDUCATION").FontSize(12).Bold().FontColor(color).LetterSpacing(0.05f); c.Item().PaddingTop(2).Height(1).Background(Colors.Grey.Lighten2); }); foreach (var item in list) { column.Item().PaddingBottom(8).Column(entry => { entry.Item().Row(row => { row.RelativeItem().Text(item.Institution).Bold().FontSize(11).FontColor(Colors.Black); row.AutoItem().Text($"{item.StartDate} - {item.EndDate}").Bold().FontSize(10); }); var details = new List<string>(); if (!string.IsNullOrWhiteSpace(item.Degree)) details.Add(item.Degree); if (!string.IsNullOrWhiteSpace(item.FieldOfStudy)) details.Add(item.FieldOfStudy); if (!string.IsNullOrWhiteSpace(item.GPA)) details.Add($"GPA: {item.GPA}"); entry.Item().Text(string.Join(" • ", details)); }); } });
        }

        private void RenderExperience(IContainer container, string json, string colorHex)
        {
            var list = JsonSerializer.Deserialize<List<PdfExp>>(json);
            if (list == null || !list.Any()) return;
            var color = HexToColor(colorHex);
            container.Column(column => { column.Item().PaddingBottom(5).Column(c => { c.Item().Text("EXPERIENCE").FontSize(12).Bold().FontColor(color).LetterSpacing(0.05f); c.Item().PaddingTop(2).Height(1).Background(Colors.Grey.Lighten2); }); foreach (var item in list) { column.Item().PaddingBottom(10).Column(entry => { entry.Item().Row(row => { var title = string.IsNullOrWhiteSpace(item.Location) ? item.Company : $"{item.Company}, {item.Location}"; row.RelativeItem().Text(title).Bold().FontSize(11).FontColor(Colors.Black); row.AutoItem().Text($"{item.StartDate} - {item.EndDate}").Bold().FontSize(10); }); entry.Item().Text(item.Position).Italic().FontSize(10.5f); if (!string.IsNullOrWhiteSpace(item.Description)) entry.Item().PaddingTop(2).Text(item.Description).FontSize(10).LineHeight(1.4f); }); } });
        }

        private void RenderSection(IContainer container, string title, string content, string colorHex)
        {
            if (string.IsNullOrWhiteSpace(content)) return;
            var color = HexToColor(colorHex);
            container.Column(column => { column.Item().PaddingBottom(5).Column(c => { c.Item().Text(title).FontSize(12).Bold().FontColor(color).LetterSpacing(0.05f); c.Item().PaddingTop(2).Height(1).Background(Colors.Grey.Lighten2); }); column.Item().Text(content).LineHeight(1.5f); });
        }

        private Color HexToColor(string hex) { try { return Color.FromHex(hex); } catch { return Colors.Blue.Medium; } }
    }
}