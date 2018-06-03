using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Timers;
using System.Diagnostics;
using System.Linq;
using System.Windows.Threading;

namespace LogitechGMineSweeper
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class ColorPopup : Window
    {
        int index = 0;

        [DllImport("user32.dll")]
        internal static extern int SetWindowCompositionAttribute(IntPtr hwnd, ref WindowCompositionAttributeData data);

        private ColorPopup()
        {
            InitializeComponent();
        }

        #region color picker

        private void ClrPcker_Background_SelectedColorChanged(object sender, EventArgs e)
        {
            Config.MineSweeper.Colors[index, 0] = ClrPcker_Background.B;
            Config.MineSweeper.Colors[index, 1] = ClrPcker_Background.G;
            Config.MineSweeper.Colors[index, 2] = ClrPcker_Background.R;

            Application.Current.Resources["buttonColorBrush" + index.ToString()] = new SolidColorBrush(System.Windows.Media.Color.FromRgb(Config.MineSweeper.Colors[index, 2], Config.MineSweeper.Colors[index, 1], Config.MineSweeper.Colors[index, 0]));

            if (index != 12 && index != 13 && index != 14)
            {
                if (Config.MineSweeper.Colors[index, 2] + Config.MineSweeper.Colors[index, 1] + Config.MineSweeper.Colors[index, 0] < Config.ForegroundThreshold)
                {
                    Application.Current.Resources["TextColorBrush" + index.ToString()] = new SolidColorBrush(Colors.White);
                }
                else
                {
                    Application.Current.Resources["TextColorBrush" + index.ToString()] = new SolidColorBrush(Colors.Black);
                }
            }

            Config.MineSweeper.PrintLogiLED(false);
        }
        #endregion
        
        private void Window_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            DragMove();
        }
        
        private void Button_static(object sender, EventArgs e)
        {
            var button = sender as Button;
            var color = button.Background as SolidColorBrush;
            ClrPcker_Background.SelectedColor = color.Color; 
        }

        #region blur

        protected override void OnStateChanged(EventArgs e)
        {
            if (WindowState == System.Windows.WindowState.Minimized)
            {
                this.Hide();
            }
            base.OnStateChanged(e);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            EnableBlur();
        }

        internal void EnableBlur()
        {
            var windowHelper = new WindowInteropHelper(this);

            var accent = new AccentPolicy();
            accent.AccentState = AccentState.ACCENT_ENABLE_BLURBEHIND;

            var accentStructSize = Marshal.SizeOf(accent);

            var accentPtr = Marshal.AllocHGlobal(accentStructSize);
            Marshal.StructureToPtr(accent, accentPtr, false);

            var data = new WindowCompositionAttributeData();
            data.Attribute = WindowCompositionAttribute.WCA_ACCENT_POLICY;
            data.SizeOfData = accentStructSize;
            data.Data = accentPtr;

            SetWindowCompositionAttribute(windowHelper.Handle, ref data);

            Marshal.FreeHGlobal(accentPtr);
        }

        #endregion

        public enum MessageBoxType
        {
            ConfirmationWithYesNo = 0,
            ConfirmationWithYesNoCancel,
            Information,
            Error,
            Warning
        }

        public enum MessageBoxImage
        {
            Warning = 0,
            Question,
            Information,
            Error,
            None
        }

        static ColorPopup _messageBox;
        static MessageBoxResult _result = MessageBoxResult.No;

        public static MessageBoxResult Show(System.Windows.Media.Color selectedColor, int index)
        {
            _messageBox = new ColorPopup{ MessageTitle = { Content = Config.ColorPickerTitles[index] } };
            _messageBox.index = index;
            _messageBox.ClrPcker_Background.SelectedColor = selectedColor;
            _messageBox.ShowDialog();
            return _result;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (sender == btnOk)
                _result = MessageBoxResult.OK;
            else if (sender == btnCancel)
                _result = MessageBoxResult.Cancel;
            else
                _result = MessageBoxResult.None;
            _messageBox.Close();
            _messageBox = null;
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            _result = MessageBoxResult.None;
            _messageBox.Close();
        }
    }
}
