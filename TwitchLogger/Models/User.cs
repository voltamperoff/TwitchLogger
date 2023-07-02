using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchLogger.Models
{
    internal class User
    {
        public int Id { get; set; }
        public string Name { get; set; } = String.Empty;
        public ICollection<Channel> Channels { get; set; } = new List<Channel>();
    }
}
