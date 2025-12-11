using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MovieApp.Group01
{
    public partial class TrailerWindow : Window
    {
        public TrailerWindow(string path)
        {
            InitializeComponent();

            this.Loaded += async (s, e) =>
            {
                await WebPlayer.EnsureCoreWebView2Async();

                // Map thư mục chứa video
                string folder = System.IO.Path.GetDirectoryName(path);
                string file = System.IO.Path.GetFileName(path);

                WebPlayer.CoreWebView2.SetVirtualHostNameToFolderMapping(
                    "local.videos",
                    folder,
                    Microsoft.Web.WebView2.Core.CoreWebView2HostResourceAccessKind.Allow
                );

                string html = $@"
<!DOCTYPE html>
<html>
<body style='margin:0; background:black;'>
    <video width='100%' height='100%' controls autoplay>
        <source src='https://local.videos/{file}' type='video/mp4'>
    </video>
</body>
</html>";

                WebPlayer.NavigateToString(html);
            };
        }
    }

}


