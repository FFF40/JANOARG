using System;
using System.Globalization;

public static class Format 
{

    public static string FileSize(long bytes) 
    {
        string[] suffixes; 
        int byteBase = (int)Chartmaker.Preferences.FileSizeBase;
        switch (byteBase) 
        {
            case (int)FileSizeBase.Binary: 
                suffixes = new [] {"B", "KiB", "MiB", "GiB", "TiB", "PiB", "EiB", "ZiB", "YiB"};
                break;
            case (int)FileSizeBase.Decimal: default: 
                suffixes = new [] {"B", "kB" , "MB" , "GB" , "TB" , "PB" , "EB" , "ZB" , "YB" };
                break;
        }

        float byteFloat = bytes;
        int suffixIndex = 0;
        while (byteFloat >= byteBase - .5f) 
        {
            byteFloat /= byteBase;
            suffixIndex++;
        }

        if (byteFloat < 100) return byteFloat.ToString("G3") + " " + suffixes[suffixIndex];
        else return byteFloat.ToString("#,##0") + " " + suffixes[suffixIndex];
    }
}


public enum FileSizeBase 
{
    Decimal = 1000,
    Binary = 1024,
}