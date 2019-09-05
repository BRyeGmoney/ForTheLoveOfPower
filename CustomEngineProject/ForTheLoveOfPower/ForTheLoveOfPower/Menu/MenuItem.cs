using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace PenisPotato.Menu
{
    public class MenuItem
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public Texture2D MenuOption { get; set; }
        public Action<Buttons> Action { get; set; }
    }
}
