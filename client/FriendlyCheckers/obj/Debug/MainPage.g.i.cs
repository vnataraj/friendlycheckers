﻿#pragma checksum "C:\Users\HP\Documents\GitHub\friendlycheckers\client\FriendlyCheckers\MainPage.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "498F70BD20342273F803375786EE040B"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.296
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using Microsoft.Phone.Controls;
using System;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Resources;
using System.Windows.Shapes;
using System.Windows.Threading;


namespace FriendlyCheckers {
    
    
    public partial class MainPage : Microsoft.Phone.Controls.PhoneApplicationPage {
        
        internal System.Windows.Controls.Grid LayoutRoot;
        
        internal System.Windows.Controls.StackPanel TitlePanel;
        
        internal System.Windows.Controls.TextBlock ApplicationTitle;
        
        internal System.Windows.Controls.TextBlock PageTitle;
        
        internal System.Windows.Controls.Grid HiddenPanel;
        
        internal System.Windows.Controls.TextBlock Versus;
        
        internal System.Windows.Controls.TextBlock Timer;
        
        internal System.Windows.Controls.TextBlock Moves;
        
        internal System.Windows.Controls.Grid ContentPanel;
        
        internal System.Windows.Controls.TextBlock WhoseTurn;
        
        internal System.Windows.Controls.Button quit;
        
        internal System.Windows.Controls.Button singleplayer;
        
        internal System.Windows.Controls.Button multiplayer_local;
        
        internal System.Windows.Controls.Button multiplayer_online;
        
        internal System.Windows.Controls.Button options;
        
        internal System.Windows.Controls.Button about;
        
        internal System.Windows.Shapes.Rectangle Shader;
        
        internal System.Windows.Controls.TextBlock Search;
        
        internal System.Windows.Controls.StackPanel OptionsPanel;
        
        internal System.Windows.Controls.CheckBox Op_Rotate;
        
        internal System.Windows.Controls.CheckBox Op_ForceJump;
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Windows.Application.LoadComponent(this, new System.Uri("/FriendlyCheckers;component/MainPage.xaml", System.UriKind.Relative));
            this.LayoutRoot = ((System.Windows.Controls.Grid)(this.FindName("LayoutRoot")));
            this.TitlePanel = ((System.Windows.Controls.StackPanel)(this.FindName("TitlePanel")));
            this.ApplicationTitle = ((System.Windows.Controls.TextBlock)(this.FindName("ApplicationTitle")));
            this.PageTitle = ((System.Windows.Controls.TextBlock)(this.FindName("PageTitle")));
            this.HiddenPanel = ((System.Windows.Controls.Grid)(this.FindName("HiddenPanel")));
            this.Versus = ((System.Windows.Controls.TextBlock)(this.FindName("Versus")));
            this.Timer = ((System.Windows.Controls.TextBlock)(this.FindName("Timer")));
            this.Moves = ((System.Windows.Controls.TextBlock)(this.FindName("Moves")));
            this.ContentPanel = ((System.Windows.Controls.Grid)(this.FindName("ContentPanel")));
            this.WhoseTurn = ((System.Windows.Controls.TextBlock)(this.FindName("WhoseTurn")));
            this.quit = ((System.Windows.Controls.Button)(this.FindName("quit")));
            this.singleplayer = ((System.Windows.Controls.Button)(this.FindName("singleplayer")));
            this.multiplayer_local = ((System.Windows.Controls.Button)(this.FindName("multiplayer_local")));
            this.multiplayer_online = ((System.Windows.Controls.Button)(this.FindName("multiplayer_online")));
            this.options = ((System.Windows.Controls.Button)(this.FindName("options")));
            this.about = ((System.Windows.Controls.Button)(this.FindName("about")));
            this.Shader = ((System.Windows.Shapes.Rectangle)(this.FindName("Shader")));
            this.Search = ((System.Windows.Controls.TextBlock)(this.FindName("Search")));
            this.OptionsPanel = ((System.Windows.Controls.StackPanel)(this.FindName("OptionsPanel")));
            this.Op_Rotate = ((System.Windows.Controls.CheckBox)(this.FindName("Op_Rotate")));
            this.Op_ForceJump = ((System.Windows.Controls.CheckBox)(this.FindName("Op_ForceJump")));
        }
    }
}

