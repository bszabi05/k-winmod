using MyTool.Engines;
using MyTool.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MyTool.Services
{
    public class StartupService
    {
        private readonly StartupEngine _startupEngine = new StartupEngine();

        public async Task<List<StartupItem>> GetStartupItemsAsync()
        {
            return await _startupEngine.GetStartupItemsAsync();
        }

        public async Task<bool> SetStartupItemEnabledAsync(StartupItem item, bool enable)
        {
            return await _startupEngine.SetStartupItemEnabledAsync(item, enable);
        }

        public async Task<bool> DeleteStartupItemAsync(StartupItem item)
        {
            return await _startupEngine.DeleteStartupItemAsync(item);
        }
    }
}
