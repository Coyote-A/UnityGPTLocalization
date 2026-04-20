using System;
using System.Reflection;
using UnityEditor;
using UnityEditor.Localization;
using UnityEngine;

namespace RedGame.Framework.EditorTools
{
    public partial class LocalizeGptWindow
    {
        private StringTableCollection _curCollection;
        private StringTableCollection[] _collections;
        private string[] _collectionNames;
        
        private void RefreshStringTableCollection()
        {
            // Preserve currently selected collection if possible
            var previous = _curCollection;
            CancelTask();

            string[] guids = AssetDatabase.FindAssets("t:StringTableCollection");
            _collections = new StringTableCollection[guids.Length];
            _collectionNames = new string[guids.Length];
            for (int i = 0; i < guids.Length; i++)
            {
                var path = AssetDatabase.GUIDToAssetPath(guids[i]);
                _collections[i] = AssetDatabase.LoadAssetAtPath<StringTableCollection>(path);
                _collectionNames[i] = _collections[i].name;
            }

            _curCollection = null;

            if (_collections.Length > 0)
            {
                if (previous != null)
                {
                    // 1. Try to keep the exact same asset instance
                    int index = Array.IndexOf(_collections, previous);
                    if (index >= 0)
                    {
                        _curCollection = _collections[index];
                    }
                    else
                    {
                        // 2. Fallback: match by name (useful if GUIDs изменились после переименования/перемещения)
                        for (int i = 0; i < _collections.Length; i++)
                        {
                            if (_collections[i] != null && _collections[i].name == previous.name)
                            {
                                _curCollection = _collections[i];
                                break;
                            }
                        }
                    }
                }

                // 3. If nothing найдено, просто берём первую коллекцию, как было раньше
                if (_curCollection == null)
                {
                    _curCollection = _collections[0];
                }
            }
            else
            {
                _curCollection = null;
            }

            _recs = null;
        }

        // Notify Localization Table Editor to refresh by calling
        // internal method LocalizationEditorSettings.EditorEvents.RaiseCollectionModified
        private void NotifyStringTableEditorRefresh()
        {
            Type classType = typeof(LocalizationEditorEvents);
            MethodInfo methodInfo = classType.GetMethod("RaiseCollectionModified",
                BindingFlags.Instance | BindingFlags.NonPublic);

            if (methodInfo != null)
            {
                try
                {
                    methodInfo.Invoke(LocalizationEditorSettings.EditorEvents, new object[] { this, _curCollection });
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
        }
    }
}
