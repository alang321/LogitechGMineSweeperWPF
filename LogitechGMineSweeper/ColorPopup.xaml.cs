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

        private void image1_MouseEnter(object sender, MouseEventArgs e)
        {
            string packUri = @"pack://application:,,,/symbols/closeWhite.png";
            image1.Source = new ImageSourceConverter().ConvertFromString(packUri) as ImageSource;
        }

        private void image1_MouseLeave(object sender, MouseEventArgs e)
        {
            string packUri = @"pack://application:,,,/symbols/close.png";
            image1.Source = new ImageSourceConverter().ConvertFromString(packUri) as ImageSource;
        }

        #region color picker

        private void ClrPcker_Background_SelectedColorChanged(object sender, EventArgs e)
        {
            MineSweeper.colors[index, 0] = ClrPcker_Background.B;
            MineSweeper.colors[index, 1] = ClrPcker_Background.G;
            MineSweeper.colors[index, 2] = ClrPcker_Background.R;

            Application.Current.Resources["buttonColorBrush" + index.ToString()] = new SolidColorBrush(System.Windows.Media.Color.FromRgb(MineSweeper.colors[index, 2], MineSweeper.colors[index, 1], MineSweeper.colors[index, 0]));

            if(index == 12 || index == 13 || index == 14)
            {
                MineSweeper.colors[9, 0] = ClrPcker_Background.B;
                MineSweeper.colors[9, 1] = ClrPcker_Background.G;
                MineSweeper.colors[9, 2] = ClrPcker_Background.R;
                Application.Current.Resources["buttonColorBrush9"] = new SolidColorBrush(System.Windows.Media.Color.FromRgb(MineSweeper.colors[9, 2], MineSweeper.colors[9, 1], MineSweeper.colors[9, 0]));
            }
            else
            {
                if (MineSweeper.colors[index, 2] + MineSweeper.colors[index, 1] + MineSweeper.colors[index, 0] < Config.foregroundThreshold)
                {
                    Application.Current.Resources["TextColorBrush" + index.ToString()] = new SolidColorBrush(Colors.White);
                }
                else
                {
                    Application.Current.Resources["TextColorBrush" + index.ToString()] = new SolidColorBrush(Colors.Black);
                }
            }

            MineSweeper.printLogiLED(false);
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
            if (index == 9)
            {
                switch (MineSweeper.currentBack)
                {
                    case 0:
                        index = 14;
                        break;
                    case 1:
                        index = 13;
                        break;
                    case 2:
                        index = 12;
                        break;
                }
            }
            _messageBox = new ColorPopup{ MessageTitle = { Content = Config.colorPickerTitles[index] } };
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
