﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using com.PixelismGames.CSLibretro.Libretro;
using UnityEngine;

namespace com.PixelismGames.CSLibretro.Controllers
{
    [AddComponentMenu("Pixelism Games/Controllers/Libretro Unity Controller")]
    [RequireComponent(typeof(Renderer))]
    public class LibretroUnityController : MonoBehaviour
    {
        public const float PIXELS_PER_UNIT = 1f;

        private Core _core;
        private Renderer _renderer;

        private List<float> _audioSampleBuffer;
        private object _audioSync;

        #if UNITY_STANDALONE_OSX
        public string CorePath = @"./Contrib/Cores/fceumm_libretro.dylib";
        #else
        public string CorePath = @".\Contrib\Cores\fceumm_libretro.dll";
        #endif
        public string ROMPath = @".\Contrib\ROMs\smb.nes";
        public string XAxis = "Horizontal";
        public string YAxis = "Vertical";
        public string AButton = "Fire2";
        public string BButton = "Fire1";
        public string YButton = "Fire3";
        public string XButton;
        public string LButton;
        public string RButton;
        public string StartButton = "Submit";
        public string SelectButton = "Cancel";
        public bool IsMuted;

        #region Properties

        public long FrameCount
        {
            get { return (_core.FrameCount); }
        }

        public int ScreenHeight
        {
            get { return (_core.ScreenHeight); }
        }

        public int ScreenWidth
        {
            get { return (_core.ScreenWidth); }
        }

        #endregion

        #region MonoBehaviour

        public void Awake()
        {
            _renderer = gameObject.GetComponent<Renderer>();

            _audioSync = new object();
            lock (_audioSync)
            {
                _audioSampleBuffer = new List<float>();
            }
        }

        public void Start()
        {
            #if UNITY_STANDALONE_OSX
            _core = new Core(OS.OSX, CorePath);
            #else
            _core = new Core(OS.Windows, CorePath);
            #endif

            _core.AudioSampleBatchHandler += audioSampleBatchHandler;
            _core.LogHandler += logHandler;
            _core.VideoFrameHandler += videoFrameHandler;

            _core.Load(ROMPath);

            AudioConfiguration audioConfiguration = AudioSettings.GetConfiguration();
            audioConfiguration.sampleRate = (int)_core.AudioSampleRate;
            AudioSettings.Reset(audioConfiguration);

            // this is required for OnAudioFilterRead to work and needs to be done after setting the AudioSettings.outputSampleRate
            gameObject.AddComponent<AudioSource>();

            _core.StartFrameTiming();
        }

        public void Update()
        {
            List<JoypadInputID> validInputs = new List<JoypadInputID>() { JoypadInputID.Up, JoypadInputID.Down, JoypadInputID.Left, JoypadInputID.Right, JoypadInputID.Start, JoypadInputID.Select, JoypadInputID.A, JoypadInputID.B, JoypadInputID.X, JoypadInputID.Y, JoypadInputID.L, JoypadInputID.R };
            foreach (CSLibretro.Input input in _core.Inputs.Where(i => (i.Port == 0) && (validInputs.Contains(i.JoypadInputID.Value))))
            {
                switch (input.JoypadInputID)
                {
                    case JoypadInputID.Up:
                        if (!string.IsNullOrWhiteSpace(YAxis))
                            input.Value = Convert.ToInt16(UnityEngine.Input.GetAxis(YAxis) > 0f);
                        break;

                    case JoypadInputID.Down:
                        if (!string.IsNullOrWhiteSpace(YAxis))
                            input.Value = Convert.ToInt16(UnityEngine.Input.GetAxis(YAxis) < 0f);
                        break;

                    case JoypadInputID.Left:
                        if (!string.IsNullOrWhiteSpace(XAxis))
                            input.Value = Convert.ToInt16(UnityEngine.Input.GetAxis(XAxis) < 0f);
                        break;

                    case JoypadInputID.Right:
                        if (!string.IsNullOrWhiteSpace(XAxis))
                            input.Value = Convert.ToInt16(UnityEngine.Input.GetAxis(XAxis) > 0f);
                        break;

                    case JoypadInputID.A:
                        if (!string.IsNullOrWhiteSpace(AButton))
                        {
                            if (UnityEngine.Input.GetButtonDown(AButton))
                                input.Value = 1;
                            if (UnityEngine.Input.GetButtonUp(AButton))
                                input.Value = 0;
                        }
                        break;

                    case JoypadInputID.B:
                        if (!string.IsNullOrWhiteSpace(BButton))
                        {
                            if (UnityEngine.Input.GetButtonDown(BButton))
                                input.Value = 1;
                            if (UnityEngine.Input.GetButtonUp(BButton))
                                input.Value = 0;
                        }
                        break;

                    case JoypadInputID.Y:
                        if (!string.IsNullOrWhiteSpace(YButton))
                        {
                            if (UnityEngine.Input.GetButtonDown(YButton))
                                input.Value = 1;
                            if (UnityEngine.Input.GetButtonUp(YButton))
                                input.Value = 0;
                        }
                        break;

                    case JoypadInputID.X:
                        if (!string.IsNullOrWhiteSpace(XButton))
                        {
                            if (UnityEngine.Input.GetButtonDown(XButton))
                                input.Value = 1;
                            if (UnityEngine.Input.GetButtonUp(XButton))
                                input.Value = 0;
                        }
                        break;

                    case JoypadInputID.L:
                        if (!string.IsNullOrWhiteSpace(LButton))
                        {
                            if (UnityEngine.Input.GetButtonDown(LButton))
                                input.Value = 1;
                            if (UnityEngine.Input.GetButtonUp(LButton))
                                input.Value = 0;
                        }
                        break;

                    case JoypadInputID.R:
                        if (!string.IsNullOrWhiteSpace(RButton))
                        {
                            if (UnityEngine.Input.GetButtonDown(RButton))
                                input.Value = 1;
                            if (UnityEngine.Input.GetButtonUp(RButton))
                                input.Value = 0;
                        }
                        break;

                    case JoypadInputID.Start:
                        if (!string.IsNullOrWhiteSpace(StartButton))
                        {
                            if (UnityEngine.Input.GetButtonDown(StartButton))
                                input.Value = 1;
                            if (UnityEngine.Input.GetButtonUp(StartButton))
                                input.Value = 0;
                        }
                        break;

                    case JoypadInputID.Select:
                        if (!string.IsNullOrWhiteSpace(SelectButton))
                        {
                            if (UnityEngine.Input.GetButtonDown(SelectButton))
                                input.Value = 1;
                            if (UnityEngine.Input.GetButtonUp(SelectButton))
                                input.Value = 0;
                        }
                        break;
                }
            }
        }

        public void FixedUpdate()
        {
            _core.RunFrame();
        }

        // the fickle timing of this makes the exact timing of audio in emulators very tough; another solution may need to be found
        public void OnAudioFilterRead(float[] data, int channels)
        {
            int sampleCount = data.Length;
            float[] sampleBuffer;

            lock (_audioSync)
            {
                if (!_audioSampleBuffer.Any())
                {
                    return;
                }
                else if (_audioSampleBuffer.Count >= sampleCount)
                {
                    // this is the easy case, the core has provided us with enough samples, just copy them over
                    Array.Copy(_audioSampleBuffer.ToArray(), data, sampleCount);
                    _audioSampleBuffer.RemoveRange(0, sampleCount);
                    return;
                }

                // the core has not given us enough samples, copy what is there and attempt to smooth the results
                sampleBuffer = new float[_audioSampleBuffer.Count];
                Array.Copy(_audioSampleBuffer.ToArray(), sampleBuffer, _audioSampleBuffer.Count);
                _audioSampleBuffer.RemoveRange(0, _audioSampleBuffer.Count);
            }

            // smooth the data by duping (averaging) every X samples so that we have enough samples
            // this ultimately needs to be done via time stretching

            int frameCount = data.Length / channels;
            int bufferFrameCount = sampleBuffer.Length / channels;

            double step = (double)frameCount / ((double)frameCount - (double)bufferFrameCount);
            double start = step / 2;

            List<int> generatedFrames = new List<int>();
            double cumulativeStep = start;
            while (cumulativeStep < frameCount)
            {
                generatedFrames.Add((int)Math.Floor(cumulativeStep));
                cumulativeStep += step;
            }

            int bufferIndex = 0;
            for (int frameIndex = 0; frameIndex < frameCount; frameIndex++)
            {
                int sampleIndex = frameIndex * channels;

                if (generatedFrames.Contains(frameIndex))
                {
                    for (int i = 0; i < channels; i++)
                    {
                        if (bufferIndex == 0)
                            data[sampleIndex + i] = sampleBuffer[bufferIndex + i];
                        else if (bufferIndex == sampleBuffer.Length)
                            data[sampleIndex + i] = sampleBuffer[bufferIndex - channels + i];
                        else
                            data[sampleIndex + i] = (sampleBuffer[bufferIndex - channels + i] + sampleBuffer[bufferIndex + i]) / 2f;
                    }
                }
                else
                {
                    for (int i = 0; i < channels; i++)
                        data[sampleIndex + i] = sampleBuffer[bufferIndex + i];

                    bufferIndex += channels;
                }
            }
        }

        #endregion

        #region Handlers

        private void audioSampleBatchHandler(short[] samples)
        {
            if (!IsMuted)
            {
                lock (_audioSync)
                {
                    _audioSampleBuffer.AddRange(samples.Select(s => (float)((double)s / (double)short.MaxValue)).ToList());
                }
            }
        }

        private void logHandler(LogLevel level, string message)
        {
            Debug.Log(string.Format("[{0}] {1}", (int)level, message));
        }

        private void videoFrameHandler(int width, int height, byte[] frameBuffer)
        {
            TextureFormat textureFormat;
            RenderTextureFormat renderTextureFormat;

            switch (_core.PixelFormat)
            {
                case PixelFormat.RGB565:
                    textureFormat = TextureFormat.RGB565;
                    renderTextureFormat = RenderTextureFormat.RGB565;
                    break;

                case PixelFormat.XRGB8888:
                    textureFormat = TextureFormat.BGRA32;
                    renderTextureFormat = RenderTextureFormat.BGRA32;
                    for (int i = 3; i < frameBuffer.Length; i += 4)
                        frameBuffer[i] = 255;
                    break;

                default:
                    textureFormat = TextureFormat.RGBA32;
                    renderTextureFormat = RenderTextureFormat.RGBAUShort;
                    break;
            }

            Texture2D videoFrameTexture = new Texture2D(width, height, textureFormat, false);
            videoFrameTexture.LoadRawTextureData(frameBuffer);
            videoFrameTexture.Apply();

            if (_renderer is SpriteRenderer)
            {
                SpriteRenderer spriteRenderer = (SpriteRenderer)_renderer;

                if (spriteRenderer.sprite == null)
                {
                    spriteRenderer.sprite = Sprite.Create(videoFrameTexture, new Rect(0f, 0f, width, height), new Vector2(0.5f, 0.5f), PIXELS_PER_UNIT);
                    spriteRenderer.sprite.texture.filterMode = FilterMode.Point;
                }
                else
                {
                    Graphics.CopyTexture(videoFrameTexture, spriteRenderer.sprite.texture);
                }
            }
            else
            {
                if (_renderer.material.mainTexture == null)
                {
                    _renderer.material.mainTexture = new RenderTexture(width, height, 0, renderTextureFormat);
                    _renderer.material.mainTexture.filterMode = FilterMode.Point;
                }
                else
                {
                    Graphics.CopyTexture(videoFrameTexture, _renderer.material.mainTexture);
                }
            }
        }

        #endregion

        #region Accessible Routines

        public byte[] ReadRAM()
        {
            return (_core.ReadRAM());
        }

        public void WriteRAM(byte[] data, int offset = 0)
        {
            _core.WriteRAM(data, offset);
        }

        public void LoadState(string stateFilePath)
        {
            _core.LoadState(stateFilePath);
        }

        public void LoadState(byte[] state)
        {
            GCHandle pinnedState = GCHandle.Alloc(state, GCHandleType.Pinned);
            _core.UnserializePassthrough(pinnedState.AddrOfPinnedObject(), (uint)state.Length);
            pinnedState.Free();
        }

        public void SaveState(string stateFilePath)
        {
            _core.SaveState(stateFilePath);
        }

        #endregion
    }
}
