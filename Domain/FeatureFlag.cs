using System.ComponentModel.DataAnnotations;

namespace VerticalSliceArchitecture.Domain
{
    public class FeatureFlag
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsEnabled { get; set; }
        public string UserType { get; set; } = "default";
    }
}
