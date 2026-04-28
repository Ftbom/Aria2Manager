namespace Aria2Manager.Core.Services.Interfaces
{
    public interface IWindowAware
    {
        //用于接收UIService注入的窗口标识
        string? WindowId { get; set; }
    }
}