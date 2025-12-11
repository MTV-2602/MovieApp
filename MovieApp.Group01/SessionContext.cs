using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieApp.Group01
{
    public static class SessionContext
    {
        public static int CurrentUserId { get; set; }
        public static string CurrentUsername { get; set; } = string.Empty;
    }
}
