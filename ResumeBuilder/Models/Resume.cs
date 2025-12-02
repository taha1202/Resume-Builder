using System;
using System.Collections.Generic;
using Newtonsoft.Json; // CRITICAL for Cosmos DB mapping

namespace ResumeBuilder.Models
{
    public class Resume
    {
        // 1. Cosmos DB REQUIRED property. Must be lowercase "id" in JSON.
        [JsonProperty("id")]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        // 2. Partition Key. We group data by User ID for speed.
        [JsonProperty("UserId")]
        public string UserId { get; set; } = "";

        public int TemplateId { get; set; }
        public string Title { get; set; } = "Untitled Resume";

        // Personal Information
        public string FullName { get; set; } = "";
        public string Email { get; set; } = "";
        public string Phone { get; set; } = "";
        public string Address { get; set; } = "";
        public string LinkedIn { get; set; } = "";
        public string Website { get; set; } = "";

        // Sections (Stored as JSON strings to simplify DB structure)
        public string Summary { get; set; } = "";
        public string Experience { get; set; } = "[]";
        public string Education { get; set; } = "[]";
        public string Skills { get; set; } = "";
        public string Projects { get; set; } = "[]";
        public string SoftSkills { get; set; } = "";
        public string Certifications { get; set; } = "[]";
        public string Academic { get; set; } = "[]";
        public string Volunteer { get; set; } = "[]";
        public string Activities { get; set; } = "[]";

        public string? ProfileImageUrl { get; set; }

        // Metadata
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}