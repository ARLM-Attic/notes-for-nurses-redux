
#if SILVERLIGHT
using System.IO.IsolatedStorage;
#endif

namespace Edward.Wilde.Note.For.Nurses.Core.Xamarin.Service {
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Linq;
    using System.Diagnostics;

    public abstract class XmlFeedParserBase<T> {
#if !SILVERLIGHT
		readonly string documentsPath = Environment.GetFolderPath (Environment.SpecialFolder.Personal);	
#endif
        string localPath;
		string xmlUrl;

		List<T> items = new List<T>();

		public XmlFeedParserBase (string url, string filename)
		{
			Debug.WriteLine ("Url: " + url);
			this.xmlUrl = url;

#if SILVERLIGHT
            localPath = filename;
#else
			
#if __ANDROID__
            string libraryPath = documentsPath;
#else
            // we need to put in /tmp/ on iOS5.1 to meet Apple's iCloud terms (don't want this backed-up)
			string libraryPath = Path.Combine (this.documentsPath, "../tmp/");
#endif
            this.localPath = Path.Combine (libraryPath, filename); // iOS or Android
#endif
            Debug.WriteLine("XmlFeedParserBase path:" + this.localPath);
			
			if (this.HasLocalData) {
                var data = this.OpenLocal ();
                this.items = this.ParseXml (data);
			}
		}

		public List<T> AllItems {
			get { return this.items; }
		}

        string OpenLocal ()
        {
#if SILVERLIGHT
            var iso = IsolatedStorageFile.GetUserStoreForApplication ();
            using (var f = new StreamReader (iso.OpenFile (localPath, FileMode.Open))) {
                return f.ReadToEnd ();
            }
#else
            using (var f = File.OpenText (this.localPath)) {
                return f.ReadToEnd ();
            }
#endif
        }

		void SaveLocal (string data)
		{
#if SILVERLIGHT
            var iso = IsolatedStorageFile.GetUserStoreForApplication ();
            using (var f = new StreamWriter (iso.CreateFile (localPath))) {
                f.Write (data);
            }
#else
            using (var f = new StreamWriter (this.localPath)) {
                f.Write (data);
            }
#endif
		}

		public DateTime GetLastRefreshTimeUtc ()
		{
			if (this.HasLocalData) {
#if SILVERLIGHT
                return IsolatedStorageFile.GetUserStoreForApplication ().GetLastWriteTime (localPath).UtcDateTime;
#else
                return new FileInfo (this.localPath).LastWriteTimeUtc;
#endif
            } else {
				return new DateTime (1990, 1, 1);
			}
		}

		public bool HasLocalData {
			get { 
#if SILVERLIGHT
                return IsolatedStorageFile.GetUserStoreForApplication ().FileExists (localPath);
#else
                return File.Exists (this.localPath); 
#endif
            }
		}

		public void Refresh (Action action)
		{			
			var webClient = new WebClient ();
			Debug.WriteLine ("Get remote xml data");
			webClient.DownloadStringCompleted += (sender, e) =>
			{
				try {
					this.SaveLocal (e.Result);
					this.items = this.ParseXml (e.Result);
				} catch (Exception ex) {
					Debug.WriteLine ("ERROR saving downloaded XML: " + ex);
				}
				action();
			};
			webClient.Encoding = System.Text.Encoding.UTF8;
			webClient.DownloadStringAsync (new Uri (this.xmlUrl));
		}

		protected abstract List<T> ParseXml (string xml);
		
		protected static DateTime ParseDateTime (string date)
		{
			var ps = date.Split (' ');
			var justDt = string.Join (" ", ps.Skip (1).Take (4).ToArray ());
			return DateTime.Parse (justDt);
		}
	}
}
