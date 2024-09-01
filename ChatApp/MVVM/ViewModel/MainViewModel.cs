using ChatApp.Core;
using ChatApp.MVVM.Model;
using System;
using System.Collections.ObjectModel;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Windows;

#pragma warning disable CS8618
namespace ChatApp.MVVM.ViewModel
{
	/// <summary>
	/// メインVeiwModel
	/// </summary>
	class MainViewModel : ObservableObject
	{
		public ObservableCollection<MessageModel> Messages { get; set; } = new ObservableCollection<MessageModel>();
		public ObservableCollection<ContactModel> Contacts { get; set; } = new ObservableCollection<ContactModel>();

		// 送信ボタン押下処理
		public RelayCommand SendCommand { get; set; }

		// 選択中の連絡先
		private ContactModel _contactModel;
		// 選択中の連絡先(外部公開)
		public ContactModel SelectedContact
		{
			get { return _contactModel; }
			set
			{
				_contactModel = value;
				OnPropertyChanged();
			}
		}

		// 入力メッセージ
		private string _message;
		// 入力メッセージ(外部公開)
		public string Message
		{
			get { return _message; }
			set
			{
				_message = value;
				OnPropertyChanged();
			}
		}

        // 入力メッセージ
        private string _username;
        // 入力メッセージ(外部公開)
        public string Username
        {
            get { return _username; }
            set
            {
                _username = value;
                OnPropertyChanged();
            }
        }

        // ポート番号
        private const int Port = 60000;

        // 他端末と送受信するメッセージデータ
        public class Payload
        {
            public string Name { get; set; }
            public string Content { get; set; }
        }

        // UDPクライアント
        private static UdpClient udpClient;

        /// <summary>
        /// コンストラクタ
        /// 画面更新用のデータを登録し、画面に反映する
        /// </summary>
        public MainViewModel()
		{
            // 初期化
            Username = "Me";

            // コンタクトリストを登録する
            // (v1.0は全体チャットの"Everyone"のみ)
            Contacts.Add(new ContactModel
			{
				Username = $"Everyone",
				IconColor = "CornflowerBlue",
				Messages = Messages
			});
			SelectedContact = Contacts[0];

            // 送信ボタン押下処理を登録する
            SendCommand = new RelayCommand(o =>
			{
                // 空文字なら送信しない
                if (Message == "") return;

                // 送信するメッセージを画面表示に反映する
                RegisterScreenMessage(
					name: Username, 
					content: Message, 
					time: DateTime.Now);

                // UDP送信にて他端末にブロードキャスト送信する
                SendMessage(
					name: Username, 
					content: Message);

                // 入力欄をクリアする
                Message = "";
			});

            // 他端末からの受信処理の開始
            // (非同期)
            udpClient = new UdpClient(Port);
            BeginReceiveMessage();
        }

        /// <summary>
        /// メッセージの送信処理
        /// <param name="name">ユーザ名</param>
        /// <param name="content">メッセージ本文</param>
        /// </summary>
        private void SendMessage(string name, string content)
		{
            /* 見本用サンプル */
            // ブロードキャストアドレスとポート番号を設定
            var broadcastAddress = "255.255.255.255";
            var port = 60000;

            // UdpClient の設定でブロードキャストを有効にする
            udpClient.EnableBroadcast = true;

            // Json文字列を成型
            string jsonString = JsonSerializer.Serialize(
                new Payload
                {
                    Name = name,
                    Content = content,
                });
            byte[] payload = Encoding.UTF8.GetBytes(jsonString);

            // データをブロードキャストアドレスに送信
            udpClient.Send(payload, payload.Length, broadcastAddress, port);
        }

        /// <summary>
        /// メッセージの受信処理の開始
        /// </summary>
		private void BeginReceiveMessage()
        {
            /* 見本用サンプル start */
            // 非同期受信関数を使用
            udpClient.BeginReceive(new AsyncCallback(ReceiveMessageCallback), null);
            /* 見本用サンプル end */
        }

        /* 見本用サンプル start */
        public void ReceiveMessageCallback(IAsyncResult ar)
        {
            // 受信したバイト配列から、Json文字列を取得
            var remoteEndPoint = new IPEndPoint(IPAddress.Any, Port);
            var receivedBytes = udpClient.EndReceive(ar, ref remoteEndPoint);
            var receivedJson = Encoding.UTF8.GetString(receivedBytes);
            if (remoteEndPoint != null)
            {
                var ip = remoteEndPoint.Address;

                // デシリアライズして画面表示登録
                var payload = JsonSerializer.Deserialize<Payload>(receivedJson);
                if (payload != null && !IsMyIPAddress(ip))
                {
                    RegisterScreenMessage(
                        name: payload.Name,
                        content: payload.Content,
                        time: DateTime.Now);
                }
            }

            // 再度受信開始
            BeginReceiveMessage();
        }
        /* 見本用サンプル end */

        /// <summary>
        /// 自身のIPアドレスか?
        /// <param name="targetIp">対象IPアドレス</param>
        /// <return>true:自身のIPアドレスに含まれる / false: 含まれない
        /// </return>
        /// </summary>
        static bool IsMyIPAddress(IPAddress targetIp)
        {
            foreach (var ip in Dns.GetHostAddresses(Dns.GetHostName()))
            {
                if (targetIp.Equals(ip))
                {
                    return true;
                }
            }
            return false;
        }


        /// <summary>
        /// 新しいメッセージの登録
        /// <param name="name">ユーザ名</param>
        /// <param name="content">メッセージ本文</param>
        /// <param name="time">受信日時</param>
        /// </summary>
        private void RegisterScreenMessage(string name, string content, DateTime time)
		{
            // メインスレッドで実行し直す
            if (!Application.Current.Dispatcher.CheckAccess())
            {
                Application.Current.Dispatcher.Invoke(() => RegisterScreenMessage(name, content, time));
                return;
            }

            // メッセージの登録
            Messages.Add(new MessageModel
            {
                Username = name,
                UsernameColor = "#409aff",
                IconColor = "CornflowerBlue",
                Message = content,
                Time = time,
                IsNativeOrigin = false,
                FirstMessage = true,
            });
        }
	}
}
