namespace Flamui.Tests;

public class FontAtlasTests
{
    [Fact]
    public void ReserveRegion_ShouldReturnValidRegion_WhenSpaceAvailable()
    {
        var atlas = new FontAtlas();
        var region = atlas.ReserveRegion(new Vector2Int(128, 128));

        Assert.NotNull(region);
        Assert.True(region.Value.P0 == new Vector2Int(0, 0));
        Assert.True(region.Value.P1 == new Vector2Int(128, 128));
    }

    [Fact]
    public void ReserveMultipleRegions()
    {
        var atlas = new FontAtlas();
        var region1 = atlas.ReserveRegion(new Vector2Int(128, 128));
        var region2 = atlas.ReserveRegion(new Vector2Int(128, 128));

        Assert.NotNull(region1);
        Assert.Equal(new Vector2Int(0, 0), region1.Value.P0);
        Assert.Equal(new Vector2Int(128, 128), region1.Value.P1);


        Assert.NotNull(region2);
        Assert.Equal(new Vector2Int(128, 0), region2.Value.P0);
        Assert.Equal(new Vector2Int(256, 128), region2.Value.P1);
    }

    [Fact]
    public void ReserveMultipleRegion2()
    {
        var atlas = new FontAtlas();
        var region1 = atlas.ReserveRegion(new Vector2Int(128, 128));
        var region2 = atlas.ReserveRegion(new Vector2Int(128, 128));
        var region3 = atlas.ReserveRegion(new Vector2Int(256, 256));

        Assert.NotNull(region1);
        Assert.Equal(new Vector2Int(0, 0), region1.Value.P0);
        Assert.Equal(new Vector2Int(128, 128), region1.Value.P1);


        Assert.NotNull(region2);
        Assert.Equal(new Vector2Int(128, 0), region2.Value.P0);
        Assert.Equal(new Vector2Int(256, 128), region2.Value.P1);

        Assert.NotNull(region3);
        Assert.Equal(new Vector2Int(256, 0), region3.Value.P0);
        Assert.Equal(new Vector2Int(512, 256), region3.Value.P1);
    }

    [Fact]
    public void ReserveRegion_ShouldReturnNull_WhenRegionTooLarge()
    {
        var atlas = new FontAtlas();
        var region = atlas.ReserveRegion(new Vector2Int(2048, 2048)); // larger than default 1024x1024

        Assert.Null(region);
    }

    [Fact]
    public void ReserveAndRelease_ShouldFreeRegion_ForReuse()
    {
        var atlas = new FontAtlas();
        var region = atlas.ReserveRegion(new Vector2Int(256, 256));

        Assert.NotNull(region);

        atlas.ReleaseRegion(region!.Value);

        // Should be able to reserve again (possibly same region)
        var region2 = atlas.ReserveRegion(new Vector2Int(256, 256));

        Assert.NotNull(region2);
    }

    [Fact]
    public void ReserveMultipleRegions_ShouldNotOverlap()
    {
        var atlas = new FontAtlas();
        var r1 = atlas.ReserveRegion(new Vector2Int(256, 256));
        var r2 = atlas.ReserveRegion(new Vector2Int(256, 256));

        Assert.NotNull(r1);
        Assert.NotNull(r2);

        Assert.False(RegionsOverlap(r1!.Value, r2!.Value));
    }

    private bool RegionsOverlap(Region r1, Region r2)
    {
        return !(r1.P1.X <= r2.P0.X || r1.P0.X >= r2.P1.X ||
                 r1.P1.Y <= r2.P0.Y || r1.P0.Y >= r2.P1.Y);
    }

    [Fact]
    public void ReserveAndRelease_VariedSizes_ShouldNotOverlap()
    {
        var atlas = new FontAtlas();
        var rnd = new Random(42); // Deterministic for test repeatability

        var reserved = new List<Region>();

        // Step 1: Reserve a variety of region sizes
        for (int i = 0; i < 20; i++)
        {
            var size = new Vector2Int(
                rnd.Next(32, 128),
                rnd.Next(32, 128)
            );

            var region = atlas.ReserveRegion(size);
            if (region != null)
            {
                reserved.Add(region.Value);
            }
        }

        // Step 2: Ensure no overlaps
        for (int i = 0; i < reserved.Count; i++)
        {
            for (int j = i + 1; j < reserved.Count; j++)
            {
                Assert.False(RegionsOverlap(reserved[i], reserved[j]),
                    $"Regions {i} and {j} overlap:\n{reserved[i]}\n{reserved[j]}");
            }
        }

        // Step 3: Release half of the regions
        for (int i = 0; i < 10; i++)
        {
            atlas.ReleaseRegion(reserved[i]);
        }

        var newReserved = new List<Region>();
        for (int i = 10; i < 20; i++)
        {
            newReserved.Add(reserved[i]);
        }

        // Step 4: Reserve more varied regions
        for (int i = 0; i < 10; i++)
        {
            var size = new Vector2Int(
                rnd.Next(40, 160),
                rnd.Next(40, 160)
            );

            var region = atlas.ReserveRegion(size);
            if (region != null)
            {
                newReserved.Add(region.Value);
            }
        }

        // Step 5: Check that all active regions do not overlap
        for (int i = 0; i < newReserved.Count; i++)
        {
            for (int j = i + 1; j < newReserved.Count; j++)
            {
                Assert.False(RegionsOverlap(newReserved[i], newReserved[j]),
                    $"Overlap between region {i} and {j}\n{newReserved[i]}\n{newReserved[j]}");
            }
        }
    }

}
