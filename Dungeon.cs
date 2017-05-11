using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dungeon
{
    public class Dungeon
    {
        //Variables
        Random rnd;
        readonly int D_HEIGHT_, D_WIDTH_;
        List<Room> room_list_;
        int r_min_height_, r_min_width_;
        int r_max_height_, r_max_width_;
        char wall_, floor_, nothing_;
        int x_pos_, y_pos_;
        int counter_;
        int min_room_num_;
        char[][] dungeon_;
        bool is_executed_;

        enum dir_t { s_e, s_w, n_e, n_w };
        struct Room
        {
            public int start_x, start_y;
            public int end_x, end_y;
            public dir_t dir;

            public Room(int x, int y, int xx, int yy, dir_t d)
            {
                start_x = x;
                start_y = y;
                end_x = xx;
                end_y = yy;
                dir = d;
            }
        }

        public Dungeon(int height, int width)
        {
            rnd = new Random();
            room_list_ = new List<Room>();
            //Make the "canvas"
            D_HEIGHT_ = height;
            D_WIDTH_ = width;
            dungeon_ = new char[D_HEIGHT_][];
            for (int i = 0; i < D_HEIGHT_; i++)
            {
                dungeon_[i] = new char[D_WIDTH_];
            }

            //Set the default parameters
            setMin(4, 4);
            setMax(D_HEIGHT_ / 4, D_WIDTH_ / 7);
            setChars('#', '-', '.');
            setMinRoomNum(30);

            //Starting point of the first room
            y_pos_ = rnd.Next(D_HEIGHT_ + 1 + 1);
            x_pos_ = rnd.Next(D_WIDTH_ + 1 + 1);

            //This is needed for genRoom() (recursive calls)
            counter_ = 1;
            is_executed_ = false;
        }

        public void setMin(int height, int width)
        {
            if (height < 3 || height > D_HEIGHT_
                || width < 3 || width > D_WIDTH_)
                throw new Exception("Wrong setMin() parameters. It has to be more than 2 and less than or equal to D_HEIGHT_/D_WIDTH_");
            r_min_height_ = height;
            r_min_width_ = width;
        }

        public void setMax(int height, int width)
        {
            if (height < r_min_height_ || height > D_HEIGHT_
                || width < r_min_width_ || width > D_WIDTH_)
                throw new Exception("Wrong setMax() parameters. It should be more than r_min_height_/r_min_width_ and less than or equal to D_HEIGHT_/D_WIDTH_");
            r_max_height_ = height;
            r_max_width_ = width;
        }


        public void setChars(char wall, char floor, char nothing)
        {
            wall_ = wall;
            floor_ = floor;
            nothing_ = nothing;

            for (int y = 0; y < D_HEIGHT_; y++)
            {
                for (int x = 0; x < D_WIDTH_; x++)
                {
                    dungeon_[y][x] = nothing_;
                }
            }
        }

        public void setMinRoomNum(int num)
        {
            if (num < 0)
                throw new Exception("Wrong setMinRoomNum() parameter. It has to be more than or equal to 0");
            min_room_num_ = num;
        }

        public char[] this[int i]
        {
            get
            {
                return dungeon_[i];
            }
        }

        public void generate()
        {
            //Draw the "dungeon" on a "canvas"
            while (!genRoom()) ;
            genPassages();
        }

        bool genRoom()
        {
            //Room width and height
            int width = rnd.Next(r_min_width_, r_max_width_+1);
            int height = rnd.Next(r_min_height_, r_max_height_+1);

            //s_e - south east; s_w - south west; n_e - north east; n_w - north west;
            dir_t s_e = dir_t.s_e; dir_t n_e = dir_t.n_e;
            dir_t s_w = dir_t.s_w; dir_t n_w = dir_t.n_w;

            //Store possible directions in %dir_list vector
            List<dir_t> dir_list = new List<dir_t>();
            if (check(s_e, width, height))
            {
                dir_list.Add(s_e);
            }
            if (check(s_w, width, height))
            {
                dir_list.Add(s_w);
            }
            if (check(n_e, width, height))
            {
                dir_list.Add(n_e);
            }
            if (check(n_w, width, height))
            {
                dir_list.Add(n_w);
            }

            //Do a little trick if there is no possible directions and less than %min_room_num rooms
            //!!! It is not guaranteed that the number of rooms will be equal to %min_room_num
            if (dir_list.Count == 0 && room_list_.Count < min_room_num_)
            {
                if (room_list_.Count - counter_ > 0)
                {
                    x_pos_ = room_list_[room_list_.Count - counter_].end_x;
                    y_pos_ = room_list_[room_list_.Count - counter_].end_y;
                    counter_++;
                    while (!genRoom()) ;
                    while (!genRoom()) ;
                }
                else if (!is_executed_ && room_list_.Count - counter_ == 0)
                {
                    x_pos_ = room_list_[0].start_x;
                    y_pos_ = room_list_[0].start_y;
                    is_executed_ = true; //This condition should be executed only ONCE
                    genRoom();
                }
            }

            //Break if no possible directions
            if (dir_list.Count == 0) return true;

            //Make room in randomly selected direction
            dir_t rnd_dir = dir_list[rnd.Next(dir_list.Count)];
            switch (rnd_dir)
            {
                case dir_t.s_e:
                        for (int y = y_pos_; y < y_pos_ + height; y++)
                        {
                            for (int x = x_pos_; x < x_pos_ + width; x++)
                            {
                                if (y == y_pos_ || y == y_pos_ + (height - 1)
                                        || x == x_pos_ || x == x_pos_ + (width - 1))
                                {
                                    dungeon_[y][x] = wall_;
                                }
                                else
                                {
                                    dungeon_[y][x] = floor_;
                                }
                            }
                        }
                        //Keep track of all rooms
                        Room r1 = new Room( x_pos_, y_pos_, x_pos_ + (width - 1), y_pos_ + (height - 1), s_e);
                        room_list_.Add(r1);
                        //Set y&&x position to the opposite corner
                        y_pos_ += (height - 1);
                        x_pos_ += (width - 1);
                    break;
                case dir_t.s_w:
                        for (int y = y_pos_; y < y_pos_ + height; y++)
                        {
                            for (int x = x_pos_; x > x_pos_ - width; x--)
                            {
                                if (y == y_pos_ || y == y_pos_ + (height - 1)
                                        || x == x_pos_ || x == x_pos_ - (width - 1))
                                {
                                    dungeon_[y][x] = wall_;
                                }
                                else
                                {
                                    dungeon_[y][x] = floor_;
                                }
                            }
                        }
                        Room r2 = new Room(x_pos_, y_pos_, x_pos_ - (width - 1), y_pos_ + (height - 1), s_w);
                        room_list_.Add(r2);
                        y_pos_ += (height - 1);
                        x_pos_ -= (width - 1);
                    break;
                case dir_t.n_e:
                        for (int y = y_pos_; y > y_pos_ - height; y--)
                        {
                            for (int x = x_pos_; x < x_pos_ + width; x++)
                            {
                                if (y == y_pos_ || y == y_pos_ - (height - 1)
                                        || x == x_pos_ || x == x_pos_ + (width - 1))
                                {
                                    dungeon_[y][x] = wall_;
                                }
                                else
                                {
                                    dungeon_[y][x] = floor_;
                                }
                            }
                        }
                        Room r3 = new Room(x_pos_, y_pos_, x_pos_ + (width - 1), y_pos_ - (height - 1), n_e);
                        room_list_.Add(r3);
                        y_pos_ -= (height - 1);
                        x_pos_ += (width - 1);
                    break;
                case dir_t.n_w:
                    for (int y = y_pos_; y > y_pos_ - height; y--)
                    {
                        for (int x = x_pos_; x > x_pos_ - width; x--)
                        {
                            if (y == y_pos_ || y == y_pos_ - (height - 1)
                                    || x == x_pos_ || x == x_pos_ - (width - 1))
                            {
                                dungeon_[y][x] = wall_;
                            }
                            else
                            {
                                dungeon_[y][x] = floor_;
                            }
                        }
                    }
                    Room r4 = new Room(x_pos_, y_pos_, x_pos_ - (width - 1), y_pos_ - (height - 1), n_w);
                    room_list_.Add(r4);
                    y_pos_ -= (height - 1);
                    x_pos_ -= (width - 1);
                    break;
            }

            //Signal that there is still possible directions left
            return false;
        }


        bool check(dir_t dir, int width, int height)
        {
            //Check if it's possible to make room in the direction(%dir) that was passed
            switch(dir) {
            case dir_t.s_e:
                if (y_pos_ + height <= D_HEIGHT_ && x_pos_ + width <= D_WIDTH_) {
                    for (int y = y_pos_; y<y_pos_ + height; y++) {
                        for (int x = x_pos_; x<x_pos_ + width; x++) {
                            if (y == y_pos_ || y == y_pos_ + (height-1)
                                    || x == x_pos_ || x == x_pos_ + (width-1)) continue; //Ignore wall_ collision
                            if (dungeon_[y][x] != nothing_) return false;
                        }
        }
                } else return false;
                return true;
            case dir_t.s_w:
                if (y_pos_ + height <= D_HEIGHT_ && x_pos_ - width >= 0) {
                    for (int y = y_pos_; y<y_pos_ + height; y++) {
                        for (int x = x_pos_; x > x_pos_ - width; x--) {
                            if (y == y_pos_ || y == y_pos_ + (height-1)
                                    || x == x_pos_ || x == x_pos_ - (width-1)) continue;
                            if (dungeon_[y][x] != nothing_) return false;
                        }
                    }
                } else return false;
                return true;
            case dir_t.n_e:
                if (y_pos_ - height >= 0 && x_pos_ + width <= D_WIDTH_) {
                    for (int y = y_pos_; y > y_pos_ - height; y--) {
                        for (int x = x_pos_; x<x_pos_ + width; x++) {
                            if (y == y_pos_ || y == y_pos_ - (height-1)
                                    || x == x_pos_ || x == x_pos_ + (width-1)) continue;
                            if (dungeon_[y][x] != nothing_) return false;
                        }
                    }
                } else return false;
                return true;
            case dir_t.n_w:
                if (y_pos_ - height >= 0 && x_pos_ - width >= 0) {
                    for (int y = y_pos_; y > y_pos_ - height; y--) {
                        for (int x = x_pos_; x > x_pos_ - width; x--) {
                            if (y == y_pos_ || y == y_pos_ - (height-1)
                                    || x == x_pos_ || x == x_pos_ - (width-1)) continue;
                            if (dungeon_[y][x] != nothing_) return false;
                        }
                    }
                } else return false;
                return true;
            }

            //Something went wrong if program reached this
            throw new Exception("Something wrong in check() function");
        }

        void genPassages()
        {
            //Make passage between rooms
            for (int i = 1; i < room_list_.Count; ++i)
            {
                for (int n = 1; n <= i; ++n)
                {
                    if (room_list_[i - n].end_y == room_list_[i].start_y
                            && room_list_[i - n].end_x == room_list_[i].start_x)
                    {
                        switch (room_list_[i - n].dir)
                        {
                            case dir_t.s_e:
                                if (room_list_[i].dir == dir_t.s_e)
                                {  //Because nested switches look ugly
                                    genVestibule(dir_t.s_e, i);
                                }
                                else if (room_list_[i].dir == dir_t.s_w)
                                {
                                    dungeon_[room_list_[i].start_y][room_list_[i].start_x - 1] = floor_;
                                }
                                else if (room_list_[i].dir == dir_t.n_e)
                                {
                                    dungeon_[room_list_[i].start_y - 1][room_list_[i].start_x] = floor_;
                                }
                                break;
                            case dir_t.s_w:
                                if (room_list_[i].dir == dir_t.s_e)
                                {
                                    dungeon_[room_list_[i].start_y][room_list_[i].start_x + 1] = floor_;
                                }
                                else if (room_list_[i].dir == dir_t.s_w)
                                {
                                    genVestibule(dir_t.s_w, i);
                                }
                                else if (room_list_[i].dir == dir_t.n_w)
                                {
                                    dungeon_[room_list_[i].start_y - 1][room_list_[i].start_x] = floor_;
                                }
                                break;
                            case dir_t.n_e:
                                if (room_list_[i].dir == dir_t.s_e)
                                {
                                    dungeon_[room_list_[i].start_y + 1][room_list_[i].start_x] = floor_;
                                }
                                else if (room_list_[i].dir == dir_t.n_e)
                                {
                                    genVestibule(dir_t.n_e, i);
                                }
                                else if (room_list_[i].dir == dir_t.n_w)
                                {
                                    dungeon_[room_list_[i].start_y][room_list_[i].start_x - 1] = floor_;
                                }
                                break;
                            case dir_t.n_w:
                                if (room_list_[i].dir == dir_t.s_w)
                                {
                                    dungeon_[room_list_[i].start_y + 1][room_list_[i].start_x] = floor_;
                                }
                                else if (room_list_[i].dir == dir_t.n_e)
                                {
                                    dungeon_[room_list_[i].start_y][room_list_[i].start_x + 1] = floor_;
                                }
                                else if (room_list_[i].dir == dir_t.n_w)
                                {
                                    genVestibule(dir_t.n_w, i);
                                }
                                break;
                        }
                    }
                }
            }
        }


        void genVestibule(dir_t dir, int i)
        {
            //This belongs to genPassages()
            //Have put this in separate method for the sake of clarity
            switch (dir)
            {
                case dir_t.s_w:
                case dir_t.n_e:
                    //Draw the wall_s if this vestibule is not collapsing with other rooms
                    if (dungeon_[room_list_[i].start_y + 1][room_list_[i].start_x + 1] == nothing_)
                    {
                        dungeon_[room_list_[i].start_y + 2][room_list_[i].start_x + 1] = wall_;
                        dungeon_[room_list_[i].start_y + 2][room_list_[i].start_x + 2] = wall_;
                        dungeon_[room_list_[i].start_y + 1][room_list_[i].start_x + 2] = wall_;
                    }
                    if (dungeon_[room_list_[i].start_y - 1][room_list_[i].start_x - 1] == nothing_)
                    {
                        dungeon_[room_list_[i].start_y - 2][room_list_[i].start_x - 2] = wall_;
                        dungeon_[room_list_[i].start_y - 2][room_list_[i].start_x - 1] = wall_;
                        dungeon_[room_list_[i].start_y - 1][room_list_[i].start_x - 2] = wall_;
                    }

                    dungeon_[room_list_[i].start_y - 1][room_list_[i].start_x] = floor_;
                    dungeon_[room_list_[i].start_y - 1][room_list_[i].start_x + 1] = floor_;
                    dungeon_[room_list_[i].start_y - 1][room_list_[i].start_x - 1] = floor_;
                    dungeon_[room_list_[i].start_y + 1][room_list_[i].start_x - 1] = floor_;
                    dungeon_[room_list_[i].start_y + 1][room_list_[i].start_x] = floor_;
                    dungeon_[room_list_[i].start_y + 1][room_list_[i].start_x + 1] = floor_;
                    dungeon_[room_list_[i].start_y][room_list_[i].start_x - 1] = floor_;
                    dungeon_[room_list_[i].start_y][room_list_[i].start_x + 1] = floor_;
                    dungeon_[room_list_[i].start_y][room_list_[i].start_x] = floor_;
                    break;
                case dir_t.s_e:
                case dir_t.n_w:
                    if (dungeon_[room_list_[i].start_y + 1][room_list_[i].start_x - 1] == nothing_)
                    {
                        dungeon_[room_list_[i].start_y + 2][room_list_[i].start_x - 1] = wall_;
                        dungeon_[room_list_[i].start_y + 2][room_list_[i].start_x - 2] = wall_;
                        dungeon_[room_list_[i].start_y + 1][room_list_[i].start_x - 2] = wall_;
                        dungeon_[room_list_[i].start_y + 1][room_list_[i].start_x - 1] = floor_;
                        dungeon_[room_list_[i].start_y + 1][room_list_[i].start_x] = floor_;
                        dungeon_[room_list_[i].start_y + 1][room_list_[i].start_x + 1] = floor_;
                        dungeon_[room_list_[i].start_y][room_list_[i].start_x - 1] = floor_;
                        dungeon_[room_list_[i].start_y][room_list_[i].start_x + 1] = floor_;
                        dungeon_[room_list_[i].start_y][room_list_[i].start_x] = floor_;
                    }
                    if (dungeon_[room_list_[i].start_y - 1][room_list_[i].start_x + 1] == nothing_)
                    {
                        dungeon_[room_list_[i].start_y - 2][room_list_[i].start_x + 2] = wall_;
                        dungeon_[room_list_[i].start_y - 2][room_list_[i].start_x + 1] = wall_;
                        dungeon_[room_list_[i].start_y - 1][room_list_[i].start_x + 2] = wall_;
                        dungeon_[room_list_[i].start_y - 1][room_list_[i].start_x] = floor_;
                        dungeon_[room_list_[i].start_y - 1][room_list_[i].start_x + 1] = floor_;
                        dungeon_[room_list_[i].start_y - 1][room_list_[i].start_x - 1] = floor_;
                        dungeon_[room_list_[i].start_y][room_list_[i].start_x - 1] = floor_;
                        dungeon_[room_list_[i].start_y][room_list_[i].start_x + 1] = floor_;
                        dungeon_[room_list_[i].start_y][room_list_[i].start_x] = floor_;
                    }
                    break;
            }
        }

    }
}
