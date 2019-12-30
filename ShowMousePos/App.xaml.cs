using Automation.Views;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Unity;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace Automation
{
    /// <summary>
    /// App.xaml の相互作用ロジック
    /// </summary>
    public partial class App
    {
        protected override Window CreateShell()
        {
            return Container.Resolve<Mouse>();
        }
        
        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
        }

        protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
        {
            // Addするモジュール名は作成したものに変更
            //moduleCatalog.AddModule<xxxx.xxxxModule>();
        }
    }

    public static class WindowManager
    {
        public static void ShowOrActivate<TWindow>()
            where TWindow : Window, new()
        {
            // 対象Windowが開かれているか探す
            var window = Application.Current.Windows.OfType<TWindow>().FirstOrDefault();
            if (window == null)
            {
                // 開かれてなかったら開く
                window = new TWindow();
                window.Show();
            }
            else
            {
                // 既に開かれていたらアクティブにする
                window.Activate();
            }
        }

        // newでインスタンスが作れない時用
        public static void ShowOrActivate<TWindow>(Func<TWindow> factory)
            where TWindow : Window
        {
            // 対象Windowが開かれているか探す
            var window = Application.Current.Windows.OfType<TWindow>().FirstOrDefault();
            if (window == null)
            {
                // 開かれてなかったら開く
                window = factory();
                window.Show();
            }
            else
            {
                // 既に開かれていたらアクティブにする
                window.Activate();
            }
        }
    }
}
