using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Microsoft.Xna.Framework.Graphics;
using Android.Graphics;

namespace Android_Checkers.Assets {
    enum CheckerTeam {
        None = 0,
        White = 1,
        Black = 2
    };
    enum CheckerType {
        Default = 0,
        King = 1
    };
    class Checker {
        public int x;
        public int y;
        CheckerTeam team;
        CheckerType type;
        public bool marked = false;
	    public bool created;
        public Checker() {
	        created = false;
        }

        public Checker(int _x, int _y, CheckerTeam _team, CheckerType _type) {
	        //shape.setTexture(texture);
	        //shape.setScale(0.52, 0.52);
	        //shape.setPosition(30, 30);
	        x = _x;
	        y = _y;
	        team = _team;
	        type = _type;
	        //shape.setPosition(30 + 56 * x, 30 + 56 * y);

            unsetMark();
            created = true;
        }

        public CheckerTeam getTeam() {
            return team;
        }

        public CheckerType getType() {
            return type;
        }

        public void setMark() {
            marked = true;
            //shape.setColor(team == CheckerTeam.White ? Color.Rgb(210, 210, 210) : Color.Rgb(70, 70, 70));
        }

        public void unsetMark() {
            marked = false;
            //shape.setColor(team == CheckerTeam.White ? Color.White : Color.Rgb(40, 40, 40));
        }

        public void Move(int _x, int _y) {
            x = _x;
            y = _y;
            if ((y == 7 && team == CheckerTeam.White) || (y == 0 && team == CheckerTeam.Black)) {
                type = CheckerType.King;
            }
            //shape.setPosition(30 + 56 * x, 30 + 56 * y);
        }

        public bool checkIntersection(int _x, int _y) {
            if (x == _x && y == _y) {
                return true;
            }
            return false;
        }

        public void getPosition(ref int _x, ref int _y) {
            _x = x;
            _y = y;
        }

       /* public static bool operator ==(Checker a, Checker b) {
            return b.checkIntersection(a.x, a.y);
        }

        public static bool operator !=(Checker a, Checker b) {
            return !(a == b);
        }*/
    }
}