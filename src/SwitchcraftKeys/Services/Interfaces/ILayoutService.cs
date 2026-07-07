using SwitchcraftKeys.Models;

namespace SwitchcraftKeys.Services.Interfaces;

public interface ILayoutService
{
    IReadOnlyList<LayoutInfo> GetAvailableLayouts();

    LayoutInfo? GetCurrentLayout();

    Task<bool> SwitchLayoutAsync(string klid);
}
