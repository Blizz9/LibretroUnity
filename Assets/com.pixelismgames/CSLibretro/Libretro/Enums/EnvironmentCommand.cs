namespace com.PixelismGames.CSLibretro.Libretro
{
    public enum EnvironmentCommand
    {
        Experimental = 0x10000,
        GetOverscan = 2,
        GetCanDupe = 3,
        SetPerformanceLevel = 8,
        GetSystemDirectory = 9,
        SetPixelFormat = 10,
        SetInputDescriptors = 11,
        GetVariable = 15,
        SetVariables = 16,
        GetVariableUpdate = 17,
        GetLogInterface = 27,
        GetSaveDirectory = 31,
        SetMemoryMaps = (36 | Experimental),
        SetGeometry = 37,
        GetCurrentSoftwareFrameBuffer = (40 | Experimental)
    }
}
