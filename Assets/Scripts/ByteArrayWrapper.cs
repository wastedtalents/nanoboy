using System;
using UnityEngine;
using System.Collections;

[Serializable]
public class ByteArrayWrapper : IDisposable
{
    public byte[] cols;

    public int Length { get { return cols.Length; } }

    public byte this[int index]
    {
        get { return cols[index]; }
        set { cols[index] = value; }
    }

    public ByteArrayWrapper(int size)
    {
        cols = new byte[size];
    }

    public void Dispose()
    {
        if (cols != null)
            cols = null;
    }
}
