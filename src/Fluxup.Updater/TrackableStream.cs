using System;
using System.IO;

//TODO: Comment this....
public class TrackableStream : FileStream
{
    public TrackableStream(string path, FileMode fileMode) : base(path, fileMode)
    {   
    }

    public override void Write(byte[] array, int offset, int count)
    {
        base.Write(array, offset, count);
        if (Length > length)
        {
            LengthChanged?.Invoke(null, Length);
            length = Length;
        }
    }

    public event EventHandler<long> LengthChanged;
    private long length;
    
    public event EventHandler<long> PositionChanged;
    private long position;
    public override long Position
    {
        get => position;
        set
        {
            position = value;
            PositionChanged?.Invoke(null, value);
        }
    }
}