using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using Prism.Mvvm;
using System.Windows;
using System.Windows.Input;
using System.Reactive.Linq;
using System.Threading;
using System.Collections.ObjectModel;

namespace Automation.Models
{
    /// <summary>
    /// マウス関連のメソッドをまとめたクラス
    /// </summary>
    public class AutoMouseClick
    {
        public AutoMouseClick(ObservableCollection<Coordinate> points, double interval = 5.0)
        {
            Points = points;
            ObservableMousePosition = Observable.Interval(TimeSpan.FromMilliseconds(interval));

            Source = Observable.Generate(
                // 開始
                1,
                // 条件式(trueなら継続/falseでonComplete)
                i => i <= Points.Max(x => x.Id),
                // iは1ずつ増える
                i => ++i,
                // 発行する値
                i => i,
                // 値はxxxms間隔で発行する
                i => TimeSpan.FromMilliseconds(Points.First(x => x.Id == i).MilliSecDelayTime)
            ).Repeat();
        }

        [DllImport("USER32.dll", CallingConvention = CallingConvention.StdCall)]
        static extern void SetCursorPos(int X, int Y);

        [DllImport("USER32.dll", CallingConvention = CallingConvention.StdCall)]
        static extern void mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);

        private const int MOUSEEVENTF_LEFTDOWN = 0x2;
        private const int MOUSEEVENTF_LEFTUP = 0x4;

        private ObservableCollection<Coordinate> Points { get; set; }

        public void SetMouseCursorPos(int x, int y)
        {
            SetCursorPos(x, y);
        }

        public void ClickMouse(int x, int y)
        {
            SetCursorPos(x, y);
            
            System.Threading.Thread.Sleep(100);

            mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
            mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
        }

        // 内部的に設定された時間でイベント発行する
        private IObservable<int> Source;

        // マウス座標観測者
        public IObservable<long> ObservableMousePosition { get; }

        // マウス座標値発行イベント 
        private EventHandler<MouseEventArgs> publish;
        public EventHandler<MouseEventArgs> Publish { get => publish; set => publish= value; }

        /// <summary>
        /// マウスカーソルの位置を取得します。
        /// </summary>
        public Point GetPosition()
        {
            var x = System.Windows.Forms.Cursor.Position.X;
            var y = System.Windows.Forms.Cursor.Position.Y;

            return new Point(x, y);
        }

        private IDisposable Subscription = null;

        public void Start()
        {
            Subscription?.Dispose();

            Subscription = this.Source.ObserveOnDispatcher().
            Where(i => Points.Any(x => x.Id == i)).
            Subscribe(
            i =>
            {
                Console.WriteLine($"{i} times");
                this.SetMouseCursorPos(Points[i].X, Points[i].Y);
                Thread.Sleep(100);
            }
            ,
            ex => Console.WriteLine("OnError({0})", ex.Message),
            () => Console.WriteLine("Completed()"));
        }

        // マウスが値を発行したときのイベント引数 
        public class MouseEventArgs : EventArgs
        {
            // 発行した値 
            public Point Value { get; private set; }

            public MouseEventArgs(Point value) { this.Value = value; }

            internal object Aggregate(Func<object, object, object> p)
            {
                throw new NotImplementedException();
            }
        }

        // 値を発行する 
        public void OnPublish(Point value) {
            this.Publish?.Invoke(this, new MouseEventArgs(value));
        }
    }
}
