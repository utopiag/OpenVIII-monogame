﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace FF8
{
    internal static partial class Module_main_menu_debug
    {
        #region Fields

        private static sbyte _slotLoc;
        private static sbyte _blockLoc;

        private static sbyte blockpage;


        private static float s_blink;

        /// <summary>
        /// Rectangle is hotspot for mouse, Point is where the finger points
        /// </summary>
        private static Tuple<Rectangle, Point, Rectangle>[] SlotLocs = new Tuple<Rectangle, Point, Rectangle>[2];

        private static Tuple<Rectangle, Point, Rectangle>[] BlockLocs = new Tuple<Rectangle, Point, Rectangle>[3];
        private static Texture2D LastPage;
        private static Vector2 PageTarget;
        private static Vector2 PageSize;
        private static Vector2 CurrentPageLoc;
        private static Vector2 CurrentLastPageLoc;
        private static Vector2 LastPageTarget;
        private static Vector2 lastscale;
        private static Vector2 scale;
        private static Vector2 TextScale;
        private static Point ml;
        private static Tuple<FF8String, FF8String, FF8String> LGSGHEADER;

        #endregion Fields

        #region Enums

        private enum Litems
        {
            GameFolder,
            Load,
            LoadFF8,
            Loading,
            Slot1,
            Slot2,
            FF8,
            Save,
            SaveFF8,
            GameFolderSlot1,
            GameFolderSlot2,
            CheckGameFolder,
            BlockToLoad,
            BlockToSave,
            Saving
        }

        #endregion Enums

        #region Properties

        public static float blink
        {
            get => s_blink; private set
            {
                if (value > 1f)
                    blinkstate = false;
                if (value < 0f)
                    blinkstate = true;
                s_blink = value;
            }
        }

        private static float PercentLoaded { get; set; } = .5f;

        private static sbyte SlotLoc
        {
            get => _slotLoc; set
            {
                if (value >= SlotLocs.Length)
                    value = 0;
                else if (value < 0)
                    value = (sbyte)(SlotLocs.Length - 1);
                _slotLoc = value;
            }
        }

        private static sbyte BlockLoc
        {
            get => _blockLoc; set
            {
                if (value >= BlockLocs.Length)
                    value = 0;
                else if (value < 0)
                    value = (sbyte)(BlockLocs.Length - 1);
                _blockLoc = value;
            }
        }

        public static sbyte Blockpage
        {
            get => blockpage; set
            {
                if (value >= 10)
                    value = 0;
                else if (value < 0)
                    value = 9;
                blockpage = value;
            }
        }

        #endregion Properties

        #region Methods

        

        private static Tuple<Rectangle, Point, Rectangle> DrawBox(Rectangle dst, FF8String buffer = null, Icons.ID? title = null, bool indent = true, bool bottom = false,Vector2? textScale=null,Vector2? boxScale = null)
        {
            if (textScale == null) textScale = Vector2.One;
            if (boxScale == null) boxScale = Vector2.One;
            Point cursor = new Point(0);
            dst.Size = (dst.Size.ToVector2()).ToPoint();
            dst.Location = (dst.Location.ToVector2()).ToPoint();
            Vector2 bgscale = new Vector2(2f)*textScale.Value;
            var box = dst.Scale(boxScale.Value);
            var backup = dst;
            if (dst.Width > 256 * bgscale.X)
                Memory.Icons.Draw(Icons.ID.Menu_BG_368, 0, box, bgscale, Fade);
            else
                Memory.Icons.Draw(Icons.ID.Menu_BG_256, 0, box, bgscale, Fade);
            Rectangle hotspot = new Rectangle(dst.Location, dst.Size);
            if (title != null)
            {
                //dst.Size = (Memory.Icons[title.Value].GetRectangle.Size.ToVector2()  * 2.823317308f).ToPoint();
                dst.Offset(15, 0);
                dst.Y = (int)(dst.Y * boxScale.Value.Y);
                Memory.Icons.Draw(title.Value, 2, dst, (bgscale + new Vector2(.5f)), fade);
            }
            Rectangle font = new Rectangle();
            dst = backup;
            if (buffer != null && buffer.Length > 0)
            {
                if (indent)
                    dst.Offset(70*textScale.Value.X, 0);//0.0546875f * vp_per.X
                else
                    dst.Offset(25 * textScale.Value.X, 0);//0.01953125f * vp_per.X

                if (bottom)
                    dst.Offset(0,(dst.Height - 48));
                else
                    dst.Offset(0, 21);

                dst.Y = (int)(dst.Y * boxScale.Value.Y);
                font = Memory.font.RenderBasicText(buffer, dst.Location.ToVector2(), new Vector2(2.545454545f, 3.0375f)*textScale.Value, Fade: fade);
                cursor = dst.Location;
                cursor.Y += (int)(18.225f); // 12 * (3.0375/2)
            }
            return new Tuple<Rectangle, Point, Rectangle>(hotspot, cursor, font);
        }




        private static void DrawLGSG()
        {
            if (State >= MainMenuStates.LoadGameChooseSlot && State <= MainMenuStates.SaveGameSaving)
            {
                DrawLGSGChooseBlocks();
                Memory.SpriteBatchStartAlpha(tm: IGM_focus);

                switch (State)
                {
                    case MainMenuStates.LoadGameChooseSlot:
                        DrawLGChooseSlot();
                        break;

                    case MainMenuStates.LoadGameCheckingSlot:
                        DrawLGCheckSlot();
                        break;

                    case MainMenuStates.LoadGameChooseGame:
                        DrawLGChooseGame();
                        break;

                    case MainMenuStates.LoadGameLoading:
                        DrawLG_Loading();
                        break;

                    case MainMenuStates.SaveGameChooseSlot:
                        DrawSGChooseSlot();
                        break;

                    case MainMenuStates.SaveGameCheckingSlot:
                        DrawSGCheckSlot();
                        break;

                    case MainMenuStates.SaveGameChooseGame:
                        DrawSGChooseGame();
                        break;

                    case MainMenuStates.SaveGameSaving:
                        DrawSG_Saving();
                        break;
                }
                Memory.SpriteBatchEnd();

                Memory.SpriteBatchStartAlpha();
                DrawLGSGHeader();
                Memory.SpriteBatchEnd();
            }
        }

        private static void UpdateLGSG()
        {
            Rectangle dst = new Rectangle
            {
                X = 220,
                Y = 42,
                Width = 840,
                Height = 477,
            };
            IGM_Size = new Vector2 { X = 843, Y = 630 };
            Vector2 Zoom = Memory.Scale(IGM_Size.X, IGM_Size.Y, Memory.ScaleMode.FitBoth);

            IGM_focus = Matrix.CreateTranslation(-dst.X + (IGM_Size.X / -2), -dst.Y + (IGM_Size.Y / -2), 0) *
                Matrix.CreateScale(new Vector3(Zoom.X, Zoom.Y, 1)) *
                Matrix.CreateTranslation(vp.X / 2, vp.Y / 2, 0);
            Memory.IsMouseVisible = true;
            ml = Input.MouseLocation.Transform(IGM_focus);
            switch (State)
            {
                case MainMenuStates.LoadGameChooseSlot:
                    UpdateLGChooseSlot();
                    break;

                case MainMenuStates.LoadGameCheckingSlot:
                    UpdateLGCheckSlot();
                    Memory.IsMouseVisible = false;
                    break;

                case MainMenuStates.LoadGameChooseGame:
                    UpdateLGChooseGame();
                    break;

                case MainMenuStates.LoadGameLoading:
                    UpdateLG_Loading();
                    Memory.IsMouseVisible = false;
                    break;

                case MainMenuStates.SaveGameChooseSlot:
                    UpdateSGChooseSlot();
                    break;

                case MainMenuStates.SaveGameCheckingSlot:
                    UpdateSGCheckSlot();
                    Memory.IsMouseVisible = false;
                    break;

                case MainMenuStates.SaveGameChooseGame:
                    UpdateSGChooseGame();
                    break;

                case MainMenuStates.SaveGameSaving:
                    UpdateSG_Saving();
                    Memory.IsMouseVisible = false;
                    break;
            }
        }



        private static void DrawLGSGHeader(FF8String info, FF8String name, FF8String help) => LGSGHEADER = new Tuple<FF8String, FF8String, FF8String>(info, name, help);

        private static Rectangle DrawLGSGHeader() => LGSGHEADER != null ? DrawLGSGHeader(LGSGHEADER) : Rectangle.Empty;

        private static Rectangle DrawLGSGHeader(Tuple<FF8String, FF8String, FF8String> tuple)
        {
            FF8String info = tuple.Item1;
            FF8String name = tuple.Item2;
            FF8String help = tuple.Item3;
            Vector2 TextZoom = Memory.Scale(Width: 960, scaleMode: Memory.ScaleMode.FitBoth);
            Vector2 BoxZoom = new Vector2(1,TextZoom.Y);

            Rectangle dst = new Rectangle((int)(vp.X * 0.82421875f), 0, (int)(vp.X * 0.17578125f), (int)(vp_per.Y * 0.0916666666666667f));
            DrawBox(dst, name, indent:false,boxScale:BoxZoom,textScale:TextZoom);
            dst = new Rectangle(0, dst.Y, (int)(vp.X * 0.8203125f), dst.Height);
            DrawBox(dst, info, Icons.ID.INFO, boxScale: BoxZoom, textScale: TextZoom);
            dst = new Rectangle((int)(vp.X * 0.0282101167315175f), (int)(dst.Height + dst.Y + vp_per.Y * 0.0041666666666667f), (int)(vp.X * 0.943579766536965f), dst.Height);
            DrawBox(dst, help, Icons.ID.HELP,indent: false, boxScale: BoxZoom, textScale: TextZoom);
            return dst;
        }

        private static void DrawLGSGLoadBar()
        {
            Rectangle dst = new Rectangle
            {
                X = (int)(vp_per.X * 0.328125f),
                Y = (int)(vp_per.Y * 0.45f),
                Width = (int)(vp_per.X * 0.34375f),
                Height = (int)(vp_per.Y * 0.1f),
            };
            dst = DrawBox(dst, null, Icons.ID.INFO).Item1;

            dst.Offset(new Vector2
            {
                X = (vp_per.X * 0.01328125f),
                Y = (vp_per.Y * 0.0333333333333333f),
            });
            dst.Size = (new Point
            {
                X = (int)(vp_per.X * 0.3171875f),
                Y = (int)(vp_per.Y * 0.0333333333333333f),
            }.ToVector2()).ToPoint();
            Memory.Icons.Draw(Icons.ID.Bar_BG, -1, dst, Vector2.UnitY, fade);
            dst.Width = (int)(dst.Width * PercentLoaded);
            Memory.Icons.Draw(Icons.ID.Bar_Fill, -1, dst, Vector2.UnitY, fade);
        }

        private static Tuple<Rectangle, Point, Rectangle> DrawLGSGSlot(Vector2 offset, FF8String title, FF8String main)
        {
            Rectangle dst = new Rectangle((int)(vp_per.X * 0.3703125f), (int)(vp_per.Y * 0.386111111f), (int)(vp_per.X * 0.259375f), (int)(vp_per.Y * 0.141666667f));
            Rectangle slot = new Rectangle(dst.Location, new Point((int)(vp_per.X * 0.1f), (int)(vp_per.Y * 0.0875f)));
            slot.Offset(vp_per.X * -0.00859375f, vp_per.Y * -0.033333333f);
            slot.Offset(offset);
            dst.Offset(offset);
            Tuple<Rectangle, Point, Rectangle> location = DrawBox(dst, main, null, false, true);
            DrawBox(slot, title, null, false, false);
            return location;
        }

        private static void DrawPointer(Point cursor, sbyte xoffset = -10,bool blink = false)
        {
            Rectangle dst = new Rectangle(cursor, new Point(24 * 2, 16 * 2));
            dst.Offset(-(dst.Width) + xoffset, -(dst.Height * .25f));
            Memory.Icons.Draw(Icons.ID.Finger_Right, 2, dst, new Vector2(2f), blink?fade*s_blink:fade);
        }


        private static void InitLoad()
        {
            strLoadScreen = new Dictionary<Enum, Item>()
            {
                { Litems.GameFolder, new Item{Text=Memory.Strings.Read(Strings.FileID.MNGRP, 1 ,86) } },
                { Litems.Load, new Item{Text=Memory.Strings.Read(Strings.FileID.MNGRP, 0 ,54) } },
                { Litems.LoadFF8, new Item{Text=Memory.Strings.Read(Strings.FileID.MNGRP, 1 ,128) } },
                { Litems.Save, new Item{Text=Memory.Strings.Read(Strings.FileID.MNGRP, 0 ,14) } },
                { Litems.SaveFF8, new Item{Text=Memory.Strings.Read(Strings.FileID.MNGRP, 1 ,117) } },
                { Litems.FF8, new Item{Text=Memory.Strings.Read(Strings.FileID.MNGRP, 1 ,127) } },
                { Litems.Loading, new Item{Text=Memory.Strings.Read(Strings.FileID.MNGRP, 1 ,93) } },
                { Litems.Saving, new Item{Text=Memory.Strings.Read(Strings.FileID.MNGRP, 1 ,94) } },
                { Litems.Slot1, new Item{Text=Memory.Strings.Read(Strings.FileID.MNGRP, 1 ,87) } },
                { Litems.Slot2, new Item{Text=Memory.Strings.Read(Strings.FileID.MNGRP, 1 ,88) } },
                { Litems.GameFolderSlot1, new Item{Text=Memory.Strings.Read(Strings.FileID.MNGRP, 1 ,121) } },
                { Litems.GameFolderSlot2, new Item{Text=Memory.Strings.Read(Strings.FileID.MNGRP, 1 ,122) } },
                { Litems.CheckGameFolder, new Item{Text=Memory.Strings.Read(Strings.FileID.MNGRP, 1 ,110) } },
                { Litems.BlockToLoad, new Item{Text=Memory.Strings.Read(Strings.FileID.MNGRP, 1 ,114) } },
                { Litems.BlockToSave, new Item{Text=Memory.Strings.Read(Strings.FileID.MNGRP, 1 ,89) } },
            };
            SlotLoc = 0;
            BlockLoc = 0;

            SlotLocs = new Tuple<Rectangle, Point, Rectangle>[2];
            BlockLocs = new Tuple<Rectangle, Point, Rectangle>[3];
        }



        #endregion Methods
    }
}