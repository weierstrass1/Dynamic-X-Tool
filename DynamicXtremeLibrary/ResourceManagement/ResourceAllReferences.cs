using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicXtremeLibrary.ResourceManagement
{
    public class ResourceAllReferences
    {
        public required IReadOnlyList<ResourceReference> Buffers { get; init; }
        public required IReadOnlyList<ResourceReference> DynamicPoses { get; init; }
        public required IReadOnlyList<ResourceReference> Palettes { get; init; }
        public required IReadOnlyList<ResourceReference> GeneralResources { get; init; }
    }
}
