using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Automation.Models
{
    public class ScreenCapture
    {
        private Bitmap capturedBitmap;

        public ScreenCapture()
        {
            
        }

        public Bitmap OnCapture(int sx, int sy, int ex, int ey)
        {
            capturedBitmap?.Dispose();
            var rc = new  Rectangle(sx, sy, ex, ey);

            capturedBitmap = new Bitmap(rc.Width, rc.Height, PixelFormat.Format32bppArgb);
            using (Graphics g = Graphics.FromImage(capturedBitmap))
            {
                g.CopyFromScreen(rc.X, rc.Y, 0, 0, rc.Size, CopyPixelOperation.SourceCopy);
            }
            return capturedBitmap;

        }
    }
}
