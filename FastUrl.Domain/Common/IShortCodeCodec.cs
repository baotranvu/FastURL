namespace FastUrl.Domain.Common;

public interface IShortCodeCodec
{
    string Encode(long id);
    long Decode(string code);
}
