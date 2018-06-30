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
    public partial class MessageBox : Window
    {

        [DllImport("user32.dll")]
        internal static extern int SetWindowCompositionAttribute(IntPtr hwnd, ref WindowCompositionAttributeData data);

        private MessageBox()
        {
            InitializeComponent();
        }

        private void Window_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            DragMove();
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

        static MessageBox _messageBox;
        static MessageBoxResult _result = MessageBoxResult.No;
        public static MessageBoxResult Show
        (string caption, string msg, MessageBoxType type)
        {
            switch (type)
            {
                case MessageBoxType.ConfirmationWithYesNo:
                    return Show(caption, msg, MessageBoxButton.YesNo,
                    MessageBoxImage.Question);
                case MessageBoxType.ConfirmationWithYesNoCancel:
                    return Show(caption, msg, MessageBoxButton.YesNoCancel,
                    MessageBoxImage.Question);
                case MessageBoxType.Information:
                    return Show(caption, msg, MessageBoxButton.OK,
                    MessageBoxImage.Information);
                case MessageBoxType.Error:
                    return Show(caption, msg, MessageBoxButton.OK,
                    MessageBoxImage.Error);
                case MessageBoxType.Warning:
                    return Show(caption, msg, MessageBoxButton.OK,
                    MessageBoxImage.Warning);
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
            MessageBoxButton.OK, MessageBoxImage.None);
        }
        public static MessageBoxResult Show
        (string caption, string text)
        {
            return Show(caption, text,
            MessageBoxButton.OK, MessageBoxImage.None);
        }
        public static MessageBoxResult Show
        (string caption, string text, MessageBoxButton button)
        {
            return Show(caption, text, button,
            MessageBoxImage.None);
        }
        public static MessageBoxResult Show
        (string caption, string text,
        MessageBoxButton button, MessageBoxImage image)
        {
            _messageBox = new MessageBox
            { txtMsg = { Text = text }, MessageTitle = { Content = caption } };
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
