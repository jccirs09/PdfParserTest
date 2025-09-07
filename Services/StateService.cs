using PickingListApp.DTOs;

namespace PickingListApp.Services;

/// <summary>
/// A simple singleton service to hold state between pages, specifically
/// the parsed PickingListDto after upload and before review/save.
/// </summary>
public class StateService
{
    public PickingListDto? CurrentPickingList { get; set; }
}
