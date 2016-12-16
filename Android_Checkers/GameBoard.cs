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
using Android_Checkers.Assets;
using Android.Graphics;

namespace Android_Checkers {
    class GameBoard {
        public List<Checker> entity;
        int gameState;
        int subState;
        int[] teams = new int[3];
        CheckerTeam myTeam;
        bool isOnline;
        int sx = -1, sy = -1;
        Checker selected;
        Sockets socket;
        Texture checkerTexture;

        public GameBoard(bool _isOnline) {            
            gameState = 0;
            subState = 0;
            isOnline = _isOnline;
            entity = new List<Checker>();
        }

        public void Clear() {
            entity.Clear();
        }

        public void SetMyTeam(CheckerTeam team) {
            myTeam = team;
        }

        public void EndGame() {
            socket.sendMessage(MessageType.EndGame);
            Clear();
            //cout << "Game end!" << endl;
            if (!isOnline) StartGame();
        }

        public void StartGame() {
            teams[(int)CheckerTeam.White] = 0;
            teams[(int)CheckerTeam.Black] = 0;
            for (int i = 0; i < 3; i++) {
                for (int j = 0; j < 4; j++) {
                    entity.Add(new Checker(j * 2 + (1 - i % 2), i, CheckerTeam.White, CheckerType.Default));
                    entity.Add(new Checker(j * 2 + (i % 2), 7 - i, CheckerTeam.Black, CheckerType.Default));
                    teams[(int)CheckerTeam.White]++;
                    teams[(int)CheckerTeam.Black]++;
                }
            }
            gameState = (int)CheckerTeam.White;
            subState = 1;
            //cout << "Game started!" << endl;
        }
        
        public void ChangeState(int state) {
            gameState = state;
        }

        public void SetSockets(Sockets sock) {
            socket = sock;
        }

        public void RemoveByXY(int x, int y) {
            foreach(var s in entity) {
                if (s.checkIntersection(x, y)) {
                    CheckerTeam t = s.getTeam();
                    teams[(int)t]--;
                    entity.Remove(s);
                    if (teams[(int)t] == 0) {
                        EndGame();
                    }
                    break;
                }
            }
        }

        public void MoveByXY(int x, int y, int new_x, int new_y) {
            //cout << x << " " << y << " " << new_x << " " << new_y << endl;
            foreach (var s in entity) {
                if (s.checkIntersection(x, y)) {
                    s.Move(new_x, new_y);
                    break;
                }
            }
        }

        public void ProceedLeftClick(float _x, float _y) {
            _x = Game1.DeabsoluteX(_x);
            _y = Game1.DeabsoluteY(_y);
            if ((isOnline == false || gameState == (int)myTeam) && _x >= 30 && _x < 478 && _y >= 30 && _y < 478) {
                int gX = (int)Math.Floor(_x - 30) / 56;
                int gY = (int)Math.Floor(_y - 30) / 56;
                if(gX == sx && gY == sy) {
                    selected.unsetMark();
                    subState = 1;
                    sx = -1;
                    sy = -1;
                    return;
                }
                System.Diagnostics.Debug.WriteLine("Pos: "+gX+" "+gY);
                if (subState == 1) {
                    //cout << "sub: 1" << endl;
                    selected = null;
                    foreach (var s in entity)
                        if (s.checkIntersection(gX, gY) && (int)s.getTeam() == gameState) {
                            selected = s;
                            selected.setMark();
                            break;
                        }
                    if (selected != null) {
                        subState = 2;
                        sx = gX;
                        sy = gY;
                    }
                } else if (subState == 2) {
                    //cout << "sub: 2" << endl;
                    int x = 0, y = 0;
                    CheckerType type = selected.getType();
                    CheckerTeam team = selected.getTeam();
                    selected.getPosition(ref x, ref y);
                    int x_diff = (gX - x);
                    int y_diff = (gY - y);
                    //cout << x_diff << " " << y_diff << endl;
                    if (Math.Abs(x_diff) != Math.Abs(y_diff) || x_diff == 0) return;
                    //cout << x << " " << y << endl;
                    int cx = x + Math.Sign(x_diff), cy = y + Math.Sign(y_diff);
                    if (type == CheckerType.Default) {
                        if (x_diff > 2) return;
                        Checker toRemove = null;
                        //cout << cx << " " << cy << endl;
                        bool invalid = false, remove = false; ;
                        foreach (var s in entity) {
                            if (s.checkIntersection(cx, cy)) {
                                if (s.getTeam() != selected.getTeam()) {
                                    toRemove = s;
                                    remove = true;
                                } else {
                                    invalid = true;
                                }
                                break;
                            }
                        }
                        if (invalid) return;
                        if ((Math.Abs(x_diff) == 1 && remove == false && ((team == CheckerTeam.White && gY > y) || (team == CheckerTeam.Black && gY < y))) || (Math.Abs(x_diff) == 2 && remove == true)) {
                            char[] s = new char[3];
                            selected.Move(gX, gY);
                            selected.unsetMark();
                            if (remove == true) {
                                teams[3 - gameState]--;
                                int ttx=0, tty=0;
                                toRemove.getPosition(ref ttx, ref tty);
                                socket.sendMessage(MessageType.Remove, ttx, tty);
                                entity.Remove(toRemove);
                            }
                            socket.sendMessage(MessageType.Move, x, y, gX, gY);
                            gameState = 3 - gameState;
                            socket.sendMessage(MessageType.ChangeState, gameState);
                            subState = 1;
                            sx = -1;
                            sy = -1;
                            if (teams[gameState] == 0) {
                                EndGame();
                            }
                        }
                    } else if (type == CheckerType.King) {
                        Checker toRemove = null;
                        int tx=0, ty=0;
                        int dist_c = Math.Abs(x_diff) + Math.Abs(y_diff);
                        //cout << cx << " " << cy << endl;
                        bool invalid = false, remove = false; ;
                        foreach (var sp in entity) {
                            sp.getPosition(ref tx, ref ty);
                            int dist_t = Math.Abs(tx - x) + Math.Abs(ty - y);
                            if (Math.Abs(tx - x) == Math.Abs(ty - y) && tx != x && cx == x + Math.Sign(tx - x) && cy == y + Math.Sign(ty - y) && dist_t < dist_c) {
                                if (sp.getTeam() != selected.getTeam()) {
                                    if (remove) {
                                        remove = false;
                                        invalid = true;
                                        //cout << "more";
                                        break;
                                    } else {
                                        toRemove = sp;
                                        remove = true;
                                        //cout << tx << " " << ty << endl;
                                    }
                                } else {
                                    invalid = true;
                                }

                            }
                        }
                        //cout << "inv";
                        if (invalid) return;
                        selected.unsetMark();
                        char[] s = new char[10];
                        if (remove == true) {
                            int ttx=0, tty=0;
                            toRemove.getPosition(ref ttx, ref tty);
                            gX = ttx + Math.Sign(ttx - x);
                            gY = tty + Math.Sign(tty - y);
                            selected.Move(gX, gY);
                            socket.sendMessage(MessageType.Remove, tx, ty);
                            socket.sendMessage(MessageType.Move, x, y, gX, gY);
                            teams[3 - gameState]--;
                            entity.Remove(toRemove);
                        } else {
                            selected.Move(gX, gY);
                            socket.sendMessage(MessageType.Move, x, y, gX, gY);
                        }
                        gameState = 3 - gameState;
                        socket.sendMessage(MessageType.ChangeState, gameState);
                        subState = 1;
                        sx = -1;
                        sy = -1;
                        if (teams[gameState] == 0) {
                            EndGame();
                        }
                    }

                }
            }
        }
    }
}