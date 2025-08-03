using System;
using System.Windows.Controls.Primitives;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows;
using System.Windows.Media;

namespace PunchPal.WPF.Converters
{
    public class EmojiPopupService
    {
        private static Popup _popup;
        private static WrapPanel _emojiPanel;
        private static Action<string> _onEmojiSelected;

        static EmojiPopupService()
        {
            _emojiPanel = new WrapPanel();
            string[] emojis = { 
                "😀", "😃", "😄", "😁", "😆", "😅", "😂", "🤣", "🥲", "🥹", 
                "😊", "😇", "🙂", "🙃", "😉", "😌", "😍", "🥰", "😘", "😗", 
                "😙", "😚", "😋", "😛", "😝", "😜", "🤪", "🤨", "🧐", "🤓", 
                "😎", "🥸", "🤩", "🥳", "🙂‍", "😏", "😒", "🙂‍", "😞", "😔", 
                "😟", "😕", "🙁", "😣", "😖", "😫", "😩", "🥺", "😢", "😭", 
                "😮‍💨", "😤", "😠", "😡", "🤬", "🤯", "😳", "🥵", "🥶", "😱", 
                "😨", "😰", "😥", "😓", "🫣", "🤗", "🫡", "🤔", "🫢", "🤭", 
                "🤫", "🤥", "😶", "😶‍🌫️", "😐", "😑", "😬", "🫨", "🫠", "🙄", 
                "😯", "😦", "😧", "😮", "😲", "🥱", "😴", "🤤", "😪", "😵", 
                "😵‍💫", "🫥", "🤐", "🥴", "🤢", "🤮", "🤧", "😷", "🤒", "🤕", 
                "🤑", "🤠", "😈", "👿", "👹", "👺", "🤡", "💩", "👻", "💀", 
                "☠️", "👽", "👾", "🤖", "🎃", "😺", "😸", "😹", "😻", "😼", 
                "😽", "🙀", "😿", "😾" 
            };

            foreach (var emoji in emojis)
            {
                var tb = new Emoji.Wpf.TextBlock
                {
                    Text = emoji,
                    FontSize = 24,
                    Margin = new Thickness(5),
                    Cursor = Cursors.Hand
                };
                tb.MouseLeftButtonDown += (s, e) =>
                {
                    var text = ((Emoji.Wpf.TextBlock)s).Text;
                    if(!string.IsNullOrWhiteSpace(text))
                    {
                        _onEmojiSelected?.Invoke(text);
                    }
                    _popup.IsOpen = false;
                };
                _emojiPanel.Children.Add(tb);
            }

            _popup = new Popup
            {
                Child = new Border
                {
                    BorderThickness = new Thickness(1),
                    Padding = new Thickness(0),
                    MaxHeight = 220,
                    MaxWidth = 400,
                    Child = new ScrollViewer
                    {
                        Content = _emojiPanel,
                        Padding = new Thickness(5),
                        VerticalScrollBarVisibility = ScrollBarVisibility.Auto
                    }
                },
                StaysOpen = false,
                AllowsTransparency = true,
                PopupAnimation = PopupAnimation.Fade
            };

            // 关闭弹窗时清除回调
            _popup.Closed += OnPopupClosed;
            //_popup.Opened += onPopupOpened;
        }

        //private static void onPopupOpened(object sender, EventArgs e)
        //{
        //    Window.GetWindow((DependencyObject)sender).Deactivated += OnDeactivated;
        //}

        //private static void OnDeactivated(object sender, EventArgs e)
        //{
        //    EmojiPopup.IsOpen = false;
        //}

        private static void OnPopupClosed(object sender, EventArgs e)
        {
            _onEmojiSelected = null;
        }

        public static void ShowAt(UIElement target, Action<string> onSelected)
        {
            Border _border = (Border)_popup.Child;
            _border.Background = (Brush)_border.FindResource("ApplicationBackgroundBrush");
            _border.BorderBrush = (Brush)_border.FindResource("CardBorderBrush");
            _popup.PlacementTarget = target;
            _popup.Placement = PlacementMode.Bottom;
            _onEmojiSelected = onSelected;
            _popup.IsOpen = true;
        }

        public static void Close() => _popup.IsOpen = false;
    }
}
