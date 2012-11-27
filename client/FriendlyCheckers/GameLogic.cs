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

    public class Vector {
        private int x;
        private int y; 

        // Constructor: 
        public Vector(int x, int y) {
            this.x = x;
            this.y = y;
        }

        //copy Constructor: 
        public Vector(Vector copyable) { 
            this.x = copyable.x; 
            this.y = copyable.y; 
        }

        public Vector add(Vector move) {
            return new Vector(this.x + move.x, 
                this.y + move.y);
        }
    }

    enum Color {RED, BLACK};
    enum PieceType { REGULAR, KING }

    public class Piece {
        Color color;
        Vector coordinates;
        PieceType type;

        public Piece(Color color, Vector coordinates, PieceType type) {
            this.color = color;
            this.coordinates = coordinates;
            this.type = type;
        } 
    }

    public class CellAlreadyFilledException : System.Exception {}
    public class CellEmptyException : System.Exception {}

    public class Cell { 
        bool filled; 
        Piece piece; 
        
        public Cell() { 
            this.filled = false; 
            this.piece = null; 
        }

        public void addPiece(Piece piece) { 
            if(this.filled) { 
                throw new CellAlreadyFilledException(); 
            } else { 
                this.filled = true; 
            }
            this.piece = piece; 
        }

        public Piece removePiece() { 
            if( ! this.filled) { 
                throw new CellEmptyException(); 
            } else { 
                this.filled = false; 
            }
            Piece temp = this.piece; 
            this.piece = null; 
            return temp; 
        }

        public Piece getPiece() { 
            if( ! this.filled) { 
                throw new CellEmptyException(); 
            }
            return this.piece; 
        }
    }



    public class BoardGrid { 
        Piece [] pieces; 
        Cell [,] grid; 
        int height; 
        int width; 

        public BoardGrid(int height, int width) { 
            this.grid = new Cell[width,height]; 
            for(int y = 0; y < height; y++) { 
                for(int x = 0; x < width; x++) { 
                    this.grid[y, x] = new Cell(); 
                }
            }
            this.height = height; 
            this.width = width; 
        }
        
        Cell getCell(int x, int y) { 
            if(!(x < width) || !(y < height)) { 
                throw new 




        

    public class GameLogic {


    }
}
