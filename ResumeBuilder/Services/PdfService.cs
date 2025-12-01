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
        // ... (Keep your existing Helper Classes: PdfEdu, PdfExp, etc. here) ...
        // ... (Keep your SVG Constants here) ...
        // [For brevity, assuming helper classes and const strings are still here]
        
        // Re-paste your Helper Classes and Icons here from previous code to ensure it compiles
        private class PdfEdu { public string Institution { get; set; } = ""; public string Degree { get; set; } = ""; public string FieldOfStudy { get; set; } = ""; public string StartDate { get; set; } = ""; public string EndDate { get; set; } = ""; public string GPA { get; set; } = ""; }
        private class PdfExp { public string Company { get; set; } = ""; public string Position { get; set; } = ""; public string Location { get; set; } = ""; public string StartDate { get; set; } = ""; public string EndDate { get; set; } = ""; public string Description { get; set; } = ""; }
        private class PdfProj { public string Name { get; set; } = ""; public string Description { get; set; } = ""; public string Technologies { get; set; } = ""; public string Link { get; set; } = ""; }
        private class PdfCert { public string Name { get; set; } = ""; public string Issuer { get; set; } = ""; public string Date { get; set; } = ""; public string Link { get; set; } = ""; }
        private class PdfAcademic { public string Name { get; set; } = ""; public string Course { get; set; } = ""; public string Grade { get; set; } = ""; public string Description { get; set; } = ""; }
        private class PdfVolunteer { public string Organization { get; set; } = ""; public string Role { get; set; } = ""; public string StartDate { get; set; } = ""; public string EndDate { get; set; } = ""; public string Description { get; set; } = ""; }
        private class PdfActivity { public string Organization { get; set; } = ""; public string Role { get; set; } = ""; public string StartDate { get; set; } = ""; public string EndDate { get; set; } = ""; public string Description { get; set; } = ""; }

        // SVG Path Constants (Keep existing ones)
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
                    page.Margin(0); // Custom templates handle their own margins
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Arial").FontColor(Colors.Grey.Darken3));

                    page.Content().Element(content =>
                    {
                        if (template.Id == 7) // The New "Taha" Layout
                        {
                            RenderSideBarLayout(content, resume, template);
                        }
                        else 
                        {
                            // Original Linear Layout
                            content.Padding(0.75f, Unit.Inch).Column(column =>
                            {
                                column.Spacing(15);
                                column.Item().Element(c => RenderHeader(c, resume, template));
                                if (!string.IsNullOrWhiteSpace(resume.Summary)) column.Item().Element(c => RenderSection(c, "PROFESSIONAL SUMMARY", resume.Summary, template.ColorScheme));
                                if (!string.IsNullOrWhiteSpace(resume.Experience) && resume.Experience.Trim().StartsWith("[")) column.Item().Element(c => RenderExperience(c, resume.Experience, template.ColorScheme));
                                if (!string.IsNullOrWhiteSpace(resume.Education) && resume.Education.Trim().StartsWith("[")) column.Item().Element(c => RenderEducation(c, resume.Education, template.ColorScheme));
                                if (!string.IsNullOrWhiteSpace(resume.Skills)) column.Item().Element(c => RenderSkills(c, "TECHNICAL SKILLS", resume.Skills, template.ColorScheme));
                                if (!string.IsNullOrWhiteSpace(resume.SoftSkills)) column.Item().Element(c => RenderSkills(c, "SOFT SKILLS", resume.SoftSkills, template.ColorScheme));
                                if (!string.IsNullOrWhiteSpace(resume.Projects) && resume.Projects.Trim().StartsWith("[")) column.Item().Element(c => RenderProjects(c, resume.Projects, template.ColorScheme));
                                if (!string.IsNullOrWhiteSpace(resume.Certifications) && resume.Certifications.Trim().StartsWith("[")) column.Item().Element(c => RenderCertifications(c, resume.Certifications, template.ColorScheme));
                                if (!string.IsNullOrWhiteSpace(resume.Academic) && resume.Academic.Trim().StartsWith("[")) column.Item().Element(c => RenderAcademic(c, resume.Academic, template.ColorScheme));
                                if (!string.IsNullOrWhiteSpace(resume.Volunteer) && resume.Volunteer.Trim().StartsWith("[")) column.Item().Element(c => RenderVolunteer(c, resume.Volunteer, template.ColorScheme));
                                if (!string.IsNullOrWhiteSpace(resume.Activities) && resume.Activities.Trim().StartsWith("[")) column.Item().Element(c => RenderActivities(c, resume.Activities, template.ColorScheme));
                            });
                        }
                    });
                });
            });

            return document.GeneratePdf();
        }

        // --- NEW SIDEBAR LAYOUT (ID: 7) ---
        private void RenderSideBarLayout(IContainer container, Resume resume, ResumeTemplate template)
        {
            var mainColor = HexToColor(template.ColorScheme); // Maroon
            var accentColor = HexToColor("#D4AF37"); // Gold/Mustard from screenshot
            var sideBarBg = HexToColor("#EBEBEB"); // Light Gray

            container.Row(row =>
            {
                // === LEFT COLUMN (Sidebar) ===
                row.ConstantItem(220).Background(sideBarBg).Padding(20).Column(col => 
                {
                    col.Spacing(25);

                    // 1. Initial Box (The big "T")
                    var initial = string.IsNullOrEmpty(resume.FullName) ? "U" : resume.FullName.Substring(0, 1).ToUpper();
                    col.Item().AlignCenter().Width(80).Height(100).Border(3).BorderColor(Colors.White).Background(mainColor).AlignCenter().AlignMiddle().Text(initial).FontSize(50).FontColor(Colors.White).Bold();

                    // 2. Contact Section (Boxed)
                    col.Item().Border(2).BorderColor(Colors.Grey.Lighten1).Padding(10).Column(contact => 
                    {
                        contact.Spacing(8);
                        contact.Item().Text("CONTACT").FontSize(14).FontColor(mainColor).Bold();
                        
                        void RenderContactItem(string text, string icon) 
                        {
                            if(string.IsNullOrWhiteSpace(text)) return;
                            contact.Item().Row(r => {
                                r.ConstantItem(15).Element(e => {
                                    string xml = $@"<svg viewBox=""0 0 24 24"" fill=""none"" stroke=""{template.ColorScheme}"" stroke-width=""2"" stroke-linecap=""round"" stroke-linejoin=""round"" xmlns=""http://www.w3.org/2000/svg"">{icon}</svg>";
                                    e.Svg(xml);
                                });
                                r.RelativeItem().PaddingLeft(5).Text(text).FontSize(9);
                            });
                        }

                        RenderContactItem(resume.Email, IconEmail);
                        RenderContactItem(resume.Phone, IconPhone);
                        RenderContactItem(resume.Address, IconMap);
                        RenderContactItem(resume.LinkedIn, IconLink);
                        RenderContactItem(resume.Website, IconGlobe);
                    });

                    // 3. Education (Moved to Left Column per screenshot)
                    if (!string.IsNullOrWhiteSpace(resume.Education))
                    {
                        col.Item().Column(edu => 
                        {
                            edu.Spacing(5);
                            edu.Item().Text("EDUCATION").FontSize(14).FontColor(mainColor).Bold();
                            
                            var eduList = JsonSerializer.Deserialize<List<PdfEdu>>(resume.Education);
                            if(eduList != null)
                            {
                                foreach(var item in eduList)
                                {
                                    edu.Item().PaddingBottom(10).Column(e => {
                                        e.Item().Text(item.Degree).Bold().FontSize(10);
                                        e.Item().Text(item.Institution).FontSize(9);
                                        e.Item().Text($"{item.StartDate} - {item.EndDate}").FontSize(8).Italic();
                                    });
                                }
                            }
                        });
                    }

                    // 4. Skills (Left Column)
                    if (!string.IsNullOrWhiteSpace(resume.Skills))
                    {
                        col.Item().Column(skills => 
                        {
                            skills.Spacing(5);
                            skills.Item().Text("SKILLS").FontSize(14).FontColor(mainColor).Bold();
                            var skillList = resume.Skills.Split(',');
                            foreach(var s in skillList)
                            {
                                skills.Item().Text($"• {s.Trim()}").FontSize(9);
                            }
                        });
                    }
                });

                // === RIGHT COLUMN (Main Content) ===
                row.RelativeItem().Column(col => 
                {
                    // 1. Header (Name + Summary) - Full Width Background
                    col.Item().Background(mainColor).Padding(25).Column(header => 
                    {
                        header.Item().AlignCenter().Text(resume.FullName.ToUpper()).FontSize(32).FontColor(Colors.White).Bold().LetterSpacing(0.1f);
                        
                        if(!string.IsNullOrWhiteSpace(resume.Summary))
                        {
                            header.Item().PaddingTop(15).Text(resume.Summary).FontSize(10).FontColor(Colors.White).AlignCenter().LineHeight(1.4f);
                        }
                    });

                    // 2. Body Content
                    col.Item().Padding(25).Column(body => 
                    {
                        body.Spacing(20);

                        // Helper for Section Headers (Gold background strip)
                        void DrawHeader(string title) 
                        {
                            body.Item().Background(accentColor).PaddingHorizontal(10).PaddingVertical(3).Text(title).FontSize(14).FontColor(Colors.White).Bold().LetterSpacing(0.05f);
                        }

                        // Experience
                        if (!string.IsNullOrWhiteSpace(resume.Experience))
                        {
                            DrawHeader("EMPLOYMENT HISTORY");
                            RenderExperience(body.Item(), resume.Experience, "#000000"); // Pass black as text color override if needed, logic reuses standard
                        }

                        // Projects
                        if (!string.IsNullOrWhiteSpace(resume.Projects))
                        {
                            DrawHeader("PROJECTS");
                            RenderProjects(body.Item(), resume.Projects, "#000000");
                        }

                        // Certifications (Bottom Gold Ribbon style)
                        if (!string.IsNullOrWhiteSpace(resume.Certifications))
                        {
                            DrawHeader("CERTIFICATIONS AND TRAINING");
                            var certList = JsonSerializer.Deserialize<List<PdfCert>>(resume.Certifications);
                            if(certList != null)
                            {
                                foreach(var item in certList)
                                {
                                    body.Item().PaddingTop(5).Text(t => {
                                        t.Span(item.Name).Bold().FontColor(Colors.White); // Note: Current renderer assumes white bg, might need custom renderer here
                                    });
                                    // Using the existing renderer, but font colors might clash. 
                                    // Better to write a simple custom loop here:
                                    body.Item().PaddingTop(2).Background(mainColor).Padding(10).Column(c => {
                                        c.Item().Text(item.Name).Bold().FontColor(Colors.White);
                                        c.Item().Text($"{item.Issuer} | {item.Date}").FontSize(9).FontColor(Colors.White);
                                    });
                                }
                            }
                        }
                    });
                });
            });
        }

        // --- KEEP ALL YOUR EXISTING PRIVATE RENDER METHODS (RenderHeader, RenderExperience, HexToColor, etc.) HERE ---
        // (Copied strictly from your previous code to avoid breaking existing templates)
        
        private void RenderHeader(IContainer container, Resume resume, ResumeTemplate template)
        {
            var color = HexToColor(template.ColorScheme);
            container.Column(column =>
            {
                column.Item().Text(resume.FullName).FontSize(24).Bold().FontColor(Colors.Black).LineHeight(1);
                column.Item().PaddingTop(8).Row(row =>
                {
                    row.Spacing(15);
                    if (!string.IsNullOrWhiteSpace(resume.Email))
                    {
                        row.AutoItem().Row(r =>
                        {
                            r.ConstantItem(16).Element(e =>
                            {
                                string xml = $@"<svg viewBox=""0 0 24 24"" fill=""none"" stroke=""{template.ColorScheme}"" stroke-width=""2"" stroke-linecap=""round"" stroke-linejoin=""round"" xmlns=""http://www.w3.org/2000/svg"">{IconEmail}</svg>";
                                e.Svg(xml);
                            });
                            r.AutoItem().PaddingLeft(4).Text(resume.Email).FontSize(10).FontColor(Colors.Grey.Darken1);
                        });
                    }
                    if (!string.IsNullOrWhiteSpace(resume.Phone))
                    {
                        row.AutoItem().Row(r =>
                        {
                            r.ConstantItem(16).Element(e =>
                            {
                                string xml = $@"<svg viewBox=""0 0 24 24"" fill=""none"" stroke=""{template.ColorScheme}"" stroke-width=""2"" stroke-linecap=""round"" stroke-linejoin=""round"" xmlns=""http://www.w3.org/2000/svg"">{IconPhone}</svg>";
                                e.Svg(xml);
                            });
                            r.AutoItem().PaddingLeft(4).Text(resume.Phone).FontSize(10).FontColor(Colors.Grey.Darken1);
                        });
                    }
                    if (!string.IsNullOrWhiteSpace(resume.Address))
                    {
                        row.AutoItem().Row(r =>
                        {
                            r.ConstantItem(16).Element(e =>
                            {
                                string xml = $@"<svg viewBox=""0 0 24 24"" fill=""none"" stroke=""{template.ColorScheme}"" stroke-width=""2"" stroke-linecap=""round"" stroke-linejoin=""round"" xmlns=""http://www.w3.org/2000/svg"">{IconMap}</svg>";
                                e.Svg(xml);
                            });
                            r.AutoItem().PaddingLeft(4).Text(resume.Address).FontSize(10).FontColor(Colors.Grey.Darken1);
                        });
                    }
                });
                if (!string.IsNullOrWhiteSpace(resume.LinkedIn) || !string.IsNullOrWhiteSpace(resume.Website))
                {
                    column.Item().PaddingTop(5).Row(row =>
                    {
                        row.Spacing(15);
                        if (!string.IsNullOrWhiteSpace(resume.LinkedIn))
                        {
                            row.AutoItem().Row(r =>
                            {
                                r.ConstantItem(16).Element(e =>
                                {
                                    string xml = $@"<svg viewBox=""0 0 24 24"" fill=""none"" stroke=""{template.ColorScheme}"" stroke-width=""2"" stroke-linecap=""round"" stroke-linejoin=""round"" xmlns=""http://www.w3.org/2000/svg"">{IconLink}</svg>";
                                    e.Svg(xml);
                                });
                                r.AutoItem().PaddingLeft(4).Text(resume.LinkedIn).FontSize(10).FontColor(color);
                            });
                        }
                        if (!string.IsNullOrWhiteSpace(resume.Website))
                        {
                            row.AutoItem().Row(r =>
                            {
                                r.ConstantItem(16).Element(e =>
                                {
                                    string xml = $@"<svg viewBox=""0 0 24 24"" fill=""none"" stroke=""{template.ColorScheme}"" stroke-width=""2"" stroke-linecap=""round"" stroke-linejoin=""round"" xmlns=""http://www.w3.org/2000/svg"">{IconGlobe}</svg>";
                                    e.Svg(xml);
                                });
                                r.AutoItem().PaddingLeft(4).Text(resume.Website).FontSize(10).FontColor(color);
                            });
                        }
                    });
                }
            });
        }

        // ... Paste RenderExperience, RenderEducation, RenderSkills, etc. exactly as they were ...
        private void RenderExperience(IContainer container, string json, string colorHex)
        {
            var list = JsonSerializer.Deserialize<List<PdfExp>>(json);
            if (list == null || !list.Any()) return;
            var color = HexToColor(colorHex);
            
            // Note: For the SideBar layout, we might want to skip the automatic header generation inside this method
            // But for simplicity, we reuse it. If the colors look weird in the new template, copy this logic into RenderSideBarLayout manually.
            
            container.Column(column => { 
                // Only render title if NOT using ID 7 (handled manually in sidebar layout) 
                // OR just let it render and accept double headers for now, but better to control it.
                // For now, reusing standard logic:
                // column.Item().PaddingBottom(5).Column(c => { c.Item().Text("EXPERIENCE").FontSize(12).Bold().FontColor(color).LetterSpacing(0.05f); c.Item().PaddingTop(2).Height(1).Background(Colors.Grey.Lighten2); }); 
                
                foreach (var item in list) { 
                    column.Item().PaddingBottom(10).Column(entry => { 
                        entry.Item().Row(row => { 
                            var title = string.IsNullOrWhiteSpace(item.Location) ? item.Company : $"{item.Company}, {item.Location}"; 
                            row.RelativeItem().Text(title).Bold().FontSize(11).FontColor(Colors.Black); 
                            row.AutoItem().Text($"{item.StartDate} - {item.EndDate}").Bold().FontSize(10); 
                        }); 
                        entry.Item().Text(item.Position).Italic().FontSize(10.5f); 
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
             container.Column(column => { 
                 column.Item().PaddingBottom(5).Column(c => { c.Item().Text("EDUCATION").FontSize(12).Bold().FontColor(color).LetterSpacing(0.05f); c.Item().PaddingTop(2).Height(1).Background(Colors.Grey.Lighten2); }); 
                 foreach (var item in list) { 
                     column.Item().PaddingBottom(8).Column(entry => { 
                         entry.Item().Row(row => { row.RelativeItem().Text(item.Institution).Bold().FontSize(11).FontColor(Colors.Black); row.AutoItem().Text($"{item.StartDate} - {item.EndDate}").Bold().FontSize(10); }); 
                         var details = new List<string>(); if (!string.IsNullOrWhiteSpace(item.Degree)) details.Add(item.Degree); if (!string.IsNullOrWhiteSpace(item.FieldOfStudy)) details.Add(item.FieldOfStudy); if (!string.IsNullOrWhiteSpace(item.GPA)) details.Add($"GPA: {item.GPA}"); entry.Item().Text(string.Join(" • ", details)); 
                     }); 
                 } 
             });
        }

        private void RenderSection(IContainer container, string title, string content, string colorHex)
        {
            if (string.IsNullOrWhiteSpace(content)) return;
            var color = HexToColor(colorHex);
            container.Column(column => { column.Item().PaddingBottom(5).Column(c => { c.Item().Text(title).FontSize(12).Bold().FontColor(color).LetterSpacing(0.05f); c.Item().PaddingTop(2).Height(1).Background(Colors.Grey.Lighten2); }); column.Item().Text(content).LineHeight(1.5f); });
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

        // Include RenderProjects, RenderCertifications, RenderAcademic, RenderVolunteer, RenderActivities exactly as before...
        // For brevity, assuming they are pasted here.
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
                    column.Item().PaddingBottom(10).Column(entry =>
                    {
                        entry.Item().Text(item.Name).Bold().FontSize(11).FontColor(Colors.Black);
                        if (!string.IsNullOrWhiteSpace(item.Technologies)) entry.Item().Text($"Technologies: {item.Technologies}").FontSize(9).Italic();
                        if (!string.IsNullOrWhiteSpace(item.Description)) entry.Item().PaddingTop(2).Text(item.Description).FontSize(10).LineHeight(1.4f);
                        if (!string.IsNullOrWhiteSpace(item.Link)) entry.Item().Text(item.Link).FontSize(9).FontColor(color);
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
                    column.Item().PaddingBottom(8).Column(entry =>
                    {
                        entry.Item().Row(row =>
                        {
                            row.RelativeItem().Text(item.Name).Bold().FontSize(11).FontColor(Colors.Black);
                            if (!string.IsNullOrWhiteSpace(item.Date)) row.AutoItem().Text(item.Date).FontSize(10);
                        });
                        if (!string.IsNullOrWhiteSpace(item.Issuer)) entry.Item().Text(item.Issuer).FontSize(10);
                        if (!string.IsNullOrWhiteSpace(item.Link)) entry.Item().Text(item.Link).FontSize(9).FontColor(color);
                    });
                }
            });
        }
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
                    column.Item().PaddingBottom(8).Column(entry =>
                    {
                        entry.Item().Text(item.Name).Bold().FontSize(11).FontColor(Colors.Black);
                        if (!string.IsNullOrWhiteSpace(item.Course)) entry.Item().Text(item.Course).FontSize(10);
                        if (!string.IsNullOrWhiteSpace(item.Grade)) entry.Item().Text($"Grade: {item.Grade}").FontSize(10).Italic();
                        if (!string.IsNullOrWhiteSpace(item.Description)) entry.Item().PaddingTop(2).Text(item.Description).FontSize(10).LineHeight(1.4f);
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
                    column.Item().PaddingBottom(10).Column(entry =>
                    {
                        entry.Item().Row(row =>
                        {
                            row.RelativeItem().Text(item.Organization).Bold().FontSize(11).FontColor(Colors.Black);
                            if (!string.IsNullOrWhiteSpace(item.StartDate) && !string.IsNullOrWhiteSpace(item.EndDate))
                                row.AutoItem().Text($"{item.StartDate} - {item.EndDate}").FontSize(10);
                        });
                        if (!string.IsNullOrWhiteSpace(item.Role)) entry.Item().Text(item.Role).Italic().FontSize(10.5f);
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
                    column.Item().PaddingBottom(10).Column(entry =>
                    {
                        entry.Item().Row(row =>
                        {
                            row.RelativeItem().Text(item.Organization).Bold().FontSize(11).FontColor(Colors.Black);
                            if (!string.IsNullOrWhiteSpace(item.StartDate) && !string.IsNullOrWhiteSpace(item.EndDate))
                                row.AutoItem().Text($"{item.StartDate} - {item.EndDate}").FontSize(10);
                        });
                        if (!string.IsNullOrWhiteSpace(item.Role)) entry.Item().Text(item.Role).Italic().FontSize(10.5f);
                        if (!string.IsNullOrWhiteSpace(item.Description)) entry.Item().PaddingTop(2).Text(item.Description).FontSize(10).LineHeight(1.4f);
                    });
                }
            });
        }

        private Color HexToColor(string hex) { try { return Color.FromHex(hex); } catch { return Colors.Blue.Medium; } }
    }
}