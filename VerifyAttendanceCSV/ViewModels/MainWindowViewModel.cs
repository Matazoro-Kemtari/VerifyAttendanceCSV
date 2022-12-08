using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Reflection;

namespace VerifyAttendanceCSV.ViewModels
{
    public class MainWindowViewModel : BindableBase, IDisposable
    {
        private readonly IRegionManager regionManager;

        private static readonly Version version = Assembly.GetExecutingAssembly().GetName().Version;
        private static readonly string assemblyName = Assembly.GetExecutingAssembly().GetName().Name;
        private string _title = $"{assemblyName} {version}";

        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }

        public MainWindowViewModel(IRegionManager regMan)
        {
            regionManager = regMan;
        }

        private bool disposedValue = false; // 重複する呼び出しを検出する

        // NOTE: https://qiita.com/Zuishin/items/9efc9c8cbb98300bbc64
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // マネージド状態を破棄します (マネージド オブジェクト)。
                    foreach (var region in this.regionManager.Regions)
                    {
                        region.RemoveAll();
                    }
                }

                disposedValue = true;
            }
        }

        /// <summary>オブジェクトを破棄します</summary>
        public void Dispose()
        {
            // このコードを変更しないでください
            // クリーンアップ コードを上の Dispose(bool disposing) に記述します
            Dispose(true);
            //GC.SuppressFinalize(this);
        }
    }
}
