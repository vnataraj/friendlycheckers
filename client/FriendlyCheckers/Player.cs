using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace FriendlyCheckers {
    public class Player {
        private PieceColor myColor;
        private String myName;
        public Player(String name, PieceColor color) {
            this.myName = name;
            this.myColor = color;
        }
        public Player(String name) : this(name, PieceColor.BLACK) { }
        public Player() : this("Player", PieceColor.BLACK) { }
        public void setName(String name) { this.myName = name; }
        public void setColor(PieceColor color) { this.myColor = color; }
        public String getName() { return myName; }
        public PieceColor getColor() { return myColor; }
        public Move getEasyMove(GameLogic logic)
        {
            return null; //stub
        }
        public Move getHardMove(GameLogic logic)
        {
            return null; //stub
        }
    }
}
