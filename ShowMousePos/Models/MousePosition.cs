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

namespace Automation.Models
{
    /// <summary>
    /// マウス関連のメソッドをまとめたクラス
    /// </summary>
    public class MousePosition
    {
        public MousePosition()
        {
            // 500ms間隔で値を発行する
            //var source = Observable.Interval(TimeSpan.FromMilliseconds(3000));
            // 購読
            //var subscription = source.Subscribe(
            //    i => {
            //        var pos = GetPosition();
            //        Console.WriteLine("OnNext(x:{0} y:{1})", pos.X, pos.Y);
            //    }
            //    ,
            //    ex => Console.WriteLine("OnError({0})", ex.Message),
            //    () => Console.WriteLine("Completed()"));

            //// マウスダウン、マウスアップ、マウスムーブのIObservableを作る
            //var mouseDown = Observable.FromEvent<MouseButtonEventHandler, MouseButtonEventArgs>(
            //    h => (s, e) => h(e),
            //    h => this.MouseDown += h,
            //    h => this.MouseDown -= h);

            //var ObservableMouseEvent = Observable.FromEvent<MouseEventHandler, MouseEventArgs>(
            //    h => (s, e) => h(e),
            //    h => this.MouseMove += h,
            //    h => this.MouseMove -= h);

            //var mouseUp = Observable.FromEvent<MouseButtonEventHandler, MouseButtonEventArgs>(
            //    h => (s, e) => h(e),
            //    h => this.MouseUp += h,
            //    h => this.MouseUp -= h);

            //var subscription = ObservableMousePosition.Subscribe(
            //    i =>
            //    {
            //        var pos = GetPosition();
            //        Console.WriteLine("OnNext(x:{0} y:{1})", pos.X, pos.Y);
            //    }
            //    ,
            //    ex => Console.WriteLine("OnError({0})", ex.Message),
            //    () => Console.WriteLine("Completed()"));

            //var drag = ObservableMouseEvent
            //    // マウスムーブをマウスダウンまでスキップ。マウスダウン時にマウスをキャプチャ
            //    .SkipUntil(mouseDown.Do(_ => this.CaptureMouse()))
            //    // マウスアップが行われるまでTake。マウスアップでマウスのキャプチャをリリース
            //    .TakeUntil(mouseUp.Do(_ => this.ReleaseMouseCapture()))
            //    // ドラッグが終了したタイミングでCompletedを表示
            //    .Finally(() => Console.WriteLine("finish"))
            //    // これを繰り返す
            //    .Repeat();

            //// ドラッグ中は、イベント引数から座標を取り出して表示用に整えてTextBlockに設定
            //drag.Select(e => e.GetPosition(null))
            //    .Select(p => string.Format("X: {0}, Y:{1}", p.X, p.Y))
            //    .Subscribe(s => Console.Write(s.ToString()));

        }

        [DllImport("USER32.dll", CallingConvention = CallingConvention.StdCall)]
        static extern void SetCursorPos(int X, int Y);

        [DllImport("USER32.dll", CallingConvention = CallingConvention.StdCall)]
        static extern void mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);

        private const int MOUSEEVENTF_LEFTDOWN = 0x2;
        private const int MOUSEEVENTF_LEFTUP = 0x4;

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

        // マウス座標観測者
        public IObservable<long> ObservableMousePosition { get; } = Observable.Interval(TimeSpan.FromMilliseconds(5));

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

        private Timer timer;
        public void Start()
        {
            timer = new Timer(_ => {
                this.OnPublish(GetPosition());
            },

            null,

            0,

            10);
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
