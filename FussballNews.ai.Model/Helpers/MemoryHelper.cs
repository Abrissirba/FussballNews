namespace FussballNews.ai.Model.Helpers;

public static class MemoryHelper
{
    public static byte[]? ToByteArray(ReadOnlyMemory<float>? readOnlyMemory)
    {
        if (readOnlyMemory == null)
        {
            return null;
        }
        var floatArray = readOnlyMemory.Value.ToArray();
        var byteArray = new byte[floatArray.Length * sizeof(float)];
        Buffer.BlockCopy(floatArray, 0, byteArray, 0, byteArray.Length);
        return byteArray;
    }

    public static ReadOnlyMemory<float>? ToReadOnlyMemory(byte[]? byteArray)
    {
        if (byteArray == null)
        {
            return null;
        }
        var floatArray = new float[byteArray.Length / sizeof(float)];
        Buffer.BlockCopy(byteArray, 0, floatArray, 0, byteArray.Length);
        return new ReadOnlyMemory<float>(floatArray);
    }
}