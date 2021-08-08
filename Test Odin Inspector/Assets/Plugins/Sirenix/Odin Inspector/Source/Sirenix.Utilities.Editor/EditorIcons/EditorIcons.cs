#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="EditorIcons.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.Utilities.Editor
{
#pragma warning disable

    using System;
    using System.Linq;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Collection of EditorIcons for use in GUI drawing.
    /// </summary>
    public static class EditorIcons
    {
        private static Texture2D testInconclusive;
        private static Texture2D testFailed;
        private static Texture2D testNormal;
        private static Texture2D testPassed;
        private static Texture2D consoleInfoicon;
        private static Texture2D consoleWarnicon;
        private static Texture2D consoleErroricon;
        private static Texture2D unityInfoIcon;
        private static Texture2D unityErrorIcon;
        private static Texture2D unityWarningIcon;
        private static Texture2D unityFolderIcon;
        private static Texture2D odinInspectorLogo;
        private static Texture2D unityLogo;
        private static EditorIcon airplane;
        private static EditorIcon alertCircle;
        private static EditorIcon alertTriangle;
        private static EditorIcon arrowDown;
        private static EditorIcon arrowLeft;
        private static EditorIcon arrowRight;
        private static EditorIcon arrowUp;
        private static EditorIcon bell;
        private static EditorIcon car;
        private static EditorIcon char1;
        private static EditorIcon char2;
        private static EditorIcon char3;
        private static EditorIcon charGraph;
        private static EditorIcon checkmark;
        private static EditorIcon clock;
        private static EditorIcon clouds;
        private static EditorIcon cloudsRainy;
        private static EditorIcon cloudsRainySunny;
        private static EditorIcon cloudsRainyThunder;
        private static EditorIcon cloudsThunder;
        private static EditorIcon crosshair;
        private static EditorIcon cut;
        private static EditorIcon dayCalendar;
        private static EditorIcon download;
        private static EditorIcon eject;
        private static EditorIcon eyeDropper;
        private static EditorIcon female;
        private static EditorIcon file;
        private static EditorIcon fileCabinet;
        private static EditorIcon finnishBanner;
        private static EditorIcon flag;
        private static EditorIcon flagFinnish;
        private static EditorIcon folder;
        private static EditorIcon folderBack;
        private static EditorIcon gKey;
        private static EditorIcon globe;
        private static EditorIcon gridBlocks;
        private static EditorIcon gridImageText;
        private static EditorIcon gridImageTextList;
        private static EditorIcon gridLayout;
        private static EditorIcon hamburgerMenu;
        private static EditorIcon house;
        private static EditorIcon image;
        private static EditorIcon imageCollection;
        private static EditorIcon info;
        private static EditorIcon letter;
        private static EditorIcon lightBulb;
        private static EditorIcon link;
        private static EditorIcon list;
        private static EditorIcon loadingBar;
        private static EditorIcon lockLocked;
        private static EditorIcon lockUnloacked;
        private static EditorIcon magnifyingGlass;
        private static EditorIcon male;
        private static EditorIcon marker;
        private static EditorIcon maximize;
        private static EditorIcon microphone;
        private static EditorIcon minimize;
        private static EditorIcon minus;
        private static EditorIcon mobilePhone;
        private static EditorIcon money;
        private static EditorIcon move;
        private static EditorIcon multiUser;
        private static EditorIcon next;
        private static EditorIcon pacmanGhost;
        private static EditorIcon paperclip;
        private static EditorIcon pause;
        private static EditorIcon pen;
        private static EditorIcon penAdd;
        private static EditorIcon penMinus;
        private static EditorIcon play;
        private static EditorIcon plus;
        private static EditorIcon podium;
        private static EditorIcon previous;
        private static EditorIcon receptionSignal;
        private static EditorIcon redo;
        private static EditorIcon refresh;
        private static EditorIcon rotate;
        private static EditorIcon ruler;
        private static EditorIcon rulerRect;
        private static EditorIcon settingsCog;
        private static EditorIcon shoppingBasket;
        private static EditorIcon shoppingCart;
        private static EditorIcon singleUser;
        private static EditorIcon smartPhone;
        private static EditorIcon sound;
        private static EditorIcon speechBubbleRound;
        private static EditorIcon speechBubbleSquare;
        private static EditorIcon speechBubblesRound;
        private static EditorIcon speechBubblesSquare;
        private static EditorIcon starPointer;
        private static EditorIcon stop;
        private static EditorIcon stretch;
        private static EditorIcon table;
        private static EditorIcon tag;
        private static EditorIcon testTube;
        private static EditorIcon timer;
        private static EditorIcon trafficStopLight;
        private static EditorIcon transparent;
        private static EditorIcon tree;
        private static EditorIcon triangleDown;
        private static EditorIcon triangleLeft;
        private static EditorIcon triangleRight;
        private static EditorIcon triangleUp;
        private static EditorIcon undo;
        private static EditorIcon upload;
        private static EditorIcon wifiSignal;
        private static EditorIcon x;
        private static Texture2D unityGameObjectIcon;

        /// <summary>
        /// Gets an icon of an airplane symbol.
        /// </summary>
        public static EditorIcon Airplane
        {
            get
            {
                if (airplane == null)
                {
                    airplane = new LazyEditorIcon(34, 34, "iVBORw0KGgoAAAANSUhEUgAAACIAAAAiCAYAAAA6RwvCAAACVklEQVRYCe2WTU4DMQyFW6RuWCIhsWCDxIYblCP0Dr0AS7hFj8Cai+QW7OAMrFkUf+l45HqcZGb4XWApjeP4PTuJJ+lyv98v/oKc/IUkyOE/EX8Sv7UjG0lkJ43+IBTrhHYpvi/STidgLP9KcEmalTxvnVr6mUEvRbf+jJ+kbZzd+rAIL0kM2cc6okO0lbaWRmBWgJ1eBUKPS90kOD/HGN5IiJH9Peg+8nY2TZSEOCJ2AqH3fLpT2cH8DI7XAxkTIBnQWHXXYXUXSZKAXrCpTx+/V2TS63MTInCUAHYW6Osrx/XBozEJ6fZDNlfgCJMQ+yIKXLJRWHMT4thKvNlenSyAOXuIxwrH1IzTdKiQUHBjvjISbsb5zBX/Lnfzhd7QjX7VmJ9UI35VU46HgmcHt9JStEOefOx4atESXCWsmbGB1a90UxKEHbLvkQa2PQmFD6YGGNPXkqBolaO0W6V3KOMU3OpJItmlGX1jkoCHevASHoc49XF7xRqdXkuitMroiq/G8pOcsT1DKj0iFXNeucfrmAS9sCDm2TEbI2MUSO/BFF+UBLb+f4TolkN1gnrhCJWPz1h9c28HBG4JRIMnXGyWR3WCRZLEOOBQED2rKIEhhEC31+JKOsG82K/rCHc0EBRjCPwnyHhKEsprd3lwHF287KuAqLfJQBj5tGz2grupcdSI7NZSGzXf2lwSLFL61DO29vryuj50r+aV9O0XNH5i7zrzdTx9sNYSweOxA99KT2Jz5FlAr9LOa+BcgDWHL5pbC8+bNJIK5acSCYNbY+torO+36h/dHUnVFLY0rAAAAABJRU5ErkJggg==");
                }

                return airplane;
            }
        }

        /// <summary>
        /// Gets an icon of an alert circle symbol.
        /// </summary>
        public static EditorIcon AlertCircle
        {
            get
            {
                if (alertCircle == null)
                {
                    alertCircle = new LazyEditorIcon(34, 34, "iVBORw0KGgoAAAANSUhEUgAAACIAAAAiCAYAAAA6RwvCAAAB4UlEQVRYCe1XO07DQBDFSKEIHZwgXIGGkoKGkoIuHCK3gCPQQknBSbgC3IAOCijMe1YszY7f2uOVjYLESpPszue9yc7s2qnqut7bhbG/C0kwh/9EfCX+9I4s8GvOIHeQVwi7ncI5dbTRZ9zgqQnKEn6PkOigL2NC+CEngK2j7MKPsYM8Qw4VQMbsgsijURGDWFm+rGEbNEUSTSb4IFaWL2tAUKQcdtvH+ifczXaJ9l5C9yH0XlU5ReR5cYiYTxeXvVnvveOEa4mtdoR3wFeQuGRHCH0A+bYc6ma9sA4zzTscKpHzmcgtbIdDlYZX9cpG9cxLS/MGzBOLqxKJdH6LUZoI45NYVZqW5Fe/VSLctpKR/MIBgA6HSuRpAMSa11gwAR75B2sYmHc5xP1/Cd3cgxzJFZ8stsbFiCw28GXDUziPDnIk3MnCGKNP3ebUmbhIIvIprI4vyxt96LHH7HG381ybHMPw7o2qWenDp+ONdxbrW+jYrJSNsHsVMTtJNE5mW1WZoiUqLknLr8itjqWbIhli+H6yPCiw697MOvL2hVA57Ftcli9rAKS3lfydOBI4HrdZ506NbzK75i16CrmCXENWEA5e27wxnyEvkOTFB+veUZJIL2CpMXd8S/GK43YmkR876s1/EisDGQAAAABJRU5ErkJggg==");
                }

                return alertCircle;
            }
        }

        /// <summary>
        /// Gets an icon of an alert triangle symbol.
        /// </summary>
        public static EditorIcon AlertTriangle
        {
            get
            {
                if (alertTriangle == null)
                {
                    alertTriangle = new LazyEditorIcon(34, 34, "iVBORw0KGgoAAAANSUhEUgAAACIAAAAiCAYAAAA6RwvCAAABwElEQVRYCe1UO04DQQzNIkIRutRpEBUFfTpyD8IhcgIaCu4Rajp6cgnKcAJaCpBY3kOx5PXOx7MBssVasmb8WdvzbG9V1/WoD3TUhyJYw1CI7cSAyG8jMkdArt0WPLXBS+Rqj/Vl4jeV7BX3c3Cn/8E+M/KoiuD1DHxtdG6xKyIXyPASyXIC/WfEFlV3QaRCtKdoxNHoLmGLmzgjhbyEvxDv8r3WT5Ve7MmztDVjPOlDPYvoaJJB3UC50IbcvbQ1XtivkJir7aYSROy6MkkMESmADxWURBc8SxCx6xoMaJTudfYiElvXHCKs6xT8bgpsiR5EmCy1rq2gRnFr5LDoWDO9lrK2ctqVFL09Z1BY34aca41dV/saT2v4zQa84CVGudZ415XxbVE6Z36dE5Dx75gjto2ojsHrnPPOt9ES6H7koHJnfHYELnVh4cGcQSWc584MK/gRETLvHprAqZU3NKzs9ZducOLOGdN/Tn2PffYAw401hobV/Te0wZzyEn4z62sLmcBhbZ0S8j1sRJC8SvhZE1FpkunXXwxobG4ag6sR4au47/9FlzrRsRI4aCzmIKQROUgBknQoRJCQszeIfAOHoAHkv90e7QAAAABJRU5ErkJggg==");
                }

                return alertTriangle;
            }
        }

        /// <summary>
        /// Gets an icon of an arrow down symbol.
        /// </summary>
        public static EditorIcon ArrowDown
        {
            get
            {
                if (arrowDown == null)
                {
                    arrowDown = new LazyEditorIcon(34, 34, "iVBORw0KGgoAAAANSUhEUgAAACIAAAAiCAYAAAA6RwvCAAABAElEQVRYCe2VPQ8CIQyGxVFHVx30r/uXnL3F+TZdse9GGngpxZgb2oQE6AdP2t415Zx3W5D9FiDAECC6Ep6MnCXIUxaaSy/cQz8sydGseOxKXlpEdyP6qsoDYvnMUvU1cukpDQnnVwWIzl1kJDKiM6DP0SOREZ0BfWY9chdjPV0tcwZv1Pxw15xBbOgdxPEhi01aUZvlJJZry5qBwAcw75bzwD2FQBxWGug/so7YTEgXArF7ILCZgTFBWEG8MGaIEZBRmIs4NBsTwbRYSlP6WMoEiFfpZNmPgiAmg3FBIKgHpAXjhpgBKWEWOUxBIFjvhwabv4i3ND+HCxCd0s1k5AswITyuETiinwAAAABJRU5ErkJggg==");
                }

                return arrowDown;
            }
        }

        /// <summary>
        /// Gets an icon of an arrow left symbol.
        /// </summary>
        public static EditorIcon ArrowLeft
        {
            get
            {
                if (arrowLeft == null)
                {
                    arrowLeft = new LazyEditorIcon(34, 34, "iVBORw0KGgoAAAANSUhEUgAAACIAAAAiCAYAAAA6RwvCAAABBElEQVRYCe2VPQ4CIRCFxVhpp7WNZ/cgHsAzaGOrnbbrvALDAgsD8szGMMmGn33MfMxAMMMwLOZgyzlAgKGD+JXoGfm7jGxlRyd/V9+MVxWLAXGvWJdcUnpYKRAgLAGhQZSAUCG0IHQIDUgOAi9myXcRPXwGZhKvbw4icKacuIru4GunQFgQNr6xHdvGbg0bwsYetTGQ80jxo0GsNGuJ/STHV5XmJRAbMkjgPlYaiACzC9RtJnBrAouVxhUxDu5eAtzcIOjnQKBJwQS1xoIamyqN6+shA1aZPnE0IBDTYbQgdJgSECpMKYgLc8SglWluTatYST81GUk6rP3ZQfzM9Yz4GXkDa6I6OJJTfAgAAAAASUVORK5CYII=");
                }

                return arrowLeft;
            }
        }

        /// <summary>
        /// Gets an icon of an arrow right symbol.
        /// </summary>
        public static EditorIcon ArrowRight
        {
            get
            {
                if (arrowRight == null)
                {
                    arrowRight = new LazyEditorIcon(34, 34, "iVBORw0KGgoAAAANSUhEUgAAACIAAAAiCAYAAAA6RwvCAAABDklEQVRYCe2WQQ4CIQxFHeMJdKsLl3puD+IpdKcL3eoVsD+RxWBhWqBkEmnCRJlSHr8tmcE5t5iDLecAAYYOEmaiK/IXipzplOvwpNL/Q8V7xF9IG9r8LQXwfhbF+qLgamUsQHBIwGzxQ2pWINj/QUMMYwmiguFAkN8bDRSfZmBjzkTKcF0DiD0XsXDuSOuvsRgciG/D2JqS+SgMl5qSjabWXsjhwDm1VsQzQICR8q0VAQjSM4LAZGsQVY3cQWhgO4qp6hrchuj9mgaIZyogV6wp/9S7n7x/nSch4GddIyIIaxAxBEBWeBiY+uPIIjVqCAhRE+RE8bIgAFKzaxAv22oqkg2BhR0klK8rEiryAVzfQYdXRM/hAAAAAElFTkSuQmCC");
                }

                return arrowRight;
            }
        }

        /// <summary>
        /// Gets an icon of an arrow up symbol.
        /// </summary>
        public static EditorIcon ArrowUp
        {
            get
            {
                if (arrowUp == null)
                {
                    arrowUp = new LazyEditorIcon(34, 34, "iVBORw0KGgoAAAANSUhEUgAAACIAAAAiCAYAAAA6RwvCAAAA7UlEQVRYCe2WOw7CMAyGCeoEG8y9CsfmOHAC2Moa7KGS5Sb1q0gVcqQocR72lz9p5VJrPeyhHPcAgQwJwm/irxS5wOkeULF1lxL8ajD4i0S/Qv9NbHU3cjUcAoMilEsZryItCHr6MxgfOiD1PYpIEBhzgnqSgtN5K4gGYvZvgrGAWCDMMFoQD4QJRgMSgVDDSCBbQKhg1kAKeKA/q9lhpL33Ng+9CRjHRAVhWkWTxPT2tvxlGrBQZe2NLBb/ciBBuLqpSCrCFeB2vpGtFHlyR1HbezU3CNyDwfHRCubN4q1xxPVeRUTH1gUJwhX7AiqoJcf78fyZAAAAAElFTkSuQmCC");
                }

                return arrowUp;
            }
        }

        /// <summary>
        /// Gets an icon of a bell symbol.
        /// </summary>
        public static EditorIcon Bell
        {
            get
            {
                if (bell == null)
                {
                    bell = new LazyEditorIcon(34, 34, "iVBORw0KGgoAAAANSUhEUgAAACIAAAAiCAYAAAA6RwvCAAABVklEQVRYCe1Vyw3CMAxtEVcuXDhzhEmAGWAHGIM5YCa4wSYUP5RKUWWSZyeHImEppHH8eXk2Sdt1XTMGmYwBBDDUALKVOKAVs1vaCqWJa9t6kdRg5BCS77wg4FeDkU8c+YmZgc4kXkbmkuUk4yEDAF5hxhp67NsEPWIcZ7FnBHZ0bNowBN0zCCIb2FM5rD0C6pc2zhvqn2QF4mlICoi3WY2k5M3/QIYc/SQjVNMNTyprys/CyFRJwqgoPwuQGZNVsaH8LEAWShJGRflZgGyYrIoN5cferGg4vLBewYGTtzLLyNGLIPjl/YnXcRu9piWfiPP1Jc6VZiUnuhWyEbuvZXGPFf13Dkiyrn0Q46xecKkeUR2MSTVzNW4KCNh4apEKdIinspwrTUFOm2uKETbSRQwxiqQGIz3Vau1ZdDUYYXMl7agnOhmhaa6ZfWq7RmmoRDmj0ZTmDQXdkolEquIfAAAAAElFTkSuQmCC");
                }

                return bell;
            }
        }

        /// <summary>
        /// Gets an icon of a car symbol.
        /// </summary>
        public static EditorIcon Car
        {
            get
            {
                if (car == null)
                {
                    car = new LazyEditorIcon(34, 34, "iVBORw0KGgoAAAANSUhEUgAAACIAAAAiCAYAAAA6RwvCAAABoklEQVRYCe2VPU4DMRCFdxE0oQstDSUngA5R5g6ROAFFKGipQ8MhwkHgEimDxB2okFjeZzKR16x3vUC0QfJIL/6Zv+cZr1NWVVXsguztAgk4ZCJhJ3JFckXCCoTrf39HpjoRL2EKZuHpG9e8rD0xl31fGcuhNU/f1nC628YTtW9etauLooRpotCORaJtkxmHjiZLrcjklyQgdsZPTFIqcirnZSxAj/1n2V569rXqWEXGMpgLKEP8BQnyXwgfHshDzlJwd2Sk8Y3FQHKjvA9U5G4gApb2mgl3ZKXxxHYHGku7I7H891IcCfQRW+aPQqpga/7EOBSI6cuLW6gisZdytn4NpxqRlTAReCGfhC5ZyADbcwFf3x+dCbH0iVTVyHaCkbYZCV9F4GN/IzInLrahcBg/J4VwRCBzIJDUmDuWWjcJNpDsEuISJxT80SGQIHexv27Wu0b6mdL/V9npCF/fv8afins/Ns6waUGsNW0+vi7WGt/Gzb9tNJDyW2aXNcXPbPCxlkf9U/5rNtXb5qTrHdlm7lrsTKRWDi1yRXJFwgqE60+R//8giDxC8wAAAABJRU5ErkJggg==");
                }

                return car;
            }
        }

        /// <summary>
        /// Gets an icon of a char1 symbol.
        /// </summary>
        public static EditorIcon Char1
        {
            get
            {
                if (char1 == null)
                {
                    char1 = new LazyEditorIcon(34, 34, "iVBORw0KGgoAAAANSUhEUgAAACIAAAAiCAYAAAA6RwvCAAACDUlEQVRYCc1XsU7DMBBNEWXoyAcgIRYGNgZgY6QTYgeJX4CBlZV+B2xMLAi2bkzsTFQq3wADDOW9kIuc8yV2Wgf1pKud87vn14udOL3ZbJYtg60sgwhqWE0o5B1cU/gD/AX+Cv+BxxlvTSIHjWe3iKzDg3MEATEkBQZNrY0w0oPXzpcPxtUuR/Xwuw0/gh/nkSw7LNrQqp8AtwP/KvDVpkmlMzZAn//KMvmX1pgVI5fklG1MRYaQ/liVX7lilWihivyhsoyV2dL40PYdIaFJhJC3aTcBvvESrDIVsbpbocst5dXx0HVlNwmJbochFmdccp1QVJdbW3Iza40MULZPr3T1gbZrxGVaw0X+0LPWyLWL7Li/K/xaSB8DVzL4D+2JzKGFlAoF0HG7J/xaSKlQAB23G8KvFyvfoNznbUwWa5scD6uFxD4dXaIkQvStcSeI7VN8Gx9bxFrIxAIljn1YfFrIvQVKHHu2+LSQsQVKHOMR0jO9WPlA+/ZQaQP8896m0BXhc/8u7bwVtktLBBG6Ioy1fekxJ9bKl5xO0BXhOM+UZxqY4PoAHPWfF+6ZQPV5XkhlFyAqzx5Wv3EQCSnE8KQXmgfLNww6BWZe20dizBxRQkjU9DlhieSt6MeKIM7aNU3rki84fmCdw3mWkNf4FH1+8z7B3+DecwKxRmsrpJFskUFr+y7CN3fu0gj5BaHvbbDN+I8aAAAAAElFTkSuQmCC");
                }

                return char1;
            }
        }

        /// <summary>
        /// Gets an icon of a char2 symbol.
        /// </summary>
        public static EditorIcon Char2
        {
            get
            {
                if (char2 == null)
                {
                    char2 = new LazyEditorIcon(34, 34, "iVBORw0KGgoAAAANSUhEUgAAACIAAAAiCAYAAAA6RwvCAAACSElEQVRYCdVWO04DMRBNkEKRMrRpQhcuABJFaBE3QNyAnjNwAw4QRINEj2g2BQUSPRWhoIYOCijCe6t49XbW6/VuNlIYaeLxfJ4n9njW3cVi0dkE2tqEJJjDv06kjz9wDJ6C52CeLZkydbTRpx6xRiJ5CL8EHEv0HYOj8GOcegCbVqw+D9gZS4zgWkEjggeBBdTkcPpQHoATNS5lYjm/wlhQiDNBY8mHw4TsTpUm4wOgrusBCSUVwrHH6j2mMgAbHEqCtjIcp08EgNhOn4385/aaDaF4t8qKeTfC/gqf0dJvD+OLxvga2rU6tCTz354I1pXIqWh3hI3oyzpFzKt2xEGw6bld2YH86Qx2RybOsKbxXHD3RS58a47UuAZ5JpinInfs0ejWqV+VHHs0xHG34w3yrgO2iTgnZ48dmyRC7CzO1kjswtZvYBV15zYRblcTekZQ9u8iAXJr2URuI0Gs2wiKS6v0zPWd8qh2m0iixpryBfzHFTETsd+LXLg1TRuaYm5j8qsKkfVWBhvaN4JmEthEfCgJ4m7xCEmsj6yrphrPl5DPu1XpDADZlxUyP/1KhSekOqtc9xmgiziZb1xislclTrmUda1Utg0t3SX89MA/brLCyDq4A2uRemuoLBGuzSb1QaFFyhWo4trrqzYWEwPbotIkuEAoEdqZDLdylccSbyEx8rcEihyheAqFU6JjpWvRYRok+hZuB3Te9UI1kktYJmx6LD6+Jw7B2hvYtm/AT+DwDsBBqUkiGt+aXFUjrS1UBbQxifwBmsFtK+NBR6kAAAAASUVORK5CYII=");
                }

                return char2;
            }
        }

        /// <summary>
        /// Gets an icon of a char3 symbol.
        /// </summary>
        public static EditorIcon Char3
        {
            get
            {
                if (char3 == null)
                {
                    char3 = new LazyEditorIcon(34, 34, "iVBORw0KGgoAAAANSUhEUgAAACIAAAAiCAYAAAA6RwvCAAACC0lEQVRYCdVWMU4DMRBMkEKREkpoUoYPgESRtDyABvEIXkRBEA09Et2loECihgp4AXRQQBFmIllaT2yfnbtEYaXF69298cRem+3OZrPOJsjWJpAgh39NpI8fcAKdQN+gPFsqbfoYY06ZsEYydR95FTRXmDuEZuHnJPUANsldPZDHb4mRXKvLhITsIPaRiLtQFwZ1DzqG8ohUduH4VKebp4jwnL9cYs1IEiqslTtxRsnEiBD4FToQoNg0RKSH5J/AB9vw/ao/dn2vkJhLQjHd/NIZMob9gSLi7SgVLcSjGoCF26QAnFc1IKGwxemHEsTHNew3Ha2RkgK1O25rpEJgZIMR2ytcrZEcgAju3H2Ov7kYhxZIiYxtsNDmmxN6P2IwZzagRE5tsMDm0TwV5DP12OYrkWWv7AVAS7/18rVYk++9/QUt2dzJueiOvLvAGkZvLSVyuwYCbokHZ3BUIpUNrti+t/haI8s+aBYz104+aN9AmeYiNchjfXi9ie4IsYfQZxorlANgv1h8rRHGmHBtk1q2ueMeCeKHdoT+WFPDWFMpaozYQbGY2hZiLnRnXCR0NG5xFlObZLxb4hZxY4oIc0iGW9mkZlgTxPBuCea+aKeUmLO9K+nemLvQEsbwY8Xqs/VnfPRGUPYT/Fc+gFL4NvDZvoE+QtM7gAQryxCx37dm19VIawvVAW0MkT8xVKOyf/SwuwAAAABJRU5ErkJggg==");
                }

                return char3;
            }
        }

        /// <summary>
        /// Gets an icon of a char graph symbol.
        /// </summary>
        public static EditorIcon CharGraph
        {
            get
            {
                if (charGraph == null)
                {
                    charGraph = new LazyEditorIcon(34, 34, "iVBORw0KGgoAAAANSUhEUgAAACIAAAAiCAYAAAA6RwvCAAABpUlEQVRYCe1XQU7DQAwkiF565FwhwY0f9AvlCag8ovyCd5RfcAk3HsCJQ5HgzhfCTBRHXtdpNoi4OdSStbbr9UztbbItqqo6m4KcT4EEOZyI2ElMtiMbMOXpXVrGY/uF+dXon1AxNriuP9nRaJKh9rE6wrHzPLZyjDMyA/oL9Ap6I0wuxAhaF8D5brA+NWbkaNaKhOZQ2xFEeB62jWoCX9oZezRzgL1DrzUo7FfoXRLjA00pzFZ0fIboClpCn6D1Iceqc6y9xOeebBC0uXiep0G9keAsVupgY++wLqB2v/gE8+QWQclJ1sTxdvbECKi7Q/Kls4fE51CL1/qt0SQ5NXpD0h12yJOsUeYSYTEZxdpD64hxtBbD9W1Q19vC4Ux16yW/69vLfnbpEir5vatNkEJc7WfW7zoPWaOw9W3xIUS4l90isMgKhq2Z5ddtVw+Wv16MeKP7gP6oWoPM/yIyCNRLjnjXeLh7sUNEeG8IEzuaHZDtC2oImeSOkbHxHjlvzLNEeGfgKztSeE3Y+8v5jNhjIIsHwbIdkXj4euiwhpI5EbHt/gV5CcRhQy4PnQAAAABJRU5ErkJggg==");
                }

                return charGraph;
            }
        }

        /// <summary>
        /// Gets an icon of a checkmark symbol.
        /// </summary>
        public static EditorIcon Checkmark
        {
            get
            {
                if (checkmark == null)
                {
                    checkmark = new LazyEditorIcon(34, 34, "iVBORw0KGgoAAAANSUhEUgAAACIAAAAiCAYAAAA6RwvCAAAB/0lEQVRYCe1WK1PDQBAmzBRRWWwRyP6DOmIBh+6fKL8BA/8AXTSOh6QejSwGCw4EiPB9ocnsbe42D3IznYGd2dw+bh93e3uXJMuyrU2A7U1Igjn8uUTGWPQcyNEPPCORcQj/Es7BUObEdRit7IlfwI8PphCW8RMyEWEC30+G/6TQxTysDHJTBPKMF45Mbk/P9Bz+LBhAGb00I6z21VmxyxyBvZOiWKW5lkEU/Qz+XsmiXGhTBDnQgQR/DLrSIX13zQBBPkVQTS4hSLWQfN+lOfMFEbITQbukPLm/pMdWi0DHG7XsEk33WZoVlrjvLtPhdsB9ORLBWKXhhURsAjNMspJguwaTyAPoLVrzI4yEFbDyQEEmt1g/arSTQB/5zmOUdg7t2xGe/Mc8y59VvoPmBRWCy5BiLfe2a8XGk2XoteRhdFYBfgK04AFKbePltXBmeYWOgQsbbncdsMTFfHPUyjrH1PM/gkmwHS3go6f9B3ndvjz9i0r9ugnMdtUu9WG9woQ9PakDX9+uyqlOhOoXIFezJNMB+Lo6T3wTH75EaMfLJwWeAtsC27U16DPic1D33yltuIupFDSlmyRCX0PgLdD6z+C8XeAbibYQKo328wFBCrRKRV2nJGCHX6UWvb6ey0uN70cBpA+BXXyVNk1Lkycd89O0NDFzyH3/J6K3+But3CYtrLpzDAAAAABJRU5ErkJggg==");
                }

                return checkmark;
            }
        }

        /// <summary>
        /// Gets an icon of a clock symbol.
        /// </summary>
        public static EditorIcon Clock
        {
            get
            {
                if (clock == null)
                {
                    clock = new LazyEditorIcon(34, 34, "iVBORw0KGgoAAAANSUhEUgAAACIAAAAiCAYAAAA6RwvCAAACcElEQVRYCdWXsTIEQRCG71SdgAipUiXjCYRSJEqM8gISXkDMawgFSqKEK5SLSQjJCKhy+t+63vp2bndn9m4Dumpq/pnu/qen97a3rz8cDnt/QWb+QhCK4d8G0rfg122c2Mhs6Ln60Fr70suuneg3kjDmzObERhs5N2P5pfDbdeKGB5HTnyJ6+UfPaTLoG8FlxSHKzLIN6emvtfarMpdV2NO3NiMilTNFNwsP3x4ZaCax7GRPUeZC/8KnAGZEHGZiMdC7LQ/yPc7y46PLangqMxLepOkHFwtEQQ2CYMTPYHOcpwrv2cDwF9ZLht+xDiHLctMru2iOb3CeN/yJ9VhBO4by0HBTEDCNQvHswOoMOIfMiG70AwNVXd4YqgJS35QROTTys8SvFfS93qlhHgLVxFB8yrLLhgPNDGQLiivgLmEGsj3gUiC7ULwCdwlfQFabkRUYTfJY5FM1tsEr+Dxab3Kfj2Z1pHBD2k2Db1OcGYgH4AGl+POVrLNPsSlVVpVfl9pvghmMVcWWe36Gziu4mJEbXGkBuEuoCuvy4EAzA7mD4gi4S8jHdE3iaSsruWI4rKyz5vDtTszI0DZVUV32HXQ0s2BeGGcRhPiZEa3nbHwIjCT29XW72BzyRr+++jTze6BPt0imEbUWjyAQf6kFyHV8hYBTO7Ti9YMv96bq0ESkR8YWz5Z5D5paX2QXdnoT9aweTJgZBRTr4sMA5JPZaLwE01iHq4hF7hJmzvd9ruxRTVk6r7QIlVirAa76v2LbtSJ7+SWdkacLv+gYVFFSJ6eaoP5l04bLvQGVbVVMzapLydI2kGTitoasrG19O7X/BX2clrc4UltZAAAAAElFTkSuQmCC");
                }

                return clock;
            }
        }

        /// <summary>
        /// Gets an icon of a clouds symbol.
        /// </summary>
        public static EditorIcon Clouds
        {
            get
            {
                if (clouds == null)
                {
                    clouds = new LazyEditorIcon(34, 34, "iVBORw0KGgoAAAANSUhEUgAAACIAAAAiCAYAAAA6RwvCAAABjklEQVRYCe1VMVLDMBCMmUmKlDRpQsFQ5QeUtPCETPKHpOINocgL6AJvoDWfoEyKPIPC7Go4z/kcxbLGmBS6mR1Jp7vT+k4+ZUVRDC5Bri6BBDkkIrYSKSMpIzYDdh1zR4YIsgBygE1IsMF8BsQJG1oLzGDbJDkMhkCbuIOMDoHCr/0KtD3A7g4IDh5aGpYjlAS53gIrYAzkAAntgTXAWHXxpHAK/QbYA2NgB8TItcfpHvpK6SqL3821cSapWPERYTzet/J8Wxqmbmvy9mDWbZaTM8YsdVbuK1Y+9ixPrCwaHB+x77KiM/JcsqtOePFiZdfgOJd9/fsG/2ri3NHoyqMz0lHcuDD/TeRTaGsi7IZ9y6scqO8IH7KmyyV+XY0jBPpmMJ2R966iB8ZZws6RoL0mwr/mhsoe5AVnvOlzdGlEP8XkKIs/GJ8Q86MWVzqbGUmQXS8HtLDL8gFkx2QnLt8KzOlDHR802tBWwAeUevfhGLWfm5/KSI1sHwp9R/o4z3tGImJTkzKSMmIzYNc/342EmxC4R6wAAAAASUVORK5CYII=");
                }

                return clouds;
            }
        }

        /// <summary>
        /// Gets an icon of a clouds rainy symbol.
        /// </summary>
        public static EditorIcon CloudsRainy
        {
            get
            {
                if (cloudsRainy == null)
                {
                    cloudsRainy = new LazyEditorIcon(34, 34, "iVBORw0KGgoAAAANSUhEUgAAACIAAAAiCAYAAAA6RwvCAAAB7ElEQVRYCe1WO07DQBCNkdKkDCVUoUBwglCRNncIh+AY4RrhIEvHKcIJoIMCCvNetJusx57xrmVHQWKkp/3N5+3MZJ2iLMvRKcjZKZAgh38ishJDZGSMICtgC7ABCQfMAV3YrD1iCl+WbHBYALWYu02dZtYJM/GdYPEMnQep16U0BZwsgZB6jo/e8TnGNz/XBpbtpnbYlCZjjxl0gCaXOBgDK2ACsAQcl0AsLFGlPLml2eAmvJElIcsXXukD4xcwAT79Hgdm9iCSmVjzJltgDfC2KTKHEm1iCU3KTAWpZKSygEa8pnEs0nl8ljInifCrcpjHsfAjFxt+zVv1LQzOVqDQfyW21iMOxbs/FLC3GfuCv7KZ9KgR4Ws4hJAI35sf6Tx0uNwfYv3indZIcF8j8jQAk7XlUyvNFEbvlmHmGV/bK0AtuZYRPkJ3mcE0dZK4BlQSNNSI8OwV4LeDZaKzHKE+7W6BGdDYF9jfi1aavcKxJlZGjsVhF6eNSPi8x6TM7o8V/ZwfynaRT23mms925anGuvaJb9CRNqNj9ohDWhZaaqzStJWAjnNkYSnnZsTBmenQCmadWRlpssshkdakPkoKkVSHspS1f+pNNwt7uaUJdr2PKRmRQeXN5Xmn9Z/OSKcbtxl1KU2bz07nv0In/cbiNYzZAAAAAElFTkSuQmCC");
                }

                return cloudsRainy;
            }
        }

        /// <summary>
        /// Gets an icon of a clouds rainy sunny symbol.
        /// </summary>
        public static EditorIcon CloudsRainySunny
        {
            get
            {
                if (cloudsRainySunny == null)
                {
                    cloudsRainySunny = new LazyEditorIcon(34, 34, "iVBORw0KGgoAAAANSUhEUgAAACIAAAAiCAYAAAA6RwvCAAACa0lEQVRYCdWXK1IDQRCGCUUQVGGCQIDC5QapQoCNRODCCTBwAjQYDhEOgVwcV8CAxYBEgAj/t5leJp2dZDfZUNBVnZ6e6fd0z0JrNBqt/QVY/wtBEMMqA2nXSbLJQFpyPIycf0bra605T8KygWzJsmVOsx1GvDnl/FRozYjOFCwbSEcWn4SW7bnW28HLWaC7ouwDA+FVvvI/TM2S2JP+oIKNjmSylNyiQeB83xltiyegZ6EBjpH1ftDtxvteoCpPdsOA6MDPAmRbQmQzIfxW4HOf+aG/rho8vbEhjCckpX6nA/qG5v3yQnWaFad94bOQCYBeCIEd4Uu+Sv/QqF0hQdDk8Iz1GOLyzFhTOUqaAu7cesRKDu07Ba6Eq6CX6B10al0NDxUZzAKr7l4Qehf9EJL9W9iD2KhD7W3Rarqj2SMTuv9aSLZVgAzjiUHHmhQ7BtjPhMhC4UsDQTkGbzw+q7LmGrpBsHAsPg/A6ASjTbJqGnBOLwBUGp/4YeQL/358M93bkbBpsH7YlGGmpic8Eb4Kb4VrPpCf5uG0OSAQnD8Kl35HFg3rIShaEHwkL0NAhU0bOdu4sUWD1B4tq8SBbN8L7Ss9dhU3TGggkcaAicuvX5SmZXqKBo3XZZtNTQ5BFC9nWLNnL++E7wkmipDR4hGq+4Ygj14qc6vOlF8/NQ22Rj1TvlljbTrbgzWe30/x8R/TKZnxvso4VaaKe1mJnH1da9v8ravJlPbxOPXy39TVlF1BVm6i0u7xPKlVVYTesH8n5sWQn6cqYspVm81XsFYQOFtVRSyRynReRbwhn7k/X5j/txVZOON5it/iDDv8GiaW8wAAAABJRU5ErkJggg==");
                }

                return cloudsRainySunny;
            }
        }

        /// <summary>
        /// Gets an icon of a clouds rainy thunder symbol.
        /// </summary>
        public static EditorIcon CloudsRainyThunder
        {
            get
            {
                if (cloudsRainyThunder == null)
                {
                    cloudsRainyThunder = new LazyEditorIcon(34, 34, "iVBORw0KGgoAAAANSUhEUgAAACIAAAAiCAYAAAA6RwvCAAAB9klEQVRYCdVWMU4DMRBMkKBImZY0iIpfpAR6GhQkxA/CL0LLE0KDeADtvSQUlEh00CARZk7naL23vrNz8SmstLK9ux6P13v2Ddfr9WAf5GAfSJDDvycywSYW0BWUZ8uWY9q3E9ZIoi4Q3yT0D6FJuEnBAG8j4QgyLgm7LZg7G1WgZ26VyJbzxtACuqp0iZY4tXVrhiqIIJxEIQjj2KbIBYJDc2oZs4hMjNUcKcO1tcnLjP58hyj5d6PsZ4atq+lWAmgi59KZuX8l8TWRa+nM3D+R+OX3Lgx9PzwshVJ0Rpy9j/ZNLqKJeE4ZmKH/IjE1kUfpzNz31tI1MsLiX5kJEP4JeiPX0Rn5hvNSBmTo8/jvNK4mQv8rNBeZB2CfQn+gvuCCtq552vjezKGh9wKuVuHcAjqD8gEMrTXQNeKz7HFkHY1cfi4HVZ9/YpaMLSNsy4DdNzelK8LHtLt0u6edL7WzRbfRgS3gPH/WQ3nUgdgiYC85NBFxOwzFSGD5v8LCDM0J2lOLtcDBTv3D3Yz4gP1Cj6D1z3MTZnfailXPmmqDGB+jfw91JOKK1AFEpDG2+HjnHEbgmccTkxHvTXAbMNoPkQ3D3WyKIaIRrHuEd8izDkwZpxZrCJtEPkPOGPuuiPCL6fSbuc3RWBvsRIKAuyJikUuy/QFdFZY49utiaAAAAABJRU5ErkJggg==");
                }

                return cloudsRainyThunder;
            }
        }

        /// <summary>
        /// Gets an icon of a clouds thunder symbol.
        /// </summary>
        public static EditorIcon CloudsThunder
        {
            get
            {
                if (cloudsThunder == null)
                {
                    cloudsThunder = new LazyEditorIcon(34, 34, "iVBORw0KGgoAAAANSUhEUgAAACIAAAAiCAYAAAA6RwvCAAAB7klEQVRYCdVXsU7DMBBNkMpQiaUrFRJj/wTYERIqiIEfgIVv6MzAB1Cx8BPwFWxFAjZGBAsS4T0rVs/OtTkHuSqWrnbunp+fLxcnLauqKtahbayDCGr490KG2MQENoPx3rLnNf3dGmsk0SbAL2uMl7Ak3iQwyNtEeIHEJXG3gbmzfk068qsYe84bwB5gs9pu0ZOnsW7DUYNIwklsJCGOfUrbB3jRnEbGNCFDZTUvSgl1dgWZiR/fEiX/qpT9WPH91XUmCWIhezKYeXwo+WMhxzKYebwr+d3zLhyrfvGwFFyLM+L9q+if5SKxkCAogRnG95IzFnItg5nHwVpxjfSx+GdmAaSfwk7kOnFGvhA8kIAM40dwnjZ4cS5qpyuP5xztAqTqmzm+NVJoDxdHsHPYjgwkjF+A5Ul9B2MmmHG1LROiTsjljGvEss7IAgJmYMQ5WBchN5jJ29bWrtoAQRzFoxWr5huL6mXRaRjv4wcRv0XUwtTm+omWnqRWcvn9wg208qcWK+vjA/YWpFW/4AvtB7YJ+9Yhc29qjTxh6tZ8+tLRNqKXsFYRjsWStgjTi64XpZ11ZMV2+qdn22FRvJuzAWBqjbgsGn54hrCWrKI7ZcSgw0HMIojOlRE+MUmfnalPjduq4SdJBPlyCTFoDSG/vBc7Lfh7iJsAAAAASUVORK5CYII=");
                }

                return cloudsThunder;
            }
        }

        /// <summary>
        /// Gets an icon of a crosshair symbol.
        /// </summary>
        public static EditorIcon Crosshair
        {
            get
            {
                if (crosshair == null)
                {
                    crosshair = new LazyEditorIcon(34, 34, "iVBORw0KGgoAAAANSUhEUgAAACIAAAAiCAYAAAA6RwvCAAACEElEQVRYCe2WMVIDMQxFE2ZoKKlpoEtBzxWoqOjgEBwDrsFFlouEhnsEvbAifzXSrrPbpIhmPLalry9Za3u93u12q1OQi1NIghzOicQvMbciN0b0Zq2zxibzxhw99uOEzXpEezDs1lqLgAPfxN8EMrJLa521KO+meOyV9Myj4If/aKxRY+98FZghvgnEQJQLOzgVeBQzGA8mCZCVqLDqzAdMpvdqOUdZmcxZdZ0zWL8pgoFH1E/H+LnAp7b/8f8gAbDRXKY2HbhFXGPOfjq6iSBwgB3jwgYPkmIrZzabS9yYlc+UfpSzutCe5Tb6kfGSofIo/56zSuSpj/ixJHLi63zOf4BY/bOS+mepjmvm06LT4zzAr0kkkVTZ476tv0t8omprituolPlaxpPPgFcD46CtJQligFM/xvClUu2Rrx59n3rNVzqf8x+Yij3y4puksA++7xEYp4V/wFFVpDukutrIeMlQ3yjK/8cZM5O536zpTSg4VtaKMWiOHZQnkOu/ZuoYE2CMS49t+t8ac8b2SYReNtZXeCCVDT8X+FJcqhQw7wf/RJBVlcGWcWkl4Jn9HoGcl5UmwzhWx1SDRLBHn0UvNF8lK9HPRGDk3ZofdXrmUfArK2G2fQwP1NrHlcagOqci6cb04Nq3JhBx10ZCBTprKszRY48+o/Pqp7fk4prlW92ss8iWOJ0TidU7mYr8AjBCx1JUqeuKAAAAAElFTkSuQmCC");
                }

                return crosshair;
            }
        }

        /// <summary>
        /// Gets an icon of a cut symbol.
        /// </summary>
        public static EditorIcon Cut
        {
            get
            {
                if (cut == null)
                {
                    cut = new LazyEditorIcon(34, 34, "iVBORw0KGgoAAAANSUhEUgAAACIAAAAiCAYAAAA6RwvCAAACvElEQVRYCc2XsW4UQQyG7xBBELrQBiREdTxBJJ7geAXCOwAPQA209DSho6de0kBBDRVJAS10gAQSx//tzS/5RjszewuKYslne+zf48x4vZv5arWanQe6cB6KoIZWIXPF7P2HYveVg1xFahVyRcj3RfQ4BwUciy/WwluFXBb4pviwlqThu5dy7NTi5o1m3RX4e0pwVfJHLdmAL+Ivyf97IKZfap3IzwB8HfSxasQUi+iTcSINPpLfdCClFW//0iDJhy1c62oolo7/3Fe9/qkecYqjH34FTPNaW1dDri/iZyHpi6CX1BjzSEHt3modWfLvSEZayPAV5BKf6URKf+qSedyGPeZq/FcfSHlrQ5LTzN8PzIw/Iea29I/BLqpjrsbgd1Le2JB8EHSrT6xIvhSPKgLMNidCfJwL2NfF9BA0panXSP1ucyKAaLq7KImOk/QY9zox9bnhSMtWExX8ndZNh1JgE76NRhxjb3s1rp838lcbmbwm+1u21jS3vRonZCPmQ06sbV0ESaaeSI/Vzycxb2foVHxLnD/S+Jr0L4XkY5zNxoz/waKmXg3J4hh38qE1+6py6okslPVDIfPoaRrxY05kKUAn5u5PxNgu4r50csBuXnwU2onBwEdinrQyNZ7xp2k4MCf4FuElZsKXzwvHE0PsUsy3iImXZ47p7cHFFMzG0ELsuL1+Zb1Jf63BRwxrLpZY45yr+IHkwCHZpaTRx0YQm8X1qLuQuIYOFXF5cLQB8pkY15wQX75uG9/Qhh0OkeM2ZK1ZT9VZdwrdha9E+DzkYswNGUVcrZDnKSGvfpN1fCV6lRzx/xhwFPe4BNo4nuzYdvuDXB8zjUfH+6nAV8K6oYmlp4jtxNCkp4aNePxy4lEuFeF1FxyxixrOwJqM84PCarHR52+UTpj9Fq6fBcV7O0NHrVnPsIzZ7C9mtZ1RWZLPMAAAAABJRU5ErkJggg==");
                }

                return cut;
            }
        }

        /// <summary>
        /// Gets an icon of a day calendar symbol.
        /// </summary>
        public static EditorIcon DayCalendar
        {
            get
            {
                if (dayCalendar == null)
                {
                    dayCalendar = new LazyEditorIcon(34, 34, "iVBORw0KGgoAAAANSUhEUgAAACIAAAAiCAYAAAA6RwvCAAAB10lEQVRYCe1XsVLDMAwlHDDAxtylbJ3Ze/wCOx8EfAMbPxJ+gpEM8At0YCDo5SwQsmTHtYdwh+5Ux096kiIrTduN43iwBDlcQhGo4U8W8kiF4xyhPWlKpO9dypFtXcGMoICzQHynteMgxlriO9FLC+HkSMTXRh1T19ie8534c2dkZWQDxsnYjP2GN2IFpn2FmYyJowH5+Zf3TzDcZU44sfQdiLQlfdPkVCEv5LxWBDkjyhRtTwLyoSwo5kJhyY7IO9G82j136zvO3BlhAjrCXWHMW0t8l/OFlpqRRR3NPfUdQ4czrVXEQTxTch3BDLXsDG7mk7R4WFsWgU648Y5gbSTRXYa4bnKZt/TxZe4pXSCBTIKWW8qc5LpPR6yvfiQZRKa1uJ53iXeNowSbNuA9PoJYfLafk4+2g6Yx6q0BBswkELghxdPGomPcBMOKVm3DHhLhucfXG0C0m+dD+hwTjpfcE+kVqSXgSc7ks++wWgmAXQbDg+fg4a0LuQ6Jei+hh7c+Gv4NgzfvzklqHk1NIU6eLGwW0vposlV4DrlCoun2As3E3XipQgYKfkvqkmcmZzfEQTzEjSQ1I/i78Box6gAUsSUt+hVfl7KQnTqawlB17v+F6P59AVUGPp2eb2PcAAAAAElFTkSuQmCC");
                }

                return dayCalendar;
            }
        }

        /// <summary>
        /// Gets an icon of a download symbol.
        /// </summary>
        public static EditorIcon Download
        {
            get
            {
                if (download == null)
                {
                    download = new LazyEditorIcon(34, 34, "iVBORw0KGgoAAAANSUhEUgAAACIAAAAiCAYAAAA6RwvCAAAB0klEQVRYCe1WMVbDMAxteHQpEywsMBSm3qBsrOUIvHIHOrGxl4ETdCsLJ2Atl2AsA9wj/B8iP8dNiORnIEP03se2LMkfSXaa5Xk+6ILsdYEEOfREwkr0GUmRkSGCzIENwCsnWGI+AeKE19eACWzbZAODIWCJO8jooBT+t29K23fYnQPq4NpmZTm0JMh1DNwCI2ALkBDHBcBYu9KQwhPol8AWGAFrIEaOGpym0FdKV1mUm4vAmaRipYkI47Hf3PlhaZi6xyBvl8Hasjz+wZilzty+x6qJPcsTK/MWxxn2i6z4Gblz7KoTNl6srFscr2Xfv77qqybOicaiPH5GEsWNC/PfRF6Ftk+Er+Ffy0oO9Inci9IwPsCWMYgrg5+YPsvEPSi4Rmxcq9CHry5fYcayCK+2O99NSqX1FZXD+dbIXENGiLvz911qviefGE6Bj0CfcskSvoQB/R6RPZKRmruuLjfZ0E8Agx2UOn+4wYJ9I360J6i7ABh3hwR0pt8jtK+THEoedla3qdXVZUTjO4URCRCUMSDrWaGx/vE71zjnBysU9xHDhmtEzdxkXBOQP3BEokkgQJIeYZkOgdom1FbI//pqfX7FLrZZk5PpiYQp7UxGvgCL+u8l4VM4cgAAAABJRU5ErkJggg==");
                }

                return download;
            }
        }

        /// <summary>
        /// Gets an icon of an eject symbol.
        /// </summary>
        public static EditorIcon Eject
        {
            get
            {
                if (eject == null)
                {
                    eject = new LazyEditorIcon(34, 34, "iVBORw0KGgoAAAANSUhEUgAAACIAAAAiCAYAAAA6RwvCAAABR0lEQVRYCe1VOw7CMAylSCyMsLLABjMzGwPiClyIc3ARYGZEsMHAzgUYyntQqY1b9xMS1CFPslInjv1qu24Ux3GnDei2gQQ5BCKyEiEjISMyA1J30SMjOL1BuFrjVyIrRH5AxslK3Q6crJayxb0icL+xz8YXEKQH2RcxyOzxnHa1/Uc0boABbE8QlqIKdxjMIc8qQ543ITKF/aWOU2Ezg34Vezm1brNucNOGBAPyHu+Xo6KOzNgO4gJs4k8FsOZ6p6w0fbzCGVKnH8rfNj094HEJeaVb3yeNCIcT54MvDOHYaGKtR46+GCR++eUZ0DJiGP1D0TLyj9hGjNYTYbPyj8qx61oK/9Raj9DY5WcLdwY4/ifZHY0Is+AbUTaA1iNk7BM5/xqRNVjkjB0x43RdSF9aaaSdd13LiPfAMkAgEjIiMyD11vTIG+3mm+wXV0jTAAAAAElFTkSuQmCC");
                }

                return eject;
            }
        }

        /// <summary>
        /// Gets an icon of an eye dropper symbol.
        /// </summary>
        public static EditorIcon EyeDropper
        {
            get
            {
                if (eyeDropper == null)
                {
                    eyeDropper = new LazyEditorIcon(34, 34, "iVBORw0KGgoAAAANSUhEUgAAACIAAAAiCAYAAAA6RwvCAAABwUlEQVRYCe2Xv07DMBDGEwRL2bogIUYWJNgY2okRlWeABfECfYw+B7wDK2KBgbFiQjwCIwMM4fuCz3IuTuI0dtWhlq73x87dr3bsJHlRFNkmtJ1NgCDDFkSvxK4OJPD3kPPY5P2A/vXViLU0YySfeApcI/YDeTdCewapN+6agTLG9dLmMCTfSIIefeKMK8fLRUP0kSokMNRN7R4dlZoVR3f28Geq4kL52v3UuWOBMI+G0cVdP9mMyB9qWw4XJMk9IhC8aTnlbTDsn0DkGqut4evsERMI7hTm9C2TF0BqxDhHeIa8QU4h3+aAeIS+MraoqRheLUQraj0T7gz7zhHZ2u640q4FegB1QfB+4Bi9TF6YVUFCISS/hpG41dZIMBM6N2FklnRfVgt0APWdieD8wQMBmAyCfz4UJClEKEhyiBCQtUB0gawNgiB5SeM9czO+5rE9/Cv7O4K1hJxDvmx0oNH2rDlD7juVXyD4XIkGwRpNMzJH3wuEb9+XkFvIAeQZ4j7c4MZpTSD8Ds1NiSfoC2PvQ8sT1oTiqCaQhZP+EPYNhGDJPpSbQMgh3ymvdFK3NpDUtSv523ZNZWBqZwuiZ/gPUIxOgMDamNQAAAAASUVORK5CYII=");
                }

                return eyeDropper;
            }
        }

        /// <summary>
        /// Gets an icon of a female symbol.
        /// </summary>
        public static EditorIcon Female
        {
            get
            {
                if (female == null)
                {
                    female = new LazyEditorIcon(34, 34, "iVBORw0KGgoAAAANSUhEUgAAACIAAAAiCAYAAAA6RwvCAAAB1ElEQVRYCe1VO04DMRDNIkJBhyhooICOE0BJGxouEDpailBRUdBxh3RBnCOUFByAJhwAwQFAYnkPeRRr48+MTUGxT5rdtf1mPJ6ZHTdt2w7+A9YqnRhDnydZQEZVthiRQhlDr4tjTBTZa6hYiJDiI2ydlNirTU3JnkGdGkfOAxavA3O6qdKcOj2pkwXGoxpbNTXSRI4aqp0IdTldkpoJ1LnZd0T4Kw8hNhjDuQm+BhOQTL+xNTVsYDPlURltdZqsqblVOkHakYE7sDiyC8P7kFfIdmKTPbf2kOCsLFkcuXTaN3jvrFhaTnzgk87SaTqvg7KoWEsCFuxMBoE37xsKcQdRFa2KBGOHtArMIcPfr/iDTvqOk5/dJ0twRiQCPKl0U3xGQbuMBkF+dh/t7yu/4QYS/gJh/lOQYn4HifVykCJzbT1HcOu84J4hLNKcE1Q5hdxDriBvkDw0YfM4Em5MJTHHajYdPkebGp6ogfB+0YKtQVKa1dGmhobYKZlvIpUe4WyBx56igiUivkH/pCzgT2+RkTPD0lljxr9iC5b5v3DEsl+U2zvSDU0fkT4i3Qh0x7U1Il20a9c8trR43zhv1TPI1E3ydr6APLmx+VXa4s0b5RR+AACWRll6OMHxAAAAAElFTkSuQmCC");
                }

                return female;
            }
        }

        /// <summary>
        /// Gets an icon of a file symbol.
        /// </summary>
        public static EditorIcon File
        {
            get
            {
                if (file == null)
                {
                    file = new LazyEditorIcon(34, 34, "iVBORw0KGgoAAAANSUhEUgAAACIAAAAiCAYAAAA6RwvCAAAA4UlEQVRYCe2XMQoCQQxFHUvt7C3cM9h4BS/sFTyFFtaWazv+D7IMIcZlmMgsJBCYzJrk8ZPCSTnnVQ+27gGCDIsA2QH0BufsWjhrsaZqydgRJh7UrPrLO1IHLd0C8dripIEsYkc0cLe7UERKG4qEIlIBGceOhCJSARnHjoQiUgEZx478S5EtGvEvYel72byMvUbzKpvgvIE/4GdxP4VeIFMDHAgxwglxgavmDTILgmSeILMhfoHwMVRrfNFxHCf413GUxS1FjvhhLczzA3Etm1ln66Vn5TX/ZinSvJlVsBuQN+7dMTMS/PxJAAAAAElFTkSuQmCC");
                }

                return file;
            }
        }

        /// <summary>
        /// Gets an icon of a file cabinet symbol.
        /// </summary>
        public static EditorIcon FileCabinet
        {
            get
            {
                if (fileCabinet == null)
                {
                    fileCabinet = new LazyEditorIcon(34, 34, "iVBORw0KGgoAAAANSUhEUgAAACIAAAAiCAYAAAA6RwvCAAABPUlEQVRYCe1XwQ3CMAxsEPAoK/DhyZ8lGIcx2IpJ4MEM8IBH8aE0ct0kJE2QUimWTN2rfXbc1C2q67qmBFmUUARqqIXIOzG7juxpBVdS7OxYRRzi/YKnxqMrunYmzSHgAZ81nxXUzscc2S0c4B3lHQHk1JJeLAQ5IfAjj8lvDA3+qwuuRZju8EJOLu8/48jbKPxo6Y0NnT97cMJRiZieV8DNloC7BpWtEBC5giXZmoA3A1uyH+zcZ/I8aunzDLj2CvAJcpndZLWtCnsJ7ZUKPFqK6UjqZo1eOQsYbNZiOuJ6alDtVJGxQaPAVUhQMFWaOkfMYl2FGIcfRp0jvEF1jvBu5LDnM0fwKpezIEcHJAfyGOGP743QHWno94QhSTSQd/CX80DnXzCROCYc+ZB38KkYQ5Ddt5iXXi1E3tsPabGwy0aUhy0AAAAASUVORK5CYII=");
                }

                return fileCabinet;
            }
        }

        /// <summary>
        /// Gets an icon of a finnish banner symbol.
        /// </summary>
        public static EditorIcon FinnishBanner
        {
            get
            {
                if (finnishBanner == null)
                {
                    finnishBanner = new LazyEditorIcon(34, 34, "iVBORw0KGgoAAAANSUhEUgAAACIAAAAiCAYAAAA6RwvCAAABu0lEQVRYCe2Wv0oDQRDG70RFEBuDIIKgICiWWomF1j6Dz6HPEHvfwGewlZTWNnYBRdFCRAQVFePvy+0km/X2UKPHFTfw3fzZ2dnd72YvSTudTlIFGanCJrSHeiPhmxj1Ak3sPee30RfgFNyAc3APHsAteAFv4B1IwkZLian2GJgGU2ABrIA1sAkWgeQA7CdqVgdUT3Z71v8YYf1ojxx399p/bGCu9t0fWa2c7LB+knJgy+sZBESt74/jv7pENfiHs33l5/jxvPywflp0a45ctXm0+kFacpKprt/ANqZso5PEFBeLJppr862ujXW136wDAzjXLnCJ1gmkJdtAbF0BMWOHsZM/E9sCh0CiXM0VYxKrm3nuaUUGgs6Zywt+IybGxMiyy9UhfMmtW8SIrliR2Fxft5mwAybAkpts47rKkty6sWaNNV5WavhnWD/arOvDr1VY4Uv9GCP62knOwB2wr+oj9hP47Ze1wdwZMAvsK46ZpLGNhA2m5L8W3SaT6KuxhNJ00fUtbRNaqN5ISHfNSM1IyEDo1z1SMxIyEPqV7BH9RLeAdBkysJ7/N6CMxaNrVPLVRHdbxkBlGPkER33Tw2Qcr7MAAAAASUVORK5CYII=");
                }

                return finnishBanner;
            }
        }

        /// <summary>
        /// Gets an icon of a flag symbol.
        /// </summary>
        public static EditorIcon Flag
        {
            get
            {
                if (flag == null)
                {
                    flag = new LazyEditorIcon(34, 34, "iVBORw0KGgoAAAANSUhEUgAAACIAAAAiCAYAAAA6RwvCAAABiElEQVRYCe2XsUoEMRCGXVELWxFEsLKy1kKxsPcdrK1tfId7DksfwPbstLCyFQ/FQhEsLbRYv/9wl1yY0eH2btliB36STCaTP5PZCVuUZbnQBVnsAglx6AyRJSciA/Tn2dyI8TO4BffgAbyAN/ANmolyxACqWs7q3t+dVaYtXyFd5Goug0c9DdqZZoVOYUiqXGH+y7CxVCco78AT+LQMPF2ESMHilJjny9Jfo1ROvYJ3sA42wBBcgVqiRIasOKpXNeuMWL4NJg4XyRFtq1PNSvZwNEFCjqNEZkXiAEcflrMoEd1xUznGwY3nxCtoub0SrIkoEi4JOY4S2ZyShRJTOWFeR+ozejWH6aJgX1ehr+NfEmN/TllOi/lyOvjtX9DuA5X1cQmgfQQDsJPoQuUde74j+31AXYscS7SRNq829tZOpY/kyC6h2wJ6aecm0co6NwKV42iyVvZza3sieWj7iPQRySOQj/sc6SOSRyAfezmyhqF+BdS2It6j18rm6SZeRFKbVvqdIfIDAlyEQJxw5bgAAAAASUVORK5CYII=");
                }

                return flag;
            }
        }

        /// <summary>
        /// Gets an icon of a flag finnish symbol.
        /// </summary>
        public static EditorIcon FlagFinnish
        {
            get
            {
                if (flagFinnish == null)
                {
                    flagFinnish = new LazyEditorIcon(34, 34, "iVBORw0KGgoAAAANSUhEUgAAACIAAAAiCAYAAAA6RwvCAAABy0lEQVRYCe1WO07EMBAlCBoot6GhQKKipIErcAck6Kih4BD0nIBLUHIBaGgoEFkBQlAiREUR3tv1i6yR7bURllJkpBfPjOfnieO46bpuaQi0PIQiWMNgCllJdOQAc5fAlmfTgn8GXoA74AGYAh/AF/AD/I24RyKAuqfTnlvMrMEkFjOqz301126ZDUZCZGXqTzRpxlXI+0bXiw2rj5A/sQ6bb0BFaM7KCnUB5h54BD6BY+AcIMlnLrlnbiF0VnK6Kph0vmxtXap+kG2vIJP7amjb8lFITBpMbOOUFMKvRYHZCSIkx/TJgkoK4SdbjVLniE36BgVXS9LqQjLnQvqZY+xR0pH3WJD/0Jd0hPlSndC89gjl7M6UdGSDkWtRaUdiK/Q7VX2P7NXqBuOWnqz+ymf+rji/U4v2iGI41/mQ+2r4wwqRCtCclaXX2IqxY+5m3XaOTKRkDHoG7AD8KTKWVis7yoRksBFK3B38m8eNE54wHgKpO4fvZ/krKIJ3ktxXw+N9E3iNrMdXTyAcAbsAT2NeB24BXgmiN7iSzYo49Sh3j9SrwEUeC7EtHjsydsR2wMqpPcKDicc4x+qUOtCqJ/cTpDri21XnB1PIL4xuRRo2RIRgAAAAAElFTkSuQmCC");
                }

                return flagFinnish;
            }
        }

        /// <summary>
        /// Gets an icon of a folder symbol.
        /// </summary>
        public static EditorIcon Folder
        {
            get
            {
                if (folder == null)
                {
                    folder = new LazyEditorIcon(34, 34, "iVBORw0KGgoAAAANSUhEUgAAACIAAAAiCAYAAAA6RwvCAAABf0lEQVRYCe1WPS8FURD15PEXNBqtRC0qWpU/4A/R+glEo9CpqURBo5CIkLyIUIlIFBRrZt89m9ljcvEy+7LFvcnufL6Zk7n3nn2Dqqpm+rBm+wBCMRQgvBNlImUiPAG2+YycSoISS+7Z4SIR9oAI7a/sNi/NvyIAoAZP5ACBX+SnxO8zz78nOqSGT2TnzKVcMBN79mIM5NpJWhbfjeMPdfHW3FH1swRiU2Ru3JPGttGPgTwikORFkifkjzL3UYiBfCCQpG7VHPkizeZyMJB36nIp9gL5Is1DFGMgzA0vkriI5A6knsF6MRB1PoxD9ftV3mvGjlS1T3MUPCAj001vw5axI9U9W8wDgpuCg7RufxCoH9laHhCQ2pUkdnljWlThAQGpnQuQrm7Mrp2G6h6Qt5R0K3Il6dHimAt6QPTK6lJO2VClg6X81FoeEJCacspqKzvGUO5gvnK3RpN+7GEMhpqjmg+drcn/0Gxsqrq3NVMFgGYFCCYBWSaCSUD2ZiLfi51kItisTFwAAAAASUVORK5CYII=");
                }

                return folder;
            }
        }

        /// <summary>
        /// Gets an icon of a folder back symbol.
        /// </summary>
        public static EditorIcon FolderBack
        {
            get
            {
                if (folderBack == null)
                {
                    folderBack = new LazyEditorIcon(34, 34, "iVBORw0KGgoAAAANSUhEUgAAACIAAAAiCAYAAAA6RwvCAAABTUlEQVRYCe1VsW7CMBBtOnTpxtyFbnxKP4cf4QsYKxYGNubCB3ToDkF0rNoFiQWJcK/iRZeTLStRbGWwJescP9+9lzvrXFRV9TCE8TgEEdCQhdhK5IzkjNgM2O/QHZmIw14mmo2e2HuR2dsoAg0NhGMPWyn7rx6s9XZISKjtFq0ZPQ6h0njc6m1dri7rN0ZyCcFfTnkgsl0zvhWCC7iTOeOByHbD+FbIVgDf5aRPn/abwayQJYFE9pM8VsiKQCL7Qx4r5ItAIvtLHivkTCCRPZDHCsH+O8EE9kQOl5AFwQT2jxyuFj8SELVDYwu1eMbpap/E8QJnlxAIuAKTGVsIOP6HqzQg39zxmKbUwV1CgM/1oUjro47rE/JxP9RQrR17WDd+1nVHwMH7gWe6fiF7INchnuWj7ls+IdohydpXmiTkmiQL0dnAOmdksBm5AfZbS8A8bu9WAAAAAElFTkSuQmCC");
                }

                return folderBack;
            }
        }

        /// <summary>
        /// Gets an icon of a gKey symbol.
        /// </summary>
        public static EditorIcon GKey
        {
            get
            {
                if (gKey == null)
                {
                    gKey = new LazyEditorIcon(34, 34, "iVBORw0KGgoAAAANSUhEUgAAACIAAAAiCAYAAAA6RwvCAAACzUlEQVRYCc2XoW9UQRDGOUIRdVQXkgZVggdXCQeCIDCkCQaJKQYU/hD8BbiikSS4w2GwgIBWkGAIkgpIOL7fY6f53r59r3v3EEzydWdmZ2dnZ3bn9SaLxeLU/0CnRwaxpvV7woHAicBMQL8UTUZkhM1+9ux2KP1FoTrdYzLyoCcI1FvC9YH5ztSYQG6ZtxfizwoT4WHSP0pj1TCmNHPtsJN2IYhfid/Q+D3xBFZFYzLyJe3wRmMEgeqe6RN78jAmkK/JfQSEyAt6lvTP01g38GpWxFWtgw6ENWEfwQhdte8xd4T6/+457iXpP/TMFdVjSnNOHrkfOd2QYqkgGgfLpC/ZkvKZUKJNKavL4bZn8uMMyJSCJvWqx4bsVnfS3Edtaba18LOQB0Eje5o7XUWuuay7crxvzg/F3xQ+CmRgXfgh0FG/CUGfxLwTvMfEXHf0OhV4au60J8HvwK5P9vDY+JoiX1TaQr+U9Am3z/tGTxyN+sRg3HGJn5v3DQtkmunXTabRMU+jc8KmtEej651Ii9yZ24aeDUMfGXIdpQwazEo46RvDORu7TTj3U0bL9xLyGILmYtxHi28JBUMvgW8aAfopaXRBzWuU8M8CceecKDbwO+EvKUq2KVsOuS0EuV0nAR2FVuW6SDkO2Qjn2BAUvGdqJhliUw8W3eDXuDlht7t0NHTW95mWrvo6092XvJPpaICXhaNM3xJrA2ER35orwm3hjtBHW2mCL/Nj4W2fYUuvlOWlGCtzgaGpUO2r9qPnwZOZqcD3hx9WjOiCKBl09+9Q+XeZqGXLhYuXIfaY8hfB8857z2B2BiflLJ/3F3QcRWLcdl7Q+XyHX6U0pVxHOZjjdw0vh9dST4VTd6I1GxpVTpQgegRjZIPsDflqzbWEyoXRtGTeEDIvxfXIS/lepo94mkk//61fE84LFwToifBSGGxeGOa0aiC5n9HyH1ePw2zCAsqTAAAAAElFTkSuQmCC");
                }

                return gKey;
            }
        }

        /// <summary>
        /// Gets an icon of a globe symbol.
        /// </summary>
        public static EditorIcon Globe
        {
            get
            {
                if (globe == null)
                {
                    globe = new LazyEditorIcon(34, 34, "iVBORw0KGgoAAAANSUhEUgAAACIAAAAiCAYAAAA6RwvCAAAC/0lEQVRYCdWXO3ITQRCGLapIHJrUBISKSchIEWeQD0HCHXQAMjJTJByCyShfwncgISGQ/281/1Srd0erJXCJrmpNv5+zWmm13++vLgFeXEIR1PBfF3KtBjbCe+GjkN2C0Mi2wlvhMuCOnIm3sivCDI9ZEPiN6JVwNsesgYK8FN4LI2zFIMefRFkvUQMKpYmTuU4q5XzTwh2Sfap87JKipqBI+C4ooLv5ugo5XYcgjJiuHNhBo00wH0gKJD5+BvuN8o4E8kBGx979WjRIYBeDjiKQR2BFnhp6YtFEBGKM8o4E1cg7Jwg2rMiB4V1AEW3Alo5dCLqdleGkibha4um5G1fnUVJM1lMMes4MLt5TcUHYuSBPmaKPYh8xVVnwFPipyDYkcsDBMH1sxduHSbp7GnCTuNhmOI8YKd0p3WQdPMWdAoogWfTFJ67Vkzqyiw7QjAygk6zDsaDsALroQ0KmgR/g5jI/+ERHaO8ZYzrZVlzrBOeAy4pdqYY7nU6MyKuCxqblb0QVevcE2wjNc1LUvwJJiel8jmt+9NQ4EYa+L5bNnSTzBWcl+DMRpuwLSzOeushW2NVgEF6T8VfSxyp/pbMIeaP+EvbgTopvPWWVr3T+FL4P/IGMVYn2yKiWbtroKk3HjBhdho0E2b7H27fpG1GDxLF13wvVNhczZ+9cXnmJhedfaN/ryDh+BzqTjPhzEp6yj6Zey9cozIU8BOWHQGdySvc2Gd2IXycZ7JcqK/U8HHE8lWZkhqm9c8HjXbItJ+uK60XmJ4nVrBEI8PeqhvOIqUob4wAUYbQj2RJwIbGB0X2KCSJN8gj+yifoUnBcN0Bsy9rZiKScSujOemuZKtAriN/K8QXY8ufL6vvzVwRfZBF8GX9E4Qz9RvqdkL8ZwGvhn4HKH2kSrcIqjy8suqObJROReQPuXo7f+EacMCJ5aeGWE/hOrkPylr8RUdihuelLpoHt6OnoxB69fc8pjCeIy0enGZChY6XnxGo2+e2br9Cz8b2n5tkKcKKLKeQJBGmCOv5+pBkAAAAASUVORK5CYII=");
                }

                return globe;
            }
        }

        /// <summary>
        /// Gets an icon of a grid blocks symbol.
        /// </summary>
        public static EditorIcon GridBlocks
        {
            get
            {
                if (gridBlocks == null)
                {
                    gridBlocks = new LazyEditorIcon(34, 34, "iVBORw0KGgoAAAANSUhEUgAAACIAAAAiCAYAAAA6RwvCAAAA30lEQVRYCe1UQQ6EMAi0+wT//zb3KV3GiMHBtT3QxgMkjVAQJlOg1FqXN8jnDSCAIYHwSyQjyQgzwDb3yCoBmxwsF3twB59KdJxUk4Vmzib6P4FPY6PjloLkRi6GuVe1HEp0XC40Zfj8crOejtkKA/k+ALA+q/Mv1mf1pzg3Nas0791E4A4+nZroODc1jHqazU8zrTAXYiDRG7M3n+uRu/6Q1tgFPu2R6DjXI5UpIzs3KxEyzuRmHVepkZmB9G7C6Dg3NdEbszefm5oGgePc/DTjKjUyJxAmKBlJRpgBtn/C/yq3Hc4+jQAAAABJRU5ErkJggg==");
                }

                return gridBlocks;
            }
        }

        /// <summary>
        /// Gets an icon of a grid image text symbol.
        /// </summary>
        public static EditorIcon GridImageText
        {
            get
            {
                if (gridImageText == null)
                {
                    gridImageText = new LazyEditorIcon(34, 34, "iVBORw0KGgoAAAANSUhEUgAAACIAAAAiCAYAAAA6RwvCAAAAyElEQVRYCe1VwQ2DQAwrlViADZii367XOViEPlmCjnK1pcIjQqSBPE7IkSx0uhj5jMk1pZRbDXWvQQQ1SIj9EnJEjlgH7FoZqd6RDgpngPPew4ieFsgt3jXADETqhWbyiCFC3Oglfx3xffB4j2C/295QDSp6Bb/BeZKYVZf6fQe44gV8b5/8NSNZDh9+jzJircsIa2pGPlahs56c/fg25wjQAf9O1xG97Y+3TNfTzyWs8RMkMzIykiJJQqyNckSOWAfsWhmp1pEvAbMvzbnmmXsAAAAASUVORK5CYII=");
                }

                return gridImageText;
            }
        }

        /// <summary>
        /// Gets an icon of a grid image text list symbol.
        /// </summary>
        public static EditorIcon GridImageTextList
        {
            get
            {
                if (gridImageTextList == null)
                {
                    gridImageTextList = new LazyEditorIcon(34, 34, "iVBORw0KGgoAAAANSUhEUgAAACIAAAAiCAYAAAA6RwvCAAAAt0lEQVRYCe1U2w2AMAi0juAG7uDGruMMuoErVO7DRwjB1PpozJGQ0F4h1yslxBirEqwugQQ4kIh+CSpCRbQCem31SCOHRnEMmKNjD9hqvQRHPDVG/mYWkUHQdjuxB9gD9ogFY7LiZp4FD7yKWYpcrZWV9wsijzfr5GjsYU7aOWQ1K76o9XNAohOfz8umn7CIpFe5IaPoZuVk1S/MyaoV+WSd82s4WTlZX23anGa9lSiJaDmpSLGKLK+RcO0wwy5eAAAAAElFTkSuQmCC");
                }

                return gridImageTextList;
            }
        }

        /// <summary>
        /// Gets an icon of a grid layout symbol.
        /// </summary>
        public static EditorIcon GridLayout
        {
            get
            {
                if (gridLayout == null)
                {
                    gridLayout = new LazyEditorIcon(34, 34, "iVBORw0KGgoAAAANSUhEUgAAACIAAAAiCAYAAAA6RwvCAAAA90lEQVRYCe1WWxKDMAhsegRv0J6h9z+KPUrKTpWJmxhR8uEHzKASYIObZ8o5P+4gzzsUgRqiEB6JYMTCyCRBsyiWEyva4Yf04jivtEuMPxKeWL6ks9g9gR85R3EWDO07AZSkaiA/zCRqiWukahMwVGKyKhXLh4eRL4OdsKtcTyEf6bgCNBSDHORuxDNZN0Bew8PI0H3Ewwg2ptdFJjA87zLXU0gugS58xz7SJc0zWbvAZ51RCDMWjAQjzADbrTlydKKu/vXNmBa7zl3un3p3FHsS3buPoh1+xPfixL0rJYb22zprLH80PKY1NMM7sQBGIcxSMMKM/AADPTxhHRDN1wAAAABJRU5ErkJggg==");
                }

                return gridLayout;
            }
        }

        /// <summary>
        /// Gets an icon of a hamburger menu symbol.
        /// </summary>
        public static EditorIcon HamburgerMenu
        {
            get
            {
                if (hamburgerMenu == null)
                {
                    hamburgerMenu = new LazyEditorIcon(34, 34, "iVBORw0KGgoAAAANSUhEUgAAACIAAAAiCAYAAAA6RwvCAAAAlElEQVRYCe2UQQ6AIAwExd/p//+DcsNp44mSPSyJiRsr3Qx0W+/9UFingonhwUZ4EiZiIiRAnd2R6y0a4VL5jB6f1ZJA25VwbXaSEbnngqL30CMjUtT7f9uMyP8fRV9thGCliThHeFxhxlmwQIcezhFSlZ4amt2ipYk4R3gHwoyzYIEOPZwjpCo9NTS7RZsIMcsQeQDriGNTE+1BhgAAAABJRU5ErkJggg==");
                }

                return hamburgerMenu;
            }
        }

        /// <summary>
        /// Gets an icon of a house symbol.
        /// </summary>
        public static EditorIcon House
        {
            get
            {
                if (house == null)
                {
                    house = new LazyEditorIcon(34, 34, "iVBORw0KGgoAAAANSUhEUgAAACIAAAAiCAYAAAA6RwvCAAABhklEQVRYCe1Wu23DQAy1ArhxuqR1CpfpU6RKnx2yhMdImRmSQTyAZ7CLpM4Ikd8zRIEijjxLOgFGIALEfUg+PvF4B1V1XS+uQW6ugQQ5zETsSZSoSGVBh6wjImsAHhrlPCVv2PyDchwnvDWOHrAvwrn2W2G9E2Mzcs197XfxPHI0edoEr9Zg1rRHuElbcrMBEvwdJvzSJfRTNjMj/egf4XdsnYUJZC75umcuBsgjYqIcra2dJAJYhQr6Dh0jjCdOlGtxdnDanTfl27EN2X5A0I8XmLq+fBe20JIkmJ94xE2+O7Yid3DcQzfQqeQI4Cfor05gK/IB45QkmJv4/NiuOE3ESm2hJYV4btPao9EseZZ8vksJq+/+/Nij0UndIO3UYx7iRUR65BjvOhOxNSxVkXsAs7m13tpk0Tq6NYwLG0wBk0BKdLznc44rVZEUiV57/5KI/N/2qoQ4zz0ilZAx1yNHcZx6zBF5AYEcma+AJG2M599ZKLkeCYNLGnMVKZkrxJqJ2PKcABWQ5US/h6OAAAAAAElFTkSuQmCC");
                }

                return house;
            }
        }

        /// <summary>
        /// Gets an icon of an image symbol.
        /// </summary>
        public static EditorIcon Image
        {
            get
            {
                if (image == null)
                {
                    image = new LazyEditorIcon(34, 34, "iVBORw0KGgoAAAANSUhEUgAAACIAAAAiCAYAAAA6RwvCAAABtklEQVRYCe1WPVrDMAxt+Jhg68wCW2/AxlyuAIdojxEGLgFnYO4BuAJwAtau4b1UDrKjxG7auBmq71P9o2frWZKdFlVVzaYgF1MgQQ5nImEmJheRAgxLKCs3p9InfcMrbg30DXoqKeF4VvCHfPhzQikuDee7UBmGI095h59csfYddgnjRnTRBzzIxhoJqtQVMNtlYONwAdWYrj5xNz1YmBppbk0zg47e+FsbpM8bpjFW/0qwXF9fCGONQOpmtCf+Q9J0i/YpKWXC1GMnczyplZp7ZSdmDtVzJcahEBNGTmOiqXFkGGKqdkibSwE3ZU3QbgnXHkwk3MCNmXs6SJVnAN1atlrMlzX1Qdsg9w9J+f8HXaO7laF+0IqhDxo/VvuSoH9XxMJFNYhPK0wy58IYFuNax3RAnxeg5dP66OnUsP8F5TV8FP7dpxJApPmB/Q7qpSZGZI0Fr5GNh5h5wGQic4B/h3hJWNMi0lWsBH4mbHg0SBeRFTywLrJJrEbGJOLViPUPTQPGJOLt7VLz7s3mHbzU7uRxYYqsr+aA92qvJfRZl4erkbwxMLy51BimvFNnImG8JxORP7jBGEfTcxZzAAAAAElFTkSuQmCC");
                }

                return image;
            }
        }

        /// <summary>
        /// Gets an icon of an image collection symbol.
        /// </summary>
        public static EditorIcon ImageCollection
        {
            get
            {
                if (imageCollection == null)
                {
                    imageCollection = new LazyEditorIcon(34, 34, "iVBORw0KGgoAAAANSUhEUgAAACIAAAAiCAYAAAA6RwvCAAAB/UlEQVRYCe1WsU7DQAxtEGXogsRcMbCVL2BjLt/ALzCUiV8oKztL2diZs/EHSCz0P0CivHe6FznXu0sVJSVDLbn22Y7tPt9dUmw2m9EQ6GgITbCHQyPhJA6I5BAp4FyCeYz6YuZnnW3i8fW8gtwHLVFENStZ0OhJSqxj2uRXfBupHFs1mjbrBNVK8C94BR6Du6L6mAxMGksFFwyljF5GYYXPPpPTlY4xpCpf02gEpVBYQ7nQooVUPjtqN6am0bwExZ6CdWw5g3Eac2RtgEdQOqzMmvYxmPB9gRdghyCkngnlBD4S42OxzokfPidyOWyimgML69tVL5UEspq/ySU384lcblug5sDC+mL6GWKuTBwLhzSDwT4rP20i52/arKmx8hh/e+cl5Cn43a9DcQLDjze23qxhUq65yz+N4wN6qgmGPZvYtAp8BF0NKmOXX7LNq2Du89kaVse9nW9kCr/dBzw5bYiniLVEoT7K7RG7D248pm9pbBs94SVm90u2kRKprxvT7x6QbeQ4kecW9nPwOuHv3JwbTefFkNCOw+q1r/hHX5kBfTFLhO8vV9YiQsMCfOc8/fy8Iu0DWH+UVdzbN2yEjn3QHEV0Al0jqc8Afj2p6z6kmriv/jVuGHupSSdSsZeYLqQuJPOrXu0eqZr7DyU1mr33cmgkhHwwiPwBfvo842eVW80AAAAASUVORK5CYII=");
                }

                return imageCollection;
            }
        }

        /// <summary>
        /// Gets an icon of an info symbol.
        /// </summary>
        public static EditorIcon Info
        {
            get
            {
                if (info == null)
                {
                    info = new LazyEditorIcon(34, 34, "iVBORw0KGgoAAAANSUhEUgAAACIAAAAiCAYAAAA6RwvCAAACRklEQVRYCdVXsU7DMBBtEGXoWFYYGMsXIDHA2l9A8BOM/AGs7AwUsbAjsaUDAzuCrWxIDLCxwFDes+Lo7NrOOQSpWLrk4nu+e3XuLm4xn897yzBWloEEOfxrIgP8gDFkAplB+G4p1DlHGzF5gzmilA3gSoh2EDuCqPxrQH04m2ijB3BcSx/JWAUBiTGE7T1hzzGtA/wRW5Aiwvf8GVvYcj5KJlY1BQI9tgyWWsbd7YcAMSKXAG+FFkTmppinaMZFEBRIIlZHzhgDbBNRm9QL1WQdyHuZwwLYoSBCUprBGDImOpE7MdB48TCneGbSc+3Ms6Ue5Q9YIKL9RakAWhtj1RvhJ+t+MJEWJ48xtQlZg9DHESR3HDgLJCvo2q0dAbsDMX2o8qFNVMDNYKx6R1YdVvqSfarWbeP+XOmvnq+mR6c9+J012e8Dnvlqvqt5fn0d5wG8P1XYCT9HXqxBcT8DxpIYQc8l4cTyidwoCFjIuVVwPxG6Vr2XQJ9IKY0N+puwHwpdq95JoE9kKo0Z+lUG1kJvrWLusoQqvTTF1XyxHZUlyINPTjN0Shdre37VkBwTz5anIfsHF1n2xr3/ajjJvtBmq41DxYWv3/aeGh7aERp5ePmqUd0qsvfUnkM7QiP7A491XQ/6tL3H8R0jQhAPul2Soa/o4TlFxJLhVv4mZ5gT9BElwUD1148l1CD84mpLG1CD5Zomv8YeS1ZDMnLh34w9CM8TuxD7jeG3g237GvIASe8AAHK0ISLXd6Y35UhngZocLQ2RHyEnjqITA9AGAAAAAElFTkSuQmCC");
                }

                return info;
            }
        }

        /// <summary>
        /// Gets an icon of a letter symbol.
        /// </summary>
        public static EditorIcon Letter
        {
            get
            {
                if (letter == null)
                {
                    letter = new LazyEditorIcon(34, 34, "iVBORw0KGgoAAAANSUhEUgAAACIAAAAiCAYAAAA6RwvCAAAB0ElEQVRYCe2WMU4DQQxFEyQa6KANB8gNqOhX1HRcgYJj5BxcZG9BFwq4x/JfNI7mm9kE0EZKsZacGdvf/o5ndpPlMAyLc5CLc2iCHuZG8knME5knkieQ7XxHlgJspbxcTqXUh8ckNwL5oyGmNx5U8sdbNDfSCfQuvZV+SKcU6l1Lv6TwuPCKr1Tb4U26lF5Ke+kUEjWpyx6peTUjd+wQ+thKr0rsNZz/XMmHh3rUDTFuMwJRrWvtwdxXvr9sySOfOlmM24yMLPazVnA3I/GWm28OnryuBSixPf9+UwIjOcNGgd/em15Y7hd48sbEuM0Yyyj+OOtDBNEwdY/dLeM240gjjJoLt5aS10lrwcYPjokcO0rjNkPJY7JSgOJx64MUPxLNxaUGBz7iYLIYtxkZWWxIOI5oImDxRESNaCLi4MkjvyWRt1vNaKD55hTrGzFc0UxuIuDkwdGKG7cZkV3WeGzjTZjCexOyQ7JREB7q1WLcZlSoSGadQqJe/SQZtxmFsdeKv06aopk8YeM2Q2wcA7e9m4K5UYO63Dl4jBun/xwvFis5PrNzQvtOtfgrYJL/jxB8MsT0xkurZGsiLdzJfa2JnJy0RTA3kqcyT+RsJ/INMj3gxpxldCsAAAAASUVORK5CYII=");
                }

                return letter;
            }
        }

        /// <summary>
        /// Gets an icon of a light bulb symbol.
        /// </summary>
        public static EditorIcon LightBulb
        {
            get
            {
                if (lightBulb == null)
                {
                    lightBulb = new LazyEditorIcon(34, 34, "iVBORw0KGgoAAAANSUhEUgAAACIAAAAiCAYAAAA6RwvCAAACF0lEQVRYCe1WPUsDQRBNJBFRbGIbC0EQLawjFjY2sREsxX9gFRt/g/4KQfF3JJVYW1gZ8aPVRrARPN+LtzI37t3ObpoUGXjZndmZ2Xeze5OrZ1lWmwSZmQQS5DAlok8itSItJOoBfYCXbJjPuxibQLzwskagCd9LICRHcIjJi8exB5DEMMRArJ9F5I4i0hebWKddK5k6HQ3Sgc+Nwc/nMgvjl29B2qyX9VoGRc53Lf4WInwLVizJSnwOS+wFs4XIYiGiVtuCXgeWgRPgEaBw3BjNij/bRdWvWe4Ie8abCOeGawBLfgeQ6BNAmQOk78iIHxKvlEbl6u/ih/LhMa0Cp8COWGN1fDLwGf/ZjK+X7h98ldcBKW2piLmpuVkbGvuBFm7segvHjnbI9XmMwX2CDiKJrgpbvasKR1/r74n4yr0qF1USX+n5tH2Ao0/4t2Daw/L6unv1ismVU/JxE+MecKDsVM+BYEf9i7Myzv1a6rFZeoo+Ntroa6oG/WIqQvLvgKzKPvQlLihhNehrFktD08naMLwII4noJrYA26fwCU5TiLBLfgcyBzupjm9og0HPch8eq5u7MBJ4cErMGHtHZG5WhUQkQpWS8YX5OEQYywpIJOdLDgSBsoo8Fx7VqIxDhLGyGpwn50sNHGDTsorcGotQcEt5fV0CflAfA/xSo5DABXAPRMs4RKI3qwpIPZqqnElrUyK6bD8UVdMzewZRyQAAAABJRU5ErkJggg==");
                }

                return lightBulb;
            }
        }

        /// <summary>
        /// Gets an icon of a link symbol.
        /// </summary>
        public static EditorIcon Link
        {
            get
            {
                if (link == null)
                {
                    link = new LazyEditorIcon(34, 34, "iVBORw0KGgoAAAANSUhEUgAAACIAAAAiCAYAAAA6RwvCAAACMUlEQVRYCc1VMVLDQAxMGGhClzofSEFDRUfJwBOYfAJKHsBM6HlBeASl+QglP4CGwuw6kWct353P2JmxZpTTSTppT1LO87IsZ1OgkymAIIbJADk9UkWWiHsHvgAX4HdwegY4IyPzBvE8fUKxAEdzzWkckTaItUvEO4ftJ2Qfc0Y8iA8kfHFJv7FfON1+mypXD5tvx4OcPYPM1ii12hTtGU7l2jwI7v3ZEBjqar9aUGUPeQ1fpRAIy8HESg3foTPyJP1+g0yO0S8Mj2K8EXnwg8YBNXo2IbFeiu1LZLwy0qd/yMWh1hxGlv4KTN0K7GPTpsS21j61oMoeMoMZiFvIRtRpbA9i6+yDK8JkfBQVBLalDmInCPhnA1kegvN9YODqRWYAsAdBH9rZHspKrUrAWFVOyxeTeTtPOyhSIGj3FAUBx86KhECEZoJJrRK9QXQB8SCKQzL+O0LtsIquYSdYElf6mi26xgweBG9rvikQ5tN7DR3wILg3Pw6hkrVjC6UOsPlnr96RZVVSEOZrM2AgbJ/VAgS3OI21sYFTISiYwNttb19ObVPK385FV//Ru5b3/1VkL/IDRrrfL9Vv89shhizR3bqQirDv0RvA5l9MtjXln7R5I4MphWaEZzyILtA+T2vfUiAJh1DJgxkdBJJFX1YPhjdegUP60GV661IHfFKtEuXB7UCMOn8tqFLkGJhRQTBfFxDaORO9vx1ymZwcs+pZzvqfH9nJP2hHThcPPxkgf1HDg1SYc+iHAAAAAElFTkSuQmCC");
                }

                return link;
            }
        }

        /// <summary>
        /// Gets an icon of a list symbol.
        /// </summary>
        public static EditorIcon List
        {
            get
            {
                if (list == null)
                {
                    list = new LazyEditorIcon(34, 34, "iVBORw0KGgoAAAANSUhEUgAAACIAAAAiCAYAAAA6RwvCAAAAe0lEQVRYCe2UQQqAMAwErf//hQ+NBqGHJdBTmz1MQTCIZJhdOiLicji3A0QyAKJJYAQjakBn6448H23ecruf3DOPjZHBFT9D+V9soqlAKKuk1TNW0bSQWINQ1pZO6FLrjijskbkyQlmPqF8tqaJZ/bPlOyCqFSMYUQM6vyOhd+UiNy0nAAAAAElFTkSuQmCC");
                }

                return list;
            }
        }

        /// <summary>
        /// Gets an icon of a loading bar symbol.
        /// </summary>
        public static EditorIcon LoadingBar
        {
            get
            {
                if (loadingBar == null)
                {
                    loadingBar = new LazyEditorIcon(34, 34, "iVBORw0KGgoAAAANSUhEUgAAACIAAAAiCAYAAAA6RwvCAAABaklEQVRYCe2VsU0EQQxFWQQJZEdKiEQBJLSAKIErgYQGyKENKOQKgWtl+W9uPDKWB2kDdBuMJZ/H3j/219+Z22me55M12OkaSMBhEIlvYigyFIkKxHyckaFIVCDm8YxMAmzlezkfof9wejODWc08kY2q3/Iv+a38Sp7Zu4o0iX6ZgVX7DFh6M4NZzDwYX1/5uXwvv6g5tQ95Zh4DznwpnnkYs6X9odFW8b6uqV3LMwNne3zs4V86eGaZlZ7WDHaFmSI1Y2tgi1N9bvss7gwQou9pWHr4/qybIuw3oGdL3exBC8P4uBRPn2iNiCkC28wKaz3wBFj/hc/UQ6FopfdZPbOvinfym5rH8BgLNX/q1MFz9aM9x4JyZjdFYAqzzLgNUQlybk9mOxWX4Ms58hs2WVfVqHucrd8W4rPr3XpbU4u8V65TT53O7EVlejPj1xkqSXlHR/7xf/FHpTKIRPmHIkORqEDMxxlZrSI/r0zbtAWlop4AAAAASUVORK5CYII=");
                }

                return loadingBar;
            }
        }

        /// <summary>
        /// Gets an icon of a lock locked symbol.
        /// </summary>
        public static EditorIcon LockLocked
        {
            get
            {
                if (lockLocked == null)
                {
                    lockLocked = new LazyEditorIcon(34, 34, "iVBORw0KGgoAAAANSUhEUgAAACIAAAAiCAYAAAA6RwvCAAABxklEQVRYCe1WMVIDMQzkGNJAlVACBXSkpggNUNDwB3gEH6CHbyQ1byAVH6DmCmihg4KCY5exMnO2T5Hjy8yRQTM7OtvSSpaESVFV1VoXZL0LSTCHlUjkEBcZA+ytgOsRUABJUiwwI5uI8ATsK5FKnB0B74pN7Si1NQN4fwBMYgIMAXKwAsQ2cAfw/A3YBWzCihjB6olc4EPzG4khdG+O7S+PRuaf3TryawsxbC6dPf18rmBtnZEe6vvlasxWWB+fZ9iyTVvAJ9Ao1hk5dwxX0NYk6HLj/E6dblaWssFm7Mo8MNpL6WlPob/sRbW1NVKFlLbw9vxL+gZK4ABolNRESJwqcgnVV5sRvgEcNj5gJCH4TWIrfN8HxwFVF60iMvF1j/zVFBRnPo2WiJTU92ljHbRJa00bAc0cK5PIMa7MyxBD8/Ujhjkzsge+V4+Tv0Uevb3YMpiRnERYBX+gGYAP2DwJEsmZkX4k2k5kz7SVk8g9IvC/sgh/Or7IIlXntEZiSZn9Nsl5TIvP7CynIjOSNj7+E/GruOFvLLBOmY1G+j/RmrIx/byDKK9WkRPEizpl5EE+8gaivSOB8TI3tIosM27A3ZlEfgCh2JSrEvEvtwAAAABJRU5ErkJggg==");
                }

                return lockLocked;
            }
        }


        /// <summary>
        /// Gets an icon of a lock unlocked symbol.
        /// </summary>
        public static EditorIcon LockUnlocked
        {
            get
            {
                if (lockUnloacked == null)
                {
                    lockUnloacked = new LazyEditorIcon(34, 34, "iVBORw0KGgoAAAANSUhEUgAAACIAAAAiCAYAAAA6RwvCAAABvklEQVRYCe1VvU7DQAwmiA7ABIyUgY2+BGKEiYkX4S3gKZA6IjEyl5dgLGJkYWQkfF96jlz3cj/JIXWIJde+898X202quq53toF2twEEMYxA7CT+oyNTFHkAc/mE59Bn4G7ishbiCnnm4BAtYJyAN2puXPicEu8ExBL+MzCBSf4pdIIgUcp9K1vFZ8y4u4EviUU0AJtfwNJ/zdYEdQ8uyVLB69d5HkL+BKK0L/ezfYmVWNZTV/gxAoJuLHxPBXSxEqvfEkDuXMInnTigPzvbtfYpAeTWJfzUiQP6l7NJXHMssSMy57WZB4DoPaHeUImOSC4BJOcu6fWzQPhWXIIPVBbqDPYxbXwqMvWFkxBBkpjWyY6GIM5baz/lDWFXuaEWiLdtuUnhzyfOIjuarOCSzqlAPlD0DEx/Pi31opQ6mhNU/TaV+Vl/N3dyzB5NKhB2wu4Pi8k3RgCIzAaSOpojqaDkvtIHq6lAXlBpoqodQ39V58Fq6mikkLTcjknsIsVPzlGZ2pFooqEOIxDbwT17ETnHdiMS3m22o+EbdCj1ymGBXAJFr0QOPWOZI5vs3zc7QakA25FSebPzjEBsy/4Ab4+NFxr6yacAAAAASUVORK5CYII=");
                }

                return lockUnloacked;
            }
        }

        /// <summary>
        /// Gets an icon of a lock unloacked symbol.
        /// </summary>
        [Obsolete("Use LockUnlocked instead.", false)]
        public static EditorIcon LockUnloacked
        {
            get
            {
                return LockUnlocked;
            }
        }

        /// <summary>
        /// Gets an icon of a magnifying glass symbol.
        /// </summary>
        public static EditorIcon MagnifyingGlass
        {
            get
            {
                if (magnifyingGlass == null)
                {
                    magnifyingGlass = new LazyEditorIcon(34, 34, "iVBORw0KGgoAAAANSUhEUgAAACIAAAAiCAYAAAA6RwvCAAACKUlEQVRYCc1XrU4DQRCmJDWVVYgicJCAxxEkCoWr5AXgLdA8AhLFGxzvQAiqCBAkBCQGcXzf5bZ8N9np7W2P0kkmNzM7P9/uTmdhUJblxjrQ5jqAIIYUICP4TcEFeAbmEZILMO1cX554NQ4PYb8BpxD96O/larUPGByhCWwvEXubaRsOr21OsfUYEA/EPRLcgZ/Au+BT8BHYUh4Yc5w8Xks89hE4drxj2AsbAL3zNdnktiemDgAbd2XAMI/1WajrInet1DVZocGQeVqaf6Gsi9y9kncdGqMyCyulnmaVQ+fIuXQdG/NL9BTxE07P4qj5xOyI2ELYVfZuJMeFJhF7qOF+9UQU6ocqHeT3Dr4NVw8I50QOHeQEMUaBsC8CcVjl0JkEab+IOS4qEE7NQJyY46AkfjmRd8T3WuR2URrK/vwKWXObrPbhU0F/pew5wmJ2snJipoCwcQTVFtdYbygIttMVpmqn3u4m9Tr9lAikelDxtTWieuz13cOFPkYulc13C34Db4HZmNoTUBvE5j8Glw2rpziID3V7S8jJJxM9phocr4OJUog9MnMck8AsAhLWCIijW0GxKIufgMPjyG82mFCsr282mL4AaJ4sMJqgT7kzmD6L21ydwNjgvvU2MPN6sYHmjZxcO/8TfADHhh8f3Wrg6eubW6gtjn9y7oM5mZUuofxOXfz258fzx7JeE+dSo+4qrkZPgfIQ/G2N/wHEYqj0VfRItLA1/gD1N/syRBbELAAAAABJRU5ErkJggg==");
                }

                return magnifyingGlass;
            }
        }

        /// <summary>
        /// Gets an icon of a male symbol.
        /// </summary>
        public static EditorIcon Male
        {
            get
            {
                if (male == null)
                {
                    male = new LazyEditorIcon(34, 34, "iVBORw0KGgoAAAANSUhEUgAAACIAAAAiCAYAAAA6RwvCAAABJklEQVRYCe1Vyw3CMAxtERsgZoEj155YoAwBp87ADCzTI0uUTSg2FNVKrTi2c4CqkaymtuPPs52Ufd8Xv7BWGYKowQZm0wFVZnuIiINqOBuuPTDUNks85Fjc4RbsHbQ2c5RG65PV9wZyYqw2DE9mWeoZnPn2SQf8KpAl94q1R0o5xfckJah9VLSlOcMxbNBnAqFe8jhrEeGmBPxFVwp6hRaRqEeP8O8D2QjZqxO09gjWPdYvVL70iFA1XqyuJW/Gz7UE8vC7nVpYT1lRDj5y26jGKGzHrbzTTg21KE0N1RX3ltKIRi0KswtEumlFkHL1CL1J0WnSbUqjm11paHKm/YJICNuCyKwRyfIaa19fiugFfo5At4F5he8O6D78qz6em1XlSFJ+ARYpCXcpwDtUAAAAAElFTkSuQmCC");
                }

                return male;
            }
        }

        /// <summary>
        /// Gets an icon of a marker symbol.
        /// </summary>
        public static EditorIcon Marker
        {
            get
            {
                if (marker == null)
                {
                    marker = new LazyEditorIcon(34, 34, "iVBORw0KGgoAAAANSUhEUgAAACIAAAAiCAYAAAA6RwvCAAAB40lEQVRYCe1VO05DMRAkCFFAF9pQAFUKOiTgCNDRg8QVcgEOwBkokOAOtOEgQAEtdNAgEWae4ujh7M9+FCmy0r7P7ux67F3bvclksrIIsroIJMhhSSSvRO2KDJFoBB1D2WRUftM2gJYLm7VAB8A+Qz0ZA9CHhnOHgUg68kYX/CdRMlEiGokhBupN9UggQtM51B3HBSAJB5OEA+fxGhmWNMf++edsrMbqwfkE3RFA9OVC209uxP8LdFewz0zerjkEUiIxSxD8YA7uNFU8ImdqpJyYxDW51ByN3amdt1XZP6nWWi8B0ghzJezc2+sRs4HMGcpOqa8apFcaNtl/yaOVyCNyZQUX+m4svFeaPoLfrQQFvk1gvzS8tyIfCDSXVEuc2e/xr5Ig1lsRYnibvvKjg2whlpNSxVsRBr5BOaNauUagSYKJIytC3Ab0kx8Vso6Yby8usiLMwfpeeMkE/zFsLokmzjrtBN+4OSNjjzshfu5ETZhoadJkS0oUKklKHC1NwrNEp+nHeG/DFyvJNEkpEYY9QK1dxF7iTiuTVKPCN0sq3cxFfdEes7RH2rPMDzpekHvQqhu7pjSJDJe/vaUPakkw4RofHYS9sg+9hbqnJzCqdCmNmrTG0aU0NeOpMUsi+dL8AmIvWjvNLK48AAAAAElFTkSuQmCC");
                }

                return marker;
            }
        }

        /// <summary>
        /// Gets an icon of a maximize symbol.
        /// </summary>
        public static EditorIcon Maximize
        {
            get
            {
                if (maximize == null)
                {
                    maximize = new LazyEditorIcon(34, 34, "iVBORw0KGgoAAAANSUhEUgAAACIAAAAiCAYAAAA6RwvCAAABlElEQVRYCe1WO1IDMQzNMqnCKUiVcIJQUXOH3IA7UDNcCjq4AV1yCihZ3nOijKy1N/FnM1usZxRbsvz0VpZjN23bzsbQbsZAghwmInYnYhnZwnEHYQHVFGISu9tYrEa20IduTwjgxW1oMI2s74yttroH4FKDhohoZrdw/tULCsYLrP1R6xs1np0j4jnrhZlj/ZEedqxYM+PkL5uI2NyNJiNzyyxB94otsk4XZ8TlYC7JyB8g+mTVG9lMlhAxUJ56D+3bs5xRhiCSTIIcaxN5AGZSJiRRtYm8AviSIpb4p74Wkbcj4iP6d0g6GXsdm/vfu6qNr7iuj/aNGNB/QNw9hp52wcHw1MTmek+BC3XdQvNio5+QEJslI28bmY9il9y+a2xBqDA3sH9CdJOt0n9wYnN+JTUSIkHQL4jUjAtyyU9JRmL4WRkZgkiMIO3ZW7PoQ02c68UK1cheBeAbk19RQ/R7Vcdw4UJEXhSRoYbPHWAcbDnjuuf53+lDX2lMTGLrWG4cKtYO2WsYQltzjbidGBMRm5J/2J1BrX24qsoAAAAASUVORK5CYII=");
                }

                return maximize;
            }
        }

        /// <summary>
        /// Gets an icon of a microphone symbol.
        /// </summary>
        public static EditorIcon Microphone
        {
            get
            {
                if (microphone == null)
                {
                    microphone = new LazyEditorIcon(34, 34, "iVBORw0KGgoAAAANSUhEUgAAACIAAAAiCAYAAAA6RwvCAAAB4ElEQVRYCe1VMU7DQBBMEDSUaWj4QPgAlLThDXwiLQ9AyjuC+AOdKfgCdFDwD8yM5Y3Gm/N5z6YIUlZa3d7e7N54b883r+t6dghycggkyGEKkSXit1CWlEqbvnHCoynUOfBbaJ9wjZiivEXgNnmOhJGrSok0zAtqydK/B/FXwH0EscU98hBNDFwJdlZakdK7Po8Sn3JronuEcEcivkz/oiIbsGZzrj37EXP7A/fmyt0avSHW/eqL8GEc9UfAlktc+bfmq4McPzmV0FexO2auR94EuWjtJ/ENmYa9EOC32B0zR+RFkNet/Si+IdOwtwJ8FrtrZh6nS3vBMH4Kjg/akNijxx5UWWCSfGiTTgGTgMkNDOKZPPcCk0RzCTDeQ03o792vd6EN4uYq55hYzBK2EqJNn61rReHurBlmN+4MSeB9FbOI9JYXGIslIRWStLXkmHS6oDPM9Yi4wQbKL7YjYB7ifJXgamIVl9wz6USw9/NIKmYtFMaQoM+3N99zZIL4VWtoVNio4fxhoCTlF7KJeTxe2AsraKgKwO32b86u+2cpmvm3J/mORDLm/qyR+D/DHIn4UkYrskIg+8Grz+fXOWfsoESblQmnyGATRytyN4FFKDZakQk8YqHRisSyTUD9ApujlyxIC0EBAAAAAElFTkSuQmCC");
                }

                return microphone;
            }
        }

        /// <summary>
        /// Gets an icon of a minimize symbol.
        /// </summary>
        public static EditorIcon Minimize
        {
            get
            {
                if (minimize == null)
                {
                    minimize = new LazyEditorIcon(34, 34, "iVBORw0KGgoAAAANSUhEUgAAACIAAAAiCAYAAAA6RwvCAAABkUlEQVRYCe1XQU7DMBBsECd4Bdz6g3LizB/KC/gDd/gUcIIXwK19BVzTHWRL47ETbNeJipSVVrbX69np7sZJu77vV6cgZ6dAAhwWIlqJoYxszXFnigZqqcAEdixoVtGtraeWOwsQxO1gEAHrK7G1Xu4N8JpBU0SY2aU5//CBI+YXdvabznc0X/1FJHDmg5Vz/pEB9lCzahwAsG7U4dh1LhGO82yLDza0mOeWhlOKuDemKTJrs3/BYUAYp6o0wL03fXMB3m3U8oDEp9svH/R5lgvEP+sb54cMvpKPt6/J5s+kRnIL75EiZ0OBv5J5YnTnk8KFjSXwye0RTTXq+2J6qxu2Dmov+4M9UksE+OgR9IrKrETGGrOKSM09MkZCs5O9LiUyCQmwLe2RsbT7X88N6W1+5L0A69x7ZI4MlHkkz620NHmoFV4LEU3av8kIPu9ayShWKiN7ioxvTDwpLZS/VznGb7gUkUciMtX0IQK293LwOnZr/LfZ8Tu70RyYwI5ipm7WiOwchlRp5ogbxViIaEoOg6NLq3Yn+rgAAAAASUVORK5CYII=");
                }

                return minimize;
            }
        }

        /// <summary>
        /// Gets an icon of a minus symbol.
        /// </summary>
        public static EditorIcon Minus
        {
            get
            {
                if (minus == null)
                {
                    minus = new LazyEditorIcon(34, 34, "iVBORw0KGgoAAAANSUhEUgAAACIAAAAiCAYAAAA6RwvCAAAAm0lEQVRYCe2WUQrAIAxD586xn+0gO/tOsh3FNX8aLezDDj9SEKyUEJ4BTTnnZYZaZzABDzLCNyEiIsIEuFdGRIQJcK+MfCWy2eBlC0/z6HWbJvSrSs43AMN7NTm2eUzuKCU9I398UlJpRGEtaWDvEcEdRlaj7xk5A13ARKPvhTXQR1/aI9KfDjyVEYYrIiLCBLhXRkSECXA/TUZeQ1opD3Q6Hr0AAAAASUVORK5CYII=");
                }

                return minus;
            }
        }

        /// <summary>
        /// Gets an icon of a mobile phone symbol.
        /// </summary>
        public static EditorIcon MobilePhone
        {
            get
            {
                if (mobilePhone == null)
                {
                    mobilePhone = new LazyEditorIcon(34, 34, "iVBORw0KGgoAAAANSUhEUgAAACIAAAAiCAYAAAA6RwvCAAABDElEQVRYCe1Xyw3DIAxtqp7aKdptukM26A4dNZmiuVI/qZbAJfCs5BAkLFnEP3i2CQlDCOF0BDofAQQwNAlkFOCTMHrJMvwRVyfsEYJH8dlCTwkurjPAgSBkdhe+CS+Ev7pc5eEjPAs/VJkbWSCKdshNUtFRsU1u1kri28y9IrZ+vSK9IrYCVm52j+DI9hDt7z3iPSCsb/HzcJjWXCzsihxnlfuYlXTFqZutiGYcZ8fq4pi/594aWxJvRdg25Pzs2onsBdLfmqR8IuRKzursXIncXGvmBL5fqMazJ+v7tzbaoKxwVMaoZHUvNayOtatgZMe1cxL2EPwRV7xuws7+BqwmspeBbc1e663O8wVa7yyPaBbOEgAAAABJRU5ErkJggg==");
                }

                return mobilePhone;
            }
        }

        /// <summary>
        /// Gets an icon of a money symbol.
        /// </summary>
        public static EditorIcon Money
        {
            get
            {
                if (money == null)
                {
                    money = new LazyEditorIcon(34, 34, "iVBORw0KGgoAAAANSUhEUgAAACIAAAAiCAYAAAA6RwvCAAABaklEQVRYCe2VvW0DMQyF7ZQpnTYDxBu4S50B0rnPKvYc9gypnS5TJBtkhcv7DqIgKzzHwt0BgiECNKkfUuR70nnZdd2iBrmroQhqaIXkTDRE5kDkpKQ8vSHd54d642qoWd7id+RwgR5oY31QGjU5NFMi0qjJ0R01noqaUbTQwVSFjEKD4Jv5oI2mxKCshpoF/zWOQtlW+iVFsC9Sb+8kc0NJTpwe5NEc2YPUYvAvSbqXxmikv5OyliNaj5qteHsO3D3I/kixCGub3iv7edL2d+luMMypzujQUt/BBicR0IqdXOmDhOVdeTFeQu2Lspd3HzROyknjrqXIGnIb8aj5TuB7lf8mXUmPYf4j2FLzqQBiof0vvVl3dMqlMjE0sCZ0liJS4kMLAk1nF3coSQp3+mqgKo9J93JILunrIZYcyFoac0UnnQw+nZOE6rFngRpbbGkh7lPu4Skle4793mWd45x/c7ZCcoiqQeQXoEVXPs2HcIwAAAAASUVORK5CYII=");
                }

                return money;
            }
        }

        /// <summary>
        /// Gets an icon of a move symbol.
        /// </summary>
        public static EditorIcon Move
        {
            get
            {
                if (move == null)
                {
                    move = new LazyEditorIcon(34, 34, "iVBORw0KGgoAAAANSUhEUgAAACIAAAAiCAYAAAA6RwvCAAABtElEQVRYCdWY31HDMAzGU47yUFbgDl67QVdoZugSMBzPGQXWAI7wfT7bUVSb2nJDi+6E/lhSfk0dE+jGcewa9IBeCm3LnK6l+dkhTD8Ym+dZGzVEwDHDWEByEE0wtSCnIMwwNSClECaYFbq6Almh5rugTpfcIFF0gVvdmYk57B766NefYF+9L02P4M0n3mGLIFhfekf87GjW8D5iNDl3cD+nsNzjrbPIV6Ypl8+UT+kcyA4lA5Sf/FzCWQN0mxzIzap0F7Y97FatyVpRFl25rn3OCsJrzNZnARb3odLbjW4QsSp1oZ4nY86SwmvF9eggqSHY5DazbBA+17XIedrnLC0RJhSnIHTTUrGDIeUemyd1JiT31ELJ3t2uhYZXjeXjy9Pw0tJfzR4JILSpDfvnT00A0jAXOUcCzFWcrBJmwNe1hoactlg6El0jY84aoEfHO3Lm1wA+Zal3Db5AmST329c0rKXpX4Js8IkfoLS/SWndbIY7J2aZdLD4y3PpV8ON+ZJmzGZZn9rQ6QY+OhVa+rcN62rmArmyAfWnYKohyGABYU8OxgTRApKCMUO0ghDmbP+o+QEwc+hJRWB7xAAAAABJRU5ErkJggg==");
                }

                return move;
            }
        }

        /// <summary>
        /// Gets an icon of a multi user symbol.
        /// </summary>
        public static EditorIcon MultiUser
        {
            get
            {
                if (multiUser == null)
                {
                    multiUser = new LazyEditorIcon(34, 34, "iVBORw0KGgoAAAANSUhEUgAAACIAAAAiCAYAAAA6RwvCAAACYElEQVRYCe1XPU5DMQzuQypCjGVgoAywlQvAxMRAuQBLj8DCJZBg4wz0IB05AWMZYGGADQaQKP5KHDl+zs+jr1IHLFlx/B8nz8mrZrNZZxVgbRWSQA5/TWSTbHuEXThpA5omMqSgU8J3wlfCT8IJYZ9wMcAZKcCKdO4IUzAiYYkvU8dkGg6vUxkI2dCwLYpRotQXgUrIrksGFZo6vKSR+WZMk+kcsWxC8ybAW3RkGGFR7DcYsfepQ1aR8DulYMgeibdPiAP8ZMjXifel+bmvZkcbFMz3SAcLeIvoXlj8XEUGZPRgGWZ4WDV6DD5zC5BoALmKBMotTtAQA8gl8hJol09wBjbK1fMtPrbPqRhjJ9xOKWlZriLQv9FGmfmVk58m9D5qsth3LfibRJcCGhj3B9AWoEuzjh89YQkFz2pOVpCes0npY2G1uCVbgyreE57VyhkytmjKZ+o5FPnZLlH1bYHYyi7Bw2pwb3DZJ0Tjopv3IxqlP/BQGbT8AaGl4/U9QYqaxr2Aqx97yiXXOnqORKEPu+i9QjJtF60InGjIJWMdap0MfKCCNV+1zEgJJeTSExkAnOjrHHPwLYAf3hLoSQiStBKJOZVOQMeS1Xo4UxxHvvJAM78zz9af6d9bs+m1L8yjJF/9+mnA/Nor/jDqajHBiTPHZ433CsM5E740rkwTXdeW5thGjjUSPj1fbg3+U/CLsCxAM0NF8ASQ75Q5X3bW22Vl4PyO3ag765wvK4IfJzzzlgn8MtOxKlmRY8pAHqQ2E4LfA+cQf4u8YM+XFWkzcGNfsiKNjds0+E9EV/MH9uuiIuaqiLsAAAAASUVORK5CYII=");
                }

                return multiUser;
            }
        }

        /// <summary>
        /// Gets an icon of a next symbol.
        /// </summary>
        public static EditorIcon Next
        {
            get
            {
                if (next == null)
                {
                    next = new LazyEditorIcon(34, 34, "iVBORw0KGgoAAAANSUhEUgAAACIAAAAiCAYAAAA6RwvCAAABe0lEQVRYCe1XO07DQBDFSNCkC20oKOEGqRKKNJwBLsEt4BiYOm3qdDlAJLrQpE6ZhsK8J9hoPFqtZy2PlUgZaeT57b6XnbFjF1VVXRyDXB4DCXI4E9GdOKkTuQf7DZRTzesIKqWEM5SBVjbvmgbdIC+FvlzDHGNXKi5rGu2CixskVlCINSG/RGwq4llmlzMyAfJbFroo7vJEwrZPMBbBsV49iBD7Frq1kmCdFxHuPYDuaVikyxnReGsE5FDrfM33JHIHpI8aWsLxJELY539NUPhLec6IBB/DWcmAtvsiQtwb6E4TCL53awIOr3PpaLvPE0nezn2dyANOIPlM6YPIC0h86VZo35vIOwA/NWjM95wRvhY8QsNrQgz/EPMkcg2UnwNSg+HVGv77mkmQowcRPkWzXgE8iLxi0+SjnKAx6XJGOJzTGIglZmnNt9pI+0wzNlN1ea7hE2CEmiD8bKAvPw9K+EMVk3mTbWlN3i9rWW1pTcut85adiejz+gVGeSoGcZaEcwAAAABJRU5ErkJggg==");
                }

                return next;
            }
        }

        /// <summary>
        /// Gets an icon of a pacman ghost symbol.
        /// </summary>
        public static EditorIcon PacmanGhost
        {
            get
            {
                if (pacmanGhost == null)
                {
                    pacmanGhost = new LazyEditorIcon(34, 34, "iVBORw0KGgoAAAANSUhEUgAAACIAAAAiCAYAAAA6RwvCAAAB8klEQVRYCe1WvU7DQAxOkTrQsaxlYKMrC2ysMDKw9SXgAdhh5RGKWJB4hoLEG7AwUAbeAiHC9yXnq+Wc2wMUqUKx5Jx/vrtzfM45vbIsi3WgjXUIgjF0gdiT+E1GRlhkAp6C52AWGZkybfQNwT8jFmsmHwA3B+cSsWNw1vo5oD4Wm+bunsBxbg+8dK9VRzNAfl9Cjrcw9gJTfgh2PdxAsbht2F7BfQ1syEsi5VswvTMHI364K1qF8/xVpqoHlkmNk7DByPFzDutGKAd3DHBqr6I6u0aa6iP4CnYeh0dM90dw/gnn1ci+t7OxfxrdUzVunAJ5gZwosLzpCDa5MyiTNuuheubijtScKHqBnEZEUewG+V3ZHoN8qGyCEx9dMkfj9tSchegUjxQgR3459guhjfeLJg830CDIxDUK1svIItKi2IHCe+BaGSnL/SJmwV2IAeMd+FnpFIlrkPfVsBbaJKmnuEdORiK4TaELxGa3y0iXEZsBq//rGnmzb2v0pN/LSBIcFuTv4DLSV3wK95QyeoHovmLn3cJwbo1Kv4d8pXQrcn6TUp0QNtsxYapIOqzn5+8lO6vtzPXs+ln1t4CLXTgK1gF9CL4Ec/MZmJvoRRiM+Inh/6tez/rP4GeAGhNlr/s2U9eyxauRlrdtLt8FYnPyDYE8Tu/zX4JDAAAAAElFTkSuQmCC");
                }

                return pacmanGhost;
            }
        }

        /// <summary>
        /// Gets an icon of a paperclip symbol.
        /// </summary>
        public static EditorIcon Paperclip
        {
            get
            {
                if (paperclip == null)
                {
                    paperclip = new LazyEditorIcon(34, 34, "iVBORw0KGgoAAAANSUhEUgAAACIAAAAiCAYAAAA6RwvCAAACWklEQVRYCe2XP07sQAzGAUFDiUTHBZYT8Do6BB0t4hIcg2Mg7kCd7nW0r6OiQ+ICFIt/q/kiM7EzSd4WW2DJGsfz2f7i+bPZ/fV6vbcLcrALJOCwM0QOJ3TkzDCXpqemH6Yvpp+m2xX2SKLH5u9MI3kwZxa3yJ8FnVTVKXxt+uj892Zn8bP9UcC+FXgrBRnpjMd5klsj4wvIvnAkjioSwpwVDAOdkn/xGJ2a27IL72z8Snbku/nPyxyb9yLBTXZHRKYEHxnon+mfAv5r46rYy4agrSv6bdKZRq1muRD2DzZLI2HJopimLwNosz4nifEjIsOmlbCZs7ypP5vQW5M8IsPJEpnObPAccclsMhkR/BxbyVQy/p6pj/1Yrb3RSWNBMr9MdMLH+M5omdQpPXt8aqcTrmCLDDl8cfD+mWVr1mkBSIhyGtSZzuy6MzUZ9gg4JMMT02tveKezSYJAYg4Z4uhEi3xfvzcsKLJ9silkwEvoio+ns1GNjS+dcEE+mch0pRpjvUya0+XGnpFwqsKaoTMAU0wFSEoRPfOmkCWXbmUzfxD0ZMAM6g4cEaj4xsgY5IdEF9p9QYRdmUMELGTogITOXJt2RSmm7tS5wSJg67nmhTYIKEl4K0n09lGclifctFHAVN9cMqvCnA4OagwcEWjE53/oaP1YPi0pnRngBo4I1PCxLyS8dZRThOliNL94j9TJaLeEorrMGNUJ7qD6zunz9IaB/tfWR7elGghkstO0qbthuOwjM4w6Me+N6VWZfbXxybT5z3DbREr9+cPSr/j5lRoRv0TqBn0DX8nYGsjwGzEAAAAASUVORK5CYII=");
                }

                return paperclip;
            }
        }

        /// <summary>
        /// Gets an icon of a pause symbol.
        /// </summary>
        public static EditorIcon Pause
        {
            get
            {
                if (pause == null)
                {
                    pause = new LazyEditorIcon(34, 34, "iVBORw0KGgoAAAANSUhEUgAAACIAAAAiCAYAAAA6RwvCAAAAt0lEQVRYCe1XQQqAMAzbfIcf8P//0afMBqS6UKE7DBRSKK41qTWr4GprrXzBli80gR7UCO/E7xTZ7A12c0w2HOvVPLIR7M3HV5Pw3TBsyEXcEazzK4ol7A1UA+4I1um/mxHvfNZCirCyUkSKsAIca0akCCvAsWZEirACHGtGpAgrwHF2Rg4mWhzlAIvyUa4v+XIk8N/86/5q16fhyIAc4xCPYJ2fPU703U+Islsz4dF9STXS61HKCRYH9XlWUOImAAAAAElFTkSuQmCC");
                }

                return pause;
            }
        }

        /// <summary>
        /// Gets an icon of a pen symbol.
        /// </summary>
        public static EditorIcon Pen
        {
            get
            {
                if (pen == null)
                {
                    pen = new LazyEditorIcon(34, 34, "iVBORw0KGgoAAAANSUhEUgAAACIAAAAiCAYAAAA6RwvCAAABlUlEQVRYCe2WPU7EMBCFN4ilgG7rpaDkBlAhJBo4A5fgGJwD7kBJREWPtIgGGlpaGiTCe6wjktmxHTsetMWO9BTN+Ge+TMZyqqZpJutgW+sAQYYNiPwS/1WRGRJXMnnXtwaZIlkNfUDf0BxSrTI+NYfIuhCZ9+G/i5hZs/Iz7ELP0K1I+iD8pcuKFNYU+71CtCOI+9/Q6dhKzpUAJo+JdSHavC3MtQucazlK9ggb8wU6UEp/jNgjxDlfyvikFAh74h460ZK4mPnxHQJxEQBcDmnfKyHGitZQyNSewIJeL/YcORjxi0EwTy5IUYhckOIQOSAmEKkgZhApIKYQQ0EIIe8KhHo26IhihfdweAfEohn8upf6zxkNwVwxkEsHQBCtMkUgYiBM3LU5HMbaK74YBPYMXnra39UOLoYzd2/cRe+PlAmk8UjrCVaBVfGtyY6Hfp5P8ULd37w3+E8QOsvAPG/HarBBqSv3zH5brI+u1SYwOY0nRhs3iW17iryH+KdnzCT823gmOyduGmrWxK3GTd+AyPr9AI8c4vdfDbvMAAAAAElFTkSuQmCC");
                }

                return pen;
            }
        }

        /// <summary>
        /// Gets an icon of a pen add symbol.
        /// </summary>
        public static EditorIcon PenAdd
        {
            get
            {
                if (penAdd == null)
                {
                    penAdd = new LazyEditorIcon(34, 34, "iVBORw0KGgoAAAANSUhEUgAAACIAAAAiCAYAAAA6RwvCAAAB7klEQVRYCe1VvUoDQRDOiYIkhRAQLWJhJfEJfIVobaWVb2DnK9j4EnkH68MmlZ1gp4W2CiKKYHF+X9iVybi7t7crxyEODPOz8/NlMrtXVFXV6wItdQEEMbQFhGMPjr4tILWD/weiR/SnJtJXv84uppQ2RPqsPj/LnQhBvIGHtlOqzAWyaho/CTAFdM0Wn/bTnlMuEFuHUoKR/ig9F8ir6pIMZlkVijU50iPwOngA5p5YIpgt8KN1xMgUICMUvgJviwYEcyN8D9AlmO9dEDmLKr++kVwg7hzsolM4++A7dTiCHVU/KgjFxqqBy8wCUwdkBR2nrq4enw/MEPHBXqEdmeBPvFz8I2utCxOxA7lh9HfIZ6N7hQ8IF7IpCNtkE8onuNGt8b0jh7ZqgjxLyOnxJrjytPMeQQfgNfDMlWB8u5C3gXP/kWOJuFia5LLt6UNjc6mDCxk6d/01+w7YH8L3InSpnkijse5AqR8lXkn+Ul5lPlolWBOnlDwN5upkNpJkQVD6iMB0nca2TpiIbjEgGE7wuk5jWyeUBkgsCALXNZJsmcQdIMWAYEz0Bw2xso9Tl04unA8EF/gYLK+xzM3WZQF+YWkTDKkEc/S/sgOoI3v90PXLOsb95+t5Deb3ojXSQFprrBu5XlYd04rdGSBfzq76MiXcjjEAAAAASUVORK5CYII=");
                }

                return penAdd;
            }
        }

        /// <summary>
        /// Gets an icon of a pen minus symbol.
        /// </summary>
        public static EditorIcon PenMinus
        {
            get
            {
                if (penMinus == null)
                {
                    penMinus = new LazyEditorIcon(34, 34, "iVBORw0KGgoAAAANSUhEUgAAACIAAAAiCAYAAAA6RwvCAAACEklEQVRYCe1WO04DMRAliBQE0YQOaEKDqClIBRUFnCGcgIo70IRrcAgKCkKFaBENTTgBXACJ5b3NOrydeD+OV1GKjDTx2B7Pezv22GklSbK2DLK+DCTIYUXE7sQqIzYjG3aggX5oGbaI2cTWjBGnE/sBsUT4NT3oO7QdQ6YVeaGRyG9G4BPtIfQn6wc1sRlRMGbmEUpywRJLZBOIV4J6BvsJGk6GWzOHdrFmDKV0oIPU+v+5hxkUN8g5C25BSYhkbqAqQWRCiBBspEhik0wbOpQxmuzXwqjlhGAXjFohjgwzocJMVeJUOZRlQcGcPYLBzFgy/JBSrLJ7pI/T/yIVUdd8huM5dFsWfIvtNYuIdOH95V1RPciL7aDaLe9RdI9c5t2CeqdB3s65YO/cHYHpqfDQ2RKdTmZG7SqBf+7M5DrZJA+blT4GnG9ZBXGt8wtqfVtz7LIl7ZvYr2Krye2c68FjEB+Ra42e2Qpw4plnpTx4xmsP2arhY+WedRdkCwaJ3EJ3oQOolR0MVJaoXaR9+1dxTydhE4AkPqA9qE/uMBhFIg1qDpdWxT7mmLERtEzSrMIh6HBaf7vYle0RHOuQ0GqysYL66sx3heKC2/diMjt5R+gzd6lireKmtg7wfigiQVKNgyshJcLtYH8IpTjwRs4A4inWjJ2CSCnwxaXw0kpSa0E/lsiCYGdhfDfrrNcCRv4ASWFm8/M3F8gAAAAASUVORK5CYII=");
                }

                return penMinus;
            }
        }

        /// <summary>
        /// Gets an icon of a play symbol.
        /// </summary>
        public static EditorIcon Play
        {
            get
            {
                if (play == null)
                {
                    play = new LazyEditorIcon(34, 34, "iVBORw0KGgoAAAANSUhEUgAAACIAAAAiCAYAAAA6RwvCAAABQElEQVRYCe2WoQ7CMBCGGQKDxIIgCBI0BofmGeAheAvCYxDeAQsOgSQhKAwaiUGM/8QlzWXJ1rtrGMmaXNrr1v++tdeuWZ7nrTqUdh0giKEBkSvx1zNyxNf05RdZ/Uyxa3ibdRD8YwXg8ZaluUMkYyFrbQEZIvjGCsDjLUvDGis09uxoaw8Qij2B3bQQNM4LhLS6sDc1NMWSIzLeFR3q5PUEoeTdSbqqvicIxVzC1lWDh+955kioO4NzDjvK2qlAKG4P9ioD4OfeS8O6VF9Cp6ydckaitnOqGaEDLupMSQFCR370KesNQv8c1X/HM0cegBjB+L6CZvXiCRKVnBLRa2kGEI5KzhQgC4g+pXCsb52RLQIeYoMWvW/JkRME50Wimj4tCO2QMeynt3iaiaknBLRUV0Ua516syeoG1IDIqfwChM9Nr9qVpWcAAAAASUVORK5CYII=");
                }

                return play;
            }
        }

        /// <summary>
        /// Gets an icon of a plus symbol.
        /// </summary>
        public static EditorIcon Plus
        {
            get
            {
                if (plus == null)
                {
                    plus = new LazyEditorIcon(34, 34, "iVBORw0KGgoAAAANSUhEUgAAACIAAAAiCAYAAAA6RwvCAAAA6klEQVRYCe1UQQ4CIQwUn+DZiz7Et+tH3KdgJ5FkKR2CIMmGlITsdhbaYbZMiDGejjDORyABDk5E/4klFLnKqd4y0e1pIgb+8wgDtwZFb0bFTbC7gVehESK1ex+qVY2PS/SIca5+yBXR2rkirYrAlJ4yk1FZT51rH1vrEwb/uewX4535CDMrvb833mRjZnqMCNjPHpnpebNquZki+IczR5GfEXlMZAESRX7WrC08ag2dNWJLMqZIy96/rnEiWk5XZElFClP6npLhWoQsHukRmJIu+hKsMKusIglGDI2k7INHFOmrSHY5ES3MBy+gN8uMLJH3AAAAAElFTkSuQmCC");
                }

                return plus;
            }
        }

        /// <summary>
        /// Gets an icon of a podium symbol.
        /// </summary>
        public static EditorIcon Podium
        {
            get
            {
                if (podium == null)
                {
                    podium = new LazyEditorIcon(34, 34, "iVBORw0KGgoAAAANSUhEUgAAACIAAAAiCAYAAAA6RwvCAAAB9ElEQVRYCe2WPU4DMRCFCSINdNAGJEQF4gCho80dQscFcgwaLhE6KprUoYIDIDpCATUSBVRI4X0hkXYXjz3ebBEknvS0u+M341l7/NOaTqdrq4D1VUiCHP4Tqc7Enx+Rbf1RXxyLE5GKh2NxIHbEPLBqMngo7UT0oiuhK75LpGBtcejtvaIbz/2jfbUkSg3hpgQP4n5KGGl/Vtux+GlpUsXalqM3iSNpWyLPKvgJ4hAvDEYkwpzpKMZRyF8YyFLUlN5LHxUhhZkD9MQL+fXnbWZ/sRphWS5TF4spONHL/eLDelqJsA+8WE4Z9h1p3zx6q1hPPc4RDatkS3QlQRwrkXMaa+JKfgeiuVRDcTdCRtn2DHvKfCYBiWTDqpHkLhfoyVWUAb+ZyZoaSx+zv8caU21WIrcpx0D7o2ycvOyu2bASucmO9ONwqceTyPmUBSuR66woZTGb4IfYK5vjX1Yir3JjL1gGIzkPRfugK0S3Vg2SrnhX0C7zuitnfs6ENSI4cD7UKdpQZxwXFLKJ2IjgRNEx302BbT+448ZGhM5x4uBqAkxPMAmCpxJBw8HFn9QtXqYX/2iNmBcV4yLTk90LbvvoXX24RJVg1BW3sAuxCjrH3hGzYqeKVSPqAnvFl1jnsJx10FQirmxjIk+xxvwba1uZRL4BnCz2j/Kcz0cAAAAASUVORK5CYII=");
                }

                return podium;
            }
        }

        /// <summary>
        /// Gets an icon of a previous symbol.
        /// </summary>
        public static EditorIcon Previous
        {
            get
            {
                if (previous == null)
                {
                    previous = new LazyEditorIcon(34, 34, "iVBORw0KGgoAAAANSUhEUgAAACIAAAAiCAYAAAA6RwvCAAABcUlEQVRYCe2XP7IBQRDGrVAoJhByAtmLJM7wHMI1ZK6AO0hlLvCKiJdIhaTr+9Sqam3s7GzPKqVMVTPTsz3fb6Z7/EnSNK29Q6u/AwQZviA6Ex91Ii3sbqV2SN8expvA9y4styXGW9PA6qdMIRFKFO+IMbtyXk1h0gDChXewm6AUcn0myPkHEEuNzATEw8KhjrIgvxCiRWtlUsPC2zgI5NEHpyYURBanZjGBhKSGQn9aPdY4BCRqceoNFAUZIzBqcWqQIjXSR9BaBzrGphrxgTQheHSIulwmEF9qpi7FKny+E8m7rpqn0hM5Q62nFasY+1JDzS1sVIW4XLMICJ9fZCZjo/Z9NSLFWAPya1/OsW+qkRAQiuUVrwmkaGoIwcbibV97kV9CQSh/gA0jc5T+X7MEyCQmTGiNaO0VHD+Z86U1okEGcPxrp8PneuY+jL/ijdZE/Fyt0cJ4D2PjO8e5OtbU3O/KMCpzawxyz0O/IPpsLlRzs6LlMyRSAAAAAElFTkSuQmCC");
                }

                return previous;
            }
        }

        /// <summary>
        /// Gets an icon of a reception signal symbol.
        /// </summary>
        public static EditorIcon ReceptionSignal
        {
            get
            {
                if (receptionSignal == null)
                {
                    receptionSignal = new LazyEditorIcon(34, 34, "iVBORw0KGgoAAAANSUhEUgAAACIAAAAiCAYAAAA6RwvCAAAArUlEQVRYCe2VYQrAIAiFa/e/xXbPNgeOeCEkpfOHQaQR9N6XWG2tlQjjiCCCNKQQfIkkkkSQAOZZI38TOR8B1MppUvyN6tzi8T+prMSqRkTnfDGuVkQk59L+8l+jdo4kOF8lIjnU7k8T2eacCeA6S0TrUHt+IGLuHElwjkS0TnadH4iwQPfVqqGpjYQVcnVWPOOCxdrp8A3DPo0vhu62JNLBeMMkkkSQAOZhauQG58865Sku130AAAAASUVORK5CYII=");
                }

                return receptionSignal;
            }
        }

        /// <summary>
        /// Gets an icon of a redo symbol.
        /// </summary>
        public static EditorIcon Redo
        {
            get
            {
                if (redo == null)
                {
                    redo = new LazyEditorIcon(34, 34, "iVBORw0KGgoAAAANSUhEUgAAACIAAAAiCAYAAAA6RwvCAAABrUlEQVRYCe1UO04DMRTMIkKRMhKCAiSgoqeiiqgQUNMg4AxwjFwjyQWoEGVOEWigoUOipEFimVnslePYz+to11qhWJrY+/w+k3m2szzPO20Ya20gQQ4rInYn/qUi1/a/jPmuU5ExCg9jipu+WY3XV78DExS4BfS3Wc+7rlMRXYQtGgGZNlSZm1BE151icQJUUmZdR0XO/LeHwBFwCuwC9hjAQGVu7A3nN89IBPrwHQIxYwznYI2gg0rCFsYSMMkyVqwlbqrgLuZXM+uS63uVz1kzdGt4Fl6AfWdf44zPknvosN45SExhewA+gE1gG7gEJLI8zO+AfwhysSXmoLS0OaU1HY01W9oTYspc5cLhfGYk3HHs27GGe7HkbSneqQqxHak1V0rHY8yyrIuCn8P0uGj2W6SXlS/iG3DgD5/b0S9o+DzMhf19hG7NhSPGZ5pgYwOIVa/I51OE1/ZHJf4uPBv+8RFhWUpNQklGqDVJSLCIRIQ9TzYkIk9g0U3FRDojfZDYAmYpyEiKfILAXgoSrCEpwv0e8MVF0yNEpOn6ZX6pNaVTisWKiK1yaxT5BZODdMLGUobnAAAAAElFTkSuQmCC");
                }

                return redo;
            }
        }

        /// <summary>
        /// Gets an icon of a refresh symbol.
        /// </summary>
        public static EditorIcon Refresh
        {
            get
            {
                if (refresh == null)
                {
                    refresh = new LazyEditorIcon(34, 34, "iVBORw0KGgoAAAANSUhEUgAAACIAAAAiCAYAAAA6RwvCAAACeElEQVRYCe1WvY4TMRDeoEsTOupQAFUkOpqgKyIqFB4B5SXyGPACPMBJ9xI0ewXirr3umiBBWlooKML3LbY1jD87u9EppGCkkeff45nZtUe73a45BXhwCkkwh/+J+E78i4qMfRKHtmYCxyXwAtgCOe3EDXANnAFHQAUrCO+UouFX0xOnsGuBfWEFwxEwxp8ZxyhLayKMg5cx2DsTZAi5gTEPMHFOfo+my1iW6o+Q/fwIXFRsvgTdk4qNV+WtQ6ZZdkHGJHkiD5TNgWOg9+XJl0DlB3EC74cpy4NFmWoHN4n62spDXKRtcyLzzQRho1nu2/W6ZO/lrEytKt6+OfPNC/wHJ38DfutkJXYKxbeSsiRXw/oIxt+NA4fxqeFr5BzKzzWDoMuGVVWEp7fw1jIVmsEvgfErqpjmKpXIa2d26/gSu4Oib+WyGOquOXdWPxx/CMvfPq8AIukM1IzwZBGGzEf0USsTiD88GVNV5GWIRIe+86E2t7KYBGVfrSLSakauocymOjocsPpr/0bFUImwjH3gGYxsG0s+L5yidXzH7psR5UMZ28fK9QE7H7R/CMw+AFWRfcEfw2C7zyjol1jtfFyBz5Kg7ZCKcHiflwIxmAP1qy8eQn01Ll5iP4H6mbg6wUr4++Y9ZOVKhtvW3obi4k0i3qh8CvB2tT6k+T7hO0Xdui3kXfWxer+OH9IaVQO2i/+FhVIGGefiFbD+hYkMIUrAk/PNqU6ZjCoEH1fVSkDfVUSVKcbl4yjqGYyv8r7QwpAHiP57V2XA03NTpWNCTHANtFUizadhaX5UrL9kakb4S/4V+nu0RSVytM3tRkP+I9bv3umTSeQ3RQlb5NcRWlQAAAAASUVORK5CYII=");
                }

                return refresh;
            }
        }

        /// <summary>
        /// Gets an icon of a rotate symbol.
        /// </summary>
        public static EditorIcon Rotate
        {
            get
            {
                if (rotate == null)
                {
                    rotate = new LazyEditorIcon(34, 34, "iVBORw0KGgoAAAANSUhEUgAAACIAAAAiCAYAAAA6RwvCAAACNUlEQVRYCc1XO04DQQwlSGkoQ0ma0NFQI3EABD1duANwCyoOAeIOHICCS4QCao5AeG+13rxxvJPdIaxiyRqP7bHf7Hg+O1oul3u7QPu7AIIYSoBMMG4OfgIvwPykZMrU0UaffsSlqfkE7R14LDqzsT0DL8Bdib6X4BFY44SyKi0BAyiYKfp9AFgcazmWMTTXmqwKG8jWwMxV+UeZsTRfIlefrV7MIbbPPXI9hsUjKPtMmLXEz63rz+WkjrYchV9GP09usNmYRJPreJUJKgeIgNUf+26lgJgl7gD17yJzJ0bEGkwmpMGiAaqzAtYxXWRu+4iSiWmgyNnrSsFEy8RYTf6SXfOBqj8OK79dOYLpJzAfQvdNvR7xTNCFZnAad3EUHx4N3LqerkyhX+QAylMzZNov2Mh9aYoBn27QM/o31CkQ57P1brQ8zTIPCYQz4xJ5IsCkRrzDoH0t1v9OXM3cJWk2yJBAjhwIdt9MNySQa0sq7avJQxVrtGOIITzQDJxvo7X1Ppv6t4ED66M6VSubnveBPKkvG15czb3QU267gZOYueAGosZSPZ5z/pGt7eZNLjxOLBpMnQdhYPo8jB5sUNBmH0YKqm0mFpOAGExf+1t7KioQytEbwoCUtozp81T9UCnOfOhui5IXGYImuZOON9Z9LgGLq5Q4dq0moEtyJx1vdH3OqA8g+iZb1MVLcpecrPzB5svqAnwOnoFJPKB4d7yA38GrwwqdTVQCZFPMIvuQl14W4C/PRos1YT31hwAAAABJRU5ErkJggg==");
                }

                return rotate;
            }
        }

        /// <summary>
        /// Gets an icon of a ruler symbol.
        /// </summary>
        public static EditorIcon Ruler
        {
            get
            {
                if (ruler == null)
                {
                    ruler = new LazyEditorIcon(34, 34, "iVBORw0KGgoAAAANSUhEUgAAACIAAAAiCAYAAAA6RwvCAAAA/ElEQVRYCe1WUQ6DMAi1y664S+0afnoJd5BlN1nH6yQhL+20zsZ+QNJYsMDzFQkhxjj0INcMiJCxwdQU8YWSzqK/M+sptlFWMwl0NaWvVsCl938D1ARrgcASWGkmWxkBAIA+nZGXgOiCEb3C0xnxGkFxWvEasWxg7zXCjDzE4H3EsuI1YtnA3vsIM+I1woxAByvNhKf4u2S6FbJNix1Tvp1LVN/7/IbF8Lxxzcs5eUSMmBD4Qvbqo/im/DyzFshIZrCASc1ekbKgfvx+TYcfYgy1QOBztCQgOoseHbw6ngNhyrphhPsIA7X6rx5jz9XstTdV/TU1CarPdnM1H34y22oHiYQYAAAAAElFTkSuQmCC");
                }

                return ruler;
            }
        }

        /// <summary>
        /// Gets an icon of a ruler rect symbol.
        /// </summary>
        public static EditorIcon RulerRect
        {
            get
            {
                if (rulerRect == null)
                {
                    rulerRect = new LazyEditorIcon(34, 34, "iVBORw0KGgoAAAANSUhEUgAAACIAAAAiCAYAAAA6RwvCAAABuElEQVRYCc1XQXLCMAxsOjyjF57RS+/9Gt9oX8I/2vykqRdYj5CVSLLDDJ4JtmVptVrbAaZlWV6eob0+AwlweDQRyG09v8X+BgJsFpHPsghHC+CLgYP9scSfJcZknBHv0EwSoGMs42suS5E1bAAQBMpYikVsfyWWD3JdtqlXkVoJkAYaCgPWnCVCRfYmkr41exFoRDw0lnUD1Vj3GFjJHFao8TBFMkRQL1VhTw30XOPqdT0fPiMakHMq5xFicVPm1jAJVdi1zxxWVpklxDiLeMXS0lnO0lYDpXFjvEXiLixLJAx8ywLiIfKZrQkB3pWZuO4ZRaBGjyKKmz3NEAFCVpUwcWtrssnsEq9WYrmELEUYZPUAtuxIq+204feG26gIQVgBA2nnHD9o0Oin1zGfLx7XD0lcmNsh36wEDAe2UI0lgsWC0t81TTbHgEQ12ZYvt2bLZ2SNSrsY1mF1gxIOITWARyLygCXyuK5pRT4K5J5kTjeK4TPCWyNLo5zhamSwGnsYzFW3Rsb/lAneFwCxHunrjb9XHOZif5drliL4c3wuz1E6inGtQtiGhxaRYdAeAN6anthdY/4BZGF6BtLaID8AAAAASUVORK5CYII=");
                }

                return rulerRect;
            }
        }

        /// <summary>
        /// Gets an icon of a settings cog symbol.
        /// </summary>
        public static EditorIcon SettingsCog
        {
            get
            {
                if (settingsCog == null)
                {
                    settingsCog = new LazyEditorIcon(34, 34, "iVBORw0KGgoAAAANSUhEUgAAACIAAAAiCAYAAAA6RwvCAAACBklEQVRYCe2XO04DQQyGN4hQUFIjpISKG0BFC9yBE9BRcoNwDTjIlpwAKoLERRZ/m53VxLFnnWwToVgaxm97Pf+MwqRpmmof6GgfmqCHQyP6JP7VRBbydSCefWeajLw1V1L5M6t+I/xHJofZsUfzoio9KTkuMpGBdSb2ZbfuO9+p7PAWocfOtOFrWcSTx60VORr94v3IZ84Cn6r9kOdeXORoSJBTpAn8o35t7kgju5973n5VFfNEjmYi+b63/cL1HqriseAbmchxoQmu64ksmmV/kGURxzS1DL2uhOTOdi27RaeitG4BeovIY/m3OtcgQQTWVsbORizX9K3zYUdG7zVPvvQE4NevnsmVwqfkwprUYkss2g+ZnNhLtBDjWm0PrE1/djYDJiDLr2RbRa3+Jr9W8sD6nEcYfEryrmxJLgOzqh5VnHySGlEmnwuvRy+qlhLwwARjhtgTRsCBRfiQd6PuhsJw8oC37a2hObeea8iCSsCjyQRcpuE1LaZ+WmZND6z6CJeimGnlFjIv66UsC9xtGg+seY1rEcY0QS7i72A8ikykluBbL8EWeqYy9/wjE7lQwSSMkPb7LQYJiEzwZHr9C40psjxgJgDjw02pu+Xdsrb+UBNDdv3OUHQoxrRHMFKa6N78iv+SLl+7Tnned/pXgvixE+l6GL9Fbs34KoEMh0b0kP4AAys2zJa23LUAAAAASUVORK5CYII=");
                }

                return settingsCog;
            }
        }

        /// <summary>
        /// Gets an icon of a shopping basket symbol.
        /// </summary>
        public static EditorIcon ShoppingBasket
        {
            get
            {
                if (shoppingBasket == null)
                {
                    shoppingBasket = new LazyEditorIcon(34, 34, "iVBORw0KGgoAAAANSUhEUgAAACIAAAAiCAYAAAA6RwvCAAABhklEQVRYCe1WTUrEUAy24mqWs/YC3slLzNxCF17Ci3TrDVyIA7oZERRxIwjWfCUpb/KS0lfypMgEhvx/+Zq2r9N0XXeyBDldAglwOBLRd+JfbGRDV4UnveWrg4aPeLHM3ciKJt3wtGfWd6wRX7M9XeH1nfG7oh6RNRnAgBZBvgi3qJjBVzKN9K0aaBGcNGNSUcGw2VspJTK2DcGatRVpFo1Ve4IaPeRCFZ+Tb21FlQ3uJVn9bCEgeqgwjIZiLcc3pNEjPoeHZwZ5CPLo8+SREsChF58N1qRcGdhTBcBlmG5AHfLAhoY/JiaRdqyjQg63uifSM59+6tSrPHOg/+JPSpPO9o74XVpUwc7wPSJPFYankBm+R0Q+YGlzpJ3he0T2kVMNrAzfI/JqNEeG7jWYR+RBFwb77xrPI/KhC4P9DN8j8hk8WMO96IBH5E0XBvtfGs8j8q0Lg/0M3/vW4Pj9CR6ewh0c70h4G8G35jrtDLS3Fpa3Eau2aszbSNWhFviRiN7KYjbyC5Six/hacRXWAAAAAElFTkSuQmCC");
                }

                return shoppingBasket;
            }
        }

        /// <summary>
        /// Gets an icon of a shopping cart symbol.
        /// </summary>
        public static EditorIcon ShoppingCart
        {
            get
            {
                if (shoppingCart == null)
                {
                    shoppingCart = new LazyEditorIcon(34, 34, "iVBORw0KGgoAAAANSUhEUgAAACIAAAAiCAYAAAA6RwvCAAABuklEQVRYCe1WPUsEMRC9FbXwsNFWEa6zuFpBUDvB0t4fpL/DTjiwtNPeThAL4UBsLLTTQsF1HuyE2WE2H7tZ2OICIW+S+Xg7mSRblGU5GkJbGgIJcFgQ0Tsx+IzsE2NUMfqFZt+HXBinZoUC/ahg2yS/qbmsorU16xRhrqIcKzm7aBH5pCgT6uci2onAvUBrazjQLoEnFjKPm+QPH+yalRFefGfQw/igffoyUpDynzbIKMO/a76M6Efokqxg3LaPXVQD+IhA/UrYPArcBn77jEJEboXxVODsMETkRUTcEzg79BUrgm1Q/xBRVwn/CjkFrpHylzCoFWuIiHXdC1+dYI1IaGvafn2IoX5Con6M7kNeE9dB4lDbLOsJQ76huaNqHneB9xga9lFToa2Bk2fhaUfgrDCGiLzI8AiWHfpdE/vQqYGdPnZNvmLna6eFjWJqBDWBq/6AjTqM1022MRlpss06H1MjMiB+lrhGtuSCgbEeqztKzQgcc5sTmLBgjFIXy2ZtsF1qRhCc2yuDhlHqSmyqp2YEj+Cs8nRKo+9yk7pnpFv7R618uCGViDPMDVK3Jnd8529BxKWiAv/grWHsl+pt4gAAAABJRU5ErkJggg==");
                }

                return shoppingCart;
            }
        }

        /// <summary>
        /// Gets an icon of a single user symbol.
        /// </summary>
        public static EditorIcon SingleUser
        {
            get
            {
                if (singleUser == null)
                {
                    singleUser = new LazyEditorIcon(34, 34, "iVBORw0KGgoAAAANSUhEUgAAACIAAAAiCAYAAAA6RwvCAAABx0lEQVRYCe1WoU4DQRDlSIooEtcgQJFaMMVUoXAIHAKH5gswhATZb+AnkIcgoQaDIBhqEChQICDheG+525Bl7nbm2muahkled+/mzey7t9dukyzLFmYhFmdBBDXMhZAeHiQFuLcE57xXL/iOGNEGPwXKgjlyTH0TFhiiBe4DsB6pGSG/AXxGeD5tfUdOURkTwebkkKsOiyN040Pd+Ye4hEHlisWRLaMI0tU1FiF7NYSoayxCOjWEqGssQmro0JdYhNzq23qmusYi5NK310/UNZavb4L1v/QaHJMPqvrFtDjChrsGIeSqRLie1jMB/KpzBmkX5JjOGhM5b87tvHDLyR/MuS3HqO5veUfCXenixiGwnyeuMZ4B9/m1adAK4aJrwB3wBGiCZ9MO8ArcRAsU9h0HO3CuqFkNaqLvTGwPe0HD4pJiWoBU3y1IwcgHkvjuXtXWaI79K1g+BJ6BTeAAqIoVJF9EQoVKPvWkI0VD0ZUyR9pQ/SYqH/+m6ErZL+vJ+OuVdhhIGcmRJt0oNCxj8l5ccJQcOfpNaGj+x3HJkUcszn/hTQdPcx+SI31kR54x+Ql7b4dtJUdCzlSuJUemsnC4yL+Q0JFvtqrrp7die4IAAAAASUVORK5CYII=");
                }

                return singleUser;
            }
        }

        /// <summary>
        /// Gets an icon of a smart phone symbol.
        /// </summary>
        public static EditorIcon SmartPhone
        {
            get
            {
                if (smartPhone == null)
                {
                    smartPhone = new LazyEditorIcon(34, 34, "iVBORw0KGgoAAAANSUhEUgAAACIAAAAiCAYAAAA6RwvCAAAA10lEQVRYCe1XwQ3CQAzjED+mgG1YAXUDdi5T8G5jRB+1aJzSe1xRIuUR2bn6fHk0ZRiGQwtxbEEENOxOSGeie0u845pED3p1YEZEdoYvBfcu8W4GMHdWFxBE4FYXwVHw0whXjxQRMikt3kEOFurf3bA6F64DpSPsYzqSjrADXOeMpCPsANc5I+kIO8B1zkizjpxYmVNP/54O5Xfor2bkbD4gN0XEEewkW0OeERFyFypehiO9eHjgG1Or4AfHythbrg30YGWdrZff6simJy9TgxB5mhrfkWeMLt0KTvIJgTEAAAAASUVORK5CYII=");
                }

                return smartPhone;
            }
        }

        /// <summary>
        /// Gets an icon of a sound symbol.
        /// </summary>
        public static EditorIcon Sound
        {
            get
            {
                if (sound == null)
                {
                    sound = new LazyEditorIcon(34, 34, "iVBORw0KGgoAAAANSUhEUgAAACIAAAAiCAYAAAA6RwvCAAACX0lEQVRYCe1WvUoDQRBOhAjGLnVAFJv0FlpZ6zPYW+tLSHwNwVdQsDkL0SdQbExhraWFgnG+vfmO7/aSmJ+7I0UGZmd2bv52dnf2msPhsLEMsLYMSSCHVSLxTtRVka4F7sfBdd6s4bA2LeCvBz0w+qQJkK+jIriW5x7w0WiLwXMUFakIW5HfxOaAvmEhZlVbgzPxbrhu+OMr7xj9cH7T6JfzgVS1NRce5E6CfRp/6fNTkafsqDJNkKGCZ4aAkSU2OcoOvTdDQM+QW9EJknQIu8FvVJiGts2IzulP7bp06hRzAGxUjz5y+qowid8PLosDbbA6wIkhZaAJhAaoBOXQAeR0+TGmcEzkVgTraKCdlhyVo5wL0KBd93Eleg0akGI/ZwHagXKlSJxyXGFAYhjLIKesEQ6MnOBZ/wnQNQloVN+GA8MdCo3Sp+oWZGVeX/QLJLEtSYCFbBxkyZVZEQQrrPQfGQoRbMqsSA+ZGNynJDdqVbIqmAYTL/wY4XWcB3A+nt1Qn/u2yx7E6YbzmlwhETzRyJiIt4Ivp9sXCHRfRXor/KHz1yLbcl6Ts9rIFZrAj7vWtGdv0HaO88cuqr2FfelI49HRNBQ9ITFUULv42UcgQK5xpaIwareduiIakI0LSalcee20GpCVjd+fuRLRgON4bol2WegmXpHctmBB4xwtKseZQVD1w2ogl9C/9Hvc0ORwL8ziNtlSA7D9Y3JseBOkMpTZ0MRtYJkEJvxTGxiv1zsoYqgykSyIMHvGa4LZpyq3JgtiDLZm1/BFhcrXlYjGHMnXvTUjk4BwlUhcmj++Yvhhd1O9bQAAAABJRU5ErkJggg==");
                }

                return sound;
            }
        }

        /// <summary>
        /// Gets an icon of a speech bubble round symbol.
        /// </summary>
        public static EditorIcon SpeechBubbleRound
        {
            get
            {
                if (speechBubbleRound == null)
                {
                    speechBubbleRound = new LazyEditorIcon(34, 34, "iVBORw0KGgoAAAANSUhEUgAAACIAAAAiCAYAAAA6RwvCAAAB2ElEQVRYCe1WMU4DMRDkEKEIXwgFZX4AFQ0UPIAG5RPhAfS8gIYuiIaCDgnRJBUVHaIMDTUNggIQYeY4n9Yr+2SMz6TIShPbu+vdiddnu5jNZkvzIMvzQIIcFkR0JVKsSIGgHYBttPyWCBP2gSEwBrjTv4D3quWYGAF7QBcIE341AejBZwTEyBiTOL8xT6MRkztALAFMteQYowJw5iwNnrVjCR48tlj1BBN3gQ8dwLdHNuGYmgRzbwM37GhxEenB6VY7JhyTzEDHc5VmCqcN7djCeBUx6xLpFeG+yEGC/2uHP0Y0kRNjyNAeyBy6NDyMckp9GssVqZU5mZhckojR/UsribAsjxlZTGQuSYT6I2lsuX8q4+vNytuVN2kOWUOSN5NIrwgPmC1jbLE9ROyaRJnHcxsOrXsz7WCKcGUlZG7nlVw5DNLmL6ORRLeKb+W2Bg4HPmg4OYXwXcP3jTOnU6mc+39kwT/CGI25VgI25EuAj3Y5g+IauAKetdE1DiGy75oI3TrwBPBqMHE+0Y+6r/Q5gjiW8BX+aml+BoaEwxSn0ueIjMLD7V4q0Of3zwcNVyKp+FaEb9bzKtMF2kvgDqhfVJUtWeMjkixBaKCm0oTGSOK3IKKX8RuO2BTapFsrVQAAAABJRU5ErkJggg==");
                }

                return speechBubbleRound;
            }
        }

        /// <summary>
        /// Gets an icon of a speech bubble square symbol.
        /// </summary>
        public static EditorIcon SpeechBubbleSquare
        {
            get
            {
                if (speechBubbleSquare == null)
                {
                    speechBubbleSquare = new LazyEditorIcon(34, 34, "iVBORw0KGgoAAAANSUhEUgAAACIAAAAiCAYAAAA6RwvCAAAA+klEQVRYCe1WwQ3CMAwkPMsgTMUQsABvmIMlGIIZOgV8eARfX5aTUwm4KEi2ZClJL+fTJVGdcs6rHmLdgwhoCCH2JP7CkUFUnyRxm70SfOAtIpFXA/C9QPstbITqoenY0Rw1aIFxwc8cwVEsHUkXYI5ozE/GIcTaHI6EI9YBO487Eo5YB+yc3ZGzBTrPD5aP/fSWbANGEbGVfGoxzBH0CugZvJ0BXyECgpgj+FYL1h7sBHypbXh3jTnC9teKfS0CxVqFXI1CFxGfCLkpIW4iJk40zw05CBaxl2zZN4udBVQKuotAjdZXo07Gd9h6WX2rK7YQosyYht048gKyPAXIdvlp5QAAAABJRU5ErkJggg==");
                }

                return speechBubbleSquare;
            }
        }

        /// <summary>
        /// Gets an icon of a speech bubbles round symbol.
        /// </summary>
        public static EditorIcon SpeechBubblesRound
        {
            get
            {
                if (speechBubblesRound == null)
                {
                    speechBubblesRound = new LazyEditorIcon(34, 34, "iVBORw0KGgoAAAANSUhEUgAAACIAAAAiCAYAAAA6RwvCAAAB+0lEQVRYCe2WPVLDMBCFMUMaGBpKJhSUaajpoA1cIdTUVHTU5A50cBBTwQ3owi0oCe/zWB79WbaDAinyZt5Y2l3vrlcrycVyudzZBOxuQhLksE3EX4ltRXJVpJAjmA19l2asiDOxFNnv3zUZL8S5OBFXB+dIgufSLcS+wHYipnxGdVGhHO2Lpbgq7vQivkkKFvW8LZ7qHGZPEjkwlZOR5YjkmMdiBomQ+ZClsOJEhwSeexqWO0jGF/AVOYG/ccThTDIntjORMmc1iF+K9vIgMyDJJn7VRPWe41xgW+YGftuu+APpvgjY9xzBdh24NU7tiiDjcDo1ygzPV/m4FNsqQggqFlTkAWFGPMnXqI8/vyJkl7NP6IEz8S2RTLQilPAk8dIQ1Y2MacTHxEsvjc7eQtb4SOPfbGUOMbYmh1cKxKm2cLOPjcB6smylOBTmfCBICuY+6kwEA76sD7Dj66ue03PS8RL2ThGciafEqY+pBLyDzr/AkJFAKaYQvWv2mmYJB3aTfUp9LX7UZjT1oXgvHos0+IXYhSsZvEeNlLpfFXNj0qyUkK80JfdtmaPHzm5uxs+iD2QxH8FvQNSo7eUe8tiuCW5e/OQOHPPHjxY7xAaVcapcTaJrth5hdYrKNT3mINWsjmGmSZCA8fvfvwEmj+D2bRR/PdhWxK/4D3ZhKVkFJo7XAAAAAElFTkSuQmCC");
                }

                return speechBubblesRound;
            }
        }

        /// <summary>
        /// Gets an icon of a speech bubbles square symbol.
        /// </summary>
        public static EditorIcon SpeechBubblesSquare
        {
            get
            {
                if (speechBubblesSquare == null)
                {
                    speechBubblesSquare = new LazyEditorIcon(34, 34, "iVBORw0KGgoAAAANSUhEUgAAACIAAAAiCAYAAAA6RwvCAAABxklEQVRYCe1XPS9EURD1JNvQrV+wURHRKKxKJCqNKHT8iFX6ObK1VumpUOgkKqvR0iFReM55WdeYnbvv+0Oyk5y4b+58nDtzZ28EURTNtUHm20CCHGZEdCf+RUU6YD0ARgBvdFbQj/5dIFECz9TQ+SXRO73BHkwvpplbRAI4PAK9aY459lbh8+Dzs+7IJozLJsH8pz4S1FsVCaHf5mYFwmqbYhGp8qeWHWDFb4E/eazWmIxLVF4j1qWO1wQRcmDr+5JM3URkOw6aIjKUibE+lN91XtYtJL4DPgUBN0W6NW5DGJexfEKQG2DXF0xXZAGGbz7jnHqSWAM+gC8Vwx1cV2RdGRb5vIIz35hlgCTOAL/w0RMIsbakD6W0S7vuwG8FGFlBZUwZsOsxJjnakUzZ4vLL1pwbdTuBbmesfzb2y1PhiGR1ND4qTz8AePr4IuOvY421r8TYyiUuh56apBNyqu6BXpJhyv1F2L3TVrYmjS+dOAWcBk5FUfmdUhRUlr6qte+i8yrEObO2pkgFQjjz1dWyBMVr1tboIFm+OX2cQi3xtNZZkR8C/DdlA+AjuA+wSsdNEEHeSamzNZPZhWZGRBQjXramIt91ty37ay/DygAAAABJRU5ErkJggg==");
                }

                return speechBubblesSquare;
            }
        }

        /// <summary>
        /// Gets an icon of a star pointer symbol.
        /// </summary>
        public static EditorIcon StarPointer
        {
            get
            {
                if (starPointer == null)
                {
                    starPointer = new LazyEditorIcon(34, 34, "iVBORw0KGgoAAAANSUhEUgAAACIAAAAiCAYAAAA6RwvCAAAB2ElEQVRYCe1WMVLDQAyMGdJkhiYUNGmgS0EDFU8AvpAvUEDDG6Cj4AXkDdThDRR0UFMAFTQUZjdzmpHXdnxnU7iwZjTRSjqdTrqTk+V5PuoDbfUhCeYwJKKdGCoyVEQroLjtHZki0FyDBUz9uMZWr+ZAS+AxfG/ARhkEv35qBvwuwGr3vgW5ACRole3VbUTxUtasxM5kquKUdCVFw8K5bETIKjHOjMARk46uSOodeUGTl9Lo44AvRH8OHP8hQ9apVZm4U1N8APPknrRljXs0OiB6lY8mw7tglJwEFo7WPZSSKpyJ4h34FzwBf4vtFvg66PiE95z9B/KnwwUxJpG6PttdWbiIV5CPwF5n5jcIBwb0t0siGqsJb0wk9dXoZgzu6cmDFHk7wvlMfJ6BeU92wB/BxoT2g8zDMS7vx2HQ8efLyWWRN7YF66vxg26FeOuWp8T9jyTsufrxz9mSFDvJOQTniT1xtDOOnyW0W4JRe8S8Gt/PU4BHr4CcBVw1V3Zhq50dPk7Kq+GG934x5BOHObA4RzzdebBRDmWNKh98WUFrAVuk62g34v8W+zKrXwmntsYOxfFNZhWU+EnYOM51AXHbRKpiddKl3JFOGzUtHhLRCg0V6W1F/gD/KgLMv0S3FwAAAABJRU5ErkJggg==");
                }

                return starPointer;
            }
        }

        /// <summary>
        /// Gets an icon of a stop symbol.
        /// </summary>
        public static EditorIcon Stop
        {
            get
            {
                if (stop == null)
                {
                    stop = new LazyEditorIcon(34, 34, "iVBORw0KGgoAAAANSUhEUgAAACIAAAAiCAYAAAA6RwvCAAAArUlEQVRYCe1WQQqAMAxz4gv8gT7E/79Cn1JbcJfQlU0mTEhhSLcmhKyTJhGZRoh5BBGmgULwJuhIiyOrFp+67Fn1WMZlnG6k4PkacHNR7zcvhe4ePBLy1Q8meULYrOgKHaEj6ADm7BE6gg5gzh6hI+gA5uyRXzpiY13vKHJGPXKoiiLwhULjMk43opk1A9wZMx82fCWqXaLD5ywkqMBXlURXU0XQq4hC0MlhHLkBqE8o/GmXLNEAAAAASUVORK5CYII=");
                }

                return stop;
            }
        }

        /// <summary>
        /// Gets an icon of a stretch symbol.
        /// </summary>
        public static EditorIcon Stretch
        {
            get
            {
                if (stretch == null)
                {
                    stretch = new LazyEditorIcon(34, 34, "iVBORw0KGgoAAAANSUhEUgAAACIAAAAiCAYAAAA6RwvCAAAB9klEQVRYCe1Xy00DMRBNECcQHYQD3EIFoYTtIR3QA+1BBXBLLjTAAa5hnpcnPc+ON7vOJsohIxnP53nmxR5by3y3283OQa7OgQQ4XIj4kyjtyNqAGxtooCkHciJ3V9CsbqzNPrY0ViCrO4fDCVg/ON/U5tYSPmrSiIgyuzXwry44QL+xtT+yfi76bB+RDKwLK3X9kVnuUrP6OkgQjZUAoUcYLS7wXB1KJF/VWs82vUsAOnxVUkvEk2DxajK1RL5ZOZj7YgG8dV0XI3GAW/9h4Scbnw62NBsx4t5cvGz6h8W9ZHx04F4JFjpkaYMY6BD1EUdMi2j/0pfmzLA4bBXGlYT6gF3YiEgojnqUO8UOfUdwZbn996Z/lfc+RfQqV70je/Kn8N0QUAkz9Nbow8VcaEzsBpoWA00Kn5fGO0LbDo3nxzk6R/i0T9gTkU+btflP1pc7xQjQuUSEZCISXM9bAgxJYB3j0Cn0pTkzDAFbhXH1QdedIIYzyegaxiJfig3tkfBYp3TWEkGTRg2MxuR1HsfT9orbxjnaPvWprkekPaEY6H25U+zQB23cr26/V7hm1IOGz7uppDdX1CNbqYxvzNJX11i/fq9qjVQuIvIqRI6lvnQSSyOxoTDjf5uNjakFOZFbaxWbtUP2FI7oaE5Rt1PjQsRvyR/DShOZie8xpAAAAABJRU5ErkJggg==");
                }

                return stretch;
            }
        }

        /// <summary>
        /// Gets an icon of a table symbol.
        /// </summary>
        public static EditorIcon Table
        {
            get
            {
                if (table == null)
                {
                    table = new LazyEditorIcon(34, 34, "iVBORw0KGgoAAAANSUhEUgAAACIAAAAiCAYAAAA6RwvCAAAA70lEQVRYCe2WUQoCMQxEt+L9L6H37DrBhTBM8tFWrZCCu9hHk8kkFlvv/dhh3XYQAQ0lhDtRjvyFI4+XSvyWv/lBTlvN3SMQ0N776pXxaXYXGRGUlxfIfAXb5x5RjvgK2Rl8z/goO1gI285CMj7KLAcLySpCoojPMCkEm6qySIAFWfHY5orn1qC4j1evHGQhqi3+XMZHmcVnIZkbSBTxGSaFYFNVFgmwICseNazsIs+Iaos/k/FRZvFZSDYLSBTxGSaFYFNVFgmwICseNazsop+RZ9AWf0a17eIjDDlt+f+s195P3tvMSAnh/pcj7MgJXDlPwPaDcZkAAAAASUVORK5CYII=");
                }

                return table;
            }
        }

        /// <summary>
        /// Gets an icon of a tag symbol.
        /// </summary>
        public static EditorIcon Tag
        {
            get
            {
                if (tag == null)
                {
                    tag = new LazyEditorIcon(34, 34, "iVBORw0KGgoAAAANSUhEUgAAACIAAAAiCAYAAAA6RwvCAAAByklEQVRYCe1WO07DQBDFCBooaaEIFAhOAFWUgiKcAQ6RY9BwCejo09CEigsgEA0U1FDSIGHekxhps97ZGcd25CIjPXk/b2ae9zN2UZblWh9svQ8iqGElJN6JjXgg6hdRX7qtHyxta26Qkcl+FXCOmAFjoLFpQvackYfgTYGJk6/SNCHicIkGt4c4l8HE8xpjXEVtKxMu80OFUkfeQBskAnM7cvaAyRFg8SoxNCESKH5DGa8ECgYo5gz4CcbMprU13HsR4z0HQ/i8Altm9oBgrUhAXai5A68vj6e1Ip4YOc4nJndzBJnrWgjzfAAnklB7LkMIcz8C2cK3LCEUky18mpB3enZgLHzJ26fdGl69J2DQgRi+5H4cV1uRbxAPgS5W5i4Wwb62IsLdROMeYJFqy7YRiC86Z9qKCIllegSwbLdhxwhSEcHAlhBySoBibtlpYKfwfVH9+fWtgStwF7ELOGXzZCcV50lNJRRv5jEJShC+ocdcIhAIB8ChVuGMDSUzxS+Z07q+6tn6n+DHjN+R2Fh/DgAedJc1FcIkR8BzkI0iWAxr/aG1IYQa+M8h15s/2claQaJmbQnR4rvHPQXNHawJcSUkXr3erMgfYXMWM9YtUwQAAAAASUVORK5CYII=");
                }

                return tag;
            }
        }

        /// <summary>
        /// Gets an icon of a test tube symbol.
        /// </summary>
        public static EditorIcon TestTube
        {
            get
            {
                if (testTube == null)
                {
                    testTube = new LazyEditorIcon(34, 34, "iVBORw0KGgoAAAANSUhEUgAAACIAAAAiCAYAAAA6RwvCAAABmUlEQVRYCe1XMU4DQQzkkKCAjpqGlLyAjpovwBNIQX5B6vwg8AbqQE2JRJcUeQNpkDhmIlZa+Zxb29kiQrHk7NqxZ53x3mavadv2YBfkcBeKYA21CjkB1gxKep+gR1CfsDUVdAaMXMYwXLgNEwLSICdPzOeEW0AHnFgl0poRwH+gHJM8p8nfOBF22fRSmPOf5ZJZtmMOHUHXTGM0t8ccmIHeYE7hGMlXc6J7pEy1MyKyR+QSc+mI2P+KkQgBnZxoa/jo8uzQdNxZxeCItkYeYHIpHnguiTLCA4ynpyavmrPkizKScCUzbiYSUJSRlF9t3BciqdwzUpORMwkG+1zxmVzR1vAxfVdWeIMv9AhHC3nEghdKIfRNFX/ZFbjcXPFWVBDGqBegTX7vycrXhq/yz1tHnOJzZYx1v9e8WIER9wE17xfPHuFf/7WjEO6XB2u8tTV8LJdWUBF3CftT+DqmpRDSy/eYbeQYyd99AJbW3PUBGL8bluIsjPCWzn5vI7xEDfoALIzcAmDTbawPO//uPje0uYURLa+67xclXu+dO1vCzQAAAABJRU5ErkJggg==");
                }

                return testTube;
            }
        }

        /// <summary>
        /// Gets an icon of a timer symbol.
        /// </summary>
        public static EditorIcon Timer
        {
            get
            {
                if (timer == null)
                {
                    timer = new LazyEditorIcon(34, 34, "iVBORw0KGgoAAAANSUhEUgAAACIAAAAiCAYAAAA6RwvCAAACvklEQVRYCe1XsVIUQRDlqCLRDEIwwDIAAhISjDAUEstY/ALKQEL+4GKKlKIKf8CEMjxCUwM1gkATf8BAqzzeW6an3vb17CwHwQV2Vd++6XnT09vTM7M3GI/Hc7Mg87MQBGP4H4hfiWkyMoCTdegQOoKyyEyvgGnfhpLXX1isPXUA3j70LvIe5AVodQ467xM1M/C1D7HA2YD9W6GvMfdZmn0wfRAfYHsOfQzlEpiyzUnZr8Lx9FOWStr8UozAXwzGlGzkq9BfuEyhMZHX1QMw17vEP+/o8y9DvxN+JgyJxNrxsg1DxH+UiKWi5DgvTW3CmP2VauSNLOYl8BPoa7Ep3EyNZ2oUfAC8BNW6Uf+3VI0qYZ+NaP3zm2AMl4XStXTkW+YaMn5aWVGHhrU2utbe+Ob4CsBspefQyHi2aiVampeS1hPBEVwU4yrwgrQjeCxGnSe89F4J+YvgCO45Y6lOjPbLAJ46Dy6JyXRK9qqpHikZuFYnnE8lz98UjERJGJ35PDkj8VzusBcRETbPJS37jWpE/ZCYydqR8LWz/XBtbXb7Qp5yehLWdJcOKRuzgjHcLRTusBpfjwbOY37CYv0or1Ervp/gPoXybd9C/0K7ZFk6dZ4wkE9CPhL8EPCdONF55qJi5dv9kwG82n9Le1rIM+aPDGZ95gKOipWdTLPJBQA/jM7NcMcnx61AT2Uc/ecgaI8y0tjxo1mhrfqVRVIg0dddKxscE2WEdkbLG1eldSRrRwVvuX6+UCsbTb9uoQDvwqbCLcdbNG+7hKMLjzy7mQEbmeoLzSbzwdAjJ+DtaecGbeSzTfsQ6qUYBIjFGnHZbIrVf0B7Tle7Wl+lGvFO+VeAXN1NnhO1D9O4zr8SHFjaNZFTs/GcWYOyeHmV70BNLgE+Q8+g36GTRQljJNMEEvm5t63v0tx7opqDmQnkBuzEptadzH7RAAAAAElFTkSuQmCC");
                }

                return timer;
            }
        }

        /// <summary>
        /// Gets an icon of a traffic stop light symbol.
        /// </summary>
        public static EditorIcon TrafficStopLight
        {
            get
            {
                if (trafficStopLight == null)
                {
                    trafficStopLight = new LazyEditorIcon(34, 34, "iVBORw0KGgoAAAANSUhEUgAAACIAAAAiCAYAAAA6RwvCAAABcElEQVRYCe1XwQ3CMAxsecKvG8AMjMMUrASDMAAzwBTwpPjUmByJEzUtjwrFUohjn11zCW5o+75vliCrJRSBGqYU0kncTQaotAZ8wBRJSSEnl/kq8zbzFPiAgWjMsMp94owkhpgjAZblIIvWDegsIVZ95vOQJFWn5WgFrPaN6M8geC3rh7MxlmGwR5LbmmOE/jaERcBr2TgqnROMZEYnvosMFaY7FWdhkQO5UjFNbmv0m4DKl1sw3SbFgtOtYyyYV7tL9T3ltkaRSICkIa04D6GENsRwQSH+sx7DyAfslOw3I3CKMYJ4dQwjih7fEzSi9pGmqX3E6gu1j/jfyHBGeF37CLMBfXzvybyI9NDxjMPJUu8jTL1197BsHBO+OL1PeLZ6gdrqfcRT5bXF3kfwv2Xr6zS1u1h3pidhLLmPaIq9KHhQSuADpkim3ND0AdgyS4puZppgCiMa+9P5Lwo5G5RYNgMWm+ackTjbDMtituYNbOUZ8FtgkOAAAAAASUVORK5CYII=");
                }

                return trafficStopLight;
            }
        }

        /// <summary>
        /// Gets an icon of a transparent symbol.
        /// </summary>
        public static EditorIcon Transparent
        {
            get
            {
                if (transparent == null)
                {
                    transparent = new LazyEditorIcon(34, 34, "iVBORw0KGgoAAAANSUhEUgAAACIAAAAiCAYAAAA6RwvCAAAATklEQVRYCe3SsQ0AIAwEMWD/nYNoPQHF0X0Xmdszs35454cj3g0d4k8kkogC7hpJRAF3jSSigLtGElHAXSOJKOCukUQUcNdIIgq4a0SRC9mOA0F8Erf/AAAAAElFTkSuQmCC");
                }

                return transparent;
            }
        }

        /// <summary>
        /// Gets an icon of a tree symbol.
        /// </summary>
        public static EditorIcon Tree
        {
            get
            {
                if (tree == null)
                {
                    tree = new LazyEditorIcon(34, 34, "iVBORw0KGgoAAAANSUhEUgAAACIAAAAiCAYAAAA6RwvCAAABkklEQVRYCe1WMU7EMBAkSNdcCT0SJS3VdbTwB6ipr0I0UEPPC46HuOEb0CA6PgBFmDlxJ2ezE1sXRwooK40uXo9355y1N1Vd13tjsP0xiKCGPytkAfErIABLYA7Q6A8A3zPBuQrIN9ZIJlbgefbgOeELQAVkxc8iIdgFsIstsCgrRxYJwV53URGt4W7OAZlPTmDRDLgE+opAiK1JMerUHKDKvgAW5nF+xSWZ14rhCWG1f6oFPf2nar0n5FyRC/g/VAxPyI0iF/AHFcMTcqTIBfxPiMH6a5kn5KXFKudg4bP+NjfyNrInhKqHtnubYH0FWyfGAThz/CVdjV6khMyQkffIkNYQ4r0aJv8G3gZU8WxjKyGWV3p8awN2Cbmz5MzxI3iHHdwrzL235tGOVONj/XgNL8DPzwLv+yRuamyayygG+SeAm891RmSK2XRgCuDzusB/OfhpWCqenFenprVzwsHPwtgaJyGeSD131UhqbdH5SYjdzmlHph2xO2DH/6ZG4i4aP9s/nBz3vVmTCXIJo3k1P48m2nRGRaQvAAAAAElFTkSuQmCC");
                }

                return tree;
            }
        }

        /// <summary>
        /// Gets an icon of a triangle down symbol.
        /// </summary>
        public static EditorIcon TriangleDown
        {
            get
            {
                if (triangleDown == null)
                {
                    triangleDown = new LazyEditorIcon(34, 34, "iVBORw0KGgoAAAANSUhEUgAAACIAAAAiCAYAAAA6RwvCAAAA/ElEQVRYCe3VMQ7CMAyFYcrKxm04Ir0GXA9G1uJ/QEJWQp6dpUiu1KHB7zV8RWXZtu2wh+O4h02wh9qIfxIlUiJewF//1W/kZrvnrZc9r/7bt64X4c16tuCjFRbXTjb3Gs0qj+ZpJfdRUefz1daHmyCriDCXVZE0uIEiwlxGRdbgBqoIs1EVWYNyVYTZiEpIg/KICPOqSkiD4ogI84pKWIPiqAiZkUpYg9KoCJlfKikNSjMi5HoqKQ0KMyLkWippDQqzImS9SlqDsqwI2W+VKQ3KZkTIf1SmNCiaESGPysVO6R+WQO+YFen1htdnRcI37AVqI16mRErEC/jrN6YOVdWgTQ39AAAAAElFTkSuQmCC");
                }

                return triangleDown;
            }
        }

        /// <summary>
        /// Gets an icon of a triangle left symbol.
        /// </summary>
        public static EditorIcon TriangleLeft
        {
            get
            {
                if (triangleLeft == null)
                {
                    triangleLeft = new LazyEditorIcon(34, 34, "iVBORw0KGgoAAAANSUhEUgAAACIAAAAiCAYAAAA6RwvCAAABHElEQVRYCe1WMQ6CQBAEoxaU1jba+QOtbP2D8RH6C96hX1BbX6GVseAhOFtcQjaAu8diKHaTCRx3NzvM7hHSsiyTIcRoCCJIgwvhlXBH3BHuAB97j/zTkQzJcp6wcUyfeGNMwHcEQoj4x40K9RMptuyBs35rklgIIQE74BYjIOzpemrWIHp3FUFiYh1ZYe8VWBCJRWgdmSPpA3gCZiLAJf4NmGEtNWEBbAHz0DpiLiAQpjjs4V5ypdJcAI0rdKp+hlZIINQ0q0hIbGleULQENsAnqOtyjXWkmpPeuO2D1qsjVSHUZHeA3D1UJzT3saWpy0GCqJGnwKluQeszOjU9IQNvLuW26JHWF5VOWpZGmrN2nQvhtrgj7gh3gI+/a4DvtGw2Wp0AAAAASUVORK5CYII=");
                }

                return triangleLeft;
            }
        }

        /// <summary>
        /// Gets an icon of a triangle right symbol.
        /// </summary>
        public static EditorIcon TriangleRight
        {
            get
            {
                if (triangleRight == null)
                {
                    triangleRight = new LazyEditorIcon(34, 34, "iVBORw0KGgoAAAANSUhEUgAAACIAAAAiCAYAAAA6RwvCAAABFElEQVRYCe1XSxLBQBAVhYWltQ2nYGXPFTgEt8g5cAUsuQQrZcE9xmtVk0q1VKUnPalk0V31mOn053nzSUmcc502WLcNJIiDEeErYYqYIlwBPrc9UlWRFIlDnhx1Tle8AAj52RaffUCSExSTUFGB8aANco4A9wtKFYdUJeKrLTG4AGpC2lNzAoknMANUplUk3/yFyQp45J3SsVaRfJ8JJnfgCoyBIItJxDdeYPAG9sDIO8u+6yBS1rPwea/Qq3PekL4GPiFlYhJpfLMSgTkwBSqdGFJOq0jjFxpd8bTRz4AD1BZ6anboOAAOQBQC2S8QvklTxA2FsUFvXV9TesVnxOsahC5NXTzsn96fsrY0XBJThCvyBWod+Lp+vcreAAAAAElFTkSuQmCC");
                }

                return triangleRight;
            }
        }

        /// <summary>
        /// Gets an icon of a triangle up symbol.
        /// </summary>
        public static EditorIcon TriangleUp
        {
            get
            {
                if (triangleUp == null)
                {
                    triangleUp = new LazyEditorIcon(34, 34, "iVBORw0KGgoAAAANSUhEUgAAACIAAAAiCAYAAAA6RwvCAAABGklEQVRYCe2WQQrCQAxFraAL3bnWhbrSE3gEPYN4A+/gYfRMutOFXkEXCtYfaKGEoUlmKnSRgaFNJpP/+zqUZnmed9owum0wQR7cCH8TTsSJcAI89jPyLyIjNKYZPbKGPvHHwsEu1kkTRsYQvxcGJrg+Ysw0cVhPFeHqfSUt36YSWUDizGSWiC8sJ4apRq5QmDKVG+IZy4lhyqtZoTs3QYKUozXTSCESolGKE5U5pvqvK5bIBiIhGqURWluXgeYaQyRD46+mOWroQVVUYohslSaoTF1rJdJD87fBCJX2MT/SHiuRvdQwsK7aYyEygMgzIKRJDVH0qiu0EDnUNRLWxL0WIoJW2rKFSJqSsNuNcEBOxIlwAjxuzRn5AeVIM5pI9JapAAAAAElFTkSuQmCC");
                }

                return triangleUp;
            }
        }

        /// <summary>
        /// Gets an icon of an undo symbol.
        /// </summary>
        public static EditorIcon Undo
        {
            get
            {
                if (undo == null)
                {
                    undo = new LazyEditorIcon(34, 34, "iVBORw0KGgoAAAANSUhEUgAAACIAAAAiCAYAAAA6RwvCAAABl0lEQVRYCe1WO07DQBDFiFCkjETnAsrcgI4apeACXAJu4Yo7JHScgIpcgB4apwgFCImSgsa8l0TWepnxfuKsLJSRnrw7MzvzPDPrJKuq6qAPctgHEuSwJ2J34t9U5Np+s9j9NhUpkHQWm9g+d2QrPPYZfKZAZ9VgztCK7IQEiYRUhCSegAse7FpCiLAdEok59EvgEXgGXoDwzzU/8R6YwSdECjiPAJ/YKx8fRwaNFZ7NAGcel8NNLAPjXIn1wEXGdWvY723lDAFeAQ67Kquyqda1IceDw6jJAoYH4B34BE6AK8Ae7Fvo7gBZXCXb2Id4ssSSaO1lO+zWqi3Sgkh6Vk+6PZKvqcsN9pdYm7Z6XS80B0HPYKb4xDjfHJjjKfr7zIjUU3NuWofQOFxizcEV/V23xojTWL5hdwzcN7Ttm0mbObYibTE12wCGH4Av/+cnICUREiSBTlvDoJ1K7IzEklBnKiURzgj/KoiSckbGYPABfElMUlbkVCNBYikrMkS+byaVJCURKX+tS9maOqm02BOxq9KbivwCbBTachSXy+wAAAAASUVORK5CYII=");
                }

                return undo;
            }
        }

        /// <summary>
        /// Gets an icon of an upload symbol.
        /// </summary>
        public static EditorIcon Upload
        {
            get
            {
                if (upload == null)
                {
                    upload = new LazyEditorIcon(34, 34, "iVBORw0KGgoAAAANSUhEUgAAACIAAAAiCAYAAAA6RwvCAAAB3klEQVRYCe2VPVLDMBCFE4Y0KWlo0gBVegroaOEITLgDqThDKLhEuANtqLgBZdIwtBwhvM9YHmUdRT9QZBjvzLOs1e7q+enH/fV63dsHO9gHEnDoiNiV6BT5V4oM9DUTYSFw9h1meh8LZcY9koGxYmO2UMBAyKnb65OQaHzte2LsSnFnQnLx1FPDcqSSgOuJcC8MhaUAIdqpQK22BSQcyT8TlsJQmAsldhRIupB/Y+k2OvXg1CRDqtRCRKjHfmvmt0uDdE9GtyvTz+ke7whmqfvNuMcqxJ7lKbVJJPFa45UqviIPDbvNFzZeqc0jibdu3D++yUfNJdftjdov4c34U7s/y+OkiUgYGmZjuw1nN3kox/qrfFeENtc43uRxFN1a48uxhYIrDtWj7uRsSgqwrJBw5sgwlmps5haR2A53xSHMv4SJreGDYOpHNf8kXxEKpBg3ra+EzYEME8SsUUOB+gnU0tTtKJLNuM0hBQWsP3QvET+z8TaZfowMhTA3uX2vBnc8UKw1r3+huXP/oRf83A+vzlm3K7XPwp1wXvv8Bv+j4PKIB/guBeq+CC3zL7TWYKJDC1xNdpoYvzXscKs3z4lCn3kp7ei/UKRdtcCzbY8UlPl9SkfEatgpsreKfAOUnn96w+68+AAAAABJRU5ErkJggg==");
                }

                return upload;
            }
        }

        /// <summary>
        /// Gets an icon of a wifi signal symbol.
        /// </summary>
        public static EditorIcon WifiSignal
        {
            get
            {
                if (wifiSignal == null)
                {
                    wifiSignal = new LazyEditorIcon(34, 34, "iVBORw0KGgoAAAANSUhEUgAAACIAAAAiCAYAAAA6RwvCAAAB+klEQVRYCe2WPU4DMRCFCSINHWlBSJQ5AUE0dIg7cAmOkXOEO0AZCiQqSkQFEuIe4X1LjMxk/LfbpMhIj43nx35+Yy87Wq1We9tg+9tAAg47IrYTW6PIgWWWGY8UOxauhGvhUjgTgn3qx7PwKjwK70L9TeDWFDBRfC70sYWKToTSGqKcToLAUijZRylBcebJEkoRuUtMjjJT4VDwavETT9XjH3m1drKxkmAfGzu+EdwJ5LdzhDH5M8Eqxph1Ql73/DdQkOK48Hbts3nxuESQOPMEqyLCAshLstfTsEtaFBPWsDMOJyps7Fg+zhw1POONdL83HF6SfEyc6rtCrkHWI+Su2cmq+56zqYJviQTeHV/CqRC/U+L0Cw1eYof7W6xdhms/MltDGVfetT8+D6GWmtw62fcIhUgbzkKL1Cgdt3JaIlLTmomkPBceXEnzzpnCR1W1JaaJODeKHQe1eKKYd9OyLVFNF69KCsl6oiBXNGfEq2+LcnsR8UgsHVb4mjbZlLzeKYvQCtuG+J9k6lYl16s5rPY48l2Ced8a4Zvl+zel/m8fIvWzN2QO+VQca52FgDJzgXF/az1UUb49uFzf5BkoxYa0JnVGeqkypDVPZsV7M24bliTLxPlu4SpjPJtfYqr5a+WQ1rTtuJA9pDWFqdvCOyJWr50iVpEfLA+dH6Is7xIAAAAASUVORK5CYII=");
                }

                return wifiSignal;
            }
        }

        /// <summary>
        /// Gets an icon of an x symbol.
        /// </summary>
        public static EditorIcon X
        {
            get
            {
                if (x == null)
                {
                    x = new LazyEditorIcon(34, 34, "iVBORw0KGgoAAAANSUhEUgAAACIAAAAiCAYAAAA6RwvCAAABTklEQVRYCe2WUQ7CIAyGnaeaR9CbeY7dwee96a1mf0IT0rVd2RaDCU0I0vKXj7I5hmVZLi3YtQUIMHQQeRK9InsqMpBooobXa5QJnDHmQgMtcviG19dpA8VmaqXdaeBpEMOc0pADuUydGchCJNDMg5EQrHdhPJCJMxi9BmNBcIon/VDXVJ15Mou9voTZguA86prp3IynCA/b24iV7kcevEqn8ftG/o8ay7tXKSkW3SXv1uvL6q3WWzkUsDNgXAisGQHBnCMwmxA1IHthQhC1ILUwYQiANPPRiz4jtdWgTSYLVyUKgoR7LQQTATkCwfCbMFsgZ0CEYP7iL5534vVlyaPVU09BddLK8DdzDdBuZ1ydshJyM1ZlZhKbtzSZRI41GA+C9RLGhcAJsNDrAcPHNEIUbJgLg9asBMVSvjRBvaj82NnMt6aDyJPvFZEV+QLtPDH5milIBAAAAABJRU5ErkJggg==");
                }

                return x;
            }
        }

        /// <summary>
        /// Gets a texture of a test inconclusive symbol.
        /// </summary>
        public static Texture2D TestInconclusive
        {
            get
            {
                if (testInconclusive == null)
                {
                    var bytes = Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAANElEQVQ4EWP8//8/AyWAiRLNIL1DxIDP58X/gzA27w4RL2BzOkxs4L3AODRSIk3TwcCHAQC4kxR1WozKsgAAAABJRU5ErkJggg==");
                    testInconclusive = TextureUtilities.LoadImage(16, 16, bytes);
                }

                return testInconclusive;
            }
        }

        /// <summary>
        /// Gets a texture of a test failed symbol.
        /// </summary>
        public static Texture2D TestFailed
        {
            get
            {
                if (testFailed == null)
                {
                    var bytes = Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAA/UlEQVQ4Ec2SvQ3CMBSEbcQ6KRghbRpoEFIGoKFgFgokBA00SCxASw0USKRhAKYIhfnOJCJAnCYNTzo/x/f+fLF1zpk21mmTrNw/L5BZm4IjyAton1avHbwCgXMC1+AMBqAPTmBTcGwx/YVvXI1JQQ7iKsd3BG7gAVJxoQmm1F5Gzh18Fxa6RrgdGIIFUEywQA9urwBZJXlE0YwjcYoJFhDnX1hNsjirRdZ9uZ/1wklC8h2vscvOZWDCRjGNIkooCcbUb6H5joEE9iJakXVGd/3GMZBgurPGVmedrag6wZtgAZEU0aOR2l4wvMaekbzFe2ssUAY1+dA7aMr54J6RCY1lipH5vwAAAABJRU5ErkJggg==");
                    testFailed = TextureUtilities.LoadImage(16, 16, bytes);
                }

                return testFailed;
            }
        }

        /// <summary>
        /// Gets a texture of a test normal symbol.
        /// </summary>
        public static Texture2D TestNormal
        {
            get
            {
                if (testNormal == null)
                {
                    var bytes = Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAA50lEQVQ4EaWSOwrCUBBFE7EV7N2GuAKx0kZwHyo2ik1A7NTGFViLhSkVa3+LEVxAvOfxCBIzscjAIW/unZlMPmGSJEGZqJRpprf0gGp2gyiKslJh/m+Dnrov4u3h3BVpFA1YqGoltqLh4bwWeC5+HsHr3GUgWuLlNS4HwRZXcROxtcFY5lR8Nyt1gTYTIzJrQFPemQIjTtKpMQcYfanM3xeSWRs85bUpMKIj/YFnDdjIW4o6RZlAw6PGHBDL24u76IuahzNvH4+awPqMeHPBgKHYCYJ8IlwzQtEA/KOHc25Y7yC3OE/8AJxtJ1c/LZIsAAAAAElFTkSuQmCC");
                    testNormal = TextureUtilities.LoadImage(16, 16, bytes);
                }

                return testNormal;
            }
        }

        /// <summary>
        /// Gets a texture of a test passed symbol.
        /// </summary>
        public static Texture2D TestPassed
        {
            get
            {
                if (testPassed == null)
                {
                    var bytes = Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAA6klEQVQ4EaWRvQ4BQRDHb4Wj9AIaiUKLQqXRK+g9gYqOd1BInEo8hOgUGqGQeAPReIFLJL6S9Vu5j2b3krOT/DJ7N/OfnZkVUkrHxjI2YqW1LpD9pwNxFB66PhRF2h0gbiPcglqem2oExAVEC1A2l035SVUA0RgqcIMJ0AfPqMM5OD0ohTHOVXiChG74X7sDWs1T+wEXqIEPO2jBmtY7+J+ZXuFF9AplWMEGlPgOA4hM24GK0kUDtwcX3pCDEbdP8ZEZl0jiiaxhkKnEZ5gF37ELl2HyLGwJPtR1OcYR4iuST8YRkmVx1LrAF/71gwD0Sjf1AAAAAElFTkSuQmCC");
                    testPassed = TextureUtilities.LoadImage(16, 16, bytes);
                }

                return testPassed;
            }
        }

        /// <summary>
        /// Gets a texture of a console info icon symbol.
        /// </summary>
        public static Texture2D ConsoleInfoIcon
        {
            get
            {
                if (consoleInfoicon == null)
                {
                    var bytes = Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAYAAABzenr0AAADYklEQVRYCe1Uu0tbcRS+eWoIYhJjWnxA1UqDmDbNJmLtYCYx/gO6SunQxXZot+JSqIqjDi66SOji5NYWiw6Cti4lBIqPqDRpUuIjCb7y6/ddjL29uVcjCA71wOHe33l837nn/s4xCCGkmxTjTZKT+7aA2w7ceAfMWlNgMBi0zLQ5oPegd6FWKAM5x0fQn9B1aBpasmgWoJHthq3F6/V29vb2Btrb25tcLpfNYrEYT05O8qlUKrewsPBjdnb2ayQS+YzY79Ak9HLhIlKrKstXU1Pzempq6ks2mz1CrK7QzzjGA8OnwtE+Au2iAh53dHS839zcjKtZl5eXxeDgoJibm1O7BOOZB0a/NqvCekEBd+rq6t7E4/HfRQwwTE5Oiu7ubjE0NCSOj4+LQpjHfFDxvujKRVPQMjw8HPJ4PE51kUSrrq6WrFar5Ha7JZPJVNRF5o2MjIQQ2qLLDodeAc6GhobOUCjUqpfMAjgtIJKfWnE9PT2twHkCn1PLT5teAVV+v7/WZrPZT09PpXw+/48y0eFwSGVlZXIHeFbHMI/5xIG7ijFaojeGJrvdzjmXgdV7gb/E6XRKlZWVciGMo00pPPPXnOGYlD7lu14BR4lEIstAfplaCA5guYiKigqJX0vVkjOcQy0fbXq/ILGysrK+s7OTMhqNRe3FrZfKy8ulrq4u+RdgGRXFMG97ezuFcV0Dz6+rFpBJJpMfx8fHl7DtZHB+dUHNZrOE4qSNjQ0pFovJl7Dg45NdY97ExMQStuQnkGf0CjgHVQKcBdsxZi/m5+ejaK/Y39+X9eDgQPA8MzMj2traxOjoqEAHzv2Mo595zAeWXZecDiVx4V2RcB8X7VU4HP5GUKxasbu7KzKZjIhGo2JsbExwI+ZyObG3tyf7Gcd45gHnvgJL+7VAqnyqIuvR8uf9/f3Ti4uLayTjF5OIwi2YTqflDuDexPr6+qYZD4x6FY7m0UBitajHDn5e1geY+6DP56vFbDc1Nzd7QGTC0xUMBr1o/WEgEHi3tbUVRmwEWjw+MBYJC1BrUdBfA3cDd/tDKDfcUxT7bGBg4MPq6moMI/kSNhu0dFGTa3XkEjQL/I8aGxvfokO8dDyXLtdQQIHMhZerkSOh1DtQILn2p94mvHYiPcDbAv7vDmACTX8A4mSm8MuVsAMAAAAASUVORK5CYII=");
                    consoleInfoicon = TextureUtilities.LoadImage(32, 32, bytes);
                }

                return consoleInfoicon;
            }
        }

        /// <summary>
        /// Gets a texture of a console warnicon symbol.
        /// </summary>
        public static Texture2D ConsoleWarnicon
        {
            get
            {
                if (consoleWarnicon == null)
                {
                    var bytes = Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAYAAABzenr0AAADgUlEQVRYCe1W3UtTYRx+zjmzFCnCID9S1E0JbOrmms6SdH6NqJvuu4swJGRGt4EEZVbQbQXddOVd/0LQXV1KHxgkhpEKUonpvk/P72yLoeecnS3BG194N34f7+95fh/vuym6ruMgl3qQ4IJ9SMBl1QJFUaxMu/USI7VbaSabzdv/tsCjabhJMI8ZoCOdsDLbDg53hDrx/N08Fi904gX9vcXOmOKYKUVXZB1h5tGFeazon6AvvsZqhQvTPFNpd84Mq9wWeMIB+Lw+nNY3gfYzqI30I0DwkltRDgGN8xmencJIekPDTrwJsTUXZiYwrKkYJokKuyrstpVDoI3Z9/rdSkPs+BR03yLS9Xfha1brxkPoI0DbbhA7uVQCFaqKyFwUg+mkS0XNZUBj22uuIpU6qty/hUHORoSAR+xAC22lEvAMdKHzXB9aMqk0EF/OvqWxZaQTKfh70DgchI8AjqtQCgHp/fDcHYwhSVwlA8Q+5wh8BBQq+RzNRjGickZIwvKRK7cCnoFuBIIBNAmQqjFM7GuWQHwp+6NCTj0+NAwFEKTV0Y1wWgEXsxp/dBtDmk4sAimSX/wHkOCbwVYYsugzUB9OY4izMk6PojfCKQF3sAPeUC/ciDEsMQkAJbFO8O/AzjoUqYi8X7QHg2jm69hFyc1tu5wQYPIYfRzldLPNxiKQwpNqigRiX1iFdc4ELUJAVhzgTRnjzIxQEmqWywkBd58XvvNB9p6BpfzGJqCKLWT+LADJjSxM3ka/Xj8aQ1746d1qiU5DMQIy+ZEHkwhrGWaSB5Bv3kLE2fAPM2zBdlYusIu/nJPz9LSsQjECbn8bOsL9vNc7DCO/+rmd/s3EqweY6ivg1CUkftImLcr70H8oBE9PO85Sa1kFOwJiG30yyQxk8CSwAOR2ij9COHkNqL8C1N0AtlXo4pf3EX/K965jnN0apWSKZaqks6zm7hZ0XwyQvWQvJc9vBtfkgn17Cay8AZaecgBZfwHN+8g3z0VCaA20G69jCzV7lt1rpWQYU0vlmEt/C5arirbN90i/DYN9hnYsZyz0E30aajJpxKC0dylWfz74n1CqM1F/Ap7qSlRldGME/0VQclcuf/OM6PzIy+LIq6luxbC9+gtLFJ8Rq5CeEcuOgBGDH7Xc8utWGFtsTpbwSnCvcWfMkrUk4CT6fvjYDeF+xC8a45DAYQX+ArKUOq1nEeeUAAAAAElFTkSuQmCC");
                    consoleWarnicon = TextureUtilities.LoadImage(32, 32, bytes);
                }

                return consoleWarnicon;
            }
        }

        /// <summary>
        /// Gets a texture of a console error icon symbol.
        /// </summary>
        public static Texture2D ConsoleErroricon
        {
            get
            {
                if (consoleErroricon == null)
                {
                    var bytes = Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAYAAABzenr0AAADNElEQVRYCe1WTUhUURT+pnEsEyNXVjBEhESBlviDSIVEa3cGgRARtBTX/a6DIl3UrgiXbaQf2rQKWgn9g0XgD2mWmlKOZTpvvH3fm3nDmzf3PcdAZuOBw7v3nnfO+c537rv3xYwxKKdsK2dy5d4CUBHWglgsFmaqo2EfNbh5PIdx2n7anG37LRSALQDXWpqB7k7gUAZY49xLatjLGNfwFHg9Cjzi8D11fREqm1o8WzuBm0tAiqXr27HqCDB3ELhK/8ZgDGse26LWAtLcyeR/gCWHiVdC9G8OFEHMHgAuM0aDP44tl7X6AIA2JWdTf6UDiVcTCbOaTBYA8kB8AGbqgWsEcNQD8T8AWk4Bt5aB5WDylXjcOP39xszMGKevrwCEGFKLPmbbcYUAXCY2CqCOG+4Ge76o5KqsQGtrTWZ4mDGNyQwNFdpy7woE2/F9D3CJIHbZAEQdRMnTwJFqoIYA3C2vbZ/XVApmbo4rXJuchL6AvC03JmAcBupOZhlIclokUZ9hhlQ62o4KHBTjOFgbH0echszYmPuO90363xUwigq1mREFwD1pFCAXRIHyImCZiQkk2G1nasp9J5hBc/mqFXnHwGBdAPzsIA2KIsaZ2CwswCEQvWMDIIZsDHrxIgHIMYoBVb42P795DAiAKgtrQXp6Gs7ICNIEIUZsDOR8Qzd7FAPxSqAiigFDAIsDA3DS6aLkxOMKY6gAhbF2IhQZHb6+AD7x+P2tyuQd1J29vdhNAFXd3S5TfrsyKjkPo9mX2YvpC6dFEgXg2xvgYQ9wj4FXRLHXDvfJ67qyowOJxkYkmprye0WJZRe1n3kt0//uPPCMU55pxRLT6WQT3/9A+wng7H3gPNHWqEpPKhsaUN3VhdTgIBweRhJF204dBX5cAO6MZ69m1kKbJVcpAOTbchzoeQBcZIIqVSlRMgESjWqT5juoTLpwDugnpMecvqO6YgNQym3o+bd1ALfZ0yUmMDZlk81zVr4fuE6nY56j97TdBaUy4MVo5V/GmXagXn32i1gQGwTwipU/4fCt366xjYGNAlCcvTk1mvjEOwbGuFbyP2EoAF/gTR2KubLKFoAtBsrOwD95xFIZWURx2AAAAABJRU5ErkJggg==");
                    consoleErroricon = TextureUtilities.LoadImage(32, 32, bytes);
                }

                return consoleErroricon;
            }
        }

        /// <summary>
        /// Gets a texture of an odin inspector logo symbol.
        /// </summary>
        public static Texture2D OdinInspectorLogo
        {
            get
            {
                if (odinInspectorLogo == null)
                {
                    var bytes = Convert.FromBase64String("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");
                    odinInspectorLogo = TextureUtilities.LoadImage(86, 75, bytes);
                }

                return odinInspectorLogo;
            }
        }

        /// <summary>
        /// Gets a texture of a scene asset icon symbol.
        /// </summary>
        public static Texture2D UnityLogo
        {
            get
            {
                if (unityLogo == null)
                {
                    var bytes = Convert.FromBase64String("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");
                    unityLogo = TextureUtilities.LoadImage(64, 64, bytes);
                }

                return unityLogo;
            }
        }

        /// <summary>
        /// Gets an icon representing a GameObject.
        /// </summary>
        public static Texture2D UnityGameObjectIcon
        {
            get
            {
                if (unityGameObjectIcon == null)
                {
                    unityGameObjectIcon = EditorGUIUtility.FindTexture("GameObject Icon");
                }

                return unityGameObjectIcon;
            }
        }

        /// <summary>
        /// Gets an icon of a unity info icon.
        /// </summary>
        public static Texture2D UnityInfoIcon
        {
            get
            {
                if (unityInfoIcon == null)
                {
                    unityInfoIcon = (Texture2D)typeof(EditorGUIUtility).GetMethod("LoadIcon", Flags.AllMembers).Invoke(null, new object[] { "console.infoicon" });

                    // TODO: We may need to fix this.
                    if (UnityVersion.IsVersionOrGreater(2019, 3) == false)
                    {
                        unityInfoIcon = unityInfoIcon.CropTexture(new Rect(3, 3, unityInfoIcon.height - 6, unityInfoIcon.height - 6));
                    }
                }

                return unityInfoIcon;
            }
        }

        /// <summary>
        /// Gets an icon of a unity warning icon.
        /// </summary>
        public static Texture2D UnityWarningIcon
        {
            get
            {
                if (unityWarningIcon == null)
                {
                    unityWarningIcon = (Texture2D)typeof(EditorGUIUtility).GetMethod("LoadIcon", Flags.AllMembers).Invoke(null, new object[] { "console.warnicon" });
                    //unityWarningIcon = unityWarningIcon.CropTexture(new Rect(3, 3, unityWarningIcon.height - 6, unityWarningIcon.height - 6));
                }

                return unityWarningIcon;
            }
        }

        /// <summary>
        /// Gets an icon of a unity error icon.
        /// </summary>
        public static Texture2D UnityErrorIcon
        {
            get
            {
                if (unityErrorIcon == null)
                {
                    unityErrorIcon = (Texture2D)typeof(EditorGUIUtility).GetMethod("LoadIcon", Flags.AllMembers).Invoke(null, new object[] { "console.erroricon" });
                    //unityErrorIcon = unityErrorIcon.CropTexture(new Rect(3, 3, unityErrorIcon.height - 6, unityErrorIcon.height - 6));
                }

                return unityErrorIcon;
            }
        }

        /// <summary>
        /// Gets an icon of a unity folder.
        /// </summary>
        public static Texture2D UnityFolderIcon
        {
            get
            {
                if (unityFolderIcon == null)
                {
                    unityFolderIcon = AssetPreview.GetMiniThumbnail(AssetDatabase.LoadAssetAtPath("Assets", typeof(UnityEngine.Object)));
                }

                return unityFolderIcon;
            }
        }
    }
}
#endif