namespace Encased.Contracts;

public class Uuid7Entity : IGuidEntity
{
    public virtual Guid Id { get; protected set; } = Guid.CreateVersion7();
    public T CopyFrom<T>(T copyFrom)
        where T : Uuid7Entity
    {
        if (copyFrom.GetType() != GetType())
            return this as T ?? copyFrom;

        var props = typeof(T).GetProperties();
        foreach (var prop in props)
        {
            if (prop.Name == nameof(Id))
                continue;

            prop.SetValue(this, prop.GetValue(copyFrom));
        }
        return this as T ?? copyFrom;
    }
    public static Guid ToGuid(byte[] value)
    {
        if(value.Length > 16)
            throw new ArgumentException("value too long");
        
        var bytes = new byte[16];
        value.CopyTo(bytes, 0);
        return new Guid(bytes);
    }
    public static Guid ToGuid(int value)
    {
        var bytes = new byte[16];
        BitConverter.GetBytes(value).CopyTo(bytes, 0);
        return new Guid(bytes);
    }
    public static Guid ToGuid(int value1, int value2)
    {
        var bytes = new byte[16];
        BitConverter.GetBytes(value1).CopyTo(bytes, 0);
        BitConverter.GetBytes(value2).CopyTo(bytes, 4);
        return new Guid(bytes);
    }

    public static Guid ToGuid(int value, int value2, int value3)
    {
        var bytes = new byte[16];
        BitConverter.GetBytes(value).CopyTo(bytes, 0);
        BitConverter.GetBytes(value2).CopyTo(bytes, 4);
        BitConverter.GetBytes(value3).CopyTo(bytes, 8);

        return new Guid(bytes);
    }
    public static Guid ToGuid(int value, long value2)
    {
        var bytes = new byte[16];
        BitConverter.GetBytes(value).CopyTo(bytes, 0);
        BitConverter.GetBytes(value2).CopyTo(bytes, 4);

        return new Guid(bytes);
    }
    public static Guid ToGuid(int value, int value2, int value3, int value4)
    {
        var bytes = new byte[16];
        BitConverter.GetBytes(value).CopyTo(bytes, 0);
        BitConverter.GetBytes(value2).CopyTo(bytes, 4);
        BitConverter.GetBytes(value3).CopyTo(bytes, 8);
        BitConverter.GetBytes(value4).CopyTo(bytes, 12);

        return new Guid(bytes);
    }
}