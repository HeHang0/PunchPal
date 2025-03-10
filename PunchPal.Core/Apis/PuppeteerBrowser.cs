using PunchPal.Core.Events;
using PuppeteerSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PunchPal.Core.Apis
{
    public class PuppeteerBrowser
    {
        private static readonly HashSet<IBrowser> Browsers = new HashSet<IBrowser>();

        private readonly TaskCompletionSource<Dictionary<string, string>> _tcs =
            new TaskCompletionSource<Dictionary<string, string>>();

        private Dictionary<string, string> _navigations = new Dictionary<string, string>();
        private string _closeUrl = string.Empty;
        public async Task<Dictionary<string, string>> Run(string url, string closeUrl, Dictionary<string, string> navigations, bool headless = false)
        {
            if (!Available)
            {
                EventManager.ShowTips(new Models.TipsOption("提示", "未检测到 Chrome 或 Edge，请安装后再运行！", Models.ControlAppearance.Caution));
                return null;
            }
            _navigations = navigations;
            _closeUrl = closeUrl;
            try
            {
                var browser = await Puppeteer.LaunchAsync(new LaunchOptions
                {
                    ExecutablePath = ChromiumPath,
                    DefaultViewport = new ViewPortOptions(),
                    Headless = headless // 是否无头模式
                });
                Browsers.Add(browser);
                var pages = (await browser.PagesAsync()).ToList();
                if (pages.Count <= 0)
                {
                    pages.Add(await browser.NewPageAsync());
                }

                var page = pages[0];
                page.DOMContentLoaded += PageOnDOMContentLoaded;
                page.FrameNavigated += PageOnFrameNavigated;
                await page.GoToAsync(url);
                browser.TargetCreated += Browser_TargetCreated;
                browser.Closed += Browser_Closed;
                if (headless)
                {
                    _ = Task.Delay(TimeSpan.FromSeconds(10)).ContinueWith(t => { _tcs.TrySetResult(null); });
                }
                ShowAllWindow();
                var result = await _tcs.Task;
                await browser.CloseAsync();
                await browser.DisposeAsync();
                Browsers.Remove(browser);
                return result;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private async void Browser_TargetCreated(object sender, TargetChangedArgs e)
        {
            if (!(sender is Browser browser))
            {
                return;
            }
            var target = e.Target;
            if (target.Type != TargetType.Page)
            {
                return;
            }
            var newPage = await target.PageAsync();
            var newUrl = newPage.Url;
            await newPage.CloseAsync();
            await newPage.DisposeAsync();
            var pages = (await browser.PagesAsync()).ToList();
            if (pages.Count > 0)
            {
                _ = pages[0].GoToAsync(newUrl);
            }
        }

        private void Browser_Closed(object sender, EventArgs e)
        {
            _tcs.TrySetResult(null);
        }


        private void PageOnFrameNavigated(object sender, FrameNavigatedEventArgs e)
        {
            if (!(sender is IPage page))
            {
                _tcs.TrySetResult(null);
                return;
            }

            var url = page.Url;
            foreach (var navigation in _navigations)
            {
                if (url.Contains(navigation.Key))
                {
                    page.GoToAsync(navigation.Value);
                    return;
                }
            }
        }

        private async void PageOnDOMContentLoaded(object sender, EventArgs e)
        {
            if (!(sender is IPage page))
            {
                _tcs.TrySetResult(null);
                return;
            }
            var url = page.Url;
            if (string.IsNullOrEmpty(_closeUrl) || url.Contains(_closeUrl))
            {
                _tcs.TrySetResult(await GetCookies(page));
            }
        }

        private static async Task<Dictionary<string, string>> GetCookies(IPage page)
        {
            if (page == null)
            {
                return null;
            }

            var cookies = await page.GetCookiesAsync();
            var cookieDict = new Dictionary<string, string>();
            foreach (var item in cookies)
            {
                cookieDict[item.Name] = item.Value;
            }

            return cookieDict.Count > 0 ? cookieDict : null;
        }

        private const string ChromePath = "Google\\Chrome\\Application\\chrome.exe";
        private const string EdgePath = "Microsoft\\Edge\\Application\\msedge.exe";

        private static readonly string ProgramFilesPath =
            Environment.GetFolderPath(Environment.SpecialFolder
                .ProgramFiles); //Environment.GetEnvironmentVariable("ProgramW6432");

        private static readonly string ProgramFilesX86Path =
            Environment.GetFolderPath(Environment.SpecialFolder
                .ProgramFilesX86); //Environment.GetEnvironmentVariable("ProgramFiles(x86)");

        private static readonly string ChromiumPath;
        public static readonly bool Available;

        static PuppeteerBrowser()
        {
            var possiblePaths = new string[]
            {
                Path.Combine(ProgramFilesPath, ChromePath),
                Path.Combine(ProgramFilesX86Path, ChromePath),
                Path.Combine(ProgramFilesPath, EdgePath),
                Path.Combine(ProgramFilesX86Path, EdgePath)
            };

            ChromiumPath = possiblePaths.FirstOrDefault(File.Exists);
            Available = !string.IsNullOrWhiteSpace(ChromiumPath);
            Process.GetCurrentProcess().Exited += OnExited;
        }

        private static void OnExited(object sender, EventArgs e)
        {
            foreach (var browser in Browsers)
            {
                try
                {
                    browser.CloseAsync();
                    browser.DisposeAsync();
                }
                catch (Exception)
                {
                    // ignore
                }
            }
        }

        public static void ShowAllWindow()
        {
            foreach (var browser in Browsers)
            {
                EventManager.SetForegroundWindow(new EventManager.ForegroundWindowOption(browser.Process.MainWindowHandle, true, true));
            }
        }
    }
}
