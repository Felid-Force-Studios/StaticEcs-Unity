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
        public static GUIStyle ButtonStyleYellow {
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
        
        public static GUIStyle ButtonIconStyleGreen {
            [MethodImpl(AggressiveInlining)]
            get {
                _buttonIconStyleGreen ??= new(EditorStyles.iconButton) {
                    normal = { textColor = Color.green },
                    font = EditorStyles.boldFont,
                    fontSize = 12,
                    alignment = TextAnchor.MiddleCenter
                };
                return _buttonIconStyleGreen;
            }
        }

        private static GUIStyle _buttonIconStyleGreen;
        
        public static GUIStyle ButtonStyleGrey {
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
        
        public static GUIStyle ButtonStyleTheme {
            [MethodImpl(AggressiveInlining)]
            get {
                _buttonStyleTheme ??= new(GUI.skin.button) {
                    normal = { textColor = EditorGUIUtility.isProSkin ? Color.white : Color.black },
                    fontSize = 12,
                    alignment = TextAnchor.MiddleCenter
                };
                return _buttonStyleTheme;
            }
        }

        private static GUIStyle _buttonStyleTheme;
        
        public static GUIStyle ButtonIconStyleTheme {
            [MethodImpl(AggressiveInlining)]
            get {
                _buttonIconStyleTheme ??= new(EditorStyles.iconButton) {
                    normal = { textColor = EditorGUIUtility.isProSkin ? Color.white : Color.black },
                    font = EditorStyles.boldFont,
                    fontSize = 12,
                    alignment = TextAnchor.MiddleCenter
                };
                return _buttonIconStyleTheme;
            }
        }

        private static GUIStyle _buttonIconStyleTheme;
        
        public static GUIStyle ButtonStyleThemeMini {
            [MethodImpl(AggressiveInlining)]
            get {
                _buttonStyleThemeMini ??= new(GUI.skin.button) {
                    normal = { textColor = EditorGUIUtility.isProSkin ? Color.white : Color.black },
                    fontSize = 12,
                    alignment = TextAnchor.MiddleCenter
                };
                return _buttonStyleThemeMini;
            }
        }

        private static GUIStyle _buttonStyleThemeMini;
        #endregion
        
        public static GUIStyle HeaderStyleTheme {
            [MethodImpl(AggressiveInlining)]
            get {
                
                _headerStyleTheme ??= new GUIStyle(EditorStyles.label) {
                    fontSize = 14,
                    normal = { textColor = EditorGUIUtility.isProSkin ? Color.white : Color.black },
                    fontStyle = FontStyle.Bold
                };
                return _headerStyleTheme;
            }
        }
        
        private static GUIStyle _headerStyleTheme;
        
        public static GUIStyle LabelStyleThemeCenter {
            [MethodImpl(AggressiveInlining)]
            get {
                _labelStyleTThemeCenter ??= new GUIStyle(EditorStyles.label) {
                    alignment = TextAnchor.MiddleCenter,
                    padding = new RectOffset(0, 0, 0, 6),
                    normal = {
                        textColor = EditorGUIUtility.isProSkin ? Color.white : Color.black
                    }
                };
                return _labelStyleTThemeCenter;
            }
        }
        
        public static GUIStyle LabelStyleThemeCenterColor(Color color) {
            return new GUIStyle(LabelStyleThemeCenter) {
                normal = {
                    textColor = color
                }
            };
        }
        
        public static GUIStyle LabelStyleThemeLeftColor(Color color) {
            return new GUIStyle(EditorStyles.label) {
                normal = {
                    textColor = color
                }
            };
        }
        
        private static GUIStyle _labelStyleTThemeCenter;
        
        public static GUIStyle LabelStyleThemeBold {
            [MethodImpl(AggressiveInlining)]
            get {
                _labelStyleThemeBold ??= new GUIStyle(EditorStyles.boldLabel) {
                    normal = {
                        textColor = EditorGUIUtility.isProSkin ? Color.white : Color.black
                    }
                };
                return _labelStyleThemeBold;
            }
        }
        
        private static GUIStyle _labelStyleThemeBold;
        
        public static GUIStyle LabelStyleThemeBold2 {
            [MethodImpl(AggressiveInlining)]
            get {
                _labelStyleThemeBold2 ??= new GUIStyle(EditorStyles.boldLabel) {
                    padding = new RectOffset(0, 0, 2, 0), 
                    normal = {
                        background = null,
                        textColor = EditorGUIUtility.isProSkin ? Color.white : Color.black
                    },
                    hover = {
                        background = null,
                        textColor = EditorGUIUtility.isProSkin ? Color.white : Color.black
                    }
                };
                return _labelStyleThemeBold2;
            }
        }
        
        private static GUIStyle _labelStyleThemeBold2;
        
        public static GUIStyle LabelStyleGreyCenter {
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
        
        public static GUIStyle LabelStyleYellowCenter {
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
        
        public static GUIStyle IconButtonStretchedStyle {
            [MethodImpl(AggressiveInlining)]
            get {
                _iconButtonStretchedStyle ??= new GUIStyle(EditorStyles.iconButton) {
                    font = EditorStyles.boldLabel.font,
                    padding = new RectOffset(0, 0, 4, 0), 
                    // normal = {
                    //     textColor = EditorGUIUtility.isProSkin ? Color.white : Color.black
                    // },
                    // hover = {
                    //     textColor = EditorGUIUtility.isProSkin ? Color.white : Color.black
                    // },
                    fixedWidth = 0, 
                    fixedHeight = 0,
                    stretchWidth = true
                };
                return _iconButtonStretchedStyle;
            }
        }
        
        private static GUIStyle _iconButtonStretchedStyle;
        
        public static GUIStyle BoxStyle {
            [MethodImpl(AggressiveInlining)]
            get {
                if (_boxStyle == null) {
                    _boxLayout = new[] {
                        GUILayout.ExpandWidth(true), GUILayout.Height(1)
                    };
                    Texture2D _backgroundBox = new Texture2D(1, 1);
                    _backgroundBox.SetPixel(0, 0, EditorGUIUtility.isProSkin ? Color.gray : Color.black);
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
        private static GUILayoutOption[] _widthMinLayout = new GUILayoutOption[1];
        private static GUILayoutOption[] _widthLayoutLine = new[] { null, GUILayout.MaxHeight(EditorGUIUtility.singleLineHeight)};
        private static GUILayoutOption[] _widthLayoutExpandWidthFalse;
        private static GUILayoutOption[] _widthLayoutExpandWidthTrue;
        
        private static readonly Dictionary<float, (string d6, string simple)> _intD6StringCache = new();
        
        [MethodImpl(AggressiveInlining)]
        public static (string d6, string simple) IntToStringD6(int val) {
            if (!_intD6StringCache.TryGetValue(val, out var res)) {
                res = (val.ToString("D6", CultureInfo.InvariantCulture), val.ToString(CultureInfo.InvariantCulture));
                _intD6StringCache.Add(val, res);
            }

            return res;
        }

        
        private static readonly Dictionary<float, GUILayoutOption> _widthCache = new();
        private static readonly Dictionary<float, GUILayoutOption> _widthMinCache = new();
        
        [MethodImpl(AggressiveInlining)]
        public static void DrawHorizontalSeparator() {
            var boxStyle = BoxStyle;
            _boxLayout[0] = WidthInternal((int) (Math.Round((EditorGUIUtility.currentViewWidth - 30f) / (double) 5) * 5));
            GUILayout.Box(GUIContent.none, boxStyle, _boxLayout);
        }
        
        [MethodImpl(AggressiveInlining)]
        public static void DrawHorizontalSeparator(float width) {
            var boxStyle = BoxStyle;
            _boxLayout[0] = WidthInternal(width);
            GUILayout.Box(GUIContent.none, boxStyle, _boxLayout);
        }
        
        [MethodImpl(AggressiveInlining)]
        public static void DrawHorizontalSeparator(GUILayoutOption[] option) {
            var boxStyle = BoxStyle;
            _boxLayout[0] = option[0];
            GUILayout.Box(GUIContent.none, boxStyle, _boxLayout);
        }
        
        [MethodImpl(AggressiveInlining)]
        public static void DrawVerticalSeparator() {
            GUILayout.Box(GUIContent.none, BoxStyle, GUILayout.MaxHeight(float.MaxValue));
        }
        
        [MethodImpl(AggressiveInlining)]
        public static void DrawSeparator() {
            EditorGUILayout.LabelField(SeparatorContent, LabelStyleThemeCenter, WidthLine(10));
        }
        
        private static readonly GUIContent SeparatorContent = new("|");

        private static GUILayoutOption WidthInternal(float width) {
            if (!_widthCache.TryGetValue(width, out var w)) {
                w = GUILayout.Width(width);
                _widthCache.Add(width, w);
            }

            return w;
        }

        private static GUILayoutOption MinWidthInternal(float width) {
            if (!_widthMinCache.TryGetValue(width, out var w)) {
                w = GUILayout.MinWidth(width);
                _widthMinCache.Add(width, w);
            }

            return w;
        }

        public static GUILayoutOption[] Width(float width) {
            _widthLayout[0] = WidthInternal(width);
            return _widthLayout;
        }

        public static GUILayoutOption[] MinWidth(float width = 0) {
            _widthMinLayout[0] = MinWidthInternal(width);
            return _widthMinLayout;
        }

        public static GUILayoutOption[] WidthLine(float width) {
            _widthLayoutLine[0] = WidthInternal(width);
            return _widthLayoutLine;
        }
        
        public static GUILayoutOption[] ExpandWidthFalse() {
            _widthLayoutExpandWidthFalse ??= new[] { GUILayout.ExpandWidth(false) };
            return _widthLayoutExpandWidthFalse;
        }
        
        public static GUILayoutOption[] ExpandWidthTrue() {
            _widthLayoutExpandWidthTrue ??= new[] { GUILayout.ExpandWidth(true) };
            return _widthLayoutExpandWidthTrue;
        }
        
        public static readonly GUILayoutOption[] Width30Height20 = {
            GUILayout.Width(30), GUILayout.Height(20)
        };
        
        public static readonly GUILayoutOption[] MaxWidth600SingleLine = {
            GUILayout.MaxWidth(600), GUILayout.MaxHeight(EditorGUIUtility.singleLineHeight)
        };
        
        public static readonly GUILayoutOption[] MaxWidth600 = {
            GUILayout.MaxWidth(600),
        };
        
        public static readonly GUILayoutOption[] MaxWidth560 = {
            GUILayout.MaxWidth(560),
        };


        public static bool MenuButton => GUILayout.Button(_iconMenu ??= EditorGUIUtility.IconContent("_Menu"), EditorStyles.iconButton, ExpandWidthFalse());
        private static GUIContent _iconMenu;
        
        public static bool HierarchyButton => GUILayout.Button(_hierarchyMenu ??= EditorGUIUtility.IconContent("d_UnityEditor.SceneHierarchyWindow"), EditorStyles.iconButton, ExpandWidthFalse());
        private static GUIContent _hierarchyMenu;
        public static bool PlusButton => GUILayout.Button(EditorGUIUtility.IconContent("d_Toolbar Plus"), EditorStyles.iconButton, ExpandWidthFalse());
        public static bool FakeButton => GUILayout.Button("", EditorStyles.iconButton, ExpandWidthFalse());
        
        public static bool MinusButton => GUILayout.Button(EditorGUIUtility.IconContent("d_Toolbar Minus"), EditorStyles.iconButton, ExpandWidthFalse());
        public static bool PlusDropDownButton => GUILayout.Button(EditorGUIUtility.IconContent("d_Toolbar Plus"), _dropdownStyle ??= new GUIStyle("DropDown"), ExpandWidthFalse());
        private static GUIStyle _dropdownStyle;
        
        public static bool SettingButton => GUILayout.Button(EditorGUIUtility.IconContent("d_Preset.Context"), EditorStyles.iconButton, ExpandWidthFalse());
        public static bool TrashButton => GUILayout.Button(_iconTrash ??= EditorGUIUtility.IconContent("TreeEditor.Trash"), EditorStyles.iconButton, ExpandWidthFalse());
        public static bool TrashButtonExpand => GUILayout.Button(_iconTrash ??= EditorGUIUtility.IconContent("TreeEditor.Trash"), EditorStyles.iconButton, ExpandWidthTrue());
        private static GUIContent _iconTrash;
        
        public static bool ViewButton => GUILayout.Button(_iconView ??= EditorGUIUtility.IconContent("ViewToolOrbit"), EditorStyles.iconButton, ExpandWidthFalse());
        public static bool ViewButtonExpand => GUILayout.Button(_iconView ??= EditorGUIUtility.IconContent("ViewToolOrbit"), EditorStyles.iconButton, ExpandWidthTrue());
        private static GUIContent _iconView;
        
        public static bool LockButtonExpand => GUILayout.Button(_iconLock ??= EditorGUIUtility.IconContent("AssemblyLock"), EditorStyles.iconButton, ExpandWidthTrue());
        private static GUIContent _iconLock;
        
        public static void DrawToolbar<T>(T[] tabs, ref T current, Func<T, string> tabName) {
            GUILayout.BeginHorizontal(EditorStyles.toolbar);
            {
                for (var i = 0; i < tabs.Length; i++) {
                    var tab = tabs[i];
                    if (GUILayout.Toggle(current.Equals(tab), tabName(tab), Ui.ButtonStyleThemeMini, Ui.WidthLine(90))) {
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
        
        public static GuiEnabledScope EnabledScopeVal(bool val) => new(val);
        public static GuiEnabledScope EnabledScope => new(true);
        public static GuiEnabledScope DisabledScope => new(false);

        public readonly struct GuiEnabledScope : IDisposable {
            private readonly bool _old;

            public GuiEnabledScope(bool val) {
                _old = GUI.enabled;
                GUI.enabled = val;
            }

            public void Dispose() {
                GUI.enabled = _old;
            }
        }
    }
}