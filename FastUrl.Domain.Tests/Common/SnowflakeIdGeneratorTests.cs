using System;
using System.Collections.Generic;
using FastUrl.Domain.Common;
using Xunit;

namespace FastUrl.Domain.Tests.Common;

public class SnowflakeIdGeneratorTests
{
    [Fact]
    public void NextId_ShouldGeneratePositive64BitId()
    {
        var generator = new SnowflakeIdGenerator(workerId: 1);
        long id = generator.NextId();

        Assert.True(id > 0);
    }

    [Fact]
    public void NextId_ShouldGenerateUniqueIdsInSequence()
    {
        var generator = new SnowflakeIdGenerator(workerId: 1);
        var set = new HashSet<long>();

        for (int i = 0; i < 1000; i++)
        {
            long id = generator.NextId();
            Assert.True(set.Add(id), $"Duplicate ID found: {id}");
        }
    }

    [Fact]
    public void DifferentWorkerIds_ShouldGenerateDifferentIdsAtSameTime()
    {
        var node1 = new SnowflakeIdGenerator(workerId: 1);
        var node2 = new SnowflakeIdGenerator(workerId: 2);

        long id1 = node1.NextId();
        long id2 = node2.NextId();

        Assert.NotEqual(id1, id2);
    }

    [Fact]
    public void InvalidWorkerId_ShouldThrowArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new SnowflakeIdGenerator(workerId: -1));
        Assert.Throws<ArgumentOutOfRangeException>(() => new SnowflakeIdGenerator(workerId: 8)); // 3 bits Max is 7
    }
}
