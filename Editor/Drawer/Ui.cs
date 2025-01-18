using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;
using static System.Runtime.CompilerServices.MethodImplOptions;

namespace FFS.Libraries.StaticEcs.Unity.Editor {
    public static class Ui {

        #region BUTTONS
        internal static GUIStyle ButtonStyleYellow {
            [MethodImpl(AggressiveInlining)]
            get {
                _buttonStyleRed ??= new(GUI.skin.button) {
                    normal = { textColor = Color.yellow },
                    fontSize = 12,
                    alignment = TextAnchor.MiddleCenter,
                    fontStyle = FontStyle.Bold
                };
                return _buttonStyleRed;
            }
        }

        private static GUIStyle _buttonStyleRed;
        
        internal static GUIStyle ButtonStyleGreen {
            [MethodImpl(AggressiveInlining)]
            get {
                _buttonStyleGreen ??= new(GUI.skin.button) {
                    normal = { textColor = Color.green },
                    fontSize = 12,
                    alignment = TextAnchor.MiddleCenter
                };
                return _buttonStyleGreen;
            }
        }

        private static GUIStyle _buttonStyleGreen;
        
        internal static GUIStyle ButtonStyleGrey {
            [MethodImpl(AggressiveInlining)]
            get {
                _buttonStyleGrey ??= new(GUI.skin.button) {
                    normal = { textColor = Color.grey },
                    fontSize = 12,
                    alignment = TextAnchor.MiddleCenter
                };
                return _buttonStyleGrey;
            }
        }

        private static GUIStyle _buttonStyleGrey;
        
        internal static GUIStyle ButtonStyleWhite {
            [MethodImpl(AggressiveInlining)]
            get {
                _buttonStyleWhite ??= new(GUI.skin.button) {
                    normal = { textColor = Color.white },
                    fontSize = 12,
                    alignment = TextAnchor.MiddleCenter
                };
                return _buttonStyleWhite;
            }
        }

        private static GUIStyle _buttonStyleWhite;
        
        internal static GUIStyle ButtonStyleWhiteMini {
            [MethodImpl(AggressiveInlining)]
            get {
                _buttonStyleWhiteMini ??= new(GUI.skin.button) {
                    normal = { textColor = Color.white },
                    fontSize = 12,
                    alignment = TextAnchor.MiddleCenter
                };
                return _buttonStyleWhiteMini;
            }
        }

        private static GUIStyle _buttonStyleWhiteMini;
        #endregion
        
        internal static GUIStyle HeaderStyleWhite {
            [MethodImpl(AggressiveInlining)]
            get {
                _headerStyleWhite ??= new GUIStyle(EditorStyles.label) {
                    fontSize = 14,
                    normal = { textColor = Color.white },
                    fontStyle = FontStyle.Bold
                };
                return _headerStyleWhite;
            }
        }
        
        private static GUIStyle _headerStyleWhite;
        
        internal static GUIStyle LabelStyleWhiteCenter {
            [MethodImpl(AggressiveInlining)]
            get {
                _labelStyleWhiteCenter ??= new GUIStyle(EditorStyles.label) {
                    alignment = TextAnchor.MiddleCenter,
                    normal = {
                        textColor = Color.white
                    }
                };
                return _labelStyleWhiteCenter;
            }
        }
        
        private static GUIStyle _labelStyleWhiteCenter;
        
        internal static GUIStyle LabelStyleWhiteBold {
            [MethodImpl(AggressiveInlining)]
            get {
                _labelStyleWhiteBold ??= new GUIStyle(EditorStyles.boldLabel) {
                    normal = {
                        textColor = Color.white
                    }
                };
                return _labelStyleWhiteBold;
            }
        }
        
        private static GUIStyle _labelStyleWhiteBold;
        
        internal static GUIStyle LabelStyleGreyCenter {
            [MethodImpl(AggressiveInlining)]
            get {
                _labelStyleGreyCenter ??= new GUIStyle(EditorStyles.label) {
                    alignment = TextAnchor.MiddleCenter,
                    normal = {
                        textColor = Color.grey
                    }
                };
                return _labelStyleGreyCenter;
            }
        }
        
        private static GUIStyle _labelStyleGreyCenter;
        
        internal static GUIStyle LabelStyleYellowCenter {
            [MethodImpl(AggressiveInlining)]
            get {
                _labelStyleYellowCenter ??= new GUIStyle(EditorStyles.label) {
                    alignment = TextAnchor.MiddleCenter,
                    normal = {
                        textColor = Color.yellow
                    }
                };
                return _labelStyleYellowCenter;
            }
        }
        
        private static GUIStyle _labelStyleYellowCenter;
        
        internal static GUIStyle BoxStyle {
            [MethodImpl(AggressiveInlining)]
            get {
                if (_boxStyle == null) {
                    _boxLayout = new[] {
                        GUILayout.ExpandWidth(true), GUILayout.Height(1)
                    };
                    Texture2D _backgroundBox = new Texture2D(1, 1);
                    _backgroundBox.SetPixel(0, 0, Color.gray);
                    _backgroundBox.Apply();
                    _boxStyle = new GUIStyle(GUI.skin.box) {
                        normal = {
                            textColor = Color.gray,
                            background = _backgroundBox
                        }
                    };
                }

                return _boxStyle;
            }
        }
        
        private static GUIStyle _boxStyle;
        private static GUILayoutOption[] _boxLayout;
        private static GUILayoutOption[] _widthLayout = new GUILayoutOption[1];
        private static GUILayoutOption[] _widthLayoutLine = new[] { null, GUILayout.MaxHeight(EditorGUIUtility.singleLineHeight)};
        
        private static readonly Dictionary<float, (string d6, string simple)> _intD6StringCache = new();
        
        [MethodImpl(AggressiveInlining)]
        internal static (string d6, string simple) IntToStringD6(int val) {
            if (!_intD6StringCache.TryGetValue(val, out var res)) {
                res = (val.ToString("D6", CultureInfo.InvariantCulture), val.ToString(CultureInfo.InvariantCulture));
                _intD6StringCache.Add(val, res);
            }

            return res;
        }

        
        private static readonly Dictionary<float, GUILayoutOption> _widthCache = new();
        
        [MethodImpl(AggressiveInlining)]
        internal static void DrawHorizontalSeparator(float width) {
            var boxStyle = BoxStyle;
            _boxLayout[0] = WidthInternal(width);
            GUILayout.Box(GUIContent.none, boxStyle, _boxLayout);
        }
        
        [MethodImpl(AggressiveInlining)]
        internal static void DrawHorizontalSeparator(GUILayoutOption[] option) {
            var boxStyle = BoxStyle;
            _boxLayout[0] = option[0];
            GUILayout.Box(GUIContent.none, boxStyle, _boxLayout);
        }
        
        [MethodImpl(AggressiveInlining)]
        internal static void DrawVerticalSeparator() {
            GUILayout.Box(GUIContent.none, BoxStyle, GUILayout.MaxHeight(float.MaxValue));
        }
        
        [MethodImpl(AggressiveInlining)]
        internal static void DrawSeparator() {
            EditorGUILayout.LabelField(SeparatorContent, LabelStyleWhiteCenter, WidthLine(10));
        }
        
        private static readonly GUIContent SeparatorContent = new("|");

        private static GUILayoutOption WidthInternal(float width) {
            if (!_widthCache.TryGetValue(width, out var w)) {
                w = GUILayout.Width(width);
                _widthCache.Add(width, w);
            }

            return w;
        }

        internal static GUILayoutOption[] Width(float width) {
            _widthLayout[0] = WidthInternal(width);
            return _widthLayout;
        }

        internal static GUILayoutOption[] WidthLine(float width) {
            _widthLayoutLine[0] = WidthInternal(width);
            return _widthLayoutLine;
        }
        
        internal static readonly GUILayoutOption[] Width30Height20 = {
            GUILayout.Width(30), GUILayout.Height(20)
        };
        
        internal static readonly GUILayoutOption[] MaxWidth600SingleLine = {
            GUILayout.MaxWidth(600), GUILayout.MaxHeight(EditorGUIUtility.singleLineHeight)
        };
        
        internal static readonly GUILayoutOption[] MaxWidth600 = {
            GUILayout.MaxWidth(600),
        };
        
        internal static readonly GUILayoutOption[] MaxWidth1200 = {
            GUILayout.MaxWidth(1200),
        };
        
        internal static GUIContent IconTrash {
            [MethodImpl(AggressiveInlining)]
            get {
                _iconTrash ??= EditorGUIUtility.IconContent("TreeEditor.Trash");
                return _iconTrash;
            }
        }

        private static GUIContent _iconTrash;
        
        internal static GUIContent IconLockOn {
            [MethodImpl(AggressiveInlining)]
            get {
                _iconLockOn ??= EditorGUIUtility.IconContent("LockIcon-On");
                return _iconLockOn;
            }
        }

        private static GUIContent _iconLockOn;
        
        internal static GUIContent IconLockOff {
            [MethodImpl(AggressiveInlining)]
            get {
                _iconLockOff ??= EditorGUIUtility.IconContent("LockIcon");
                return _iconLockOff;
            }
        }

        private static GUIContent _iconLockOff;
        
        internal static GUIContent IconView {
            [MethodImpl(AggressiveInlining)]
            get {
                _iconView ??= EditorGUIUtility.IconContent("ViewToolOrbit");
                return _iconView;
            }
        }

        private static GUIContent _iconView;
        
        internal static GUIContent IconMenu {
            [MethodImpl(AggressiveInlining)]
            get {
                _iconMenu ??= EditorGUIUtility.IconContent("_Menu");
                return _iconMenu;
            }
        }

        private static GUIContent _iconMenu;
        
        public static void DrawToolbar<T>(T[] tabs, ref T current, Func<T, string> tabName) {
            GUILayout.BeginHorizontal(EditorStyles.toolbar);
            {
                for (var i = 0; i < tabs.Length; i++) {
                    var tab = tabs[i];
                    if (GUILayout.Toggle(current.Equals(tab), tabName(tab), Ui.ButtonStyleWhiteMini, Ui.WidthLine(90))) {
                        if (!current.Equals(tab)) {
                            GUI.FocusControl("");
                            current = tab;
                        }
                    }
                }
            }
            GUILayout.EndHorizontal();
            EditorGUILayout.Space();
        }
    }
}