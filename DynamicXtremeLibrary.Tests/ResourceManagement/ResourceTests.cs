using DynamicXtremeLibrary.ResourceManagement;

namespace DynamicXtremeLibrary.Tests.ResourceManagement;

public sealed class ResourceTests
{
    [Fact]
    public void Resource_ExposesIdentityLengthAndDiagnosticString()
    {
        Resource resource = new(3, "Hero", ResourceType.DynamicPose, [1, 2, 3]);

        Assert.Equal(3, resource.ID);
        Assert.Equal("Hero", resource.Name);
        Assert.Equal(ResourceType.DynamicPose, resource.Type);
        Assert.Equal(3, resource.Length);
        Assert.Equal("(3, Hero, DynamicPose, 3)", resource.ToString());
    }

    [Fact]
    public void ResourceReference_ExposesBufferPositionAndDiagnosticString()
    {
        Resource resource = new(3, "Hero", ResourceType.DynamicPose, [1]);
        ResourceReference reference = new(2, 0x1234, resource);

        Assert.Equal(2, reference.BufferID);
        Assert.Equal(0x1234, reference.Position);
        Assert.Same(resource, reference.Resource);
        Assert.Equal("(3, DynamicPose, 2, 4660)", reference.ToString());
    }

    [Fact]
    public void ResourceBuffer_PacksReferencesAtTheirDeclaredOffsets()
    {
        ResourceBuffer buffer = new(4);
        Resource first = new(0, "First", ResourceType.Palette, [1, 2]);
        Resource second = new(1, "Second", ResourceType.GeneralResource, [9, 8, 7]);
        buffer.AddReference(new ResourceReference(4, 0, first));
        buffer.AddReference(new ResourceReference(4, 4, second));

        Assert.Equal([1, 2, 0, 0, 9, 8, 7], buffer.Data);
        Assert.Equal("(4, 7, 2)", buffer.ToString());
    }

    [Fact]
    public void ResourceBuffer_RebuildsDataAfterAnotherReferenceIsAdded()
    {
        ResourceBuffer buffer = new(0);
        buffer.AddReference(new ResourceReference(0, 0,
            new Resource(0, "A", ResourceType.Palette, [1])));
        Assert.Equal([1], buffer.Data);

        buffer.AddReference(new ResourceReference(0, 1,
            new Resource(1, "B", ResourceType.Palette, [2])));

        Assert.Equal([1, 2], buffer.Data);
    }

    [Fact]
    public void ResourceBuffer_AddOffsetPosition_ConvertsAllContainedPositions()
    {
        ResourceBuffer buffer = new(0);
        ResourceReference first = new(0, 0,
            new Resource(0, "A", ResourceType.Palette, [1]));
        ResourceReference second = new(0, 5,
            new Resource(1, "B", ResourceType.Palette, [2]));
        buffer.AddReference(first);
        buffer.AddReference(second);

        buffer.AddOffsetPosition(0x200);

        Assert.Equal(0x200, first.Position);
        Assert.Equal(0x205, second.Position);
    }

    [Fact]
    public void ResourceAllReferences_ComputesSizesPerCategory()
    {
        ResourceReference Ref(ResourceType type, int length) =>
            new(0, 0, new Resource(0, type.ToString(), type, new byte[length]));
        ResourceAllReferences references = new()
        {
            Buffers = [Ref(ResourceType.Buffer, 10)],
            DynamicPoses = [Ref(ResourceType.DynamicPose, 20), Ref(ResourceType.DynamicPose, 5)],
            Palettes = [Ref(ResourceType.Palette, 7)],
            GeneralResources = [Ref(ResourceType.GeneralResource, 9)]
        };

        Assert.Equal(10, references.BufferSize);
        Assert.Equal(25, references.DynamicPosesSize);
        Assert.Equal(7, references.PaletteSize);
        Assert.Equal(9, references.GeneralResourceSize);
    }
}
