// PixelBeav Patch: PixelPatch-1.1.32  |  2025-10-18  |  Changed: yes
// PixelBeav Release: 1.1  |  2025-10-18  |  Changed: no
namespace PixelBeav.App.ViewModels
{
    public partial class MainViewModel
    {
        public async void RunFullScanFromUI() => await FullRescanAsync();
    }
}
