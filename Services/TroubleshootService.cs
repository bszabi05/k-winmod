using System;
using System.Threading.Tasks;
using MyTool.Engines;

namespace MyTool.Services
{
    public class TroubleshootService
    {
        private readonly HardwareEngine _hardwareEngine = new HardwareEngine();



        public async Task<bool> RestartTouchpadDriverAsync()
        {

            return await _hardwareEngine.RestartTouchpadDriverAsync();
        }

        // check I2C HID status
        public async Task<bool> CheckTouchpadErrorStatusAsync()
        {
            return await _hardwareEngine.HasDeviceErrorAsync("I2C HID");
        }

    }
}