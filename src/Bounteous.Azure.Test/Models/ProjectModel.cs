using System;

namespace Bounteous.Azure.Test.Models
{
    public class ProjectModel
    {
        public Guid ProjectId { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;
    }
}