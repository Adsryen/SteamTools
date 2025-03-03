using ReactiveUI;
using System.Application.Services;
using System.Collections.Generic;
using System.Linq;
using System.Properties;
using System.Reactive;
using System.Windows;
using System.Windows.Threading;

namespace System.Application.UI.ViewModels
{
    public partial class TaskBarWindowViewModel : WindowViewModel
    {
        public string Version => ThisAssembly.Version;

        public IEnumerable<TabItemViewModel>? Tabs { get; }

        public async void Show(int x, int y)
        {
            SetPosition(x, y);
            await IShowWindowService.Instance.Show(CustomWindow.TaskBar, this, string.Empty, ResizeModeCompat.NoResize, isParent: false);
        }

        public void SetPosition(int x, int y)
        {
            SizePosition.X = x;
            SizePosition.Y = y;
        }

        public ReactiveCommand<object, Unit> MenuClickCommand { get; }

        public TaskBarWindowViewModel()
        {
            MenuClickCommand = ReactiveCommand.Create<object>(MenuClick);
            if (IWindowService.Instance.MainWindow is MainWindowViewModel mainwindow)
            {
                Tabs = mainwindow.TabItems.Where(x => x.Id != default);
            }
        }

        public const TabItemViewModel.TabItemId SettingsId = TabItemViewModel.TabItemId.Settings;

        public void MenuClick(object tag)
        {
            if (tag is string str)
            {
                OnMenuClickCore(str, Hide);
            }
            else if (tag is TabItemViewModel.TabItemId id)
            {
                OnMenuClickCore(string.Empty, Hide, id);
            }
        }

        static void OnMenuClick(Func<TabItemViewModel, bool> predicate, Action? hideTaskBarWindow = null)
        {
            if (IWindowService.Instance.MainWindow is MainWindowViewModel main)
            {
                var tab = main.TabItems.FirstOrDefault(predicate);
                tab ??= main.FooterTabItems.FirstOrDefault(predicate);
                if (tab != null)
                {
                    DI.Get<IDesktopAppService>().RestoreMainWindow();
                    main.SelectedItem = tab;
                    hideTaskBarWindow?.Invoke();
                }
            }
        }

        static bool OnMenuClickCore(string tag, Action? hideTaskBarWindow = null, TabItemViewModel.TabItemId tabItemId = default)
        {
            switch (tag)
            {
                case CommandExit:
                    DI.Get<IDesktopAppService>().Shutdown();
                    return true;
                default:
                    MainThread2.BeginInvokeOnMainThread(() =>
                    {
                        if (tabItemId != default)
                            OnMenuClick(s =>
                                s is TabItemViewModel item &&
                                    item.Id == tabItemId, hideTaskBarWindow);
                        else
                            OnMenuClick(s => s.Name == tag, hideTaskBarWindow);
                    }, DispatcherPriorityCompat.MaxValue);
                    break;
            }
            return false;
        }

        public static bool OnMenuClick(string command)
        {
            if (Enum.TryParse<TabItemViewModel.TabItemId>(command, out var tabItemId))
                return OnMenuClick(tabItemId);
            else
                return OnMenuClickCore(command);
        }

        public static bool OnMenuClick(TabItemViewModel.TabItemId tabItemId) => OnMenuClickCore(string.Empty, tabItemId: tabItemId);
    }
}