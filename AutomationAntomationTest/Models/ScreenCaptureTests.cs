using Microsoft.VisualStudio.TestTools.UnitTesting;
using Automation.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;

namespace Automation.Models.Tests
{
    [TestClass()]
    public class ScreenCaptureTests
    {
        [TestMethod()]
        public void OnCaptureTest()
        {
            var sc = new ScreenCapture();

            int sx = 2483;
            int sy = 328;
            int ex = 2731;
            int ey = 498;

            var bm = sc.OnCapture(sx, sy, ex - sx, ey - sy);
            bm.Save(@"C:\Users\Owner\Pictures\Capture\TestBmp.bmp", System.Drawing.Imaging.ImageFormat.Bmp);
            //Assert.Fail();
        }
    }
}