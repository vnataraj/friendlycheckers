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
        public MoveAttempt getEasyMove(GameLogic logic)
        {
            return logic.getAnyDoableMoveAttempt(); //stub
        }
        public MoveAttempt getHardMove(GameLogic logic)
        {
            return logic.getAnyDoableMoveAttempt(); //stub
        }
    }
    public class MoveAttempt { 
        int yStart; 
        int xStart;
        int yEnd;
        int xEnd; 
        public MoveAttempt(int yStart, int xStart, int yEnd, int xEnd) { 
            this.yStart = yStart; 
            this.xStart = xStart; 
            this.yEnd = yEnd; 
            this.xEnd = xEnd; 
        }
        public int getYStart() { 
            return yStart; 
        }
        public int getXStart() {
            return xStart; 
        }
        public int getYEnd() { 
            return yEnd; 
        }
        public int getXEnd() { 
            return xEnd; 
        }
    }
}
