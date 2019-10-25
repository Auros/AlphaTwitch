using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace AlphaTwitch
{
    public class Types
    {

    }

    public struct Viewer
    {
        public string Name { get; set; }
        public string ID { get; set; }
        public string Color { get; set; }
        public Role Role { get; set; }
        public Type Type { get; set; }
        public Texture2D Profile { get; set; }
    }

    public enum Role
    {
        Other,
        Broadcaster,
        Mod,
        Vip,
        Sub,
        None
    }

    public enum Type
    {
        None,
        Affiliate,
        Partner,
        Staff,
        Admin
    }
}
