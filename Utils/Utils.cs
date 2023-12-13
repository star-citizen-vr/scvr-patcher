using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCVRPatcher {
    internal class Utils {
        public static Resolution GetMainScreenResolution() {
            var screen = System.Windows.Forms.Screen.PrimaryScreen;
            return new Resolution() { Width = screen.Bounds.Width, Height = screen.Bounds.Height };
        }
    }
}
