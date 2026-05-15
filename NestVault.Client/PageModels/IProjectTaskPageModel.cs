using CommunityToolkit.Mvvm.Input;
using NestVault.Client.Models;

namespace NestVault.Client.PageModels
{
    public interface IProjectTaskPageModel
    {
        IAsyncRelayCommand<ProjectTask> NavigateToTaskCommand { get; }
        bool IsBusy { get; }
    }
}