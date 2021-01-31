# Libretro Unity

This is a very early implementation of the [libretro API](http://www.libretro.com) in C# and is usable in Unity. It has all been wrapped up in a `controller` that can be dropped on any Unity `Renderer` or `SpriteRenderer` and just work (mostly).

There is also an included example scene that shows how to use the `LibretroUnityController` on various surfaces.

## File Paths

To change the core and ROM file paths, simply edit the inspector properties on the `LibretroUnityController`. Compressed files (`zip`, `rar`, etc) are not currently supported. 

## Input Notes

This is being prepped to be placed on the Unity Asset Store so some decisions were made with this in mind. The default controls are very limited and are designed to support the default Unity `Input Manager` settings. Only up, down, left, right, A, B, Y, Start, and Select will be mapped in the example. It is simple to add input mappings by editing the inspector properties on the `LibretroUnityController`.

## Sound Timing

The biggest TODOs are currently around the sound timing, especially on MacOS. This is still in a "proof-of-concept" state and will be the first thing to focus on. The current state of this makes this whole project something to simply play around with for now, and not to consider final by any measure.

## Supported Platforms

This currently works on Windows and MacOS. Depending on the C# --> OS-API layer in Unity, this **should** be possible on Windows, MacOS, and Linux. The feasability on iOS, Android, and WebGL is unclear.

## Tested cores

The following cores have been tested **extremely** lightly and work to some degree.

* [FCEUmm](http://buildbot.libretro.com/nightly/windows/x86_64/latest/fceumm_libretro.dll.zip)
* [Nestopia UE](http://buildbot.libretro.com/nightly/windows/x86_64/latest/nestopia_libretro.dll.zip)
* [Snes9x](http://buildbot.libretro.com/nightly/windows/x86_64/latest/snes9x_libretro.dll.zip)
* [Gambatte](http://buildbot.libretro.com/nightly/windows/x86_64/latest/gambatte_libretro.dll.zip)
* [Beetle PSX](https://buildbot.libretro.com/nightly/windows/x86_64/latest/mednafen_psx_libretro.dll.zip)

## Vewlix model

The vewlix model used for one of the examples was done by [Eric M.](https://3dwarehouse.sketchup.com/model/96776355cdda6e5cb8ad76492f5638ee/Vewlix-Arcade-Cabinet)
