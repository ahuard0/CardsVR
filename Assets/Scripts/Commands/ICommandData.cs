using System;

public interface ICommandData
{
    public byte getEventID { get; }
    public object[] ToObjectArray();
    public string ToString();
}
