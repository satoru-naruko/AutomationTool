using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Prism.Mvvm;
using Automation.Models;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Prism.Commands;
using System.Reactive.Concurrency;
using System.Collections.ObjectModel;
using Newtonsoft.Json;
using System.IO;
using System.Windows.Forms;

namespace Automation.ViewModels
{
    class MouseViewModel : BindableBase
    {
        #region コンストラクタ
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public MouseViewModel()
        {
            ClickCommand = new DelegateCommand<object>(
                (_id) => {
                    // 変換
                    var id = int.Parse(_id.ToString());
                    // ランダム値
                    Random rnd = new System.Random();
                    // 要素取得
                    var pt = Points.First(x => x.Coordinate.Id == id);
                    // クリックイベント発行
                    mousePositionModel.ClickMouse(pt.Coordinate.X + rnd.Next(-20, 10), pt.Coordinate.Y + rnd.Next(-20, 10));
                    // クリックが完了するまでSleepする
                    System.Threading.Thread.Sleep(100);
                    // Windowをアクティブにする
                    WindowManager.ShowOrActivate<Views.Mouse>();
                }
            );

            SaveCommand = new DelegateCommand(SavePoints);

            AddCommand = new DelegateCommand(
                () => {
                    var id = Points.Count() + 1;
                    Points.Add(new CoordinateDispInfo(new Models.Coordinate(id, 0, 0)));
                    Subscribe?.RaiseCanExecuteChanged();
                }
            );

            DeleteCommand = new DelegateCommand(
                () => {
                    for (int index = 0; index < Points.Count; index++)
                    {
                        if ( Points[index].IsSelected )
                        {
                            Points.RemoveAt(index);
                            Subscribe?.RaiseCanExecuteChanged();
                        }
                    }
                }
            );

            StopCommand = new DelegateCommand( () => Subscription?.Dispose());

            var mousemove = Observable.FromEvent<EventHandler<Models.MousePosition.MouseEventArgs>, Models.MousePosition.MouseEventArgs>(
                    h => (s, e) => h(e),
                    h => mousePositionModel.Publish += h,
                    h => mousePositionModel.Publish -= h
            );

            mousemove.Subscribe(s =>
            {
                //Console.WriteLine("x:{0},y:{1}", s.Value.X, s.Value.Y);
                PosX = (int)s.Value.X;
                PosY = (int)s.Value.Y;
                //Console.WriteLine("ClickPos => x:{0},y:{1}", s.Value.X, s.Value.Y);

            });

            // マウス自動クリックイベント購読開始
            Subscribe = new DelegateCommand(
                () =>
                {
                    Subscription?.Dispose();

                    Subscription = this.Source.ObserveOnDispatcher().
                    Where(i => Points.Any(x => x.Coordinate.Id == i)).
                    Subscribe(
                    i =>
                    {
                         Console.WriteLine($"{i} times");
                         ClickCommand?.Execute(i);
                    }
                    ,
                    ex => Console.WriteLine("OnError({0})", ex.Message),
                    () => Console.WriteLine("Completed()"));
                },
                CanExecute
            );

            Dispose = new DelegateCommand( () => Subscription?.Dispose() );

            //LoadPoints();

            mousePositionModel.Start();
        }

        #endregion

        #region デストラクタ

        ~MouseViewModel()
        {
            Dispose.Execute();
        }
        
        # endregion

        private IObservable<int> Source = Observable.Generate(
            // 開始
            1,
            // 条件式(trueなら継続/falseでonComplete)
            i => i <= Points.Max(x => x.Coordinate.Id),
            // iは1ずつ増える
            i => ++i,
            // 発行する値
            i => i,
            // 値はxxxms間隔で発行する
            i => TimeSpan.FromMilliseconds(Points.First(x => x.Coordinate.Id == i).Coordinate.MilliSecDelayTime)
        ).Repeat();

        private IDisposable Subscription = null;

        public DelegateCommand<object> ClickCommand { get; private set; }
        public DelegateCommand SaveCommand { get; private set; }
        public DelegateCommand DeleteCommand { get; private set; }
        public DelegateCommand StopCommand { get; private set; }
        public DelegateCommand AddCommand { get; private set; }
        public DelegateCommand Dispose { get; private set; }
        public DelegateCommand Subscribe { get; private set; }

        private int posX = 100;
        public int PosX
        {
            get { return this.posX; }
            private set
            {
                this.SetProperty(ref this.posX, value);
            }
        }

        private int posY = 100;
        public int PosY
        {
            get { return this.posY; }
            private set
            {
                this.SetProperty(ref this.posY, value);
            }
        }

        private string state = "停止中";
        public string State
        {
            get { return this.state; }
            private set
            {
                this.SetProperty(ref this.state, value);
            }
        }

        public class CoordinateDispInfo
        {
            public CoordinateDispInfo(Models.Coordinate codinate )
            {
                this.Coordinate = codinate;
            }

            public Models.Coordinate Coordinate { get; set; } = new Models.Coordinate();
            public bool IsSelected { get; set; } = false;
        }

        public static ObservableCollection<CoordinateDispInfo> Points { get; set; } = new ObservableCollection<CoordinateDispInfo>();

        public List<Models.Coordinate> Coordinates
        {
            get { return Points.Select( x => x.Coordinate ).ToList(); }
            set {
                foreach (var pt in value ?? Enumerable.Empty<Models.Coordinate>())
                {
                    Points.Add(new CoordinateDispInfo(pt));
                }
            }
        }

        public void SavePoints()
        {
            using (var dialog = new SaveFileDialog())
            {
                dialog.Title = "ファイルを保存";
                dialog.Filter = "テキストファイル|*.json";

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    Coordinate.Serialize(Coordinates, dialog.FileName);
                }
                else
                {
                    Console.WriteLine("キャンセルされました");
                }
            }
        }

        public void LoadPoints()
        {
            Points.Clear();

            using (var dialog = new OpenFileDialog())
            {
                dialog.Title = "ファイルを開く";
                dialog.Filter = "テキストファイル|*.json";

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    var jsonString = File.ReadAllText(dialog.FileName);
                    Coordinates = JsonConvert.DeserializeObject<List<Models.Coordinate>>(jsonString);
                }
                else
                {
                    Console.WriteLine("キャンセルされました");
                }
            }
        }



        private bool CanExecute()
        {
            Console.WriteLine("CanExecute method calld");
            return Points.Count() > 0;
        }

        private Models.MousePosition mousePositionModel = new Models.MousePosition();

    }
}
