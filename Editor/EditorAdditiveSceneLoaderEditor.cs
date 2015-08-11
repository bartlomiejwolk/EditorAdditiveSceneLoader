#define DEBUG_LOGGER

// Copyright (c) 2015 Bartlomiej Wolk (bartlomiejwolk@gmail.com)
//  
// This file is part of the EditorAdditiveSceneLoader extension for Unity.
// Licensed under the MIT license. See LICENSE file in the project root folder.

using System.Collections.Generic;
using FileLogger;
using Rotorz.ReorderableList;
using UniRx;
using UnityEditor;
using UnityEngine;

namespace EditorAdditiveSceneLoaderModule {

    [CustomEditor(typeof(EditorAdditiveSceneLoader))]
    [CanEditMultipleObjects]
    public sealed class EditorAdditiveSceneLoaderEditor : Editor {
        #region FIELDS

        private EditorAdditiveSceneLoader Script { get; set; }

        #endregion FIELDS

        #region SERIALIZED PROPERTIES

        private SerializedProperty description;
        private SerializedProperty scenesToLoad;

        #endregion SERIALIZED PROPERTIES

        #region UNITY MESSAGES

        public override void OnInspectorGUI() {
            serializedObject.Update();

            DrawVersionLabel();
            DrawDescriptionTextArea();

            EditorGUILayout.Space();

            DrawSceneToLoadList();

            EditorGUILayout.BeginHorizontal();

            DrawLoadScenesButton();
            DrawUnloadScenesButton();

            EditorGUILayout.EndHorizontal();

            serializedObject.ApplyModifiedProperties();
        }
        private void OnEnable() {
            Script = (EditorAdditiveSceneLoader)target;

            description = serializedObject.FindProperty("description");
            scenesToLoad = serializedObject.FindProperty("scenesToLoad");
        }

        #endregion UNITY MESSAGES

        #region INSPECTOR CONTROLS
        private void DrawUnloadScenesButton() {
            var buttonPressed = GUILayout.Button(new GUIContent(
                "Unload Scenes",
                "Destroy loaded scene game objects. Each must have a tag " +
                "with the scene name attached."));

            if (buttonPressed) {
                var scenesNo = scenesToLoad.arraySize;

                for (int i = 0; i < scenesNo; i++) {
                    var sceneProperty = scenesToLoad.GetArrayElementAtIndex(i);
                    var sceneObject = sceneProperty.objectReferenceValue;
                    var sceneName = "";
                    var sceneObjectString = sceneObject.ToString();
                    if (sceneObjectString.Contains(" (UnityEngine.SceneAsset)")) {
                        sceneName = sceneObjectString.Substring(
                            0,
                            sceneObjectString.IndexOf(
                                " (UnityEngine.SceneAsset)"));
                    }

                    DestroyImmediate(GameObject.FindWithTag(sceneName));
                }
            }
        }

        private void DrawLoadScenesButton() {
            var buttonPressed = GUILayout.Button(new GUIContent(
                "Load Scenes",
                "Load scenes additively in order specified in the " +
                "list"));

            if (buttonPressed) {
                LoadScenesAdditively();
            }
        }
        private void DrawSceneToLoadList() {
            ReorderableListGUI.Title("Scenes");
            ReorderableListGUI.ListField(scenesToLoad);
        }


        private void DrawVersionLabel() {
            EditorGUILayout.LabelField(
                string.Format(
                    "{0} ({1})",
                    EditorAdditiveSceneLoader.Version,
                    EditorAdditiveSceneLoader.Extension));
        }

        private void DrawDescriptionTextArea() {
            description.stringValue = EditorGUILayout.TextArea(
                description.stringValue);
        }

        #endregion INSPECTOR

        #region METHODS
        private void LoadScenesAdditively() {
            // Number of scenes to load.
            var scenesNo = scenesToLoad.arraySize;
            for (int i = 0; i < scenesNo; i++) {
                var sceneProperty = scenesToLoad.GetArrayElementAtIndex(i);
                var sceneObject = sceneProperty.objectReferenceValue;
                var scenePath =
                    AssetDatabase.GetAssetOrScenePath(sceneObject);
                // Load scene.
                EditorApplication.OpenSceneAdditive(scenePath);
            }
        }


        [MenuItem("Component/Modules/EditorAdditiveSceneLoader")]
        private static void AddEntryToComponentMenu() {
            if (Selection.activeGameObject != null) {
                Selection.activeGameObject.AddComponent(typeof(EditorAdditiveSceneLoader));
            }
        }

        #endregion METHODS
    }

}