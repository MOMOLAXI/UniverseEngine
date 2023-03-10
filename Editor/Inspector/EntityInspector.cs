using System.Collections.Generic;
using System.Globalization;
using UnityEditor;
using UnityEngine;

namespace Universe
{
    [CustomEditor(typeof(Entity), true)]
    public class EntityInspector : Editor
    {
        static readonly List<EntityProperty> s_VarList = new();
        static string s_SeekPropStr = string.Empty;
        static string s_SeekRecStr = string.Empty;

        static readonly string[] s_CriticalProps =
        {
            "ConfigID",
            "Name",
            "RoleName",
        };

        const int LABEL_HEIGHT = 16;
        const int LINE_HEIGHT = 18;
        int m_ContainNum = 10; // list view show num
        float m_PropBarValue = 100;
        // float[] m_RecBarValueArr;
        readonly GUILayoutOption m_LabelHeightLayoutOp = GUILayout.Height(LABEL_HEIGHT);

        Entity igo;
        readonly List<string> m_PropNameList = new();
        readonly List<string> m_PropSearchResultList = new();
        readonly List<string> m_RecoNameList = new();
        readonly List<int> m_RecoSearchResultList = new();

        private void OnEnable()
        {
            InitData();
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            EditorGUILayout.Space();

            if (IsDataInvalid())
            {
                return;
            }

            DrawSetPropArea();

            // 属性
            if (DrawHeader("PropArea"))
            {
                // 列表数量调节
                GUI.changed = false;
                m_ContainNum = EditorGUILayout.IntSlider("list view show num:", m_ContainNum, 5, 50); // scroll view 显示数量
                if (GUI.changed)
                {
                    EditorPrefs.SetInt("list view show num", m_ContainNum);
                }

                // 搜索区
                GUI.changed = false;
                bool showResearch;
                EditorGUILayout.BeginHorizontal();
                s_SeekPropStr = GUILayout.TextField(s_SeekPropStr);
                if (GUILayout.Button("清空搜索", GUILayout.MaxWidth(80)))
                {
                    s_SeekPropStr = string.Empty;
                }
                EditorGUILayout.EndHorizontal();
                showResearch = !string.IsNullOrEmpty(s_SeekPropStr);
                if (GUI.changed)
                {
                    GUI.changed = false;
                    if (showResearch)
                    {
                        RefreshPropSearchResult();
                        m_PropBarValue = 100;
                    }
                    else if (m_PropSearchResultList.Count > 0)
                    {
                        m_PropSearchResultList.Clear();
                        m_PropBarValue = 100;
                    }
                }

                // view
                if (showResearch)
                {
                    ShowPropListView(m_PropSearchResultList, ref m_PropBarValue);
                }
                else
                {
                    ShowPropListView(m_PropNameList, ref m_PropBarValue);
                }
            }

            // Record
            // if (DrawHeader("Rec Area"))
            // {
            //     // 搜索区
            //     GUI.changed = false;
            //     bool showResearch;
            //     EditorGUILayout.BeginHorizontal();
            //     m_SeekRecStr = GUILayout.TextField(m_SeekRecStr);
            //     if (GUILayout.Button("清空搜索", GUILayout.MaxWidth(80)))
            //     {
            //         m_SeekRecStr = string.Empty;
            //     }
            //     EditorGUILayout.EndHorizontal();
            //     showResearch = !string.IsNullOrEmpty(m_SeekRecStr);
            //     if (GUI.changed)
            //     {
            //         GUI.changed = false;
            //         if (showResearch)
            //         {
            //             RefreshRecoSearchResult();
            //             for (int i = 0; i < m_RecBarValueArr.Length; i++)
            //             {
            //                 m_RecBarValueArr[i] = 100;
            //             }
            //         }
            //         else if (m_RecoSearchResultList.Count > 0)
            //         {
            //             m_RecoSearchResultList.Clear();
            //             for (int i = 0; i < m_RecBarValueArr.Length; i++)
            //             {
            //                 m_RecBarValueArr[i] = 100;
            //             }
            //         }
            //     }
            // }
        }

        void InitData()
        {
            ClearData();
            m_ContainNum = EditorPrefs.GetInt("list view show num", m_ContainNum);

            igo = target as Entity;
            if (igo == null)
            {
                return;
            }

            EntityID objectID = igo.ID;

            // 属性数据
            s_VarList.Clear();
            if (!objectID.GetAllProperties(s_VarList))
            {
                return;
            }

            for (int i = 0; i < s_CriticalProps.Length; ++i)
            {
                string key = s_CriticalProps[i];
                if (!igo.HasProp(key))
                {
                    continue;
                }

                m_PropNameList.Add(key);
            }

            for (int i = 0; i < s_VarList.Count; ++i)
            {
                string key = s_VarList[i].Name;
                if (key == "Script" || IsCriticalProp(key))
                {
                    continue;
                }

                m_PropNameList.Add(key);
            }

            // // Record 数据
            // m_VarList.Clear();
            // if (!igo.GetRecordList(m_VarList))
            // {
            //     return;
            // }
            //
            // Sort(m_VarList);
            //
            // for (int i = 0; i < m_VarList.count; i++)
            // {
            //     m_RecoNameList.Add(m_VarList.GetString(i));
            // }
            //
            // m_RecBarValueArr = new float[m_RecoNameList.Count];
            // for (int i = 0; i < m_RecBarValueArr.Length; i++)
            // {
            //     m_RecBarValueArr[i] = 100;
            // }
        }

        void ClearData()
        {
            m_PropBarValue = 100;
            // m_RecBarValueArr = null;

            s_VarList.Clear();
            s_SeekPropStr = string.Empty;
            s_SeekRecStr = string.Empty;
            igo = null;
            m_PropNameList.Clear();
            m_PropSearchResultList.Clear();
            m_RecoNameList.Clear();
            m_RecoSearchResultList.Clear();
        }

        bool IsDataInvalid()
        {
            if (igo == null)
            {
                return true;
            }

            // if (m_RecBarValueArr == null || m_RecBarValueArr.Length != m_RecoNameList.Count)
            // {
            //     return true;
            // }

            return false;
        }

        /// <summary>
        /// 属性搜索刷新
        /// </summary>
        void RefreshPropSearchResult()
        {
            m_PropSearchResultList.Clear();
            for (int i = 0; i < m_PropNameList.Count; i++)
            {
                if (m_PropNameList[i].ToLower().Contains(s_SeekPropStr.ToLower()))
                {
                    m_PropSearchResultList.Add(m_PropNameList[i]);
                }
            }
        }

        /// <summary>
        /// Record 搜索刷新
        /// </summary>
        void RefreshRecoSearchResult()
        {
            m_RecoSearchResultList.Clear();
            for (int i = 0; i < m_RecoNameList.Count; i++)
            {
                if (m_RecoNameList[i].ToLower().Contains(s_SeekRecStr.ToLower()))
                {
                    m_RecoSearchResultList.Add(i);
                }
            }
        }

        /// <summary>
        /// 显示属性 List View
        /// </summary>
        /// <param name="propList"></param>
        /// <param name="barValue"></param>
        void ShowPropListView(List<string> propList, ref float barValue)
        {
            if (propList == null || propList.Count < 1) return;

            float ratio;
            int from, to;

            if (propList.Count <= m_ContainNum)
            {
                from = 0;
                to = propList.Count;

                EditorGUILayout.BeginVertical();
                for (int i = from; i < to; i++)
                {
                    ShowProp(igo, propList[i]);
                }
                EditorGUILayout.EndVertical();
            }
            else
            {
                ratio = Mathf.Clamp01(1 - barValue / 100f);
                from = Mathf.FloorToInt(ratio * (propList.Count - m_ContainNum));
                to = Mathf.Min(from + m_ContainNum, propList.Count);

                EditorGUILayout.BeginHorizontal();

                float size, top, bottom;
                ratio = 1f * m_ContainNum / propList.Count;
                size = Mathf.CeilToInt(ratio * 100);
                top = 100 + size;
                bottom = 0;
                barValue = GUILayout.VerticalScrollbar(barValue, size, top, bottom, GUILayout.Height(LINE_HEIGHT * m_ContainNum));

                EditorGUILayout.BeginVertical();
                for (int i = from; i < to; i++)
                {
                    ShowProp(igo, propList[i]);
                }
                EditorGUILayout.EndVertical();

                EditorGUILayout.EndHorizontal();
            }
        }

        // void DumpTarget(IKernel kernel, EntityID self)
        // {
        //     XmlDocument doc = new();
        //
        //     XmlDeclaration dec = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
        //     doc.AppendChild(dec);
        //
        //     LocalHelper.DumpObject2Xml(kernel, self, doc, doc);
        //
        //     string saveFilePath = FilePath.persistentDataPath4Temp + "TempSoulObjectTree.xml";
        //     doc.Save(saveFilePath);
        //
        //     Application.OpenURL(saveFilePath);
        // }

        static bool IsCriticalProp(string key)
        {
            for (int i = 0; i < s_CriticalProps.Length; ++i)
            {
                if (s_CriticalProps[i] == key)
                {
                    return true;
                }
            }

            return false;
        }

        static string ShowTextField(Var var)
        {
            switch (var.VariableType)
            {
                case VarType.Bool: return var.GetBool().ToString();
                case VarType.Int32: return var.GetInt().ToString();
                case VarType.Int64: return var.GetInt64().ToString();
                case VarType.Float: return var.GetFloat().ToString(CultureInfo.InvariantCulture);
                case VarType.Double: return var.GetDouble().ToString(CultureInfo.InvariantCulture);
                case VarType.String: return var.GetString();
                case VarType.WideStr: return var.GetString();
                case VarType.Object: return var.GetEntity().ToString();
                case VarType.UserData: return var.GetObject().ToString();
                case VarType.Binary: return var.GetBinary().ToString();
                default: return string.Empty;
            }
        }

        void ShowProp(Entity entity, string key)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(key, m_LabelHeightLayoutOp);
            VarType vType = entity.GetVariableType(key);

            switch (vType)
            {
                case VarType.Bool:
                    entity.SetBool(key, EditorGUILayout.Toggle(entity.GetBool(key), m_LabelHeightLayoutOp));
                    break;
                case VarType.Int32:
                    entity.SetInt(key, EditorGUILayout.IntField(entity.GetInt(key), m_LabelHeightLayoutOp));
                    break;
                case VarType.Int64:
                    entity.SetInt64(key, EditorGUILayout.LongField(entity.GetInt64(key), m_LabelHeightLayoutOp));
                    break;
                case VarType.Float:
                    entity.SetFloat(key, EditorGUILayout.FloatField(entity.GetFloat(key), m_LabelHeightLayoutOp));
                    break;
                case VarType.Double:
                    entity.SetDouble(key, EditorGUILayout.DoubleField(entity.GetDouble(key), m_LabelHeightLayoutOp));
                    break;
                case VarType.String:
                    entity.SetString(key, EditorGUILayout.TextField(entity.GetString(key), m_LabelHeightLayoutOp));
                    break;
                case VarType.Object:
                    EditorGUILayout.LabelField(entity.GetEntityID(key).ToString(), m_LabelHeightLayoutOp);
                    break;
                default:
                    EditorGUILayout.LabelField("Not Support VarType:" + vType, m_LabelHeightLayoutOp);
                    break;
            }
            EditorGUILayout.EndHorizontal();
        }

        public static bool DrawHeader(string text) { return DrawHeader(text, text, false, false); }

        public static bool DrawHeader(string text, string key, bool forceOn, bool minimalistic)
        {
            bool state = EditorPrefs.GetBool(key, true);

            if (!minimalistic) GUILayout.Space(3f);
            if (!forceOn && !state) GUI.backgroundColor = new(0.8f, 0.8f, 0.8f);
            GUILayout.BeginHorizontal();
            GUI.changed = false;

            if (minimalistic)
            {
                if (state) text = "\u25BC" + (char)0x200a + text;
                else text = "\u25BA" + (char)0x200a + text;

                GUILayout.BeginHorizontal();
                GUI.contentColor = EditorGUIUtility.isProSkin ? new(1f, 1f, 1f, 0.7f) : new Color(0f, 0f, 0f, 0.7f);
                if (!GUILayout.Toggle(true, text, "PreToolbar2", GUILayout.MinWidth(20f))) state = !state;
                GUI.contentColor = Color.white;
                GUILayout.EndHorizontal();
            }
            else
            {
                text = "<b><size=11>" + text + "</size></b>";
                if (state) text = "\u25BC " + text;
                else text = "\u25BA " + text;
                if (!GUILayout.Toggle(true, text, "dragtab", GUILayout.MinWidth(20f))) state = !state;
            }

            if (GUI.changed) EditorPrefs.SetBool(key, state);

            if (!minimalistic) GUILayout.Space(2f);
            GUILayout.EndHorizontal();
            GUI.backgroundColor = Color.white;
            if (!forceOn && !state) GUILayout.Space(3f);
            return state;
        }

        static string s_PropName;
        static string s_PropValue;

        static VarType s_VarType = VarType.String;

        /// <summary>
        /// 绘制(设置/添加)属性区域
        /// </summary>
        void DrawSetPropArea()
        {
            if (igo == null)
            {
                return;
            }

            if (DrawHeader("SetPropArea(设置/添加 属性)"))
            {
                EditorGUILayout.BeginVertical("Box");
                s_VarType = (VarType)EditorGUILayout.EnumPopup("PropType", s_VarType);
                s_PropName = EditorGUILayout.TextField("PropName:", s_PropName);
                s_PropValue = EditorGUILayout.TextField("PropValue:", s_PropValue);

                if (GUILayout.Button("SetProp"))
                {
                    if (string.IsNullOrEmpty(s_PropName) || string.IsNullOrEmpty(s_PropValue))
                    {
                        Log.Error("EntityEditor:DrawSetPropArea. PropName or PropValue is empty");
                        return;
                    }

                    Log.Info($"EntityEditor:DrawSetPropArea. Set prop finish. EntityID:{igo.ID}, Name:{s_PropName}, Value:{s_PropValue}");

                    switch (s_VarType)
                    {
                        case VarType.Bool:
                            igo.ID.SetProp(s_PropName, new(s_PropValue.ToBool()));
                            break;
                        case VarType.Int32:
                            igo.ID.SetProp(s_PropName, new(s_PropValue.ToInt()));
                            break;
                        case VarType.Int64:
                            igo.ID.SetProp(s_PropName, new(s_PropValue.ToInt64()));
                            break;
                        case VarType.Float:
                            igo.ID.SetProp(s_PropName, new(s_PropValue.ToFloat()));
                            break;
                        case VarType.String:
                            igo.ID.SetProp(s_PropName, new(s_PropValue));
                            break;

                        case VarType.None:
                        case VarType.Double:
                        case VarType.WideStr:
                        case VarType.Object:
                        case VarType.UserData:
                        case VarType.Binary:
                        case VarType.Max:
                        default:
                            Log.Error($"EntityEditor:DrawSetPropArea. Type {s_VarType} Unprocessed");
                            break;
                    }
                }
                EditorGUILayout.EndVertical();
            }
        }
    }
}