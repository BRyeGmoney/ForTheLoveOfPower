using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace PenisPotato.Menu
{
    class Menu
    {
        //private bool initialized = false;
        public bool isShowing = false;
        public Texture2D menuOption;

        private List<MenuItem> menuItems { get; set; }
        public int Count
        {
            get { return menuItems.Count; }
        }
        
        public string Title { get; set; }
        public string InfoText { get; set; }
        private int lastNavigated { get; set; }

        private int _selectedIndex;
        public int selectedIndex
        {
            get
            {
                return _selectedIndex;
            }
            protected set
            {
                if (value >= menuItems.Count || value < 0)
                {
                    throw new ArgumentOutOfRangeException();
                }
                _selectedIndex = value;
            }
        }
        public MenuItem SelectedItem
        {
            get
            {
                return menuItems[selectedIndex];
            }
        }

        public Menu(Texture2D texture)
        {
            menuItems = new List<MenuItem>();
            menuOption = texture;
        }

        public virtual void AddMenuItem(string title, Action<Buttons> action)
        {
            AddMenuItem(title, action, "");
        }

        public virtual void AddMenuItem(string title, Action<Buttons> action, string description)
        {
            menuItems.Add(new MenuItem { Title = title, Description = description, Action = action, MenuOption = menuOption });
            selectedIndex = 0;
        }

        public void CreateMainMenu()
        {
            //AddMenuItem("Civil", new Action<Buttons> (), "");
        }

        public void DrawMenu(SpriteBatch spriteBatch, Vector2 centerPos, Color itemColor)
        {
            if (isShowing)
            {
                spriteBatch.Draw(menuOption, new Rectangle((int)(centerPos.X - 32), (int)(centerPos.Y - (32/ 2)), 32, 32), itemColor);
                spriteBatch.Draw(menuOption, new Rectangle((int)(centerPos.X), (int)(centerPos.Y - 32), 32, 32), itemColor);
                spriteBatch.Draw(menuOption, new Rectangle((int)(centerPos.X), (int)(centerPos.Y), 32, 32), itemColor);
            }
        }
    }
}
