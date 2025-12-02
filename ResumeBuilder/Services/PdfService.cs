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
        private readonly HttpClient _httpClient;

        // Inject HttpClient to download the profile image
        public PdfService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        // --- HELPERS ---
        private class PdfEdu { public string Institution { get; set; } = ""; public string Degree { get; set; } = ""; public string FieldOfStudy { get; set; } = ""; public string StartDate { get; set; } = ""; public string EndDate { get; set; } = ""; public string GPA { get; set; } = ""; }
        private class PdfExp { public string Company { get; set; } = ""; public string Position { get; set; } = ""; public string Location { get; set; } = ""; public string StartDate { get; set; } = ""; public string EndDate { get; set; } = ""; public string Description { get; set; } = ""; }
        private class PdfProj { public string Name { get; set; } = ""; public string Description { get; set; } = ""; public string Technologies { get; set; } = ""; public string Link { get; set; } = ""; }
        private class PdfCert { public string Name { get; set; } = ""; public string Issuer { get; set; } = ""; public string Date { get; set; } = ""; public string Link { get; set; } = ""; }
        private class PdfAcademic { public string Name { get; set; } = ""; public string Course { get; set; } = ""; public string Grade { get; set; } = ""; public string Description { get; set; } = ""; }
        private class PdfVolunteer { public string Organization { get; set; } = ""; public string Role { get; set; } = ""; public string StartDate { get; set; } = ""; public string EndDate { get; set; } = ""; public string Description { get; set; } = ""; }
        private class PdfActivity { public string Organization { get; set; } = ""; public string Role { get; set; } = ""; public string StartDate { get; set; } = ""; public string EndDate { get; set; } = ""; public string Description { get; set; } = ""; }

        // --- ICONS (Feather Style) ---
        private const string IconEmail = @"<path d=""M4 4h16c1.1 0 2 .9 2 2v12c0 1.1-.9 2-2 2H4c-1.1 0-2-.9-2-2V6c0-1.1.9-2 2-2z"" /><polyline points=""22,6 12,13 2,6"" />";
        private const string IconPhone = @"<path d=""M22 16.92v3a2 2 0 0 1-2.18 2 19.79 19.79 0 0 1-8.63-3.07 19.5 19.5 0 0 1-6-6 19.79 19.79 0 0 1-3.07-8.67A2 2 0 0 1 4.11 2h3a2 2 0 0 1 2 1.72 12.05 12.05 0 0 0 .7 2.81 2 2 0 0 1-.45 2.11L8.09 9.91a16 16 0 0 0 6 6l1.27-1.27a2 2 0 0 1 2.11-.45 12.05 12.05 0 0 0 2.81.7A2 2 0 0 1 22 16.92z"" />";
        private const string IconMap = @"<path d=""M21 10c0 7-9 13-9 13s-9-6-9-13a9 9 0 0 1 18 0z"" /><circle cx=""12"" cy=""10"" r=""3"" />";
        private const string IconLink = @"<path d=""M10 13a5 5 0 0 0 7.54.54l3-3a5 5 0 0 0-7.07-7.07l-1.72 1.71"" /><path d=""M14 11a5 5 0 0 0-7.54-.54l-3 3a5 5 0 0 0 7.07 7.07l1.71-1.71"" />";
        private const string IconGlobe = @"<circle cx=""12"" cy=""12"" r=""10"" /><line x1=""2"" y1=""12"" x2=""22"" y2=""12"" /><path d=""M12 2a15.3 15.3 0 0 1 4 10 15.3 15.3 0 0 1-4 10 15.3 15.3 0 0 1-4-10 15.3 15.3 0 0 1 4-10z"" />";

        public byte[] GenerateResumePDF(Resume resume, ResumeTemplate template)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            // 1. Download Image if URL exists
            byte[]? userImageBytes = null;
            if (!string.IsNullOrEmpty(resume.ProfileImageUrl))
            {
                try
                {
                    // Synchronously block to get the image for PDF generation
                    userImageBytes = _httpClient.GetByteArrayAsync(resume.ProfileImageUrl).Result;
                }
                catch { /* Fallback to initials if download fails */ }
            }

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.Letter);
                    page.Margin(0);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Arial").FontColor(Colors.Grey.Darken3));

                    page.Content().Element(content =>
                    {
                        // Pass image bytes to layouts
                        if (template.Id == 7) RenderSideBarLayout(content, resume, template, userImageBytes);
                        else if (template.Id == 8) RenderExecutiveProfileLayout(content, resume, template, userImageBytes);
                        else if (template.Id == 9) RenderCreativeDesignerLayout(content, resume, template, userImageBytes);
                        else RenderStandardLayout(content, resume, template);
                    });
                });
            });

            return document.GeneratePdf();
        }

        // ==========================================
        // LAYOUT 1: STANDARD (Template 1-6)
        // ==========================================
        private void RenderStandardLayout(IContainer container, Resume resume, ResumeTemplate template)
        {
            var mainColor = HexToColor(template.ColorScheme);
            container.Padding(0.75f, Unit.Inch).Column(col =>
            {
                col.Spacing(12);
                // Name: Bold, Black, Larger
                col.Item().Text(resume.FullName).FontSize(28).Bold().FontColor(Colors.Black).LineHeight(1);

                // FIX: Contact (Above Line)
                col.Item().PaddingTop(6).Row(row =>
                {
                    row.Spacing(15);
                    void Add(string t, string i) { if (!string.IsNullOrWhiteSpace(t)) row.AutoItem().Row(r => { r.ConstantItem(11).Svg(GetIcon(i, "#6B7280")); r.AutoItem().PaddingLeft(4).Text(t).FontSize(9).FontColor(Colors.Grey.Darken1); }); }
                    Add(resume.Email, IconEmail); Add(resume.Phone, IconPhone); Add(resume.Address, IconMap); Add(resume.LinkedIn, IconLink); Add(resume.Website, IconGlobe);
                });

                col.Item().PaddingTop(10).Height(3).Background(mainColor);
                col.Item().PaddingBottom(10);

                // false = No Underline for section headers in Standard template
                RenderBodyContent(col, resume, template, true, false, false); 
            });
        }

        // ==========================================
        // LAYOUT 2: ELEGANT SIDEBAR (Template 7)
        // ==========================================
        private void RenderSideBarLayout(IContainer container, Resume resume, ResumeTemplate template, byte[]? image)
        {
            var mainColor = HexToColor(template.ColorScheme);
            var accentColor = HexToColor("#D4AF37");
            var sideBarBg = HexToColor("#EBEBEB");

            container.Row(row =>
            {
                // Left Sidebar
                row.ConstantItem(200).Background(sideBarBg).Padding(15).Column(col =>
                {
                    col.Spacing(20);
                    // Image or Initials
                    col.Item().AlignCenter().Element(c => {
                        if (image != null) DrawUserImage(c, image, 80, 3);
                        else DrawInitialsBox(c, resume, mainColor, Colors.White, 80, 3);
                    });

                    col.Item().Border(1).BorderColor(Colors.Grey.Lighten1).Padding(10).Column(c =>
                    {
                        c.Spacing(8);
                        c.Item().Text("CONTACT").FontSize(12).FontColor(mainColor).Bold();
                        RenderContactListVertical(c, resume, template.ColorScheme);
                    });

                    RenderEducation(col.Item(), resume.Education, template.ColorScheme, true);
                    RenderSkills(col.Item(), "SKILLS", resume.Skills, template.ColorScheme, true);
                    if(!string.IsNullOrWhiteSpace(resume.SoftSkills)) RenderSkills(col.Item(), "SOFT SKILLS", resume.SoftSkills, template.ColorScheme, true);
                });

                // Right Content
                row.RelativeItem().Column(col =>
                {
                    col.Item().Background(mainColor).Padding(20).Column(h =>
                    {
                        h.Item().AlignCenter().Text(resume.FullName.ToUpper()).FontSize(30).FontColor(Colors.White).Bold().LetterSpacing(0.1f);
                        if (!string.IsNullOrWhiteSpace(resume.Summary)) h.Item().PaddingTop(10).Text(resume.Summary).FontSize(10).FontColor(Colors.White).AlignCenter();
                    });
                    col.Item().Padding(20).Column(body =>
                    {
                        body.Spacing(15);
                        void Head(string t) => body.Item().Background(accentColor).PaddingHorizontal(10).PaddingVertical(3).Text(t).FontSize(12).FontColor(Colors.White).Bold();

                        // Render ALL sections in main body
                        if (!string.IsNullOrWhiteSpace(resume.Experience)) { Head("EMPLOYMENT HISTORY"); RenderExperience(body.Item(), resume.Experience, "#000000", false); }
                        if (!string.IsNullOrWhiteSpace(resume.Projects)) { Head("PROJECTS"); RenderProjects(body.Item(), resume.Projects, "#000000", false); }
                        if (!string.IsNullOrWhiteSpace(resume.Certifications)) { Head("CERTIFICATIONS"); RenderCertifications(body.Item(), resume.Certifications, template.ColorScheme, false, true); }
                        if (!string.IsNullOrWhiteSpace(resume.Academic)) { Head("ACADEMIC"); RenderAcademic(body.Item(), resume.Academic, "#000000", false, false); }
                        if (!string.IsNullOrWhiteSpace(resume.Volunteer)) { Head("VOLUNTEER"); RenderVolunteer(body.Item(), resume.Volunteer, "#000000", false, false); }
                        if (!string.IsNullOrWhiteSpace(resume.Activities)) { Head("ACTIVITIES"); RenderActivities(body.Item(), resume.Activities, "#000000", false, false); }
                    });
                });
            });
        }

        // ==========================================
        // LAYOUT 3: EXECUTIVE PROFILE (Template 8)
        // ==========================================
        private void RenderExecutiveProfileLayout(IContainer container, Resume resume, ResumeTemplate template, byte[]? image)
        {
            var mainColor = HexToColor(template.ColorScheme);
            container.Column(col =>
            {
                col.Item().Background(mainColor).Padding(25).Row(header =>
                {
                    // Image or Initials
                    header.ConstantItem(80).Element(c => {
                        if (image != null) DrawUserImage(c, image, 80, 3);
                        else DrawInitialsBox(c, resume, Colors.White, mainColor, 80, 0);
                    });

                    header.RelativeItem().PaddingLeft(20).Column(info =>
                    {
                        info.Item().Text(resume.FullName.ToUpper()).FontSize(28).FontColor(Colors.White).Bold();
                        info.Item().PaddingTop(8).Row(contact =>
                        {
                            contact.Spacing(15);
                            void Add(string t, string i) { if (!string.IsNullOrWhiteSpace(t)) contact.AutoItem().Row(r => { r.ConstantItem(12).Svg(GetIcon(i, "#FFFFFF")); r.AutoItem().PaddingLeft(4).Text(t).FontSize(9).FontColor(Colors.White); }); }
                            Add(resume.Email, IconEmail); Add(resume.Phone, IconPhone); Add(resume.Address, IconMap);
                        });
                    });
                });
                col.Item().Padding(25).Row(body =>
                {
                    body.ConstantItem(180).Column(left =>
                    {
                        left.Spacing(20);
                        if (!string.IsNullOrWhiteSpace(resume.Summary)) { left.Item().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingBottom(5).Text("PROFILE").FontSize(11).FontColor(mainColor).Bold(); left.Item().PaddingTop(5).Text(resume.Summary).FontSize(9).LineHeight(1.4f); }
                        RenderSkills(left.Item(), "SKILLS", resume.Skills, template.ColorScheme, true);
                        RenderEducation(left.Item(), resume.Education, template.ColorScheme, true);
                    });
                    body.RelativeItem().PaddingLeft(25).BorderLeft(1).BorderColor(Colors.Grey.Lighten3).PaddingLeft(25).Column(right =>
                    {
                        right.Spacing(15);
                        RenderBodyContent(right, resume, template, true, true, true);
                    });
                });
            });
        }

        // ==========================================
        // LAYOUT 4: CREATIVE DESIGNER (Template 9)
        // ==========================================
        private void RenderCreativeDesignerLayout(IContainer container, Resume resume, ResumeTemplate template, byte[]? image)
        {
            var mainColor = HexToColor(template.ColorScheme);
            container.Row(row =>
            {
                row.ConstantItem(210).Background(mainColor).Padding(25).Column(left =>
                {
                    left.Spacing(25);
                    
                    // Image or Initials
                    left.Item().AlignCenter().Element(c => {
                        if (image != null) DrawUserImage(c, image, 100, 0);
                        else DrawInitialsBox(c, resume, Colors.White.WithAlpha(50), Colors.White, 90, 0);
                    });

                    left.Item().Column(c => { c.Item().BorderBottom(1).BorderColor(Colors.White.WithAlpha(128)).PaddingBottom(5).Text("CONTACT").FontSize(11).FontColor(Colors.White).Bold(); c.Item().PaddingTop(10).Column(con => { con.Spacing(8); void Add(string t, string i) { if (!string.IsNullOrWhiteSpace(t)) con.Item().Row(r => { r.ConstantItem(12).Svg(GetIcon(i, "#FFFFFF")); r.RelativeItem().PaddingLeft(5).Text(t).FontSize(9).FontColor(Colors.White); }); } Add(resume.Email, IconEmail); Add(resume.Phone, IconPhone); Add(resume.Address, IconMap); Add(resume.LinkedIn, IconLink); }); });
                    if (!string.IsNullOrWhiteSpace(resume.Skills)) { left.Item().Column(c => { c.Item().BorderBottom(1).BorderColor(Colors.White.WithAlpha(128)).PaddingBottom(5).Text("SKILLS").FontSize(11).FontColor(Colors.White).Bold(); var s = DeserializeList<string>(resume.Skills, true); c.Item().PaddingTop(10).Inlined(i => { i.Spacing(5); i.VerticalSpacing(5); if (s != null) foreach (var sk in s) i.Item().Background(Colors.White.WithAlpha(50)).PaddingVertical(2).PaddingHorizontal(6).Text(sk).FontSize(9).FontColor(Colors.White); }); }); }
                });
                row.RelativeItem().Padding(30).Column(right =>
                {
                    right.Spacing(15);
                    right.Item().Column(c => { c.Item().Text(resume.FullName.ToUpper()).FontSize(32).FontColor(mainColor).Bold(); c.Item().PaddingTop(5).Width(50).Height(4).Background(mainColor); });
                    if (!string.IsNullOrWhiteSpace(resume.Summary)) { right.Item().Text("ABOUT ME").FontSize(11).FontColor(mainColor).Bold(); right.Item().Text(resume.Summary).FontSize(10); }
                    RenderBodyContent(right, resume, template, true, true, true);
                });
            });
        }

        // --- SHARED RENDERER CONTROLLER ---
        private void RenderBodyContent(ColumnDescriptor col, Resume resume, ResumeTemplate template, bool showHeaders, bool skipSidebar = false, bool underlineHeaders = true)
        {
            string color = template.ColorScheme;

            if (!string.IsNullOrWhiteSpace(resume.Experience)) RenderExperience(col.Item(), resume.Experience, color, showHeaders, underlineHeaders);
            if (!string.IsNullOrWhiteSpace(resume.Education) && !skipSidebar) RenderEducation(col.Item(), resume.Education, color, false);
            if (!string.IsNullOrWhiteSpace(resume.Skills) && !skipSidebar) RenderSkills(col.Item(), "TECHNICAL SKILLS", resume.Skills, color, showHeaders, underlineHeaders);
            if (!string.IsNullOrWhiteSpace(resume.SoftSkills) && !skipSidebar) RenderSkills(col.Item(), "SOFT SKILLS", resume.SoftSkills, color, showHeaders, underlineHeaders);
            if (!string.IsNullOrWhiteSpace(resume.Projects)) RenderProjects(col.Item(), resume.Projects, color, showHeaders, underlineHeaders);
            if (!string.IsNullOrWhiteSpace(resume.Certifications)) RenderCertifications(col.Item(), resume.Certifications, color, showHeaders, false, underlineHeaders);
            if (!string.IsNullOrWhiteSpace(resume.Academic)) RenderAcademic(col.Item(), resume.Academic, color, showHeaders, underlineHeaders);
            if (!string.IsNullOrWhiteSpace(resume.Volunteer)) RenderVolunteer(col.Item(), resume.Volunteer, color, showHeaders, underlineHeaders);
            if (!string.IsNullOrWhiteSpace(resume.Activities)) RenderActivities(col.Item(), resume.Activities, color, showHeaders, underlineHeaders);
        }

        // --- SECTION RENDERERS ---
        private void RenderExperience(IContainer container, string json, string colorHex, bool showHeader, bool underline = true)
        {
            var list = DeserializeList<PdfExp>(json);
            if (list == null || !list.Any()) return;
            container.Column(col => {
                if (showHeader) AddSectionTitle(col, "EXPERIENCE", colorHex, underline);
                foreach (var item in list)
                {
                    col.Item().PaddingBottom(10).Column(e => {
                        e.Item().Row(r => {
                            r.RelativeItem().Text(item.Position).Bold().FontSize(11);
                            r.AutoItem().Text($"{item.StartDate} - {item.EndDate}".Trim(' ', '-')).FontSize(9).Italic().FontColor(Colors.Grey.Darken2);
                        });
                        e.Item().Text(string.IsNullOrWhiteSpace(item.Location) ? item.Company : $"{item.Company}, {item.Location}").FontSize(10).FontColor(Colors.Grey.Darken3).Medium();
                        if (!string.IsNullOrWhiteSpace(item.Description)) e.Item().Text(item.Description).FontSize(10).LineHeight(1.4f);
                    });
                }
            });
        }

        private void RenderProjects(IContainer container, string json, string colorHex, bool showHeader, bool underline = true)
        {
            var list = DeserializeList<PdfProj>(json);
            if (list == null || !list.Any()) return;
            container.Column(col => {
                if (showHeader) AddSectionTitle(col, "PROJECTS", colorHex, underline);
                foreach (var item in list)
                {
                    col.Item().PaddingBottom(8).Column(e => {
                        e.Item().Text(t => { t.Span(item.Name).Bold().FontSize(11); if (!string.IsNullOrWhiteSpace(item.Link)) t.Span("  🔗").FontSize(9).FontColor(HexToColor(colorHex)); });
                        if (!string.IsNullOrWhiteSpace(item.Technologies)) e.Item().Text($"Tech: {item.Technologies}").FontSize(9).Italic().FontColor(Colors.Grey.Darken2);
                        if (!string.IsNullOrWhiteSpace(item.Description)) e.Item().Text(item.Description).FontSize(10).LineHeight(1.4f);
                    });
                }
            });
        }

        private void RenderEducation(IContainer container, string json, string colorHex, bool sidebarMode)
        {
            var list = DeserializeList<PdfEdu>(json);
            if (list == null || !list.Any()) return;
            container.Column(col => {
                if (!sidebarMode) AddSectionTitle(col, "EDUCATION", colorHex, false);
                else col.Item().Text("EDUCATION").FontSize(12).FontColor(HexToColor(colorHex)).Bold();

                foreach (var item in list)
                {
                    col.Item().PaddingBottom(6).Column(e => {
                        if (sidebarMode)
                        {
                            e.Item().Text(item.Degree).Bold().FontSize(10);
                            e.Item().Text(item.Institution).FontSize(9);
                            e.Item().Text($"{item.StartDate} - {item.EndDate}".Trim(' ', '-')).FontSize(8).Italic().FontColor(Colors.Grey.Darken1);
                        }
                        else
                        {
                            e.Item().Row(r => { r.RelativeItem().Text(item.Institution).Bold().FontSize(11); r.AutoItem().Text($"{item.StartDate} - {item.EndDate}".Trim(' ', '-')).FontSize(9); });
                            e.Item().Text($"{item.Degree} in {item.FieldOfStudy} • GPA: {item.GPA}".Trim(' ', '•')).FontSize(10);
                        }
                    });
                }
            });
        }

        private void RenderSkills(IContainer container, string title, string csv, string colorHex, bool showHeader, bool underline = true)
        {
            var skills = DeserializeList<string>(csv, true);
            if (skills == null || !skills.Any()) return;
            container.Column(col => {
                if (showHeader) AddSectionTitle(col, title, colorHex, underline);
                col.Item().PaddingTop(3).Inlined(i => {
                    i.Spacing(5); i.VerticalSpacing(5);
                    foreach (var s in skills) i.Item().Background(HexToColor(colorHex).WithAlpha(30)).PaddingVertical(2).PaddingHorizontal(6).Text(s).FontSize(9);
                });
            });
        }

        private void RenderCertifications(IContainer container, string json, string colorHex, bool showHeader, bool blockStyle, bool underline = true)
        {
            var list = DeserializeList<PdfCert>(json);
            if (list == null || !list.Any()) return;
            container.Column(col => {
                if (showHeader) AddSectionTitle(col, "CERTIFICATIONS", colorHex, underline);
                foreach (var item in list)
                {
                    if (blockStyle)
                    {
                        col.Item().PaddingBottom(4).Background(HexToColor(colorHex)).Padding(8).Column(c => { c.Item().Text(item.Name).Bold().FontSize(10).FontColor(Colors.White); c.Item().Text($"{item.Issuer} | {item.Date}".Trim(' ', '|')).FontSize(8).FontColor(Colors.White); });
                    }
                    else
                    {
                        col.Item().PaddingBottom(4).Row(r => { r.RelativeItem().Text($"{item.Name} - {item.Issuer}".Trim(' ', '-')).Bold().FontSize(10); if (!string.IsNullOrWhiteSpace(item.Date)) r.AutoItem().Text(item.Date).FontSize(9); });
                        if (!string.IsNullOrWhiteSpace(item.Link)) col.Item().PaddingLeft(5).Text(item.Link).FontSize(9).FontColor(HexToColor(colorHex));
                    }
                }
            });
        }

        private void RenderAcademic(IContainer c, string j, string h, bool u, bool under = true) => RenderGeneric<PdfAcademic>(c, j, h, "ACADEMIC", u, under, i => i.Name, i => i.Description, i => { string t = i.Course; if (!string.IsNullOrWhiteSpace(i.Grade)) t += $" • Grade: {i.Grade}"; return t; });
        private void RenderVolunteer(IContainer c, string j, string h, bool u, bool under = true) => RenderGeneric<PdfVolunteer>(c, j, h, "VOLUNTEER", u, under, i => i.Organization, i => i.Description, i => $"{i.Role} ({i.StartDate}-{i.EndDate})".Replace("()", "").Trim());
        private void RenderActivities(IContainer c, string j, string h, bool u, bool under = true) => RenderGeneric<PdfActivity>(c, j, h, "ACTIVITIES", u, under, i => i.Organization, i => i.Description, i => $"{i.Role} ({i.StartDate}-{i.EndDate})".Replace("()", "").Trim());

        private void RenderGeneric<T>(IContainer container, string json, string colorHex, string title, bool showHeader, bool underline, Func<T, string> getName, Func<T, string> getDesc, Func<T, string> getSub)
        {
            var list = DeserializeList<T>(json);
            if (list == null || !list.Any()) return;
            container.Column(col => {
                if (showHeader) AddSectionTitle(col, title, colorHex, underline);
                foreach (var item in list)
                {
                    col.Item().PaddingBottom(6).Column(e => {
                        e.Item().Text(getName(item)).Bold().FontSize(11);
                        var sub = getSub(item); if (!string.IsNullOrEmpty(sub)) e.Item().Text(sub).FontSize(10).Italic().FontColor(Colors.Grey.Darken2);
                        var desc = getDesc(item); if (!string.IsNullOrEmpty(desc)) e.Item().Text(desc).FontSize(10);
                    });
                }
            });
        }

        // --- UTILITIES ---
        private void AddSectionTitle(ColumnDescriptor col, string title, string colorHex, bool underline)
        {
            col.Item().PaddingBottom(4).Column(c => {
                c.Item().Text(title).FontSize(12).Bold().FontColor(HexToColor(colorHex)).LetterSpacing(0.05f);
                if (underline) c.Item().Height(1).Background(Colors.Grey.Lighten2);
            });
        }

        private void RenderContactListVertical(ColumnDescriptor col, Resume r, string color)
        {
            void Add(string t, string i) { if (!string.IsNullOrWhiteSpace(t)) col.Item().Row(row => { row.ConstantItem(15).Svg(GetIcon(i, color)); row.RelativeItem().PaddingLeft(5).Text(t).FontSize(9); }); }
            Add(r.Email, IconEmail); Add(r.Phone, IconPhone); Add(r.Address, IconMap); Add(r.LinkedIn, IconLink);
        }

        private void DrawInitialsBox(IContainer container, Resume r, Color bgColor, Color textColor, float size, float border = 0)
        {
            var initials = string.IsNullOrEmpty(r.FullName) ? "U" : r.FullName.Substring(0, 1).ToUpper();
            var box = container.Width(size).Height(size).Background(bgColor);
            if (border > 0) box = box.Border(border).BorderColor(Colors.White);
            box.AlignCenter().AlignMiddle().Text(initials).FontSize(size / 2.5f).FontColor(textColor).Bold();
        }
        
        private void DrawUserImage(IContainer container, byte[] imageBytes, float size, float border = 0)
        {
            var box = container.Width(size).Height(size);
            if (border > 0) box = box.Border(border).BorderColor(Colors.White);
            // FitArea maintains aspect ratio while filling the square
            box.Image(imageBytes).FitArea();
        }

        private string GetIcon(string path, string colorHex) => $@"<svg viewBox=""0 0 24 24"" fill=""none"" stroke=""{colorHex}"" stroke-width=""2"" stroke-linecap=""round"" stroke-linejoin=""round"" xmlns=""http://www.w3.org/2000/svg"">{path}</svg>";
        private Color HexToColor(string hex) { try { return Color.FromHex(hex); } catch { return Colors.Blue.Medium; } }
        private List<T>? DeserializeList<T>(string json, bool isCsv = false)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(json)) return null;
                if (isCsv && !json.Trim().StartsWith("[")) return (List<T>)(object)json.Split(',').Select(s => s.Trim()).Where(s => !string.IsNullOrEmpty(s)).ToList();
                return JsonSerializer.Deserialize<List<T>>(json);
            }
            catch { return null; }
        }
    }
}