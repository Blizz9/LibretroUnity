using com.PixelismGames.CSLibretro.Libretro;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace com.PixelismGames.CSLibretro
{
    // TODO : figure out if I can find the PC, ROM, and whether I can write to it or not (might be done with Memory maps?)
    // TODO : change environment handling to a map and breakout into individual methods
    public class Core
    {
        private APIVersionSignature _apiVersion;
        private DeinitSignature _deinit;
        private GetMemoryDataSignature _getMemoryData;
        private GetMemorySizeSignature _getMemorySize;
        private GetSystemAVInfoSignature _getSystemAVInfo;
        private GetSystemInfoSignature _getSystemInfo;
        private InitSignature _init;
        private LoadGameSignature _loadGame;
        private ResetSignature _reset;
        private RunSignature _run;
        private SerializeSignature _serialize;
        private SerializeSizeSignature _serializeSize;
        private SetAudioSampleSignature _setAudioSample;
        private SetAudioSampleBatchSignature _setAudioSampleBatch;
        private SetControllerPortDeviceSignature _setControllerPortDevice;
        private SetEnvironmentSignature _setEnvironment;
        private SetInputPollSignature _setInputPoll;
        private SetInputStateSignature _setInputState;
        private SetVideoRefreshSignature _setVideoRefresh;
        private UnloadGameSignature _unloadGame;
        private UnserializeSignature _unserialize;

        private AudioSampleHandler _audioSampleHandler;
        private AudioSampleBatchHandler _audioSampleBatchHandler;
        private EnvironmentHandler _environmentHandler;
        private InputPollHandler _inputPollHandler;
        private InputStateHandler _inputStateHandler;
        private VideoRefreshHandler _videoRefreshHandler;

		private OS _os;

        private string _libretroDLLPath;
        private IntPtr _libretroDLL;

        private Stopwatch _frameTimer;
        private long _framePeriodNanoseconds;
        private long _frameLeftoverNanoseconds;

        private SystemInfo _systemInfo;
        private SystemAVInfo _systemAVInfo;

        private bool _variablesDirty;

        private IntPtr _ramAddress;
        private int _ramSize;

        public bool IsRunning;
        public long FrameCount = 0;

        public PixelFormat PixelFormat = PixelFormat.Unknown;
        public List<Variable> Variables;
        public List<Input> Inputs;
        public int StateSize;

        public event Action<short, short> AudioSampleHandler;
        public event Action<short[]> AudioSampleBatchHandler;
        public event Action InputPollHandler;
        public event Func<int, Device, int, short> InputStateHandler;
        public event Action<LogLevel, string> LogHandler;
        public event Action<int, int, byte[]> VideoFrameHandler;

        public AudioSampleBatchHandler AudioSampleBatchPassthroughHandler;
        public InputStateHandler InputStatePassthroughHandler;
        public LogHandler LogPassthroughHandler;
        public VideoRefreshHandler VideoFramePassthroughHandler;

        #region Properties

        public int APIVersion
        {
            get { return ((int)_apiVersion()); }
        }

        public double AudioSampleRate
        {
            get { return (_systemAVInfo.Timing.SampleRate); }
        }

        public double FramePeriodNanoseconds
        {
            get { return (_framePeriodNanoseconds); }
        }

        public double FrameRate
        {
            get { return (_systemAVInfo.Timing.FPS); }
        }

        public string Name
        {
            get { return (_systemInfo.LibraryName); }
        }

        public int ScreenHeight
        {
            get { return ((int)_systemAVInfo.Geometry.BaseHeight); }
        }

        public int ScreenWidth
        {
            get { return ((int)_systemAVInfo.Geometry.BaseWidth); }
        }

        public string ValidExtensions
        {
            get { return (_systemInfo.ValidExtensions); }
        }

        public string Version
        {
            get { return (_systemInfo.LibraryVersion); }
        }

        #endregion

		public Core(OS os, string libretroDLLPath)
        {
			_os = os;

            _frameTimer = new Stopwatch();

            _variablesDirty = true;

            Variables = new List<Variable>();
            Inputs = new List<Input>();

            _libretroDLLPath = libretroDLLPath;
			if (_os == OS.OSX)
                _libretroDLL = OSXAPI.dlopen(libretroDLLPath, 2);
			else
                _libretroDLL = WindowsAPI.LoadLibrary(libretroDLLPath);

            _apiVersion = GetDelegate<APIVersionSignature>("retro_api_version");
            _deinit = GetDelegate<DeinitSignature>("retro_deinit");
            _getMemoryData = GetDelegate<GetMemoryDataSignature>("retro_get_memory_data");
            _getMemorySize = GetDelegate<GetMemorySizeSignature>("retro_get_memory_size");
            _getSystemAVInfo = GetDelegate<GetSystemAVInfoSignature>("retro_get_system_av_info");
            _getSystemInfo = GetDelegate<GetSystemInfoSignature>("retro_get_system_info");
            _init = GetDelegate<InitSignature>("retro_init");
            _loadGame = GetDelegate<LoadGameSignature>("retro_load_game");
            _reset = GetDelegate<ResetSignature>("retro_reset");
            _run = GetDelegate<RunSignature>("retro_run");
            _serialize = GetDelegate<SerializeSignature>("retro_serialize");
            _serializeSize = GetDelegate<SerializeSizeSignature>("retro_serialize_size");
            _setAudioSample = GetDelegate<SetAudioSampleSignature>("retro_set_audio_sample");
            _setAudioSampleBatch = GetDelegate<SetAudioSampleBatchSignature>("retro_set_audio_sample_batch");
            _setControllerPortDevice = GetDelegate<SetControllerPortDeviceSignature>("retro_set_controller_port_device");
            _setEnvironment = GetDelegate<SetEnvironmentSignature>("retro_set_environment");
            _setInputPoll = GetDelegate<SetInputPollSignature>("retro_set_input_poll");
            _setInputState = GetDelegate<SetInputStateSignature>("retro_set_input_state");
            _setVideoRefresh = GetDelegate<SetVideoRefreshSignature>("retro_set_video_refresh");
            _unloadGame = GetDelegate<UnloadGameSignature>("retro_unload_game");
            _unserialize = GetDelegate<UnserializeSignature>("retro_unserialize");
        }

        #region Load

        public void Load(string romPath)
        {
            if (AudioSampleBatchPassthroughHandler == null)
                _audioSampleBatchHandler = new AudioSampleBatchHandler(audioSampleBatchCallback);
            else
                _audioSampleBatchHandler = AudioSampleBatchPassthroughHandler;

            if (InputStatePassthroughHandler == null)
                _inputStateHandler = new InputStateHandler(inputStateCallback);
            else
                _inputStateHandler = InputStatePassthroughHandler;

            if (VideoFramePassthroughHandler == null)
                _videoRefreshHandler = new VideoRefreshHandler(videoRefreshCallback);
            else
                _videoRefreshHandler = VideoFramePassthroughHandler;

            _audioSampleHandler = new AudioSampleHandler(audioSampleCallback);
            _environmentHandler = new EnvironmentHandler(environmentCallback); // options for passing through some commands need to be done here
            _inputPollHandler = new InputPollHandler(inputPollCallback);

            _setEnvironment(_environmentHandler);
            _setVideoRefresh(_videoRefreshHandler);
            _setInputPoll(_inputPollHandler);
            _setInputState(_inputStateHandler);
            _setAudioSample(_audioSampleHandler);
            _setAudioSampleBatch(_audioSampleBatchHandler);

            _init();

            byte[] romBytes = File.ReadAllBytes(romPath);
            IntPtr romBytesPointer = Marshal.AllocHGlobal(romBytes.Length);
            Marshal.Copy(romBytes, 0, romBytesPointer, romBytes.Length);
            GameInfo gameInfo = new GameInfo() { Path = romPath, Data = romBytesPointer, Size = (uint)romBytes.Length, Meta = null };
            _loadGame(ref gameInfo);
            Marshal.FreeHGlobal(romBytesPointer);

            _systemInfo = new SystemInfo();
            _getSystemInfo(out _systemInfo);
            _systemInfo.LibraryName = Marshal.PtrToStringAnsi(_systemInfo.LibraryNameAddress);
            _systemInfo.LibraryVersion = Marshal.PtrToStringAnsi(_systemInfo.LibraryVersionAddress);
            _systemInfo.ValidExtensions = Marshal.PtrToStringAnsi(_systemInfo.ValidExtensionsAddress);

            _systemAVInfo = new SystemAVInfo();
            _getSystemAVInfo(out _systemAVInfo);

            _framePeriodNanoseconds = (long)(1000000000 / _systemAVInfo.Timing.FPS);

            StateSize = (int)_serializeSize();
            _ramAddress = _getMemoryData(MemoryType.RAM);
            _ramSize = (int)_getMemorySize(MemoryType.RAM);
        }

        #endregion

        #region Run

        public void Run()
        {
            IsRunning = true;

            while (IsRunning)
            {
                RestartFrameTiming();

                _run();

                FrameCount++;

                StopFrameTiming();
                SleepRemainingFrameTime();
            }
        }

        public void RunFrame()
        {
            _run();

            FrameCount++;
        }

        #endregion

        #region Frame Timing

        public void StartFrameTiming()
        {
            _frameTimer.Start();
        }

        public void RestartFrameTiming()
        {
            _frameTimer.Reset();
            StartFrameTiming();
        }

        public void StopFrameTiming()
        {
            _frameTimer.Stop();
        }

        public bool HasFramePeriodElapsed()
        {
            long frameElapsedNanoseconds = (long)(((double)_frameTimer.ElapsedTicks / (double)Stopwatch.Frequency) * 1000000000);

            if ((frameElapsedNanoseconds + _frameLeftoverNanoseconds) > _framePeriodNanoseconds)
            {
                RestartFrameTiming();

                _frameLeftoverNanoseconds += frameElapsedNanoseconds - _framePeriodNanoseconds;

                // not interested in tracking leftover nanoseconds in extreme situations
                if ((_frameLeftoverNanoseconds < 0) || (_frameLeftoverNanoseconds >= _framePeriodNanoseconds))
                    _frameLeftoverNanoseconds = 0;

                return (true);
            }

            return (false);
        }

        public void SleepRemainingFrameTime()
        {
            long frameElapsedNanoseconds = (long)(((double)_frameTimer.ElapsedTicks / (double)Stopwatch.Frequency) * 1000000000);
            _frameLeftoverNanoseconds += _framePeriodNanoseconds - frameElapsedNanoseconds;

            if (_frameLeftoverNanoseconds > 0)
            {
                Thread.Sleep((int)(_frameLeftoverNanoseconds / 1000000));
                _frameLeftoverNanoseconds %= 1000000;
            }
            else
            {
                _frameLeftoverNanoseconds = 0;
            }
        }

        #endregion

        #region Handlers

        private void audioSampleCallback(short left, short right)
        {
            if (AudioSampleHandler != null)
                AudioSampleHandler(left, right);
        }

        private uint audioSampleBatchCallback(IntPtr data, uint frames)
        {
            if (AudioSampleBatchHandler != null)
            {
                short[] audioFrames = new short[frames * 2];
                Marshal.Copy(data, audioFrames, 0, ((int)frames * 2));

                AudioSampleBatchHandler(audioFrames);
            }

            return (frames);
        }

        // build a way for the user of core to passthrough their choice of commands to handle
        private bool environmentCallback(uint command, IntPtr data)
        {
            switch ((EnvironmentCommand)command)
            {
                case EnvironmentCommand.GetOverscan:
                    Marshal.WriteByte(data, 0, Convert.ToByte(true));
                    return (true);

                case EnvironmentCommand.GetCanDupe:
                    Marshal.WriteByte(data, 0, Convert.ToByte(true));
                    return (true);

                case EnvironmentCommand.SetPerformanceLevel:
                    uint temp = (uint)Marshal.ReadInt32(data); // this isn't being stored anywhere
                    return (true);

                case EnvironmentCommand.GetSystemDirectory:
                    Marshal.WriteIntPtr(data, Marshal.StringToHGlobalAnsi(Directory.GetCurrentDirectory()));
                    return (true);

                case EnvironmentCommand.SetPixelFormat:
                    PixelFormat = (PixelFormat)Marshal.ReadInt32(data);
                    return (true);

                case EnvironmentCommand.SetInputDescriptors:
                    IntPtr inputDescriptorAddress = data;
                    while (true)
                    {
                        InputDescriptor inputDescriptor = (InputDescriptor)Marshal.PtrToStructure(inputDescriptorAddress, typeof(InputDescriptor));
                        if (inputDescriptor.Description == null)
                            break;
                        Inputs.Add(new Input(inputDescriptor));
                        inputDescriptorAddress = (IntPtr)((long)inputDescriptorAddress + Marshal.SizeOf(inputDescriptor));
                    }
                    return (true);

                case EnvironmentCommand.GetVariable:
                    Variable variable = (Variable)Marshal.PtrToStructure(data, typeof(Variable));
                    IEnumerable<string> matchedVariables = Variables.Where(v => v.Key == variable.Key).Select(v => v.Value);
                    if (matchedVariables.Any())
                    {
                        string firstValue = Variables.Where(v => v.Key == variable.Key).Select(v => v.Value).First();
                        firstValue = firstValue.Substring(firstValue.IndexOf(';') + 2);
                        if (firstValue.Contains("|"))
                            firstValue = firstValue.Substring(0, firstValue.IndexOf('|'));
                        variable.Value = firstValue;
                        Marshal.StructureToPtr(variable, data, false);
                    }
                    return (true);

                case EnvironmentCommand.SetVariables:
                    IntPtr variableAddress = data;
                    while (true)
                    {
                        variable = (Variable)Marshal.PtrToStructure(variableAddress, typeof(Variable));
                        if (variable.Key == null)
                            break;
                        Variables.Add(variable);
                        variableAddress = (IntPtr)((long)variableAddress + Marshal.SizeOf(variable));
                    }
                    return (true);

                case EnvironmentCommand.GetVariableUpdate:
                    Marshal.WriteByte(data, 0, Convert.ToByte(_variablesDirty));
                    _variablesDirty = false;
                    return (true);

                case EnvironmentCommand.GetLogInterface:
                    LogCallback logCallbackStruct = new LogCallback();
                    if (LogPassthroughHandler == null)
                        logCallbackStruct.Log = logCallback;
                    else
                        logCallbackStruct.Log = LogPassthroughHandler;
                    Marshal.StructureToPtr(logCallbackStruct, data, false);
                    return (true);

                case EnvironmentCommand.GetSaveDirectory:
                    Marshal.WriteIntPtr(data, Marshal.StringToHGlobalAnsi(Directory.GetCurrentDirectory()));
                    return (true);

                case EnvironmentCommand.SetMemoryMaps: // not saved anywhere
                    return (true);

                case EnvironmentCommand.SetGeometry:
                    GameGeometry gameGeometry = (GameGeometry)Marshal.PtrToStructure(data, typeof(GameGeometry));
                    return (true);

                case EnvironmentCommand.GetCurrentSoftwareFrameBuffer: // not implemented
                    return (false);

                default:
                    Debug.WriteLine("Unhandled Environment Command: " + command);
                    return (false);
            }
        }

        private void inputPollCallback()
        {
            if (InputPollHandler != null)
                InputPollHandler();
        }

        private short inputStateCallback(uint port, uint device, uint index, uint id)
        {
            short returnValue = 0;

            if (InputStateHandler == null)
            {
                Input input = Inputs.Where(i => (i.Port == (int)port) && (i.Device == (Device)device) && (i.RawInputID == (int)id)).FirstOrDefault();
                if (input != null)
                    returnValue = input.Value;
            }
            else
                returnValue = InputStateHandler((int)port, (Device)device, (int)id);

            return (returnValue);
        }

        private void logCallback(LogLevel level, string fmt, IntPtr arg0, IntPtr arg1, IntPtr arg2, IntPtr arg3, IntPtr arg4, IntPtr arg5, IntPtr arg6, IntPtr arg7, IntPtr arg8, IntPtr arg9)
        {
            if (LogHandler != null)
            {
                StringBuilder logMessage = new StringBuilder(256);

                do
                {
                    int length;
                    if (_os == OS.OSX)
                        length = OSXAPI.snprintf(logMessage, (uint)logMessage.Capacity, fmt, arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9);
                    else
                        length = WindowsAPI._snprintf(logMessage, (uint)logMessage.Capacity, fmt, arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9);

                    if ((length <= 0) || (length >= logMessage.Capacity))
                    {
                        logMessage.Capacity *= 2;
                        continue;
                    }

                    logMessage.Length = length;
                    break;
                } while (logMessage.Length >= logMessage.Capacity);

                LogHandler(level, logMessage.ToString());
            }
        }

        private void videoRefreshCallback(IntPtr data, uint width, uint height, uint pitch)
        {
            if (data == IntPtr.Zero)
                return;

            if (VideoFrameHandler != null)
            {
                int pixelMemorySize;
                switch (PixelFormat)
                {
                    case PixelFormat.RGB1555:
                    case PixelFormat.RGB565:
                        pixelMemorySize = 2;
                        break;

                    case PixelFormat.XRGB8888:
                        pixelMemorySize = 4;
                        break;

                    default:
                        pixelMemorySize = 4;
                        break;
                }

                int rowSize = (int)width * pixelMemorySize;
                int size = (int)height * rowSize;
                byte[] frameBuffer = new byte[size];

                if (rowSize == (int)pitch)
                {
                    Marshal.Copy(data, frameBuffer, 0, size);
                }
                else
                {
                    // if the data also contains the back buffer, we have to rip out just the first frame
                    for (int i = 0; i < height; i++)
                    {
                        IntPtr rowAddress = (IntPtr)((long)data + (i * (int)pitch));
                        int newRowIndex = i * rowSize;
                        Marshal.Copy(rowAddress, frameBuffer, newRowIndex, rowSize);
                    }
                }

                VideoFrameHandler((int)width, (int)height, frameBuffer);
            }
        }

        #endregion

        #region State

        public void LoadState(string stateFilePath)
        {
            byte[] state = File.ReadAllBytes(stateFilePath);

            GCHandle pinnedState = GCHandle.Alloc(state, GCHandleType.Pinned);
            _unserialize(pinnedState.AddrOfPinnedObject(), (uint)state.Length);
            pinnedState.Free();
        }

        public void SaveState(string stateFilePath)
        {
            byte[] state = new byte[StateSize];

            GCHandle pinnedState = GCHandle.Alloc(state, GCHandleType.Pinned);
            _serialize(pinnedState.AddrOfPinnedObject(), (uint)state.Length);
            pinnedState.Free();

            File.WriteAllBytes(stateFilePath, state);
        }

        public bool SerializePassthrough(IntPtr data, uint size)
        {
            return (_serialize(data, size));
        }

        public bool UnserializePassthrough(IntPtr data, uint size)
        {
            return (_unserialize(data, size));
        }

        #endregion

        #region Memory

        public byte[] ReadRAM(int offset = 0, int? length = null)
        {
            if (!length.HasValue)
                length = _ramSize - offset;

            byte[] ram = new byte[length.Value];

            IntPtr ramAddressOffset = (IntPtr)((long)_ramAddress + offset);
            Marshal.Copy(ramAddressOffset, ram, 0, length.Value);

            return (ram);
        }

        public void WriteRAM(byte[] data, int offset = 0)
        {
            IntPtr ramAddressOffset = (IntPtr)((long)_ramAddress + offset);
            Marshal.Copy(data, 0, ramAddressOffset, data.Length);
        }

        #endregion

        #region Delegates

        public T GetDelegate<T>(string libretroFunctionName)
        {
			if (_os == OS.OSX)
				return ((T)Convert.ChangeType(Marshal.GetDelegateForFunctionPointer(OSXAPI.dlsym(_libretroDLL, libretroFunctionName), typeof(T)), typeof(T)));
			else
            	return ((T)Convert.ChangeType(Marshal.GetDelegateForFunctionPointer(WindowsAPI.GetProcAddress(_libretroDLL, libretroFunctionName), typeof(T)), typeof(T)));

        }

        #endregion
    }
}
