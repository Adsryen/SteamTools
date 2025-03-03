using ReactiveUI;
using System.Application.Services;
using System.Application.UI.Resx;
using System.Linq;
using System.Properties;
using System.Reactive;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using static System.Application.Services.CloudService.Constants;

namespace System.Application.UI.ViewModels
{
    public partial class AboutPageViewModel
    {
        public AboutPageViewModel()
        {
#if !__MOBILE__
            IconKey = nameof(AboutPageViewModel);
#endif

            OpenBrowserCommand = ReactiveCommand.CreateFromTask<string>(Browser2.OpenAsync);

            //#if !__MOBILE__
            //            CopyLinkCommand =
            //                ReactiveCommand.CreateFromTask<string>(Clipboard2.SetTextAsync);
            //#endif

            CheckUpdateCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                await IAppUpdateService.Instance.CheckUpdateAsync(showIsExistUpdateFalse: true);
            });

            DelAccountCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                if (!UserService.Current.IsAuthenticated) return;
                var r = await MessageBoxCompat.ShowAsync(AppResources.DelAccountTips, ThisAssembly.AssemblyTrademark, MessageBoxButtonCompat.OKCancel);
                if (r == MessageBoxResultCompat.OK)
                {
                    await UserService.Current.DelAccountAsync();
                }
            });

#if __MOBILE__

            preferenceButtons = new(Enum2.GetAll<PreferenceButton>().Select(x => PreferenceButtonViewModel.Create(x, this)));

            UserService.Current.WhenAnyValue(x => x.User).Subscribe(value =>
            {
                if (value == null)
                {
                    PreferenceButtonViewModel.RemoveAuthorized(preferenceButtons, this);
                }
                else
                {
                    var delAccount = preferenceButtons.FirstOrDefault(x => x.Id == PreferenceButton.账号注销);
                    if (delAccount == null)
                    {
                        delAccount = PreferenceButtonViewModel.Create(PreferenceButton.账号注销, this);
                        preferenceButtons.Add(delAccount);
                    }
                }
            }).AddTo(this);
#endif
        }

        public ReactiveCommand<Unit, Unit> CheckUpdateCommand { get; }

        public ReactiveCommand<string, Unit> OpenBrowserCommand { get; }

        //#if !__MOBILE__
        //        public ReactiveCommand<string, Unit> CopyLinkCommand { get; }
        //#endif

        public ReactiveCommand<Unit, Unit> DelAccountCommand { get; }

        public string VersionDisplay => $"{ThisAssembly.VersionDisplay} for {DeviceInfo2.OSName} ({RuntimeInformation.ProcessArchitecture})";

        public string LabelVersionDisplay => ThisAssembly.IsAlphaRelease ? "Alpha Version:" : (ThisAssembly.IsBetaRelease ? "Beta Version:" : "Current Version:");

        public string Copyright
        {
            get
            {
                // https://www.w3cschool.cn/html/html-copyright.html
                int startYear = 2020, thisYear = 2021;
                var nowYear = DateTime.Now.Year;
                if (nowYear < thisYear) nowYear = thisYear;
                return $"© {startYear}{(nowYear == startYear ? startYear : "-" + nowYear)} {ThisAssembly.AssemblyCompany}. All Rights Reserved.";
            }
        }

        const string Zhengye = "Zhengye";
        const string 沙中金 = "沙中金";
        const string EspRoy = "EspRoy";

        //public ICommand ContributorsCommand { get; } = ReactiveCommand.CreateFromTask<string?>(async (p, _) =>
        //{
        //    switch (p)
        //    {
        //        case 沙中金:
        //            await Email2.ComposeAsync(new() { To = new() { "" } });
        //            break;
        //        case EspRoy:
        //            await Email2.ComposeAsync(new() { To = new() { "" } });
        //            break;
        //    }
        //});
    }
}