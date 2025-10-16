using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace BansheeGz.BGDatabase.Editor
{
    [CustomEditor(typeof(BGEntityOdinGo), true)]
    public class BGEditorEntityOdinGo : OdinEditor
    {
        protected BGEditorEntityGoDefault delegateEditor;

        protected override void OnEnable()
        {
            base.OnEnable();
            if (delegateEditor == null) delegateEditor = new BGEditorEntityGoDefault(new BGEditorEntityGoDefault.BGEditorEntityGoContext((BGEntityGo) target, serializedObject, Repaint));
        }

        public override void OnInspectorGUI()
        {
            delegateEditor.Gui();
            DrawDefaultInspector();
        }
    }
}