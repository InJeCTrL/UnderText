using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Serialization;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace UnderText
{
    [DesignTimeVisible(false)]
    public partial class MainPage : ContentPage
    {
        /// <summary>
        /// list saved AES key object
        /// </summary>
        public static ObservableCollection<AESKey> AESKeys;
        /// <summary>
        /// use RSA to encrypt and decrypt AES key
        /// </summary>
        private RSACryptoServiceProvider rsaProvider = new RSACryptoServiceProvider(2048);
        /// <summary>
        /// RSA public key
        /// </summary>
        private string PK;
        /// <summary>
        /// load storaged list and start listening AESKeys
        /// </summary>
        public MainPage()
        {
            InitializeComponent();
            ReadList();
            list_key.ItemsSource = AESKeys;
            PK = rsaProvider.ToXmlString(false);
            AESKeys.CollectionChanged += AESKeys_CollectionChanged;
        }
        /// <summary>
        /// save AESKeys to ".xml" while the list changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AESKeys_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            SaveList();
        }
        /// <summary>
        /// check clipboard content
        /// </summary>
        private async void CheckClipboard()
        {
            string text = await Clipboard.GetTextAsync();
            string head = "UnderText://";
            try
            {
                if (text.StartsWith(head))
                {
                    text = text.Substring(text.IndexOf(head) + head.Length);
                    // get aes key encrypted by rsa public key
                    // then search key list
                    if (text.StartsWith("."))
                    {
                        byte[] data = Convert.FromBase64String(text.Substring(1));
                        string dec = Encoding.ASCII.GetString(rsaProvider.Decrypt(data, false));
                        if (AESKey.Validate(dec))
                        {
                            bool no_key = true;
                            foreach (var key in AESKeys)
                            {
                                if (key.Key.Equals(dec))
                                {
                                    no_key = false;
                                    break;
                                }
                            }
                            if (no_key)
                            {
                                AESKey newKey = new AESKey(dec);
                                AESKeys.Add(newKey);
                                await Navigation.PushAsync(new KeySetting(newKey));
                            }
                        }
                    }
                    // get rsa public key
                    // then generate aes key and send
                    else if (text.StartsWith(","))
                    {
                        string PK_recved = Encoding.ASCII.GetString(Convert.FromBase64String(text.Substring(1)));
                        if (!PK_recved.Equals(PK))
                        {
                            RSACryptoServiceProvider tRSAProvider = new RSACryptoServiceProvider(2048);
                            tRSAProvider.FromXmlString(PK_recved);
                            AESKey newKey = new AESKey(AESKey.Generate());
                            AESKeys.Add(newKey);
                            await Clipboard.SetTextAsync("UnderText://." + Convert.ToBase64String(tRSAProvider.Encrypt(Encoding.ASCII.GetBytes(newKey.Key), false)));
                            await DisplayAlert("Handshake(Step 2)", "AES packet has set to clipboard,\npaste and send the string to partner(s) who send you RSA packet,\nthen others will have the AES key.", "OK");
                            await Navigation.PushAsync(new KeySetting(newKey));
                        }
                    }
                    // get text data encrypted by aes key
                    // find right aes key in the list to decrypt
                    else
                    {
                        foreach (var key in AESKeys)
                        {
                            if (key.Decrypt(text) != string.Empty)
                            {
                                await Navigation.PushAsync(new KeySetting(key));
                                break;
                            }
                        }
                    }
                }
            }
            catch
            {
                ;
            }
        }
        /// <summary>
        /// save AESKeys
        /// </summary>
        public static void SaveList()
        {
            try
            {
                string folder = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
                using (FileStream fs = new FileStream(folder + "/keylist.xml", FileMode.Create))
                {
                    XmlSerializer xmlSerializer = new XmlSerializer(typeof(ObservableCollection<AESKey>));
                    xmlSerializer.Serialize(fs, AESKeys);
                }
            }
            catch
            {
                ;
            }
        }
        /// <summary>
        /// load AESKeys from local file
        /// </summary>
        private void ReadList()
        {
            try
            {
                string folder = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
                using (FileStream fs = new FileStream(folder + "/keylist.xml", FileMode.Open))
                {
                    XmlSerializer xmlSerializer = new XmlSerializer(typeof(ObservableCollection<AESKey>));
                    AESKeys = (ObservableCollection<AESKey>)xmlSerializer.Deserialize(fs);
                }
            }
            catch
            {
                AESKeys = new ObservableCollection<AESKey>();
            }
        }
        /// <summary>
        /// load RSA public key to clipboard as Handshake packet 1
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Handshake_Clicked(object sender, EventArgs e)
        {
            Clipboard.SetTextAsync("UnderText://," + Convert.ToBase64String(Encoding.UTF8.GetBytes(PK)));
            DisplayAlert("Handshake(Step 1)", "RSA packet has set to clipboard,\npaste and send the string,\nthen others can send AES packet to you.", "OK");
        }
        /// <summary>
        /// check clipboard once while top activity switched to mainpage(not from KeySetting page)
        /// </summary>
        protected override void OnAppearing()
        {
            base.OnAppearing();
            if (KeySetting.BackFromSetting)
            {
                KeySetting.BackFromSetting = false;
            }
            else
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
        /// <summary>
        /// listitem clicked, then push keysetting page to top
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void list_key_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            list_key.SelectedItem = null;
            Navigation.PushAsync(new KeySetting((AESKey)e.Item));
        }
    }
    public class KeyMask : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string tKey = value.ToString();
            return tKey.Substring(0, 26).PadRight(32, '*');
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }
}
