using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ratatöskrs_Great_Adventure.Screens
{
    class AboutMenuScreen : MenuScreen
    {
        MenuEntry aboutMenuEntry;

        public AboutMenuScreen()
            : base("About")
        {
            aboutMenuEntry = new MenuEntry(string.Empty);

            SetMenuEntryText();
                        
            MenuEntries.Add(aboutMenuEntry);
        }

        void SetMenuEntryText()
        {
            aboutMenuEntry.Text = "(c)2012 by:\nMeelogic Consulting AG\nCode & Graphics by:\nSteffen Roth"; 
        }
    }
}
