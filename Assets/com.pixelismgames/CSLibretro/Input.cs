using com.PixelismGames.CSLibretro.Libretro;

namespace com.PixelismGames.CSLibretro
{
    public class Input
    {
        public int Port;
        public Device Device;
        public int RawInputID;
        public JoypadInputID? JoypadInputID;
        public string CoreDescription;
        public short Value;

        public Input(InputDescriptor inputDescriptor)
        {
            Port = (int)inputDescriptor.Port;
            Device = (Device)inputDescriptor.Device;
            RawInputID = (int)inputDescriptor.ID;
            CoreDescription = inputDescriptor.Description;

            switch (Device)
            {
                case Device.Joypad:
                    JoypadInputID = (JoypadInputID)inputDescriptor.ID;
                    break;
            }
        }
    }
}