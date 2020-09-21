using System;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace UnderText
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class KeySetting : ContentPage
    {
        /// <summary>
        /// whether MainPage is backed from KeySetting page
        /// </summary>
        public static bool BackFromSetting = false;
        /// <summary>
        /// AES key used in this page
        /// </summary>
        private AESKey Key;
        /// <summary>
        /// initialize page by AES key object
        /// </summary>
        /// <param name="aeskey">AES key object</param>
        /// <param name="encrypted">encrypted text ready to decrypt</param>
        public KeySetting(AESKey aeskey)
        {
            InitializeComponent();
            Key = aeskey;
            // mask AES key
            string tKey = aeskey.Key.Substring(0, 26).PadRight(32, '*');
            txt_key.Text = tKey;
            txt_remark.Text = aeskey.Remark;
            BackFromSetting = true;
        }
        /// <summary>
        /// check clipboard content and try to decrypt text
        /// </summary>
        private void CheckClipboard()
        {
            string text = Clipboard.GetTextAsync().Result;
            string head = "UnderText://";
            try
            {
                if (text.StartsWith(head))
                {
                    text = text.Substring(text.IndexOf(head) + head.Length);
                    string decrypted = Key.Decrypt(text);
                    if (decrypted != string.Empty)
                    {
                        txt_Received.Text = decrypted;
                    }
                }
            }
            catch
            {
                ;
            }
        }
        /// <summary>
        /// save remark of the key to list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Save(object sender, EventArgs e)
        {
            Key.Remark = txt_remark.Text;
        }
        /// <summary>
        /// remove the key from the AESKey list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Delete(object sender, EventArgs e)
        {
            MainPage.AESKeys.Remove(Key);
            Navigation.PopAsync();
        }
        /// <summary>
        /// copy encrypted text
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Clicked(object sender, EventArgs e)
        {
            var text = txt_Send.Text;
            if (text == null || text.Length == 0)
            {
                DisplayAlert("TIP", "Wanna text can't be empty.", "OK");
            }
            else
            {
                string encrypted = "UnderText://" + Key.Encrypt(text);
                Clipboard.SetTextAsync(encrypted);
                txt_Send.Text = "";
            }
        }
        /// <summary>
        /// check clipboard once while top activity switched to keysetting
        /// </summary>
        protected override void OnAppearing()
        {
            Android.OS.Handler a = new Android.OS.Handler();
            a.Post(() =>
            {
                CheckClipboard();
            });
            /*
            a.PostDelayed(() =>
            {
                CheckClipboard();
            }, 1000);*/
        }
    }
}