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
        public static LogitechGMineSweeper.MainWindow main;
        private System.Windows.Shapes.Rectangle c;

        [DllImport("user32.dll")]
        internal static extern int SetWindowCompositionAttribute(IntPtr hwnd, ref WindowCompositionAttributeData data);

        private ColorPopup()
        {
            InitializeComponent();
        }

        private void image1_MouseEnter(object sender, MouseEventArgs e)
        {
            string packUri = @"pack://application:,,,/closeWhite.png";
            image1.Source = new ImageSourceConverter().ConvertFromString(packUri) as ImageSource;
        }

        private void image1_MouseLeave(object sender, MouseEventArgs e)
        {
            string packUri = @"pack://application:,,,/close.png";
            image1.Source = new ImageSourceConverter().ConvertFromString(packUri) as ImageSource;
        }

        #region color picker
        private void ClrPcker_Background_SelectedColorChanged(object sender, EventArgs e)
        {
            MineSweeper.colors[index, 0] = ClrPcker_Background.B;
            MineSweeper.colors[index, 1] = ClrPcker_Background.G;
            MineSweeper.colors[index, 2] = ClrPcker_Background.R;

            main.UpdateColors();
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
        public static MessageBoxResult Show
        (string caption, string msg, MessageBoxType type)
        {
            switch (type)
            {
                case MessageBoxType.ConfirmationWithYesNo:
                    return Show(caption, msg, MessageBoxButton.YesNo);
                case MessageBoxType.ConfirmationWithYesNoCancel:
                    return Show(caption, msg, MessageBoxButton.YesNoCancel);
                case MessageBoxType.Information:
                    return Show(caption, msg, MessageBoxButton.OK);
                case MessageBoxType.Error:
                    return Show(caption, msg, MessageBoxButton.OK);
                case MessageBoxType.Warning:
                    return Show(caption, msg, MessageBoxButton.OK);
                default:
                    return MessageBoxResult.No;
            }
        }
        public static MessageBoxResult Show(string msg, MessageBoxType type)
        {
            return Show(string.Empty, msg, type);
        }
        public static MessageBoxResult Show(string msg)
        {
            return Show(string.Empty, msg,
            MessageBoxButton.OK);
        }
        public static MessageBoxResult Show
        (string caption, string text)
        {
            return Show(caption, text,
            MessageBoxButton.OK);
        }
        public static MessageBoxResult Show
        (string caption, string text, MessageBoxButton button)
        {
            _messageBox = new ColorPopup
            { MessageTitle = { Content = caption } };
            SetVisibilityOfButtons(button);
            _messageBox.ShowDialog();
            return _result;
        }


        public static MessageBoxResult Show
        (string caption, System.Windows.Media.Color a, MessageBoxButton button, int b, System.Windows.Shapes.Rectangle c)
        {
            _messageBox = new ColorPopup
            { MessageTitle = { Content = caption } };
            main = App.Current.MainWindow as MainWindow;
            _messageBox.c = c;
            _messageBox.index = b;
            _messageBox.ClrPcker_Background.SelectedColor = a;
            SetVisibilityOfButtons(button);
            _messageBox.ShowDialog();
            return _result;
        }

        public static MessageBoxResult Show
        (string caption, System.Windows.Media.Color a, MessageBoxButton button, int b)
        {
            _messageBox = new ColorPopup
            { MessageTitle = { Content = caption } };
            main = App.Current.MainWindow as MainWindow;
            _messageBox.index = b;
            _messageBox.ClrPcker_Background.SelectedColor = a;
            SetVisibilityOfButtons(button);
            _messageBox.ShowDialog();
            return _result;
        }

        private static void SetVisibilityOfButtons(MessageBoxButton button)
        {
            switch (button)
            {
                case MessageBoxButton.OK:
                    _messageBox.btnCancel.Visibility = Visibility.Collapsed;
                    _messageBox.btnNo.Visibility = Visibility.Collapsed;
                    _messageBox.btnYes.Visibility = Visibility.Collapsed;
                    _messageBox.btnOk.Focus();
                    break;
                case MessageBoxButton.OKCancel:
                    _messageBox.btnNo.Visibility = Visibility.Collapsed;
                    _messageBox.btnYes.Visibility = Visibility.Collapsed;
                    _messageBox.btnCancel.Focus();
                    break;
                case MessageBoxButton.YesNo:
                    _messageBox.btnOk.Visibility = Visibility.Collapsed;
                    _messageBox.btnCancel.Visibility = Visibility.Collapsed;
                    _messageBox.btnNo.Focus();
                    break;
                case MessageBoxButton.YesNoCancel:
                    _messageBox.btnOk.Visibility = Visibility.Collapsed;
                    _messageBox.btnCancel.Focus();
                    break;
                default:
                    break;
            }
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (sender == btnOk)
                _result = MessageBoxResult.OK;
            else if (sender == btnYes)
                _result = MessageBoxResult.Yes;
            else if (sender == btnNo)
                _result = MessageBoxResult.No;
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
