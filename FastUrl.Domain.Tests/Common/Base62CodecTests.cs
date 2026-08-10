using System;
using FastUrl.Domain.Common;
using Xunit;

namespace FastUrl.Domain.Tests.Common;

public class Base62CodecTests
{
    [Fact]
    public void Encode_GivenZero_ShouldReturnZeroString()
    {
        var base62Codec = new Base62Codec();
        Assert.Equal("0", base62Codec.Encode(0));
    }

    [Theory]
    [InlineData(0, "0")]
    [InlineData(1, "1")]
    [InlineData(61, "Z")]
    [InlineData(62, "10")]
    [InlineData(1250457, "5fiF")]
    public void Encode_ValidInputs_ShouldReturnExpectedBase62String(long id, string expectedCode)
    {
        var base62Codec = new Base62Codec();
        Assert.Equal(expectedCode, base62Codec.Encode(id));
    }

    [Fact]
    public void Encode_NegativeId_ShouldThrowArgumentOutOfRangeException()
    {
        var base62Codec = new Base62Codec();
        Assert.Throws<ArgumentOutOfRangeException>(() => base62Codec.Encode(-1));
    }

    [Fact]
    public void Decode_InvalidCharacters_ShouldThrowArgumentException()
    {
        var base62Codec = new Base62Codec();
        Assert.Throws<ArgumentException>(() => base62Codec.Decode("abc@123"));
    }

    [Fact]
    public void Decode_OverflowCode_ShouldThrowOverflowException()
    {
        var base62Codec = new Base62Codec();
        Assert.Throws<OverflowException>(() => base62Codec.Decode("zzzzzzzzzzzz"));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(61)]
    [InlineData(62)]
    [InlineData(1250457)]
    [InlineData(long.MaxValue - 1)]
    public void Encode_Decode_ShouldBeBijective(long id)
    {
        var codec = new Base62Codec();
        string code = codec.Encode(id);
        Assert.Equal(id, codec.Decode(code));
    }

    [Fact]
    public void Decode_EmptyOrWhitespace_ShouldThrowArgumentException()
    {
        var base62Codec = new Base62Codec();
        Assert.Throws<ArgumentException>(() => base62Codec.Decode(""));
        Assert.Throws<ArgumentException>(() => base62Codec.Decode("   "));
    }
}
